namespace Water.Bill.Infrastructure.Data.Entities;

public class ConsumerDisconnectionCase
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = null!;

    public string ConsumerNo { get; set; } = null!;

    public string CaseType { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime NoticeDate { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal OutstandingAmount { get; set; }

    public decimal DisconnectionFee { get; set; }

    public decimal ReconnectionFee { get; set; }

    public DateTime? DisconnectedOn { get; set; }

    public DateTime? ReconnectionRequestedOn { get; set; }

    public DateTime? ReconnectedOn { get; set; }

    public string? ChallanNo { get; set; }

    public string? FieldOfficerName { get; set; }

    public string? Remarks { get; set; }

    public string? PreviousConsumerCategory { get; set; }

    public int? PreviousStatus { get; set; }

    public int? PreviousNewStatus { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public string? UpdatedByName { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ConsumerDisconnectionCaseHistory> Histories { get; set; } = new List<ConsumerDisconnectionCaseHistory>();
}
