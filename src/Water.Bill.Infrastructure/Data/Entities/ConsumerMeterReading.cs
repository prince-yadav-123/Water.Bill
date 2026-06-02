namespace Water.Bill.Infrastructure.Data.Entities;

public class ConsumerMeterReading
{
    public long Id { get; set; }

    public string ReadingNo { get; set; } = null!;

    public string ConsumerNo { get; set; } = null!;

    public DateTime ReadingDate { get; set; }

    public DateTime? PeriodFrom { get; set; }

    public DateTime? PeriodTo { get; set; }

    public decimal? PreviousReading { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal Consumption { get; set; }

    public string MeterStatus { get; set; } = null!;

    public string? MeterNo { get; set; }

    public string? Remarks { get; set; }

    public string Source { get; set; } = null!;

    public int? RecordedByUserId { get; set; }

    public string? RecordedByName { get; set; }

    public DateTime RecordedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }
}
