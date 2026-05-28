namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NotificationLog
{
    public long Id { get; set; }

    public long? ApplicationId { get; set; }

    public string? ApplicationNo { get; set; }

    public long? WorkflowInstanceId { get; set; }

    public int? StageId { get; set; }

    public string Channel { get; set; } = null!;

    public string Recipient { get; set; } = null!;

    public string? Message { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SentOn { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public DateTime CreatedOn { get; set; }
}
