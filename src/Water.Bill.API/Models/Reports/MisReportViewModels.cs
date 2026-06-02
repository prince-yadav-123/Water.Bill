namespace Water.Bill.API.Models.Reports;

public class MisReportIndexViewModel
{
    public string ReportType { get; set; } = "Collection";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ConsumerNo { get; set; }
    public int? DevType { get; set; }
    public string? Status { get; set; }
    public MisReportSummaryViewModel Summary { get; set; } = new();
    public IReadOnlyList<MisReportRowViewModel> Rows { get; set; } = [];
}

public class MisReportSummaryViewModel
{
    public int TotalCount { get; set; }
    public double TotalAmount { get; set; }
    public double PaidAmount { get; set; }
    public double PendingAmount { get; set; }
}

public class MisReportRowViewModel
{
    public string? ReferenceNo { get; set; }
    public string? ConsumerNo { get; set; }
    public string? ConsumerName { get; set; }
    public string? PropertyNo { get; set; }
    public string? Division { get; set; }
    public string? Status { get; set; }
    public DateTime? Date { get; set; }
    public double Amount { get; set; }
    public double PaidAmount { get; set; }
}
