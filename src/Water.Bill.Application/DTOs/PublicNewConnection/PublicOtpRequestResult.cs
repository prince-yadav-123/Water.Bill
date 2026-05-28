namespace Water.Bill.Application.DTOs.PublicNewConnection;

public class PublicOtpRequestResult
{
    public string MobileNumber { get; set; } = null!;

    public string MaskedMobileNumber { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public int ResendAvailableInSeconds { get; set; }

    public string? DevelopmentOtp { get; set; }
}
