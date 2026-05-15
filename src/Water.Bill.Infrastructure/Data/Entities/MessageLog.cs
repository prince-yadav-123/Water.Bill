using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MessageLog
{
    public int Id { get; set; }

    public string MobNo { get; set; } = null!;

    public string Jobid { get; set; } = null!;

    public string Errorcode { get; set; } = null!;

    public string Errormessage { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Flag { get; set; } = null!;

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }
}
