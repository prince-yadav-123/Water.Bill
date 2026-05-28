namespace Water.Bill.Infrastructure.Data.Entities;

public partial class SupportQueryCategory
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ConsumerSupportQuery> Queries { get; set; } = new List<ConsumerSupportQuery>();
}
