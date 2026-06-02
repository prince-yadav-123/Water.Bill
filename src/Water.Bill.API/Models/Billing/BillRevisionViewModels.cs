using System.ComponentModel.DataAnnotations;

namespace Water.Bill.API.Models.Billing;

public class BillRevisionIndexViewModel
{
    public string? Search { get; set; }
    public string? ConsumerNo { get; set; }
    public string? BillNo { get; set; }
    public IReadOnlyList<BillRevisionRowViewModel> Bills { get; set; } = [];
}

public class BillRevisionRowViewModel
{
    public string? BillNo { get; set; }
    public string? ConsumerNo { get; set; }
    public string? ConsumerName { get; set; }
    public string? PropertyNo { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? BillDateFrom { get; set; }
    public DateTime? BillDateTo { get; set; }
    public double? TotalAmount { get; set; }
    public double? PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Status { get; set; }
}

public class BillRevisionDetailsViewModel
{
    public BillRevisionRowViewModel Bill { get; set; } = new();
    public bool CanReverse { get; set; }
}

public class BillReverseRequestViewModel
{
    [Required]
    public string BillNo { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}
