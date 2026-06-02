using Water.Bill.Application.DTOs.Communication;

namespace Water.Bill.Application.Interfaces;

public interface ICommunicationService
{
    Task SendAsync(
        string purposeKey,
        NotificationRecipient recipient,
        IReadOnlyDictionary<string, string?> values,
        NotificationChannelOptions channels,
        string? referenceType = null,
        string? referenceId = null,
        string? referenceNo = null,
        CancellationToken ct = default);
}

public interface ITemplateRenderer
{
    string Render(string template, IReadOnlyDictionary<string, string?> values);

    IReadOnlyList<string> ExtractPlaceholders(string template);
}

public interface IEmailSender
{
    Task<CommunicationSendResult> SendAsync(string toEmail, string? toName, string subject, string htmlBody, CancellationToken ct = default);
}

public interface ISmsSender
{
    Task<CommunicationSendResult> SendAsync(string mobileNo, string message, string? externalTemplateId, CancellationToken ct = default);
}

public interface IWhatsAppSender
{
    Task<CommunicationSendResult> SendAsync(string mobileNo, string message, string? externalTemplateId, CancellationToken ct = default);
}

public interface IInAppNotificationSender
{
    Task<CommunicationSendResult> SendAsync(NotificationRecipient recipient, string title, string message, string purposeKey, string? referenceType, string? referenceId, string? referenceNo, CancellationToken ct = default);
}

public sealed record CommunicationSendResult(string Status, string? ErrorMessage = null)
{
    public static CommunicationSendResult Sent() => new("Sent");

    public static CommunicationSendResult Skipped(string reason) => new("Skipped", reason);

    public static CommunicationSendResult Failed(string error) => new("Failed", error);
}

