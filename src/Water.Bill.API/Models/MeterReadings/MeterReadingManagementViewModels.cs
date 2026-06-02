using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Water.Bill.API.Models.MeterReadings;

public static class MeterReadingStatuses
{
    public const string Normal = "Normal";
    public const string Locked = "Locked";
    public const string Faulty = "Faulty";
    public const string NoMeter = "NoMeter";
    public const string Average = "Average";
    public const string NotAvailable = "NotAvailable";

    public static string Display(string? status) => status switch
    {
        Normal => "Normal",
        Locked => "Locked",
        Faulty => "Faulty Meter",
        NoMeter => "No Meter",
        Average => "Average Reading",
        NotAvailable => "Not Available",
        _ => status ?? "-"
    };

    public static IReadOnlyList<SelectListItem> Options(string? selected = null) =>
    [
        new(Display(Normal), Normal, string.Equals(selected, Normal, StringComparison.OrdinalIgnoreCase)),
        new(Display(Locked), Locked, string.Equals(selected, Locked, StringComparison.OrdinalIgnoreCase)),
        new(Display(Faulty), Faulty, string.Equals(selected, Faulty, StringComparison.OrdinalIgnoreCase)),
        new(Display(NoMeter), NoMeter, string.Equals(selected, NoMeter, StringComparison.OrdinalIgnoreCase)),
        new(Display(Average), Average, string.Equals(selected, Average, StringComparison.OrdinalIgnoreCase)),
        new(Display(NotAvailable), NotAvailable, string.Equals(selected, NotAvailable, StringComparison.OrdinalIgnoreCase))
    ];
}

public class MeterReadingIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public string? MeterStatus { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool HasConsumerSearch { get; set; }

    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];

    public IReadOnlyList<MeterReadingConsumerSearchRowViewModel> Consumers { get; set; } = [];

    public IReadOnlyList<MeterReadingListRowViewModel> Readings { get; set; } = [];
}

public class MeterReadingConsumerSearchRowViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? ConnectionType { get; set; }

    public int? DevType { get; set; }

    public decimal? LastReading { get; set; }

    public DateTime? LastReadingDate { get; set; }
}

public class MeterReadingListRowViewModel
{
    public long Id { get; set; }

    public string ReadingNo { get; set; } = string.Empty;

    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public DateTime ReadingDate { get; set; }

    public decimal? PreviousReading { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal Consumption { get; set; }

    public string MeterStatus { get; set; } = string.Empty;

    public string? MeterNo { get; set; }
}

public class MeterReadingCreateViewModel
{
    [Required]
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    public MeterReadingConsumerSummaryViewModel? Consumer { get; set; }

    [Required(ErrorMessage = "Reading date is required.")]
    [Display(Name = "Reading Date")]
    public DateTime ReadingDate { get; set; } = DateTime.Today;

    [Display(Name = "Period From")]
    public DateTime? PeriodFrom { get; set; }

    [Display(Name = "Period To")]
    public DateTime? PeriodTo { get; set; }

    [Display(Name = "Previous Reading")]
    public decimal? PreviousReading { get; set; }

    [Required(ErrorMessage = "Current reading is required.")]
    [Range(0, 999999999, ErrorMessage = "Current reading must be valid.")]
    [Display(Name = "Current Reading")]
    public decimal CurrentReading { get; set; }

    [Display(Name = "Consumption")]
    public decimal Consumption { get; set; }

    [Required(ErrorMessage = "Meter status is required.")]
    [Display(Name = "Meter Status")]
    public string MeterStatus { get; set; } = MeterReadingStatuses.Normal;

    [StringLength(50)]
    [Display(Name = "Meter No")]
    public string? MeterNo { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
}

public class MeterReadingConsumerSummaryViewModel
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

    public int? PipeSize { get; set; }

    public int? DevType { get; set; }
}

public class MeterReadingDetailsViewModel : MeterReadingListRowViewModel
{
    public MeterReadingConsumerSummaryViewModel? Consumer { get; set; }

    public DateTime? PeriodFrom { get; set; }

    public DateTime? PeriodTo { get; set; }

    public string? Remarks { get; set; }

    public string Source { get; set; } = string.Empty;

    public string? RecordedByName { get; set; }

    public DateTime RecordedAt { get; set; }

    public IReadOnlyList<MeterReadingListRowViewModel> History { get; set; } = [];
}
