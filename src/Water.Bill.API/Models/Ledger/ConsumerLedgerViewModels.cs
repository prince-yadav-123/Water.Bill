using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Ledger;

public class ConsumerLedgerIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? EntryType { get; set; }

    public bool HasSearched { get; set; }

    public ConsumerLedgerConsumerViewModel? Consumer { get; set; }

    public IReadOnlyList<ConsumerLedgerRowViewModel> Rows { get; set; } = [];

    public IReadOnlyList<ConsumerLedgerConsumerSearchRowViewModel> Consumers { get; set; } = [];

    public List<SelectListItem> EntryTypeOptions { get; set; } = [];

    public decimal OpeningBalance { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal ClosingBalance { get; set; }
}

public class ConsumerLedgerConsumerViewModel
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

    public int? DevType { get; set; }

    public int? Status { get; set; }
}

public class ConsumerLedgerConsumerSearchRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? ConnectionType { get; set; }

    public int? DevType { get; set; }
}

public class ConsumerLedgerRowViewModel
{
    public DateTime Date { get; set; }

    public string EntryType { get; set; } = string.Empty;

    public string ReferenceNo { get; set; } = string.Empty;

    public string? LinkedReferenceNo { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public decimal Balance { get; set; }

    public bool AffectsBalance { get; set; }

    public int SortOrder { get; set; }
}

public static class ConsumerLedgerEntryTypes
{
    public const string Bill = "Bill";
    public const string Challan = "Challan";
    public const string Payment = "Payment";
    public const string Adjustment = "Adjustment";

    public static IEnumerable<SelectListItem> Options(string? selected = null)
    {
        yield return new SelectListItem("Bills", Bill, selected == Bill);
        yield return new SelectListItem("Challans", Challan, selected == Challan);
        yield return new SelectListItem("Payments", Payment, selected == Payment);
        yield return new SelectListItem("Adjustments", Adjustment, selected == Adjustment);
    }
}
