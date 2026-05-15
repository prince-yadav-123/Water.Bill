using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Menuitem
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ParentId { get; set; }

    public string Label { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Url { get; set; }

    public string? SectionLabel { get; set; }

    public string? Module { get; set; }

    public Guid? ModuleId { get; set; }

    public int Order { get; set; }

    public bool? IsActive { get; set; }

    public bool ShowInSidebar { get; set; } = true;

    public bool OpenInNewTab { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Menuitem> InverseParent { get; set; } = new List<Menuitem>();

    public virtual PermissionModule? PermissionModule { get; set; }

    public virtual Menuitem? Parent { get; set; }
}
