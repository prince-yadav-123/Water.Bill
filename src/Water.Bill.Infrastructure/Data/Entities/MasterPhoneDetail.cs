using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterPhoneDetail
{
    public double? TelId { get; set; }

    public string? TelNo { get; set; }

    public double? DevType { get; set; }

    public double? Status { get; set; }

    public string? EntryDate { get; set; }
}
