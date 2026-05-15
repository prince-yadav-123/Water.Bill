using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerXmlDetail
{
    public string? ConsNo { get; set; }

    public string? XmlData { get; set; }

    public int? Status { get; set; }

    public int? DevType { get; set; }
}
