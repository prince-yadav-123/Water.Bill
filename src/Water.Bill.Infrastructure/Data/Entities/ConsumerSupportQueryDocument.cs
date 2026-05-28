namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerSupportQueryDocument
{
    public long Id { get; set; }

    public long QueryId { get; set; }

    public string DocumentType { get; set; } = "Support Document";

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public int? UploadedByConsumerUserId { get; set; }

    public DateTime UploadedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsumerSupportQuery Query { get; set; } = null!;
}
