using Microsoft.Extensions.Configuration;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class WhatsAppSender : IWhatsAppSender
{
    private readonly ICommunicationConfigurationService _communicationConfigurationService;

    public WhatsAppSender(ICommunicationConfigurationService communicationConfigurationService)
        => _communicationConfigurationService = communicationConfigurationService;

    public async Task<CommunicationSendResult> SendAsync(string mobileNo, string message, string? externalTemplateId, CancellationToken ct = default)
    {
        var settings = await _communicationConfigurationService.GetWhatsAppSettingsAsync(ct);
        if (!settings.IsEnabled)
            return CommunicationSendResult.Skipped("WhatsApp sending is disabled in configuration.");

        if (string.IsNullOrWhiteSpace(settings.Provider) || string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ApiKey))
            return CommunicationSendResult.Skipped("WhatsApp provider is not configured.");

        return CommunicationSendResult.Skipped($"WhatsApp provider '{settings.Provider}' integration is pending.");
    }
}
