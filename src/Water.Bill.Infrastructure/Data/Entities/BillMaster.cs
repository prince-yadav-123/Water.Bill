using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class BillMaster
{
    public int Id { get; set; }

    public string? ConsNo { get; set; }

    public string? FYear { get; set; }

    public int? PipeSize { get; set; }

    public int? PlotSize { get; set; }

    public string? ConType { get; set; }

    public string? ConCtg { get; set; }

    public double? MinRate { get; set; }

    public double? TotalRate { get; set; }

    public double? CessAmt { get; set; }

    public double? PaidAmt { get; set; }

    public double? BalAmt { get; set; }

    public double? DiffAmt { get; set; }

    public DateTime? PaidDate { get; set; }

    public int? PaidMonth { get; set; }

    public double? ArrearInt { get; set; }

    public double? CessInt { get; set; }

    public double? OldRate { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DevType { get; set; }
}
