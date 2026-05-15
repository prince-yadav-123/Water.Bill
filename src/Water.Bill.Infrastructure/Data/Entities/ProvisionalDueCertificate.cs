using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ProvisionalDueCertificate
{
    public int AutoId { get; set; }

    public string? ConsNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public DateTime? CreateOn { get; set; }
}
