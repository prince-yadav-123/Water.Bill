namespace Water.Bill.Core.Entities;

public class AppRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Permissions { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
