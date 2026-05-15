using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerNodue
{
    public int NodueId { get; set; }

    public string? ConsNo { get; set; }

    public double? NodueAmt { get; set; }

    public DateTime? NodueDt { get; set; }

    public string? ChallanNo { get; set; }

    public DateTime? ChallanDt { get; set; }

    public string? ChallanBank { get; set; }

    public double? Secu { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }

    public DateTime? NdcUpto { get; set; }

    public int? DevType { get; set; }

    public virtual ConsumerDetailsMaster? ConsNoNavigation { get; set; }
}
