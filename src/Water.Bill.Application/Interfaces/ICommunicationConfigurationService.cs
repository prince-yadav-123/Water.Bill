using Water.Bill.Application.DTOs.Communication;

namespace Water.Bill.Application.Interfaces;

public interface ICommunicationConfigurationService
{
    Task<IReadOnlyList<CommunicationChannelSettingsDto>> GetAllAsync(CancellationToken ct = default);

    Task<CommunicationChannelSettingsDto> GetAsync(string channelName, CancellationToken ct = default);

    Task<CommunicationChannelSettingsDto> SaveAsync(
        CommunicationChannelSettingsDto model,
        int? updatedByUserId = null,
        string? updatedByName = null,
        CancellationToken ct = default);

    Task<EmailCommunicationSettings> GetEmailSettingsAsync(CancellationToken ct = default);

    Task<SmsCommunicationSettings> GetSmsSettingsAsync(CancellationToken ct = default);

    Task<WhatsAppCommunicationSettings> GetWhatsAppSettingsAsync(CancellationToken ct = default);
}
