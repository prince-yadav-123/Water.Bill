namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerChallanIndexViewModel
{
    public string ActiveStatus { get; set; } = "Pending";

    public string? Search { get; set; }

    public IReadOnlyList<ConsumerChallanListItemViewModel> Challans { get; set; } = [];
}

public class ConsumerChallanListItemViewModel
{
    public long Id { get; set; }

    public string ChallanNo { get; set; } = string.Empty;

    public string ConsumerNo { get; set; } = string.Empty;

    public string Purpose { get; set; } = "Challan";

    public double Amount { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public string Status { get; set; } = "PendingPayment";

    public bool CanPay => Status.Equals("PendingPayment", StringComparison.OrdinalIgnoreCase);
}

public class ConsumerChallanDetailsViewModel
{
    public long Id { get; set; }

    public string ChallanNo { get; set; } = string.Empty;

    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? Address { get; set; }

    public string Purpose { get; set; } = "Challan";

    public string? SourceBillNo { get; set; }

    public double Amount { get; set; }

    public double BillAmount { get; set; }

    public double Surcharge { get; set; }

    public double NdcAmount { get; set; }

    public double ConnectionCharge { get; set; }

    public double OtherCharge { get; set; }

    public DateTime? BillPeriodFrom { get; set; }

    public DateTime? BillPeriodTo { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public string Status { get; set; } = "PendingPayment";

    public string? BankCode { get; set; }

    public string? BankName { get; set; }

    public string? PaymentMode { get; set; }

    public string? TransactionReferenceNo { get; set; }

    public IReadOnlyList<ConsumerChallanPaymentHistoryItemViewModel> Payments { get; set; } = [];

    public bool CanPay => Status.Equals("PendingPayment", StringComparison.OrdinalIgnoreCase);
}

public class ConsumerChallanPaymentViewModel : ConsumerChallanDetailsViewModel
{
    public int Step { get; set; } = 1;

    public string PaymentMethod { get; set; } = "UPI";

    public string? PaymentIdentifier { get; set; }

    public double ConvenienceFee { get; set; }

    public double FinalPayableAmount => Amount + ConvenienceFee;
}

public class ConsumerChallanPaymentHistoryItemViewModel
{
    public DateTime PaymentDate { get; set; }

    public double Amount { get; set; }

    public string? PaymentMode { get; set; }

    public string? BankName { get; set; }

    public string? TransactionReferenceNo { get; set; }
}
