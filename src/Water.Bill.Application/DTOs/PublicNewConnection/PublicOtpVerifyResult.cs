namespace Water.Bill.Application.DTOs.PublicNewConnection;

public class PublicOtpVerifyResult
{
    public string MobileNumber { get; set; } = null!;

    public DateTime VerifiedAt { get; set; }
}
