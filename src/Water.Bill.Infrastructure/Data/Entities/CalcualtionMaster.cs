using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CalcualtionMaster
{
    public int Id { get; set; }

    public string? ConsNo { get; set; }

    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public int? Status { get; set; }

    public string? CalMeth { get; set; }

    public DateTime? EntryDtae { get; set; }

    public int? DevType { get; set; }
}
