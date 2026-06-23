using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Water.Bill.Application.DTOs.Consumer;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Options;

namespace Water.Bill.Infrastructure.Services;

public class PimsConsumerInfoService : IPimsConsumerInfoService
{
    private static readonly HttpClient HttpClient = new();
    private readonly PimsApiSettings _settings;
    private readonly ILogger<PimsConsumerInfoService> _logger;

    public PimsConsumerInfoService(IOptions<PimsApiSettings> settings, ILogger<PimsConsumerInfoService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ConsumerPimsContactResult> GetDetailsByRidAsync(long rid, CancellationToken ct = default)
    {
        if (rid <= 0)
            throw new InvalidOperationException("RID was not found for this Consumer Number. Please contact the Authority.");

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) || string.IsNullOrWhiteSpace(_settings.GetDetailsByRidEndpoint))
            throw new InvalidOperationException("PIMS API settings are not configured.");

        var url = CombineUrl(_settings.BaseUrl, _settings.GetDetailsByRidEndpoint);
        var content = new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["RID"] = rid.ToString(),
            ["UserName"] = _settings.UserName,
            ["Password"] = _settings.Password
        }.Where(x => !string.IsNullOrWhiteSpace(x.Value))!
        .ToDictionary(x => x.Key, x => x.Value!));

        using var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Content = content
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (_settings.TimeoutSeconds > 0)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

        try
        {
            using var response = await HttpClient.SendAsync(request, timeoutCts.Token);
            var payload = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"PIMS API returned HTTP {(int)response.StatusCode}.");

            return ParseContact(payload);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new InvalidOperationException("PIMS API request timed out. Please try again later.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "PIMS API request failed for RID {Rid}.", rid);
            throw new InvalidOperationException("Unable to reach PIMS API. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "PIMS API response parsing failed for RID {Rid}.", rid);
            throw new InvalidOperationException("PIMS API response could not be read.");
        }
    }

    private static ConsumerPimsContactResult ParseContact(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        var mobile = FindValue(root, "Mobile")
            ?? FindValue(root, "mobile")
            ?? FindValue(root, "MobileNo")
            ?? FindValue(root, "MobileNumber");
        var email = FindValue(root, "Email")
            ?? FindValue(root, "email")
            ?? FindValue(root, "EmailId")
            ?? FindValue(root, "EmailID");

        return new ConsumerPimsContactResult
        {
            MobileNo = NormalizeMobile(mobile),
            Email = NormalizeText(email)
        };
    }

    private static string? FindValue(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return property.Value.ToString();

                var nested = FindValue(property.Value, name);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindValue(item, name);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }

        return null;
    }

    private static string CombineUrl(string baseUrl, string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        var baseTrimmed = baseUrl.TrimEnd('/');
        var endpointTrimmed = endpoint.TrimStart('/');
        return $"{baseTrimmed}/{endpointTrimmed}";
    }

    private static string? NormalizeMobile(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 10 ? digits : null;
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
            ? null
            : value.Trim();
}
