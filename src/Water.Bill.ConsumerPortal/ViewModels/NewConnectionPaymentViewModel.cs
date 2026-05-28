using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class NewConnectionPaymentViewModel
{
    public NewConnectionApplicationDetailsDto Application { get; set; } = null!;

    public NewConnectionFeeQuoteDto Fee { get; set; } = null!;

    public int Step { get; set; } = 1;

    public string PaymentMethod { get; set; } = "UPI";

    public string? PaymentIdentifier { get; set; }

    public bool IsPublicFlow { get; set; }

    public decimal ConvenienceFee { get; set; }

    public decimal TotalPayable => Fee.TotalAmount + ConvenienceFee;
}
