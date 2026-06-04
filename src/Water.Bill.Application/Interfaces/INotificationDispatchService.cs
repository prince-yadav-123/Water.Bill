namespace Water.Bill.Application.Interfaces;

public interface INotificationDispatchService
{
    /// <summary>Send a saved notification to all resolved target users.</summary>
    Task<NotificationDispatchResult> SendAsync(long notificationId, CancellationToken ct = default);

    /// <summary>Preview how many users will receive the notification without actually sending.</summary>
    Task<int> PreviewTargetCountAsync(string targetAudience, IReadOnlyList<NotificationTargetInput> targets, CancellationToken ct = default);
}

public sealed record NotificationTargetInput(string TargetType, string? TargetId, string? TargetName);

public sealed record NotificationDispatchResult(
    bool Success,
    int TotalTargeted,
    int InAppSent,
    int EmailSent,
    int EmailFailed,
    string? ErrorMessage = null)
{
    public static NotificationDispatchResult Ok(int total, int inApp, int emailSent, int emailFailed)
        => new(true, total, inApp, emailSent, emailFailed);

    public static NotificationDispatchResult Failed(string error)
        => new(false, 0, 0, 0, 0, error);
}
