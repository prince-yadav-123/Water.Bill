namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerDashboardViewModel
{
    public string? ConsumerNo { get; set; }
    public ConsumerSummaryViewModel? Consumer { get; set; }
    public CurrentBillSummaryViewModel? CurrentBill { get; set; }
    public DashboardMetricViewModel Metrics { get; set; } = new();
    public IReadOnlyList<RecentPaymentViewModel> RecentPayments { get; set; } = [];
    public IReadOnlyList<RecentBillViewModel> RecentBills { get; set; } = [];
    public IReadOnlyList<DashboardAlertViewModel> Alerts { get; set; } = [];
}

public class ConsumerSummaryViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Sector { get; set; }
    public string? Block { get; set; }
    public string? FlatNo { get; set; }
    public string? PlotNo { get; set; }
    public string? Address { get; set; }
    public string? Category { get; set; }
    public string? ConnectionType { get; set; }
    public string? PropertySummary { get; set; }
}

public class CurrentBillSummaryViewModel
{
    public string? BillNo { get; set; }
    public string? ChallanNo { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? BillDateFrom { get; set; }
    public DateTime? BillDateTo { get; set; }
    public DateTime? DueDate { get; set; }
    public double CurrentAmount { get; set; }
    public double ArrearAmount { get; set; }
    public double TotalPayableAmount { get; set; }
    public double LastPaidAmount { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public string? BillPeriod { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsDueSoon { get; set; }
}

public class RecentPaymentViewModel
{
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public double Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? BankName { get; set; }
}

public class RecentBillViewModel
{
    public string BillNo { get; set; } = string.Empty;
    public string? BillPeriod { get; set; }
    public DateTime? DueDate { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardMetricViewModel
{
    public double TotalDue { get; set; }
    public double LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string ServiceStatus { get; set; } = "Active";
    public int RecentBillCount { get; set; }
}

public class DashboardAlertViewModel
{
    public string Type { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
