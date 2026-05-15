using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterApplicationDetailHistory
{
    public string ApplicationId { get; set; } = null!;

    public int? SerialNumber { get; set; }

    public string? Division { get; set; }

    public string? CurrentHoldingPer { get; set; }

    public DateOnly? ForwardDate { get; set; }

    public string? Remark { get; set; }

    public string? Flag { get; set; }

    public string? CurentStatus { get; set; }

    public string? Status { get; set; }
}
