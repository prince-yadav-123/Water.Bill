namespace Water.Bill.API.Models.Payments;

public class OnlinePaymentHistoryIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? JalRefId { get; set; }

    public string? TransactionNo { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool HasSearched { get; set; }

    public IReadOnlyList<OnlinePaymentHistoryRowViewModel> Items { get; set; } = [];
}

public class OnlinePaymentHistoryRowViewModel
{
    public string JalRefId { get; set; } = string.Empty;

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? PropertyNo { get; set; }

    public string? BillNo { get; set; }

    public string? ChallanNo { get; set; }

    public double? Amount { get; set; }

    public string? Bank { get; set; }

    public string? PaymentStatus { get; set; }

    public string? TransactionStatus { get; set; }

    public string? TransactionNo { get; set; }

    public DateTime? EntryDate { get; set; }
}

public class OnlinePaymentHistoryDetailsViewModel
{
    public string JalRefId { get; set; } = string.Empty;

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? PropertyNo { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public double? Amount { get; set; }

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    public string? DepositBank { get; set; }

    public string? PaymentStatus { get; set; }

    public string? BillNo { get; set; }

    public string? ChallanNo { get; set; }

    public string? DueDate { get; set; }

    public string? BillNdc { get; set; }

    public DateTime? EntryDate { get; set; }

    public IReadOnlyList<OnlinePaymentTransactionViewModel> Transactions { get; set; } = [];
}

public class OnlinePaymentTransactionViewModel
{
    public string? MerchantId { get; set; }

    public string? TransactionReferenceNo { get; set; }

    public string? BankReferenceNo { get; set; }

    public string? TransactionAmount { get; set; }

    public string? BankId { get; set; }

    public string? TransactionDate { get; set; }

    public string? AuthStatus { get; set; }

    public string? ErrorStatus { get; set; }

    public string? ErrorDescription { get; set; }

    public DateTime? EntryDate { get; set; }
}
