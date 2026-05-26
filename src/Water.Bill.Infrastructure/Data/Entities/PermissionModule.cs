namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PermissionModule
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Menuitem> Menuitems { get; set; } = new List<Menuitem>();

    public virtual ICollection<Rolepermission> Rolepermissions { get; set; } = new List<Rolepermission>();
}
