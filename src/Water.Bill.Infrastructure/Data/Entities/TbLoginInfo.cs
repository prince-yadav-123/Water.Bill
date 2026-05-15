using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class TbLoginInfo
{
    public int LoginId { get; set; }

    public string? Division { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }

    public string? UserRole { get; set; }

    public string? Status { get; set; }

    public string? IfFor { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? Level { get; set; }

    public bool? IsAdmin { get; set; }
}
