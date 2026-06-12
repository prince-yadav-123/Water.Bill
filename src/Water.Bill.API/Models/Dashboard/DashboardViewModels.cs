namespace Water.Bill.API.Models.Dashboard;

public class DashboardIndexViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsAdminView { get; set; }
    public IReadOnlyList<DashboardStatCardViewModel> SummaryCards { get; set; } = Array.Empty<DashboardStatCardViewModel>();
    public DashboardDonutChartViewModel? PrimaryStatusChart { get; set; }
    public DashboardDonutChartViewModel? ChallanStatusChart { get; set; }
    public DashboardDistributionChartViewModel? DivisionConsumerChart { get; set; }
    public DashboardAmountDistributionChartViewModel? PaymentCollectionChart { get; set; }
    public DashboardDefaulterWidgetViewModel? DefaulterWidget { get; set; }
    public DashboardBarChartViewModel? SecondaryBarChart { get; set; }
    public DashboardTrendChartViewModel? TrendChart { get; set; }
    public DashboardStatusPanelViewModel? ServiceDeskPanel { get; set; }
    public IReadOnlyList<DashboardRecentApplicationViewModel> RecentApplications { get; set; } = Array.Empty<DashboardRecentApplicationViewModel>();
    public IReadOnlyList<DashboardPendingApprovalViewModel> PendingApprovals { get; set; } = Array.Empty<DashboardPendingApprovalViewModel>();
    public IReadOnlyList<DashboardRecentChallanViewModel> RecentChallans { get; set; } = Array.Empty<DashboardRecentChallanViewModel>();
    public IReadOnlyList<DashboardServiceDeskItemViewModel> RecentServiceDeskItems { get; set; } = Array.Empty<DashboardServiceDeskItemViewModel>();
    public IReadOnlyList<DashboardActivityItemViewModel> RecentActivities { get; set; } = Array.Empty<DashboardActivityItemViewModel>();
    public IReadOnlyList<DashboardQuickLinkViewModel> QuickLinks { get; set; } = Array.Empty<DashboardQuickLinkViewModel>();
}

public class DashboardStatCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string? SubValue { get; set; }
    public string Tone { get; set; } = "primary";
    public string? Url { get; set; }
    public string? Icon { get; set; }
}

public class DashboardDonutChartViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public IReadOnlyList<DashboardChartSliceViewModel> Items { get; set; } = Array.Empty<DashboardChartSliceViewModel>();
}

public class DashboardChartSliceViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Color { get; set; } = "#2563eb";
}

public class DashboardDistributionChartViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public IReadOnlyList<DashboardDistributionSliceViewModel> Items { get; set; } = Array.Empty<DashboardDistributionSliceViewModel>();
}

public class DashboardDistributionSliceViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = "#2563eb";
}

public class DashboardAmountDistributionChartViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public IReadOnlyList<DashboardAmountDistributionSliceViewModel> Items { get; set; } = Array.Empty<DashboardAmountDistributionSliceViewModel>();
}

public class DashboardAmountDistributionSliceViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = "#2563eb";
}

public class DashboardDefaulterWidgetViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public int TotalConsumers { get; set; }
    public int ConsumerCount { get; set; }
    public int NonDefaulterCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal ThresholdAmount { get; set; }
    public decimal ConfiguredThresholdAmount { get; set; }
    public string? Url { get; set; }
}

public class DashboardBarChartViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public IReadOnlyList<DashboardBarItemViewModel> Items { get; set; } = Array.Empty<DashboardBarItemViewModel>();
}

public class DashboardBarItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "#2563eb";
    public string? Url { get; set; }
}

public class DashboardTrendChartViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string PrimaryLabel { get; set; } = string.Empty;
    public string SecondaryLabel { get; set; } = string.Empty;
    public IReadOnlyList<DashboardTrendPointViewModel> Points { get; set; } = Array.Empty<DashboardTrendPointViewModel>();
}

public class DashboardTrendPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public decimal SecondaryValue { get; set; }
}

public class DashboardStatusPanelViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public IReadOnlyList<DashboardStatusGroupViewModel> Groups { get; set; } = Array.Empty<DashboardStatusGroupViewModel>();
}

public class DashboardStatusGroupViewModel
{
    public string Label { get; set; } = string.Empty;
    public IReadOnlyList<DashboardStatusItemViewModel> Items { get; set; } = Array.Empty<DashboardStatusItemViewModel>();
}

public class DashboardStatusItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Tone { get; set; } = "secondary";
    public string? Url { get; set; }
}

public class DashboardRecentApplicationViewModel
{
    public string Type { get; set; } = string.Empty;
    public string ApplicationNo { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Property { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? Url { get; set; }
}

public class DashboardPendingApprovalViewModel
{
    public long TaskId { get; set; }
    public string ApplicationType { get; set; } = string.Empty;
    public string ApplicationNo { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedOn { get; set; }
    public int DaysSinceAssigned { get; set; }
    public string? AssignedTo { get; set; }
    public string? Url { get; set; }
}

public class DashboardRecentChallanViewModel
{
    public long ChallanId { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public string ConsumerNo { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? GeneratedOn { get; set; }
    public string? Url { get; set; }
}

public class DashboardServiceDeskItemViewModel
{
    public string Type { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? Url { get; set; }
}

public class DashboardActivityItemViewModel
{
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Username { get; set; }
    public bool? Success { get; set; }
    public DateTime OccurredOn { get; set; }
    public string? Details { get; set; }
}

public class DashboardQuickLinkViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}
