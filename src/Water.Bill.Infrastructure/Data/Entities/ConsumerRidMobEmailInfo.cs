using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerRidMobEmailInfo
{
    public string? ConsNo { get; set; }

    public double? Rid { get; set; }

    public string? ConsNm1 { get; set; }

    public string? FlatNo { get; set; }

    public string? BlkNo { get; set; }

    public string? Sector { get; set; }

    public string? MobNo { get; set; }

    public string? EmailId { get; set; }

    public string? Status { get; set; }
}
