using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalBankMaster
{
    public string? BankId { get; set; }

    public string? BankName { get; set; }

    public string? AccountNo { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }
}
