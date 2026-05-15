using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ReceiptHeadLink
{
    public int ReceiptSubheadId { get; set; }

    public int? DepId { get; set; }

    public int? HeadId { get; set; }
}
