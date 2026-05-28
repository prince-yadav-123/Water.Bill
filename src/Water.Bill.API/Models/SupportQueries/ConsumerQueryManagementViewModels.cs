using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Models.SupportQueries;

public class ConsumerQueryManagementListViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];

    public IReadOnlyList<ConsumerSupportQuery> Queries { get; set; } = [];
}

public class ConsumerQueryAdminActionViewModel
{
    public long QueryId { get; set; }

    public string Status { get; set; } = "InProgress";

    public string? Remarks { get; set; }
}
