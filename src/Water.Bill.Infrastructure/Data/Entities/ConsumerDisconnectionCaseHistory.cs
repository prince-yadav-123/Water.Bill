namespace Water.Bill.Infrastructure.Data.Entities;

public class ConsumerDisconnectionCaseHistory
{
    public long Id { get; set; }

    public long CaseId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActionByUserId { get; set; }

    public string? ActionByName { get; set; }

    public DateTime ActionAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsumerDisconnectionCase Case { get; set; } = null!;
}
