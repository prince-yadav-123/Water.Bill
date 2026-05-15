using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class SectorDetail
{
    public int SNo { get; set; }

    public string SectorId { get; set; } = null!;

    public string? SectorNo { get; set; }

    public int? Status { get; set; }

    public int? OrderBy { get; set; }

    public int? DevType { get; set; }
}
