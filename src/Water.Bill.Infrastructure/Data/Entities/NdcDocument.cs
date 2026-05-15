using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NdcDocument
{
    public int AutoId { get; set; }

    public string? ConsumerNo { get; set; }

    public bool? Status { get; set; }

    public string? AttachmentPath { get; set; }

    public string? AttachmentName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? NdcAutoId { get; set; }
}
