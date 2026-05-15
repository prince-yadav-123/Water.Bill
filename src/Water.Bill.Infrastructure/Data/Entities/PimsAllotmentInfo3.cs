using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PimsAllotmentInfo3
{
    public double? Rid { get; set; }

    public double? SectorName { get; set; }

    public string? BlockName { get; set; }

    public string? PropertyNo { get; set; }

    public string? PropertyName { get; set; }

    public string? CurrentAlloteeName { get; set; }

    public double? Mobile { get; set; }

    public string? Email { get; set; }

    public double? TotalArea { get; set; }

    public DateTime? Allotmentdate { get; set; }

    public double? PropertyTypeId { get; set; }

    public string? PropertyTypeName { get; set; }
}
