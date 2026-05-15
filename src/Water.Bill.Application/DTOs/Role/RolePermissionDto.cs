namespace Water.Bill.Application.DTOs.Menu;

public class RolePermissionDto
{
    public Guid RoleId { get; set; }
    public string Module { get; set; } = string.Empty;
    public Guid? ModuleId { get; set; }
    public bool CanSeeMenu { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanDownload { get; set; }
    public bool CanExport { get; set; }
    public bool CanApprove { get; set; }
    public bool CanForward { get; set; }
    public bool CanPrint { get; set; }
}
