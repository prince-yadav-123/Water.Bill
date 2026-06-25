using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Water.Bill.API.Models.Adjustments;

public static class ConsumerAdjustmentTypes
{
    public const string Arrear = "Arrear";
    public const string Credit = "Credit";
    public const string Advance = "Advance";
    public const string Rebate = "Rebate";
    public const string Penalty = "Penalty";
    public const string Surcharge = "Surcharge";
    public const string OtherDebit = "OtherDebit";
    public const string OtherCredit = "OtherCredit";

    public static readonly IReadOnlySet<string> CreditTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Credit,
        Advance,
        Rebate,
        OtherCredit
    };

    public static string Display(string? type) => type switch
    {
        Arrear => "Arrear",
        Credit => "Credit",
        Advance => "Advance",
        Rebate => "Rebate",
        Penalty => "Penalty",
        Surcharge => "Surcharge",
        OtherDebit => "Other Debit",
        OtherCredit => "Other Credit",
        _ => type ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(Arrear), Arrear, string.Equals(selected, Arrear, StringComparison.OrdinalIgnoreCase)),
        new(Display(Credit), Credit, string.Equals(selected, Credit, StringComparison.OrdinalIgnoreCase)),
        new(Display(Advance), Advance, string.Equals(selected, Advance, StringComparison.OrdinalIgnoreCase)),
        new(Display(Rebate), Rebate, string.Equals(selected, Rebate, StringComparison.OrdinalIgnoreCase)),
        new(Display(Penalty), Penalty, string.Equals(selected, Penalty, StringComparison.OrdinalIgnoreCase)),
        new(Display(Surcharge), Surcharge, string.Equals(selected, Surcharge, StringComparison.OrdinalIgnoreCase)),
        new(Display(OtherDebit), OtherDebit, string.Equals(selected, OtherDebit, StringComparison.OrdinalIgnoreCase)),
        new(Display(OtherCredit), OtherCredit, string.Equals(selected, OtherCredit, StringComparison.OrdinalIgnoreCase))
    ];

    public static decimal SignedAmount(string type, decimal amount)
        => CreditTypes.Contains(type) ? -Math.Abs(amount) : Math.Abs(amount);
}

public static class ConsumerAdjustmentStatuses
{
    public const string Pending = "Pending";
    public const string Applied = "Applied";
    public const string Cancelled = "Cancelled";
    public const string Reversed = "Reversed";
}

public class ConsumerAccountAdjustmentIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? Status { get; set; }

    public string? AdjustmentType { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public IReadOnlyList<SelectListItem> TypeOptions { get; set; } = [];

    public IReadOnlyList<ConsumerAccountAdjustmentRowViewModel> Rows { get; set; } = [];
}

public class ConsumerAccountAdjustmentRowViewModel
{
    public long Id { get; set; }

    public string AdjustmentNo { get; set; } = string.Empty;

    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string AdjustmentType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal SignedAmount { get; set; }

    public DateTime EffectiveDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? AppliedBillNo { get; set; }

    public DateTime? AppliedOn { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class ConsumerAccountAdjustmentCreateViewModel
{
    [Required]
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    [Required(ErrorMessage = "Adjustment type is required.")]
    [Display(Name = "Adjustment Type")]
    public string AdjustmentType { get; set; } = ConsumerAdjustmentTypes.Arrear;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 99999999, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Effective date is required.")]
    [Display(Name = "Effective Date")]
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    [StringLength(30)]
    [Display(Name = "Source Bill No")]
    public string? SourceBillNo { get; set; }

    [StringLength(30)]
    [Display(Name = "Source Challan No")]
    public string? SourceChallanNo { get; set; }

    [Required(ErrorMessage = "Remarks are required for audit.")]
    [StringLength(500)]
    public string? Remarks { get; set; }

    public IReadOnlyList<SelectListItem> TypeOptions { get; set; } = [];
}

public class ConsumerAccountAdjustmentDetailsViewModel
{
    public long Id { get; set; }

    public string AdjustmentNo { get; set; } = string.Empty;

    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string AdjustmentType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal SignedAmount { get; set; }

    public DateTime EffectiveDate { get; set; }

    public string? SourceBillNo { get; set; }

    public string? SourceChallanNo { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? AppliedBillNo { get; set; }

    public DateTime? AppliedOn { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedByName { get; set; }

    public IReadOnlyList<ConsumerAccountAdjustmentHistoryViewModel> Histories { get; set; } = [];
}

public class ConsumerAccountAdjustmentHistoryViewModel
{
    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public string? ActionByName { get; set; }

    public DateTime ActionAt { get; set; }
}
