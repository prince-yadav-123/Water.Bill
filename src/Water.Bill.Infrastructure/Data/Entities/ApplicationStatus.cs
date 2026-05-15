using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ApplicationStatus
{
    public int AutoId { get; set; }

    public string? StatusName { get; set; }

    public bool? IsActive { get; set; }
}
