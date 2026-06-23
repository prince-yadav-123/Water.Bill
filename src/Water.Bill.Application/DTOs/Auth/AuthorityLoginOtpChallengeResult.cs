namespace Water.Bill.Application.DTOs.Auth;

public class AuthorityLoginOtpChallengeResult
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string DeliverySummary { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int ResendAvailableInSeconds { get; set; }
}
