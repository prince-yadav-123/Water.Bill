namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionPaymentRequestDto
{
    public NewConnectionFeeQuoteDto? FeeQuote { get; set; }

    public int? ActionBy { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentIdentifier { get; set; }

    public bool StartWorkflow { get; set; } = true;
}
