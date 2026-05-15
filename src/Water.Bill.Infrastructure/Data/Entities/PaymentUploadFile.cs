using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PaymentUploadFile
{
    public int FileId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public int? TotalCount { get; set; }

    public int? SuccessCount { get; set; }

    public int? FailureCount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? LastUpdatedOn { get; set; }
}
