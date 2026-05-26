namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerOtpVerification
{
    public long Id { get; set; }

    public string ConsumerNo { get; set; } = null!;

    public string MobileNo { get; set; } = null!;

    public string OtpHash { get; set; } = null!;

    public string OtpSalt { get; set; } = null!;

    public string Purpose { get; set; } = "ConsumerLogin";

    public DateTime ExpiresAt { get; set; }

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual ConsumerDetailsMaster Consumer { get; set; } = null!;
}
