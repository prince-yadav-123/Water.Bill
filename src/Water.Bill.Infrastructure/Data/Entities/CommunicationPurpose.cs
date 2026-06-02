namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CommunicationPurpose
{
    public int Id { get; set; }

    public string PurposeKey { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Description { get; set; }

    public string AllowedPlaceholders { get; set; } = "[]";

    public bool IsSystem { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CommunicationTemplate> Templates { get; set; } = new List<CommunicationTemplate>();
}

