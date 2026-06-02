namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerComplaintHistory
{
    public long Id { get; set; }

    public long ComplaintId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActionByUserId { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public DateTime ActionAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsumerComplaint Complaint { get; set; } = null!;
}
