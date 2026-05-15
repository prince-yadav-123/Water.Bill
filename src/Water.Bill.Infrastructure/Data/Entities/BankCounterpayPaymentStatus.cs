using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class BankCounterpayPaymentStatus
{
    public string? Id { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? Status { get; set; }
}
