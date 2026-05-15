using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterDocumentUpload
{
    public int DocumentId { get; set; }

    public string? DocumentName { get; set; }

    public int? Status { get; set; }

    public string? DocFor { get; set; }
}
