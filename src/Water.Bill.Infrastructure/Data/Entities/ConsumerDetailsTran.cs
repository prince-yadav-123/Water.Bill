using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerDetailsTran
{
    public int Id { get; set; }

    public string ConsNo { get; set; } = null!;

    public string? CalDate { get; set; }

    public string? AllotDate { get; set; }

    public string? PosDate { get; set; }

    public string? CompDate { get; set; }

    public string? SsiDate { get; set; }

    public string? AffidavitYn { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DevType { get; set; }

    public virtual ConsumerDetailsMaster ConsNoNavigation { get; set; } = null!;
}
