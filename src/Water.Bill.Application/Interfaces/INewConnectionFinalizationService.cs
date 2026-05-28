namespace Water.Bill.Application.Interfaces;

public interface INewConnectionFinalizationService
{
    Task<string> CreateFinalConsumerAsync(
        long applicationId,
        int? actionByUserId,
        string? actionByName,
        string? actionByRole,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct = default);
}
