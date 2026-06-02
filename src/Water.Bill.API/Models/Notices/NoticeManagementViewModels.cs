using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Notices;

public static class NoticeStatuses
{
    public const string Draft = "Draft";
    public const string Issued = "Issued";
    public const string Cancelled = "Cancelled";

    public static string Display(string? status) => status switch
    {
        Draft => "Draft",
        Issued => "Issued",
        Cancelled => "Cancelled",
        _ => status ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(Draft), Draft, string.Equals(selected, Draft, StringComparison.OrdinalIgnoreCase)),
        new(Display(Issued), Issued, string.Equals(selected, Issued, StringComparison.OrdinalIgnoreCase)),
        new(Display(Cancelled), Cancelled, string.Equals(selected, Cancelled, StringComparison.OrdinalIgnoreCase))
    ];
}

public static class NoticeTypes
{
    public const string DueNotice = "DueNotice";
    public const string DisconnectionNotice = "DisconnectionNotice";
    public const string DemandNotice = "DemandNotice";
    public const string ReconnectionOrder = "ReconnectionOrder";
    public const string GeneralNotice = "GeneralNotice";

    public static string Display(string? type) => type switch
    {
        DueNotice => "Due Notice",
        DisconnectionNotice => "Disconnection Notice",
        DemandNotice => "Demand Notice",
        ReconnectionOrder => "Reconnection Order",
        GeneralNotice => "General Notice",
        _ => type ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(DueNotice), DueNotice, string.Equals(selected, DueNotice, StringComparison.OrdinalIgnoreCase)),
        new(Display(DisconnectionNotice), DisconnectionNotice, string.Equals(selected, DisconnectionNotice, StringComparison.OrdinalIgnoreCase)),
        new(Display(DemandNotice), DemandNotice, string.Equals(selected, DemandNotice, StringComparison.OrdinalIgnoreCase)),
        new(Display(ReconnectionOrder), ReconnectionOrder, string.Equals(selected, ReconnectionOrder, StringComparison.OrdinalIgnoreCase)),
        new(Display(GeneralNotice), GeneralNotice, string.Equals(selected, GeneralNotice, StringComparison.OrdinalIgnoreCase))
    ];
}

public class NoticeManagementIndexViewModel
{
    public string? Search { get; set; }
    public string? ConsumerNo { get; set; }
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? NoticeType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool HasConsumerSearch { get; set; }
    public IReadOnlyList<SelectListItem> TypeOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
    public IReadOnlyList<NoticeConsumerSearchRowViewModel> Consumers { get; set; } = [];
    public IReadOnlyList<NoticeListRowViewModel> Notices { get; set; } = [];
}

public class NoticeConsumerSearchRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? PropertyNo { get; set; }
    public string? ConnectionType { get; set; }
    public int? DevType { get; set; }
}

public class NoticeListRowViewModel
{
    public long Id { get; set; }
    public string NoticeNo { get; set; } = string.Empty;
    public string ConsumerNo { get; set; } = string.Empty;
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? PropertyNo { get; set; }
    public string NoticeType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime NoticeDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
}

public class NoticeCreateViewModel
{
    [Required]
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    public NoticeConsumerSummaryViewModel? Consumer { get; set; }

    [Display(Name = "Template")]
    public int? TemplateId { get; set; }

    [Required]
    [Display(Name = "Notice Type")]
    public string NoticeType { get; set; } = NoticeTypes.GeneralNotice;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Notice Date")]
    public DateTime NoticeDate { get; set; } = DateTime.Today;

    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Amount Due")]
    [Range(0, 999999999)]
    public decimal AmountDue { get; set; }

    [StringLength(30)]
    [Display(Name = "Related Bill No")]
    public string? RelatedBillNo { get; set; }

    [StringLength(30)]
    [Display(Name = "Related Challan No")]
    public string? RelatedChallanNo { get; set; }

    [Display(Name = "Related Disconnection Case")]
    public long? RelatedDisconnectionCaseId { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public IReadOnlyList<SelectListItem> TemplateOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> TypeOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> CaseOptions { get; set; } = [];
}

public class NoticeConsumerSummaryViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string? ConsumerName { get; set; }
    public string? FatherName { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? PropertyNo { get; set; }
    public string? Address { get; set; }
    public string? ConnectionType { get; set; }
    public string? Category { get; set; }
    public int? DevType { get; set; }
}

public class NoticeDetailsViewModel : NoticeListRowViewModel
{
    public NoticeConsumerSummaryViewModel? Consumer { get; set; }
    public int? TemplateId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? RelatedBillNo { get; set; }
    public string? RelatedChallanNo { get; set; }
    public long? RelatedDisconnectionCaseId { get; set; }
    public string? Remarks { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanIssue { get; set; }
    public bool CanCancel { get; set; }
    public IReadOnlyList<NoticeHistoryViewModel> Histories { get; set; } = [];
}

public class NoticeHistoryViewModel
{
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? ActionByName { get; set; }
    public DateTime ActionAt { get; set; }
}

public class NoticeTemplateIndexViewModel
{
    public IReadOnlyList<NoticeTemplateRowViewModel> Rows { get; set; } = [];
}

public class NoticeTemplateRowViewModel
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string NoticeType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
