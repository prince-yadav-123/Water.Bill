using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class UserMaster
{
    public int SlNo { get; set; }

    public string? UserFname { get; set; }

    public string? UserDepart { get; set; }

    public string? UserEmail { get; set; }

    public string? UserMob { get; set; }

    public string? Userid { get; set; }

    public string? UserPass { get; set; }

    public string? UserRole { get; set; }

    public int? DevType { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }
}
