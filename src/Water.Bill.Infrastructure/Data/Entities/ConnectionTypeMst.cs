using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConnectionTypeMst
{
    public int AutoId { get; set; }

    public string? ConnectionName { get; set; }

    public string? ConnectionMainId { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public int? LastUpdatedBy { get; set; }
}
