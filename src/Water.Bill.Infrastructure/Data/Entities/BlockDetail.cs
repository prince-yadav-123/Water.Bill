using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class BlockDetail
{
    public string SectorId { get; set; } = null!;

    public string Block { get; set; } = null!;

    public int? Status { get; set; }

    public int? DevType { get; set; }
}
