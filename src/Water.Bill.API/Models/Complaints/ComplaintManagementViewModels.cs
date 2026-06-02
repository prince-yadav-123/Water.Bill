using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Models.Complaints;

public class ComplaintManagementListViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];

    public IReadOnlyList<ConsumerComplaint> Complaints { get; set; } = [];
}

public class ComplaintAdminActionViewModel
{
    public long ComplaintId { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }
}
