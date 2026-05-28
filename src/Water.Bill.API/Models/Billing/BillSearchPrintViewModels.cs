using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Billing;

public class BillSearchPrintIndexViewModel
{
    public string? ConsumerNo { get; set; }

    public string? BillNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public int? DevType { get; set; }

    public List<SelectListItem> DivisionOptions { get; set; } = [];

    public IReadOnlyList<BillSearchPrintConsumerViewModel> Consumers { get; set; } = [];

    public bool HasSearched { get; set; }
}

public class BillSearchPrintConsumerViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public DateTime? ConnectionDate { get; set; }

    public int? DevType { get; set; }

    public IReadOnlyList<BillSearchPrintBillViewModel> Bills { get; set; } = [];
}

public class BillSearchPrintBillViewModel
{
    public string BillNo { get; set; } = string.Empty;

    public DateTime? BillDateFrom { get; set; }

    public DateTime? BillDateTo { get; set; }

    public DateTime? BillDate { get; set; }

    public DateTime? DueDate { get; set; }

    public double? BillAmount { get; set; }

    public double? TotalBillAmount { get; set; }

    public string? ChallanNo { get; set; }

    public bool HasPrintableContent { get; set; }
}

public class BillPrintViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string BillNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? PropertyNo { get; set; }

    public string? Address { get; set; }

    public string? ConnectionType { get; set; }

    public string? Category { get; set; }

    public string? FlatType { get; set; }

    public int? PipeSize { get; set; }

    public int? DevType { get; set; }

    public DateTime? BillDate { get; set; }

    public DateTime? BillDueDate { get; set; }

    public DateTime? BillDateFrom { get; set; }

    public DateTime? BillDateTo { get; set; }

    public double? MinimumRate { get; set; }

    public double? MinimumTotalAmount { get; set; }

    public double? CessAmount { get; set; }

    public double? ArrearAmount { get; set; }

    public double? ArrearInterest { get; set; }

    public double? RebateAmount { get; set; }

    public double? AdvanceAmount { get; set; }

    public double? LastPaidAmount { get; set; }

    public double? TotalBillAmount { get; set; }

    public string? ChallanNo { get; set; }

    public string? BankCode { get; set; }

    public string? ChallanContent { get; set; }
}
