namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerBillHistoryViewModel
{
    public string ActiveStatus { get; set; } = "All";
    public string? Search { get; set; }
    public string? SelectedConsumerNo { get; set; }
    public IReadOnlyList<ConsumerConnectionFilterViewModel> Connections { get; set; } = [];
    public IReadOnlyList<ConsumerBillHistoryItemViewModel> Bills { get; set; } = [];
}

public class ConsumerConnectionFilterViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class ConsumerBillHistoryItemViewModel
{
    public string BillNo { get; set; } = string.Empty;
    public string ConsumerNo { get; set; } = string.Empty;
    public string? BillPeriod { get; set; }
    public string? Consumption { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = "Due";
    public DateTime? PaidOn { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPaid => Status.Equals("Paid", StringComparison.OrdinalIgnoreCase);
}
