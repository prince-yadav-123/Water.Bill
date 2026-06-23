using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class CommunicationConfigurationService : ICommunicationConfigurationService
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommunicationConfigurationService> _logger;

    public CommunicationConfigurationService(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger<CommunicationConfigurationService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommunicationChannelSettingsDto>> GetAllAsync(CancellationToken ct = default)
        => new[]
        {
            await GetAsync(CommunicationChannels.Email, ct),
            await GetAsync(CommunicationChannels.Sms, ct),
            await GetAsync(CommunicationChannels.WhatsApp, ct)
        };

    public async Task<CommunicationChannelSettingsDto> GetAsync(string channelName, CancellationToken ct = default)
    {
        var normalized = NormalizeChannelName(channelName);
        return normalized switch
        {
            CommunicationChannels.Email => await LoadEmailAsync(ct),
            CommunicationChannels.Sms => await LoadSmsAsync(ct),
            CommunicationChannels.WhatsApp => await LoadWhatsAppAsync(ct),
            _ => throw new InvalidOperationException($"Unsupported communication channel '{channelName}'.")
        };
    }

    public async Task<CommunicationChannelSettingsDto> SaveAsync(
        CommunicationChannelSettingsDto model,
        int? updatedByUserId = null,
        string? updatedByName = null,
        CancellationToken ct = default)
    {
        var channel = NormalizeChannelName(model.ChannelName);
        if (string.IsNullOrWhiteSpace(channel))
            throw new InvalidOperationException("Channel name is required.");

        var normalizedJson = channel switch
        {
            CommunicationChannels.Email => NormalizeSettingsForStorage(ParseSettings<EmailCommunicationSettings>(model.ConfigurationJson)),
            CommunicationChannels.Sms => NormalizeSettingsForStorage(ParseSettings<SmsCommunicationSettings>(model.ConfigurationJson)),
            CommunicationChannels.WhatsApp => NormalizeSettingsForStorage(ParseSettings<WhatsAppCommunicationSettings>(model.ConfigurationJson)),
            _ => throw new InvalidOperationException($"Unsupported communication channel '{model.ChannelName}'.")
        };

        var row = await _db.CommunicationChannelSettings
            .FirstOrDefaultAsync(x => x.ChannelName == channel && !x.IsDeleted, ct);

        var now = AppTime.IndiaNow;
        if (row is null)
        {
            row = new CommunicationChannelSetting
            {
                ChannelName = channel,
                CreatedAt = now,
                CreatedByUserId = updatedByUserId,
                CreatedByName = updatedByName,
                IsDeleted = false
            };
            _db.CommunicationChannelSettings.Add(row);
        }

        row.IsEnabled = model.IsEnabled;
        row.ConfigurationJson = normalizedJson;
        row.UpdatedAt = now;
        row.UpdatedByUserId = updatedByUserId;
        row.UpdatedByName = updatedByName;
        row.IsDeleted = false;

        await _db.SaveChangesAsync(ct);

        return BuildDto(row, normalizedJson, updatedByName);
    }

    public async Task<EmailCommunicationSettings> GetEmailSettingsAsync(CancellationToken ct = default)
    {
        var fallback = BuildEmailFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.Email, ct);
        if (row is null)
            return fallback;

        var parsed = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.Email);
        parsed.IsEnabled = row.IsEnabled;
        return parsed;
    }

    public async Task<SmsCommunicationSettings> GetSmsSettingsAsync(CancellationToken ct = default)
    {
        var fallback = BuildSmsFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.Sms, ct);
        if (row is null)
            return fallback;

        var parsed = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.Sms);
        parsed.IsEnabled = row.IsEnabled;
        return parsed;
    }

    public async Task<WhatsAppCommunicationSettings> GetWhatsAppSettingsAsync(CancellationToken ct = default)
    {
        var fallback = BuildWhatsAppFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.WhatsApp, ct);
        if (row is null)
            return fallback;

        var parsed = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.WhatsApp);
        parsed.IsEnabled = row.IsEnabled;
        return parsed;
    }

    private async Task<CommunicationChannelSettingsDto> LoadEmailAsync(CancellationToken ct)
    {
        var fallback = BuildEmailFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.Email, ct);
        if (row is null)
            return BuildDto(null, SerializeSettingsForUi(fallback), null, CommunicationChannels.Email, fallback.IsEnabled, false);

        var settings = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.Email);
        settings.IsEnabled = row.IsEnabled;
        return BuildDto(row, SerializeSettingsForUi(settings), row.UpdatedByName ?? row.CreatedByName, CommunicationChannels.Email, settings.IsEnabled, true);
    }

    private async Task<CommunicationChannelSettingsDto> LoadSmsAsync(CancellationToken ct)
    {
        var fallback = BuildSmsFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.Sms, ct);
        if (row is null)
            return BuildDto(null, SerializeSettingsForUi(fallback), null, CommunicationChannels.Sms, fallback.IsEnabled, false);

        var settings = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.Sms);
        settings.IsEnabled = row.IsEnabled;
        return BuildDto(row, SerializeSettingsForUi(settings), row.UpdatedByName ?? row.CreatedByName, CommunicationChannels.Sms, settings.IsEnabled, true);
    }

    private async Task<CommunicationChannelSettingsDto> LoadWhatsAppAsync(CancellationToken ct)
    {
        var fallback = BuildWhatsAppFallback();
        var row = await GetChannelRowAsync(CommunicationChannels.WhatsApp, ct);
        if (row is null)
            return BuildDto(null, SerializeSettingsForUi(fallback), null, CommunicationChannels.WhatsApp, fallback.IsEnabled, false);

        var settings = TryParseSettings(row.ConfigurationJson, fallback, CommunicationChannels.WhatsApp);
        settings.IsEnabled = row.IsEnabled;
        return BuildDto(row, SerializeSettingsForUi(settings), row.UpdatedByName ?? row.CreatedByName, CommunicationChannels.WhatsApp, settings.IsEnabled, true);
    }

    private async Task<CommunicationChannelSetting?> GetChannelRowAsync(string channelName, CancellationToken ct)
        => await _db.CommunicationChannelSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChannelName == channelName && !x.IsDeleted, ct);

    private CommunicationChannelSettingsDto BuildDto(
        CommunicationChannelSetting? row,
        string configurationJson,
        string? lastUpdatedByName,
        string? channelName = null,
        bool? isEnabled = null,
        bool isConfiguredInDb = true)
    {
        var actualChannel = NormalizeChannelName(channelName ?? row?.ChannelName ?? string.Empty);
        return new CommunicationChannelSettingsDto
        {
            Id = row?.Id ?? 0,
            ChannelName = actualChannel,
            DisplayName = GetDisplayName(actualChannel),
            IsEnabled = isEnabled ?? row?.IsEnabled ?? true,
            ConfigurationJson = configurationJson,
            LastUpdatedByName = lastUpdatedByName,
            LastUpdatedAt = row?.UpdatedAt ?? row?.CreatedAt,
            IsConfiguredInDb = isConfiguredInDb && row is not null
        };
    }

    private static string GetDisplayName(string channelName)
        => NormalizeChannelName(channelName) switch
        {
            CommunicationChannels.Email => "Email",
            CommunicationChannels.Sms => "SMS / Text Message",
            CommunicationChannels.WhatsApp => "WhatsApp",
            _ => channelName
        };

    private static string NormalizeChannelName(string? channelName)
    {
        var value = (channelName ?? string.Empty).Trim();
        if (string.Equals(value, CommunicationChannels.Email, StringComparison.OrdinalIgnoreCase)) return CommunicationChannels.Email;
        if (string.Equals(value, CommunicationChannels.Sms, StringComparison.OrdinalIgnoreCase) || string.Equals(value, "Text Message", StringComparison.OrdinalIgnoreCase)) return CommunicationChannels.Sms;
        if (string.Equals(value, CommunicationChannels.WhatsApp, StringComparison.OrdinalIgnoreCase)) return CommunicationChannels.WhatsApp;
        return value;
    }

    private EmailCommunicationSettings BuildEmailFallback()
    {
        var section = _configuration.GetSection("Communication:Email");
        return new EmailCommunicationSettings
        {
            IsEnabled = ReadBool(section["Enabled"], true),
            Provider = ReadString(section["Provider"], "Smtp"),
            Host = ReadString(section["Host"]),
            Port = ReadInt(section["Port"], 587),
            Username = ReadString(section["Username"]),
            Password = ReadString(section["Password"]),
            FromEmail = ReadString(section["FromEmail"]),
            FromName = ReadString(section["FromName"], "Noida Water Billing"),
            EnableSsl = ReadBool(section["EnableSsl"], true),
            FooterText = ReadString(section["FooterText"], "This is an automated message from Noida Water Billing System.")
        };
    }

    private SmsCommunicationSettings BuildSmsFallback()
    {
        var primary = _configuration.GetSection("Communication:Sms");
        var legacy = _configuration.GetSection("Sms:Otp");

        return new SmsCommunicationSettings
        {
            IsEnabled = ReadBool(primary["Enabled"], ReadBool(legacy["Enabled"], true)),
            Provider = ReadString(primary["Provider"], ReadString(legacy["Provider"])),
            BaseUrl = ReadString(primary["BaseUrl"], ReadString(legacy["BaseUrl"])),
            ApiKey = ReadString(primary["ApiKey"], ReadString(legacy["ApiKey"])),
            SenderId = ReadString(primary["SenderId"], ReadString(legacy["SenderId"])),
            PeId = ReadString(primary["PeId"], ReadString(legacy["PeId"])),
            TemplateId = ReadString(primary["TemplateId"], ReadString(legacy["TemplateId"])),
            CountryCode = ReadString(primary["CountryCode"], ReadString(legacy["CountryCode"], "91")),
            Msg91AuthKey = ReadString(primary["Msg91AuthKey"], ReadString(legacy["Msg91AuthKey"])),
            Route = ReadString(primary["Route"], ReadString(legacy["Route"], "4")),
            DefaultOtp = NormalizeConfiguredOtp(primary["DefaultOtp"]) ?? NormalizeConfiguredOtp(legacy["DefaultOtp"]) ?? string.Empty
        };
    }

    private WhatsAppCommunicationSettings BuildWhatsAppFallback()
    {
        var section = _configuration.GetSection("Communication:WhatsApp");
        return new WhatsAppCommunicationSettings
        {
            IsEnabled = ReadBool(section["Enabled"], true),
            Provider = ReadString(section["Provider"]),
            BaseUrl = ReadString(section["BaseUrl"]),
            ApiKey = ReadString(section["ApiKey"]),
            SenderPhoneNo = ReadString(section["SenderPhoneNo"])
        };
    }

    private static T ParseSettings<T>(string? configurationJson) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
            return new T();

        var parsed = JsonSerializer.Deserialize<T>(configurationJson, ReadOptions);
        if (parsed is null)
            throw new InvalidOperationException("Configuration JSON could not be parsed.");

        return parsed;
    }

    private T TryParseSettings<T>(string? configurationJson, T fallback, string channelName) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
            return fallback;

        try
        {
            var parsed = JsonSerializer.Deserialize<T>(configurationJson, ReadOptions);
            return parsed ?? fallback;
        }
        catch (JsonException)
        {
            _logger.LogWarning("Communication channel configuration for {ChannelName} contains invalid JSON. Falling back to appsettings.", channelName);
            return fallback;
        }
    }

    private static string SerializeSettings<T>(T settings) where T : class
        => JsonSerializer.Serialize(settings, WriteOptions);

    private static string SerializeSettingsForUi<T>(T settings) where T : class
        => StripManagedConfigurationKeys(SerializeSettings(settings));

    private static string NormalizeSettingsForStorage<T>(T settings) where T : class
        => StripManagedConfigurationKeys(SerializeSettings(settings));

    private static string StripManagedConfigurationKeys(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        try
        {
            var node = JsonNode.Parse(json);
            if (node is JsonObject obj)
            {
                obj.Remove("IsEnabled");
                return obj.ToJsonString(WriteOptions);
            }
        }
        catch (JsonException)
        {
            // Ignore here and keep the original validated JSON.
        }

        return json;
    }

    private static bool ReadBool(string? value, bool defaultValue)
        => bool.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static int ReadInt(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static string ReadString(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static string? NormalizeConfiguredOtp(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 6 ? digits : null;
    }
}
