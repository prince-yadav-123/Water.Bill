using Microsoft.Extensions.Configuration;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class WhatsAppSender : IWhatsAppSender
{
    private readonly IConfiguration _configuration;

    public WhatsAppSender(IConfiguration configuration) => _configuration = configuration;

    public Task<CommunicationSendResult> SendAsync(string mobileNo, string message, string? externalTemplateId, CancellationToken ct = default)
    {
        var section = _configuration.GetSection("Communication:WhatsApp");
        var provider = section["Provider"];
        var baseUrl = section["BaseUrl"];
        var apiKey = section["ApiKey"];
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            return Task.FromResult(CommunicationSendResult.Skipped("WhatsApp provider is not configured."));

        return Task.FromResult(CommunicationSendResult.Skipped($"WhatsApp provider '{provider}' integration is pending."));
    }
}

