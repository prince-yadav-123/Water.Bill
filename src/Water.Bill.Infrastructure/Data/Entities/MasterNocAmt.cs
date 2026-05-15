using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterNocAmt
{
    public string ConId { get; set; } = null!;

    public string? ConName { get; set; }

    public string? ConMainId { get; set; }

    public int? NocAmt { get; set; }

    public string? Status { get; set; }

    public string? Amount { get; set; }

    public string? Sgst { get; set; }

    public string? Cgst { get; set; }

    public string? ExpiryTime { get; set; }
}
