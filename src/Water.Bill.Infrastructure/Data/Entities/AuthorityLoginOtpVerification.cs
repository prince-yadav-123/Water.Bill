namespace Water.Bill.Infrastructure.Data.Entities;

public partial class AuthorityLoginOtpVerification
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string ChallengeToken { get; set; } = null!;
    public string Channels { get; set; } = null!;
    public string? DeliverySummary { get; set; }
    public string OtpHash { get; set; } = null!;
    public string OtpSalt { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public virtual Appuser User { get; set; } = null!;
}
