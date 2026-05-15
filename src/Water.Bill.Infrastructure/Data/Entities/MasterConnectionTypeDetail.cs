using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterConnectionTypeDetail
{
    public string ConId { get; set; } = null!;

    public string? ConName { get; set; }

    public string? ConMainId { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }
}
