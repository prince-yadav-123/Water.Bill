namespace Water.Bill.Application.DTOs.Payments;

public class PaymentActorContextDto
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
