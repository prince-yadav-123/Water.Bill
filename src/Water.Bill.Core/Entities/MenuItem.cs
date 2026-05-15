namespace Water.Bill.Core.Entities;

public class MenuItem : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public string? SectionLabel { get; set; }
    public string? Module { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public bool OpenInNewTab { get; set; }

    public MenuItem? Parent { get; set; }
    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
}
