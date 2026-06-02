namespace Water.Bill.Infrastructure.Data.Entities;

public class ConsumerNotice
{
    public long Id { get; set; }

    public string NoticeNo { get; set; } = null!;

    public string ConsumerNo { get; set; } = null!;

    public int? TemplateId { get; set; }

    public string NoticeType { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime NoticeDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public string? RelatedBillNo { get; set; }

    public string? RelatedChallanNo { get; set; }

    public long? RelatedDisconnectionCaseId { get; set; }

    public decimal AmountDue { get; set; }

    public string? Remarks { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public string? UpdatedByName { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual NoticeTemplate? Template { get; set; }

    public virtual ICollection<ConsumerNoticeHistory> Histories { get; set; } = new List<ConsumerNoticeHistory>();
}
