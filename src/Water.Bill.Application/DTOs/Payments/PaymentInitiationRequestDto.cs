namespace Water.Bill.Application.DTOs.Payments;

public class PaymentInitiationRequestDto
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string? ConsumerProperty { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string BillNo { get; set; } = string.Empty;
    public string? ChallanNo { get; set; }
    public DateTime? BillDateFrom { get; set; }
    public DateTime? BillDateTo { get; set; }
    public DateTime? DueDate { get; set; }
    public double Amount { get; set; }
    public string GatewayCode { get; set; } = "AX";
    public string BillOrNdc { get; set; } = "Bill";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
