using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class VillageDetail
{
    public int VillageNo { get; set; }

    public int? VillageId { get; set; }

    public string VillageName { get; set; } = null!;

    public string? VillageStr { get; set; }

    public int? Status { get; set; }

    public int? DevType { get; set; }
}
