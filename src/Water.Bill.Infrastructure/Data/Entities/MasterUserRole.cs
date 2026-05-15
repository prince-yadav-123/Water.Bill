using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterUserRole
{
    public int? RoleId { get; set; }

    public string? UserRole { get; set; }

    public string? RoleDis { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? Entrydate { get; set; }

    public int? DevType { get; set; }
}
