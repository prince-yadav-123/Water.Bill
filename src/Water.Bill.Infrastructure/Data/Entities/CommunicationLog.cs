namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CommunicationLog
{
    public long Id { get; set; }

    public string PurposeKey { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string? RecipientName { get; set; }

    public string? RecipientEmail { get; set; }

    public string? RecipientMobile { get; set; }

    public string? Subject { get; set; }

    public string MessageBody { get; set; } = null!;

    public int? TemplateId { get; set; }

    public string? ExternalTemplateId { get; set; }

    public string Status { get; set; } = "Pending";

    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ReferenceType { get; set; }

    public string? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public int RetryCount { get; set; }
}

