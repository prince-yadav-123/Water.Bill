using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Water.Bill.API.Models.Challans;

public static class ChallanPurposes
{
    public const string ExistingBillDue = "ExistingBillDue";
    public const string NdcNoDuesFee = "NdcNoDuesFee";
    public const string NewConnectionFee = "NewConnectionFee";
    public const string OtherServiceCharge = "OtherServiceCharge";

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new("Existing consumer bill / due", ExistingBillDue, selected == ExistingBillDue),
        new("NDC / No Dues fee", NdcNoDuesFee, selected == NdcNoDuesFee),
        new("New Connection fee", NewConnectionFee, selected == NewConnectionFee),
        new("Other service charge", OtherServiceCharge, selected == OtherServiceCharge)
    ];

    public static string Display(string? purpose) => purpose switch
    {
        ExistingBillDue => "Existing consumer bill / due",
        NdcNoDuesFee => "NDC / No Dues fee",
        NewConnectionFee => "New Connection fee",
        OtherServiceCharge => "Other service charge",
        _ => string.IsNullOrWhiteSpace(purpose) ? "Challan" : purpose
    };
}

public static class ChallanStatuses
{
    public const string PendingPayment = "PendingPayment";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
}

public static class ChallanPaymentModes
{
    public const string Cash = "Cash";
    public const string Cheque = "Cheque";
    public const string DemandDraft = "DemandDraft";
    public const string BankTransfer = "BankTransfer";
    public const string Upi = "UPI";
    public const string Card = "Card";

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new("Cash", Cash, selected == Cash),
        new("Cheque", Cheque, selected == Cheque),
        new("Demand Draft", DemandDraft, selected == DemandDraft),
        new("Bank Transfer", BankTransfer, selected == BankTransfer),
        new("UPI", Upi, selected == Upi),
        new("Card", Card, selected == Card)
    ];

    public static string Display(string? mode) => mode switch
    {
        DemandDraft => "Demand Draft",
        BankTransfer => "Bank Transfer",
        Upi => "UPI",
        null or "" => "-",
        _ => mode
    };
}

public class ChallanManagementIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool HasConsumerSearch { get; set; }

    public IReadOnlyList<ChallanConsumerSearchRowViewModel> Consumers { get; set; } = [];

    public IReadOnlyList<ChallanListRowViewModel> Challans { get; set; } = [];
}

public class ChallanConsumerSearchRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? ConnectionType { get; set; }

    public int? DevType { get; set; }
}

public class ChallanListRowViewModel
{
    public long Id { get; set; }

    public string? ChallanNo { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? Purpose { get; set; }

    public double Amount { get; set; }

    public string Status { get; set; } = ChallanStatuses.PendingPayment;

    public DateTime? GeneratedOn { get; set; }

    public int? DevType { get; set; }
}

public class ChallanCreateViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public ChallanConsumerSummaryViewModel? Consumer { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public bool HasConsumerSearch { get; set; }

    public IReadOnlyList<ChallanConsumerSearchRowViewModel> Consumers { get; set; } = [];

    [Required(ErrorMessage = "Please select challan purpose.")]
    [Display(Name = "Challan Purpose")]
    public string Purpose { get; set; } = ChallanPurposes.ExistingBillDue;

    [Required(ErrorMessage = "Please enter challan amount.")]
    [Range(1, 99999999, ErrorMessage = "Amount must be greater than zero.")]
    [Display(Name = "Amount")]
    public double? Amount { get; set; }

    [Display(Name = "Bill Period From")]
    public DateTime? BillPeriodFrom { get; set; }

    [Display(Name = "Bill Period To")]
    public DateTime? BillPeriodTo { get; set; }

    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Bank / Payment Counter")]
    public string? BankCode { get; set; }

    public string? Remarks { get; set; }

    public List<SelectListItem> PurposeOptions { get; set; } = [];

    public List<SelectListItem> BankOptions { get; set; } = [];

    [Display(Name = "Source Bill")]
    public string? BillNo { get; set; }

    public List<SelectListItem> BillOptions { get; set; } = [];

    public string? SuggestedAmountNote { get; set; }
}

public class ChallanConsumerSummaryViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? FatherName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? PropertyNo { get; set; }

    public string? Address { get; set; }

    public string? ConnectionType { get; set; }

    public string? Category { get; set; }

    public string? FlatType { get; set; }

    public int? PipeSize { get; set; }

    public int? PlotSize { get; set; }

    public int? DevType { get; set; }
}

public class ChallanDetailsViewModel
{
    public long Id { get; set; }

    public string? ChallanNo { get; set; }

    public string? ReceiptNo { get; set; }

    public string? Purpose { get; set; }

    public string? SourceBillNo { get; set; }

    public ChallanConsumerSummaryViewModel Consumer { get; set; } = new();

    public DateTime? BillPeriodFrom { get; set; }

    public DateTime? BillPeriodTo { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? GeneratedOn { get; set; }

    public double BillAmount { get; set; }

    public double Surcharge { get; set; }

    public double Arrear { get; set; }

    public double NocAmount { get; set; }

    public double ConnectionCharge { get; set; }

    public double PenaltyCharge { get; set; }

    public double TotalAmount { get; set; }

    public string? BankCode { get; set; }

    public string? BankName { get; set; }

    public string Status { get; set; } = ChallanStatuses.PendingPayment;

    public string? GeneratedBy { get; set; }

    public string? Remarks { get; set; }

    public bool CanCancel { get; set; }

    public bool CanPostPayment { get; set; }

    public ChallanPaymentPostViewModel PaymentPost { get; set; } = new();

    public IReadOnlyList<ChallanPaymentHistoryRowViewModel> Payments { get; set; } = [];

    public IReadOnlyList<ChallanHistoryRowViewModel> Histories { get; set; } = [];
}

public class ChallanPaymentPostViewModel
{
    public long ChallanId { get; set; }

    [Required(ErrorMessage = "Payment date is required.")]
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Paid amount is required.")]
    [Range(1, 99999999, ErrorMessage = "Paid amount must be greater than zero.")]
    [Display(Name = "Paid Amount")]
    public double? Amount { get; set; }

    [Required(ErrorMessage = "Payment mode is required.")]
    [Display(Name = "Payment Mode")]
    public string PaymentMode { get; set; } = ChallanPaymentModes.Cash;

    [Display(Name = "Bank / Payment Counter")]
    public string? BankCode { get; set; }

    [Display(Name = "Transaction / Reference No")]
    [StringLength(100)]
    public string? TransactionReferenceNo { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public List<SelectListItem> PaymentModeOptions { get; set; } = [];

    public List<SelectListItem> BankOptions { get; set; } = [];
}

public class ChallanPaymentHistoryIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ChallanNo { get; set; }

    public string? PaymentMode { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public IReadOnlyList<ChallanPaymentHistoryRowViewModel> Payments { get; set; } = [];

    public List<SelectListItem> PaymentModeOptions { get; set; } = [];
}

public class ChallanPaymentHistoryRowViewModel
{
    public long Id { get; set; }

    public long ChallanId { get; set; }

    public string? ChallanNo { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? SourceBillNo { get; set; }

    public double Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? PaymentMode { get; set; }

    public string? BankName { get; set; }

    public string? TransactionReferenceNo { get; set; }

    public string? PostedByName { get; set; }

    public DateTime PostedOn { get; set; }
}

public class ChallanHistoryRowViewModel
{
    public string Action { get; set; } = string.Empty;

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public string? Remarks { get; set; }

    public string? ActionByName { get; set; }

    public DateTime ActionOn { get; set; }
}
