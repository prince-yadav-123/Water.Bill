using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalIntrestRateMaster
{
    public int IntId { get; set; }

    public DateTime? IntDateFrom { get; set; }

    public DateTime? IntDateTo { get; set; }

    public double? Intrest { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }
}
