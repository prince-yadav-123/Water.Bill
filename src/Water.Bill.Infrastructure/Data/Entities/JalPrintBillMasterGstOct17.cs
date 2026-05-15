using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalPrintBillMasterGstOct17
{
    public string BillNo { get; set; } = null!;

    public string? ConsNo { get; set; }

    public double? Cgst { get; set; }

    public double? Sgst { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }
}
