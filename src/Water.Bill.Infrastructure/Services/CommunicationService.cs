using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class CommunicationService : ICommunicationService
{
    private readonly ApplicationDbContext _db;
    private readonly ITemplateRenderer _renderer;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IInAppNotificationSender _inAppSender;
    private readonly IConfiguration _configuration;

    public CommunicationService(
        ApplicationDbContext db,
        ITemplateRenderer renderer,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IWhatsAppSender whatsAppSender,
        IInAppNotificationSender inAppSender,
        IConfiguration configuration)
    {
        _db = db;
        _renderer = renderer;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _whatsAppSender = whatsAppSender;
        _inAppSender = inAppSender;
        _configuration = configuration;
    }

    public async Task SendAsync(
        string purposeKey,
        NotificationRecipient recipient,
        IReadOnlyDictionary<string, string?> values,
        NotificationChannelOptions channels,
        string? referenceType = null,
        string? referenceId = null,
        string? referenceNo = null,
        CancellationToken ct = default)
    {
        foreach (var channel in ResolveChannels(channels))
        {
            try
            {
                await SendChannelAsync(purposeKey, channel, recipient, values, referenceType, referenceId, referenceNo, ct);
            }
            catch
            {
                // Notification infrastructure must never break the business flow.
            }
        }
    }

    private async Task SendChannelAsync(string purposeKey, string channel, NotificationRecipient recipient, IReadOnlyDictionary<string, string?> values, string? referenceType, string? referenceId, string? referenceNo, CancellationToken ct)
    {
        var now = DateTime.Now;
        var purpose = await _db.CommunicationPurposes.AsNoTracking().FirstOrDefaultAsync(x => x.PurposeKey == purposeKey && x.IsActive, ct);
        var template = await _db.CommunicationTemplates
            .AsNoTracking()
            .Where(x => x.PurposeKey == purposeKey
                && x.Channel == channel
                && x.IsActive
                && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (purpose is null || template is null)
        {
            await LogAsync(purposeKey, channel, recipient, null, null, null, null, "Skipped", "Active communication template was not found.", referenceType, referenceId, referenceNo, now, ct);
            return;
        }

        var allowed = ParsePlaceholders(purpose.AllowedPlaceholders);
        var used = _renderer.ExtractPlaceholders($"{template.Subject} {template.Body}");
        var unknown = used.Where(x => !allowed.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();
        if (unknown.Count > 0)
        {
            await LogAsync(purposeKey, channel, recipient, template.Id, template.ExternalTemplateId, template.Subject, template.Body, "Failed", $"Template contains unknown placeholder(s): {string.Join(", ", unknown)}.", referenceType, referenceId, referenceNo, now, ct);
            return;
        }

        var subject = _renderer.Render(template.Subject ?? template.TemplateName, values);
        var body = _renderer.Render(template.Body, values);
        var result = await DispatchAsync(channel, recipient, subject, body, template.ExternalTemplateId, purposeKey, referenceType, referenceId, referenceNo, ct);
        await LogAsync(purposeKey, channel, recipient, template.Id, template.ExternalTemplateId, subject, body, result.Status, result.ErrorMessage, referenceType, referenceId, referenceNo, now, ct);
    }

    private async Task<CommunicationSendResult> DispatchAsync(string channel, NotificationRecipient recipient, string subject, string body, string? externalTemplateId, string purposeKey, string? referenceType, string? referenceId, string? referenceNo, CancellationToken ct)
    {
        if (channel == CommunicationChannels.Email)
        {
            if (string.IsNullOrWhiteSpace(recipient.Email))
                return CommunicationSendResult.Skipped("Recipient email is not available.");
            return await _emailSender.SendAsync(recipient.Email, recipient.Name, subject, ApplyEmailLayout(subject, body), ct);
        }

        if (channel == CommunicationChannels.Sms)
        {
            if (string.IsNullOrWhiteSpace(recipient.Mobile))
                return CommunicationSendResult.Skipped("Recipient mobile is not available.");
            return await _smsSender.SendAsync(recipient.Mobile, body, externalTemplateId, ct);
        }

        if (channel == CommunicationChannels.WhatsApp)
        {
            if (string.IsNullOrWhiteSpace(recipient.Mobile))
                return CommunicationSendResult.Skipped("Recipient mobile is not available.");
            return await _whatsAppSender.SendAsync(recipient.Mobile, body, externalTemplateId, ct);
        }

        if (channel == CommunicationChannels.InApp)
            return await _inAppSender.SendAsync(recipient, subject, body, purposeKey, referenceType, referenceId, referenceNo, ct);

        return CommunicationSendResult.Skipped($"Channel '{channel}' is not supported.");
    }

    private string ApplyEmailLayout(string title, string body)
    {
        var layoutPath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "DefaultEmailLayout.html");
        if (!File.Exists(layoutPath))
            layoutPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "DefaultEmailLayout.html");

        if (!File.Exists(layoutPath))
            return body;

        var layout = File.ReadAllText(layoutPath);
        return layout
            .Replace("{{EmailTitle}}", WebUtility.HtmlEncode(title))
            .Replace("{{EmailBody}}", body)
            .Replace("{{FooterText}}", _configuration["Communication:Email:FooterText"] ?? "This is an automated message from Noida Water Billing System.");
    }

    private async Task LogAsync(string purposeKey, string channel, NotificationRecipient recipient, int? templateId, string? externalTemplateId, string? subject, string? body, string status, string? error, string? referenceType, string? referenceId, string? referenceNo, DateTime createdAt, CancellationToken ct)
    {
        _db.CommunicationLogs.Add(new CommunicationLog
        {
            PurposeKey = purposeKey,
            Channel = channel,
            RecipientName = recipient.Name,
            RecipientEmail = recipient.Email,
            RecipientMobile = recipient.Mobile,
            Subject = subject,
            MessageBody = body ?? string.Empty,
            TemplateId = templateId,
            ExternalTemplateId = externalTemplateId,
            Status = status,
            ErrorMessage = error,
            SentAt = status == "Sent" ? DateTime.Now : null,
            CreatedAt = createdAt,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNo = referenceNo
        });
        await _db.SaveChangesAsync(ct);
    }

    private static IReadOnlyList<string> ResolveChannels(NotificationChannelOptions channels)
    {
        var result = new List<string>();
        if (channels.Email) result.Add(CommunicationChannels.Email);
        if (channels.Sms) result.Add(CommunicationChannels.Sms);
        if (channels.WhatsApp) result.Add(CommunicationChannels.WhatsApp);
        if (channels.InApp) result.Add(CommunicationChannels.InApp);
        return result;
    }

    private static IReadOnlyList<string> ParsePlaceholders(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

}
