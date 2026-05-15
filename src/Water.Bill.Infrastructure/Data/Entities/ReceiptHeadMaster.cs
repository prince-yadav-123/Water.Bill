using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ReceiptHeadMaster
{
    public int HeadId { get; set; }

    public string? HeadName { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? EntryDate { get; set; }
}
