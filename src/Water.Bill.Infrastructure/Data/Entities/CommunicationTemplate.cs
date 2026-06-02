namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CommunicationTemplate
{
    public int Id { get; set; }

    public int PurposeId { get; set; }

    public string PurposeKey { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string TemplateName { get; set; } = null!;

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public string? ExternalTemplateId { get; set; }

    public string? Language { get; set; }

    public bool IsDefault { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CommunicationPurpose Purpose { get; set; } = null!;
}
