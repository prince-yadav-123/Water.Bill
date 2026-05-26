using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Rolepermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string Module { get; set; } = null!;

    public int? ModuleId { get; set; }

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

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual PermissionModule? PermissionModule { get; set; }

    public virtual Approle Role { get; set; } = null!;
}

