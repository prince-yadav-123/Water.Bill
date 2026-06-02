namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerAccountAdjustment
{
    public long Id { get; set; }

    public string AdjustmentNo { get; set; } = null!;

    public string ConsumerNo { get; set; } = null!;

    public string AdjustmentType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime EffectiveDate { get; set; }

    public string? SourceBillNo { get; set; }

    public string? SourceChallanNo { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = null!;

    public string? AppliedBillNo { get; set; }

    public DateTime? AppliedOn { get; set; }

    public long? ReversalOfAdjustmentId { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public string? UpdatedByName { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ConsumerAccountAdjustmentHistory> Histories { get; set; } = new List<ConsumerAccountAdjustmentHistory>();
}
