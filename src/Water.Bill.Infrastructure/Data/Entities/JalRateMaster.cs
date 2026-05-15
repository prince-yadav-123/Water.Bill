using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalRateMaster
{
    public int Id { get; set; }

    public string? PropertyType { get; set; }

    public string? IdT { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }

    public virtual ICollection<JalRateTran> JalRateTrans { get; set; } = new List<JalRateTran>();
}
