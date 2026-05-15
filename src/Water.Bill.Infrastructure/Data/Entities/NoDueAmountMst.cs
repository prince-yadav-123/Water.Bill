using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NoDueAmountMst
{
    public int AutoId { get; set; }

    public string? ConTp { get; set; }

    public string? Amount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }
}
