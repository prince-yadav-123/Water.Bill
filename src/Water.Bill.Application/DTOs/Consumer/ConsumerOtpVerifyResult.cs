namespace Water.Bill.Application.DTOs.Consumer;

public class ConsumerOtpVerifyResult
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string ConsumerName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public int? ConsumerRoleId { get; set; }
}
