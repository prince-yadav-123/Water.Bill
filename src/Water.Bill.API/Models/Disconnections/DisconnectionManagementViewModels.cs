using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.Disconnections;

public static class DisconnectionCaseStatuses
{
    public const string NoticeGenerated = "NoticeGenerated";
    public const string Disconnected = "Disconnected";
    public const string ReconnectionRequested = "ReconnectionRequested";
    public const string Reconnected = "Reconnected";
    public const string Cancelled = "Cancelled";

    public static string Display(string? status) => status switch
    {
        NoticeGenerated => "Notice Generated",
        Disconnected => "Disconnected",
        ReconnectionRequested => "Reconnection Requested",
        Reconnected => "Reconnected",
        Cancelled => "Cancelled",
        _ => status ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(NoticeGenerated), NoticeGenerated, string.Equals(selected, NoticeGenerated, StringComparison.OrdinalIgnoreCase)),
        new(Display(Disconnected), Disconnected, string.Equals(selected, Disconnected, StringComparison.OrdinalIgnoreCase)),
        new(Display(ReconnectionRequested), ReconnectionRequested, string.Equals(selected, ReconnectionRequested, StringComparison.OrdinalIgnoreCase)),
        new(Display(Reconnected), Reconnected, string.Equals(selected, Reconnected, StringComparison.OrdinalIgnoreCase)),
        new(Display(Cancelled), Cancelled, string.Equals(selected, Cancelled, StringComparison.OrdinalIgnoreCase))
    ];
}

public static class DisconnectionReasons
{
    public const string NonPayment = "NonPayment";
    public const string IllegalConnection = "IllegalConnection";
    public const string ConsumerRequest = "ConsumerRequest";
    public const string TemporaryClosure = "TemporaryClosure";
    public const string Other = "Other";

    public static string Display(string? reason) => reason switch
    {
        NonPayment => "Non-payment / dues",
        IllegalConnection => "Illegal connection",
        ConsumerRequest => "Consumer request",
        TemporaryClosure => "Temporary closure",
        Other => "Other",
        _ => reason ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(NonPayment), NonPayment, string.Equals(selected, NonPayment, StringComparison.OrdinalIgnoreCase)),
        new(Display(IllegalConnection), IllegalConnection, string.Equals(selected, IllegalConnection, StringComparison.OrdinalIgnoreCase)),
        new(Display(ConsumerRequest), ConsumerRequest, string.Equals(selected, ConsumerRequest, StringComparison.OrdinalIgnoreCase)),
        new(Display(TemporaryClosure), TemporaryClosure, string.Equals(selected, TemporaryClosure, StringComparison.OrdinalIgnoreCase)),
        new(Display(Other), Other, string.Equals(selected, Other, StringComparison.OrdinalIgnoreCase))
    ];
}

public class DisconnectionManagementIndexViewModel
{
    public string? Search { get; set; }
    public string? ConsumerNo { get; set; }
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? Sector { get; set; }
    public string? Block { get; set; }
    public string? PlotNo { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool HasConsumerSearch { get; set; }
    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
    public IReadOnlyList<DisconnectionConsumerSearchRowViewModel> Consumers { get; set; } = [];
    public IReadOnlyList<DisconnectionCaseRowViewModel> Cases { get; set; } = [];
}

public class DisconnectionConsumerSearchRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? PropertyNo { get; set; }
    public string? ConnectionType { get; set; }
    public int? DevType { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string? ActiveCaseNo { get; set; }
}

public class DisconnectionCaseRowViewModel
{
    public long Id { get; set; }
    public string CaseNo { get; set; } = string.Empty;
    public string ConsumerNo { get; set; } = string.Empty;
    public string? ConsumerName { get; set; }
    public string? MobileNo { get; set; }
    public string? PropertyNo { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime NoticeDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal DisconnectionFee { get; set; }
    public decimal ReconnectionFee { get; set; }
}

public class DisconnectionCreateViewModel
{
    [Required]
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    public DisconnectionConsumerSummaryViewModel? Consumer { get; set; }

    [Required]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = DisconnectionReasons.NonPayment;

    [Required]
    [Display(Name = "Notice Date")]
    public DateTime NoticeDate { get; set; } = DateTime.Today;

    [Display(Name = "Due / Compliance Date")]
    public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(15);

    [Display(Name = "Outstanding Amount")]
    [Range(0, 999999999)]
    public decimal OutstandingAmount { get; set; }

    [Display(Name = "Disconnection Fee")]
    [Range(0, 999999999)]
    public decimal DisconnectionFee { get; set; }

    [Display(Name = "Reconnection Fee")]
    [Range(0, 999999999)]
    public decimal ReconnectionFee { get; set; }

    [StringLength(100)]
    [Display(Name = "Field Officer")]
    public string? FieldOfficerName { get; set; }

    [Required(ErrorMessage = "Remarks are required.")]
    [StringLength(500)]
    public string? Remarks { get; set; }

    public IReadOnlyList<SelectListItem> ReasonOptions { get; set; } = [];
}

public class DisconnectionConsumerSummaryViewModel
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
    public int? Status { get; set; }
}

public class DisconnectionDetailsViewModel : DisconnectionCaseRowViewModel
{
    public DisconnectionConsumerSummaryViewModel? Consumer { get; set; }
    public string? FieldOfficerName { get; set; }
    public string? Remarks { get; set; }
    public string? ChallanNo { get; set; }
    public DateTime? DisconnectedOn { get; set; }
    public DateTime? ReconnectionRequestedOn { get; set; }
    public DateTime? ReconnectedOn { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanDisconnect { get; set; }
    public bool CanRequestReconnect { get; set; }
    public bool CanReconnect { get; set; }
    public bool CanCancel { get; set; }
    public IReadOnlyList<DisconnectionHistoryViewModel> Histories { get; set; } = [];
}

public class DisconnectionHistoryViewModel
{
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? ActionByName { get; set; }
    public DateTime ActionAt { get; set; }
}
