using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerTransfer
{
    public int TransfId { get; set; }

    public string? ConsNo { get; set; }

    public string? ConsNm { get; set; }

    public string? ConsFnm { get; set; }

    public DateTime? TransDate { get; set; }

    public double? TransAmt { get; set; }

    public string? ChallanNo { get; set; }

    public string? BankNm { get; set; }

    public DateTime? ChallanDate { get; set; }

    public double? Secu { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }

    public int? DevType { get; set; }

    public virtual ConsumerDetailsMaster? ConsNoNavigation { get; set; }
}
