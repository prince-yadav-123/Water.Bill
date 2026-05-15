using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterVersionInfo
{
    public int Id { get; set; }

    public string? VersionInfo { get; set; }

    public string? Text { get; set; }

    public string? Link { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }
}
