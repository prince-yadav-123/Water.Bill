using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class TransApplicationDocument
{
    public string? ApplicationId { get; set; }

    public int? DocumentId { get; set; }

    public string? DocumentNo { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public byte[]? DocumentUplod { get; set; }

    public int? Status { get; set; }

    public DateOnly EntryDate { get; set; }

    public string? DocumentsUplod { get; set; }

    public virtual MasterApplicationDetail? Application { get; set; }
}
