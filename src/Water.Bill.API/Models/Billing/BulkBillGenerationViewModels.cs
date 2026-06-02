using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Billing;

public class BulkBillGenerationViewModel
{
    public DateTime BillDate { get; set; } = DateTime.Today;

    public DateTime BillDueDate { get; set; } = DateTime.Today.AddDays(15);

    public DateTime BillDateFrom { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public DateTime BillDateTo { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

    public int? DevType { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? ConsumerNo { get; set; }

    public string BillType { get; set; } = "B";

    public bool IncludeArrears { get; set; } = true;

    public bool IncludeCess { get; set; } = true;

    public bool IncludeGst { get; set; }

    public double GstPercent { get; set; }

    public int RebatePercent { get; set; }

    public string RebateMode { get; set; } = "Manual";

    public string? SchemeId { get; set; }

    public int? BillPercentage { get; set; }

    public double ArrearInterestPercent { get; set; } = 15;

    public double AfterDueSurchargePercent { get; set; }

    public bool UseLegacyRateDate { get; set; }

    public DateTime LegacyRateDate { get; set; } = new(2013, 7, 1);

    public bool ApplyCreditRebateGuard { get; set; }

    public int CreditRebateThresholdMonths { get; set; } = 2;

    public int MaxConsumers { get; set; } = 100;

    public bool HasPreview { get; set; }

    public IReadOnlyList<SelectListItem> DivisionOptions { get; set; } = [];

    public IReadOnlyList<BulkBillPreviewRowViewModel> PreviewRows { get; set; } = [];

    public BulkBillGenerationResultViewModel? Result { get; set; }
}

public class BulkBillPreviewRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? PropertyNo { get; set; }

    public int? DevType { get; set; }

    public string? ConnectionType { get; set; }

    public string? Category { get; set; }

    public int? PipeSize { get; set; }

    public int? PlotSize { get; set; }

    public double MonthlyRate { get; set; }

    public double MinimumTotalAmount { get; set; }

    public double CessAmount { get; set; }

    public double ArrearAmount { get; set; }

    public double ArrearInterest { get; set; }

    public double DebitAdjustmentAmount { get; set; }

    public double CreditAdjustmentAmount { get; set; }

    public double RebateAmount { get; set; }

    public int AppliedRebatePercent { get; set; }

    public double GstAmount { get; set; }

    public double TotalBillAmount { get; set; }

    public double AfterDueAmount { get; set; }

    public int? BillPercentage { get; set; }

    public string? SchemeId { get; set; }

    public int OldRateFlag { get; set; }

    public bool AlreadyGenerated { get; set; }

    public string? RateSource { get; set; }

    public string? Warning { get; set; }
}

public class BulkBillGenerationResultViewModel
{
    public int RequestedCount { get; set; }

    public int GeneratedCount { get; set; }

    public int SkippedCount { get; set; }

    public IReadOnlyList<string> GeneratedBillNos { get; set; } = [];
}
