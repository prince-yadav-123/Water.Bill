using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterConnectionTypeDetailsTran
{
    public int SubConId { get; set; }

    public string? ConId { get; set; }

    public string? SubConName { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }
}
