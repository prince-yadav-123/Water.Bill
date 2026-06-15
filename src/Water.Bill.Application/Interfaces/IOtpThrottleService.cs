namespace Water.Bill.Application.Interfaces;

public interface IOtpThrottleService
{
    bool TryConsumeRequest(string purpose, string? subjectKey, out TimeSpan retryAfter);
}
