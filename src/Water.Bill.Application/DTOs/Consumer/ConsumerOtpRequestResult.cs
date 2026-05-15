namespace Water.Bill.Application.DTOs.Consumer;

public class ConsumerOtpRequestResult
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string MaskedMobileNo { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int ResendAvailableInSeconds { get; set; }

    public string? DevelopmentOtp { get; set; }
}
