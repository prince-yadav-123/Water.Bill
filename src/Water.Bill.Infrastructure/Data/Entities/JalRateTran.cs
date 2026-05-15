using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalRateTran
{
    public int Sid { get; set; }

    public int? Id { get; set; }

    public int? AreaStart { get; set; }

    public int? AreaEnd { get; set; }

    public double? Regular { get; set; }

    public double? Temporary { get; set; }

    public double? MainRate { get; set; }

    public double? EstRateReg { get; set; }

    public double? EstRateTemp { get; set; }

    public int? PipeSize { get; set; }

    public double? CessRate { get; set; }

    public DateTime? EffFrom { get; set; }

    public DateTime? EffTo { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }

    public virtual JalRateMaster? IdNavigation { get; set; }
}
