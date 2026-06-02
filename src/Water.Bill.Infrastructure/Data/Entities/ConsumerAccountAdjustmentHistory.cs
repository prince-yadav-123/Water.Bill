namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerAccountAdjustmentHistory
{
    public long Id { get; set; }

    public long AdjustmentId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActionByUserId { get; set; }

    public string? ActionByName { get; set; }

    public DateTime ActionAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsumerAccountAdjustment Adjustment { get; set; } = null!;
}
