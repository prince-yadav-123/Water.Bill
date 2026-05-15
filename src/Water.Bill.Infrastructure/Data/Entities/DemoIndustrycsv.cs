using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class DemoIndustrycsv
{
    public int Rid { get; set; }

    public string AlloteeName { get; set; } = null!;

    public string? MobileNumber { get; set; }

    public string SectorName { get; set; } = null!;

    public string? BlockName { get; set; }

    public string? PropertyNo { get; set; }
}
