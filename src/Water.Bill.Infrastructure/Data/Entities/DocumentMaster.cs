using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class DocumentMaster
{
    public int AutoId { get; set; }

    public string ApplicationId { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string DocumentPath { get; set; } = null!;

    public bool? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
