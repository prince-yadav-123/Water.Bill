using System.ComponentModel.DataAnnotations;

namespace Water.Bill.API.Models.NotificationManagement;

// ── List ────────────────────────────────────────────────────────────────────

public class NotificationListViewModel
{
    public IReadOnlyList<NotificationListRowViewModel> Items { get; set; } = [];
    public NotificationListFilterViewModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class NotificationListRowViewModel
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Channels { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }
}

public class NotificationListFilterViewModel
{
    public string? Search { get; set; }
    public string? TargetAudience { get; set; }
    public string? Channel { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

// ── Create ───────────────────────────────────────────────────────────────────

public class NotificationCreateViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string NotificationType { get; set; } = "General";

    [Required]
    public string TargetAudience { get; set; } = "Consumer";

    [Required]
    public List<string> Channels { get; set; } = new() { "InApp" };

    [Required]
    public string Priority { get; set; } = "Normal";

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    [MaxLength(1000)]
    public string? RedirectUrl { get; set; }

    public string Action { get; set; } = "Draft";  // Draft | Send

    // ── Targeting ──

    // Consumer targets
    public bool AllConsumers { get; set; }
    public string? ConsumerNo { get; set; }            // comma-separated
    public string? ConsumerUserIds { get; set; }       // comma-separated int ids

    // Internal targets
    public bool AllInternalUsers { get; set; }
    public List<int> SelectedUserIds { get; set; } = [];
    public List<int> SelectedRoleIds { get; set; } = [];
    public List<int> SelectedDepartmentIds { get; set; } = [];

    // Lookup data for UI
    public IReadOnlyList<SelectOption> RoleOptions { get; set; } = [];
    public IReadOnlyList<SelectOption> DepartmentOptions { get; set; } = [];
    public IReadOnlyList<SelectOption> UserOptions { get; set; } = [];

    public int? PreviewCount { get; set; }
}

// ── Details ─────────────────────────────────────────────────────────────────

public class NotificationDetailsViewModel
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Channels { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? RedirectUrl { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }

    public IReadOnlyList<NotificationTargetRowViewModel> Targets { get; set; } = [];

    // Delivery stats
    public int InAppTotalSent { get; set; }
    public int InAppUnread { get; set; }
    public int InAppRead { get; set; }
}

public class NotificationTargetRowViewModel
{
    public string TargetType { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? TargetName { get; set; }
}

// ── Shared ───────────────────────────────────────────────────────────────────

public class SelectOption
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}
