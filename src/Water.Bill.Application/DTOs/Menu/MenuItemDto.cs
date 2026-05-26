namespace Water.Bill.Application.DTOs.Menu;

public class MenuItemDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? ParentId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public string? SectionLabel { get; set; }
    public string? Module { get; set; }
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
    public bool ShowInSidebar { get; set; } = true;
    public bool OpenInNewTab { get; set; }
    public IReadOnlyList<MenuItemDto> Children { get; set; } = [];
}

