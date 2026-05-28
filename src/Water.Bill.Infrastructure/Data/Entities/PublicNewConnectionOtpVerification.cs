namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PublicNewConnectionOtpVerification
{
    public long Id { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string OtpHash { get; set; } = null!;

    public string OtpSalt { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int AttemptCount { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
}
