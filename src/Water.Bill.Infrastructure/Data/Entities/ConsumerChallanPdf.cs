using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerChallanPdf
{
    public int AutoId { get; set; }

    public string? FileName { get; set; }

    public string? FileUrl { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? BillPeriod { get; set; }

    public string? MobileNo { get; set; }

    public string? ChallanNo { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }
}
