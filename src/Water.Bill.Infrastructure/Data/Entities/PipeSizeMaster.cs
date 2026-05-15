using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PipeSizeMaster
{
    public int? PipeSizeId { get; set; }

    public int? PipeSize { get; set; }

    public int? Status { get; set; }

    public int? DevType { get; set; }
}
