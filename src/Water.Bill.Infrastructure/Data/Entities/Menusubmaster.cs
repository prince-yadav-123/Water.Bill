using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Menusubmaster
{
    public int? Menuid { get; set; }

    public int? Submenuid { get; set; }

    public string? Menuname { get; set; }

    public string? Linkurl { get; set; }

    public string? Role { get; set; }

    public string? Haschild { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? Entrydate { get; set; }
}
