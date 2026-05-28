namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerSupportQuery
{
    public long Id { get; set; }

    public string QueryNo { get; set; } = null!;

    public int? ConsumerUserId { get; set; }

    public string ConsumerNo { get; set; } = null!;

    public string ConsumerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Priority { get; set; } = "Normal";

    public string Status { get; set; } = "Open";

    public string? RelatedBillNo { get; set; }

    public string? RelatedApplicationNo { get; set; }

    public string? AdminRemarks { get; set; }

    public int? AssignedToUserId { get; set; }

    public int? ResolvedByUserId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual SupportQueryCategory Category { get; set; } = null!;

    public virtual ICollection<ConsumerSupportQueryDocument> Documents { get; set; } = new List<ConsumerSupportQueryDocument>();

    public virtual ICollection<ConsumerSupportQueryHistory> Histories { get; set; } = new List<ConsumerSupportQueryHistory>();
}
