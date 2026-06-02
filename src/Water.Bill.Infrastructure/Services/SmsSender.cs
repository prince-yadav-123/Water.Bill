using Microsoft.Extensions.Configuration;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class SmsSender : ISmsSender
{
    private static readonly HttpClient HttpClient = new();
    private readonly IConfiguration _configuration;

    public SmsSender(IConfiguration configuration) => _configuration = configuration;

    public async Task<CommunicationSendResult> SendAsync(string mobileNo, string message, string? externalTemplateId, CancellationToken ct = default)
    {
        var section = _configuration.GetSection("Communication:Sms");
        var provider = section["Provider"]?.Trim();
        var baseUrl = section["BaseUrl"];
        var apiKey = section["ApiKey"];
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            return CommunicationSendResult.Skipped("SMS provider is not configured.");

        return provider.ToLowerInvariant() switch
        {
            "authkey" => await SendAuthkeyAsync(section, mobileNo, message, externalTemplateId, ct),
            "msg91" => await SendMsg91Async(section, mobileNo, message, ct),
            _ => CommunicationSendResult.Skipped($"SMS provider '{provider}' is not supported.")
        };
    }

    private static async Task<CommunicationSendResult> SendAuthkeyAsync(IConfigurationSection section, string mobileNo, string message, string? externalTemplateId, CancellationToken ct)
    {
        var baseUrl = section["BaseUrl"] ?? "https://api.authkey.io/request";
        var query = new Dictionary<string, string?>
        {
            ["authkey"] = section["ApiKey"],
            ["mobile"] = SanitizePhone(mobileNo),
            ["country_code"] = string.IsNullOrWhiteSpace(section["CountryCode"]) ? "91" : section["CountryCode"],
            ["sms"] = message,
            ["sender"] = section["SenderId"],
            ["pe_id"] = section["PeId"],
            ["template_id"] = externalTemplateId ?? section["TemplateId"]
        };

        return await SendGetAsync(baseUrl, query, ct);
    }

    private static async Task<CommunicationSendResult> SendMsg91Async(IConfigurationSection section, string mobileNo, string message, CancellationToken ct)
    {
        var baseUrl = string.IsNullOrWhiteSpace(section["BaseUrl"])
            ? "https://api.msg91.com/api/sendhttp.php"
            : section["BaseUrl"]!;

        var query = new Dictionary<string, string?>
        {
            ["authkey"] = section["Msg91AuthKey"] ?? section["ApiKey"],
            ["mobiles"] = SanitizePhone(mobileNo),
            ["message"] = message,
            ["sender"] = section["SenderId"],
            ["route"] = string.IsNullOrWhiteSpace(section["Route"]) ? "4" : section["Route"],
            ["country"] = string.IsNullOrWhiteSpace(section["CountryCode"]) ? "91" : section["CountryCode"]
        };

        return await SendGetAsync(baseUrl, query, ct);
    }

    private static async Task<CommunicationSendResult> SendGetAsync(string baseUrl, IReadOnlyDictionary<string, string?> query, CancellationToken ct)
    {
        var url = BuildUrl(baseUrl, query);
        try
        {
            using var response = await HttpClient.GetAsync(url, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return response.IsSuccessStatusCode
                ? CommunicationSendResult.Sent()
                : CommunicationSendResult.Failed($"SMS provider returned HTTP {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex)
        {
            return CommunicationSendResult.Failed(ex.Message);
        }
    }

    private static string BuildUrl(string baseUrl, IReadOnlyDictionary<string, string?> query)
    {
        var pairs = query
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}");

        var separator = baseUrl.Contains('?') ? "&" : "?";
        return baseUrl + separator + string.Join("&", pairs);
    }

    private static string SanitizePhone(string phone)
        => (phone ?? string.Empty)
            .Replace("+", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty);
}
