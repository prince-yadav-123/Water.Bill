using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Audit;

public enum ActivityLogAudience
{
    Authority = 1,
    Consumer = 2
}

public class ActivityLogIndexViewModel
{
    public string? Search { get; set; }
    public int? Action { get; set; }
    public string? Module { get; set; }
    public bool? Success { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public ActivityLogAudience Audience { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActiveMenu { get; set; } = string.Empty;
    public string DetailsRouteName { get; set; } = string.Empty;
    public string LegacyRouteName { get; set; } = string.Empty;
    public string ExportRouteName { get; set; } = string.Empty;
    public IReadOnlyList<SelectListItem> ActionOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> ModuleOptions { get; set; } = [];
    public IReadOnlyList<ActivityLogRowViewModel> Rows { get; set; } = [];
}

public class ActivityLogRowViewModel
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public int Action { get; set; }
    public string ActionLabel { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string ModuleLabel { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
    public bool Success { get; set; }
    public string PortalType { get; set; } = string.Empty;
}

public class ActivityLogDetailsViewModel
{
    public int Id { get; set; }
    public ActivityLogAudience Audience { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ActiveMenu { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackRouteName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public int Action { get; set; }
    public string ActionLabel { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string ModuleLabel { get; set; } = string.Empty;
    public string EntityLabel { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public bool Success { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; }
}
