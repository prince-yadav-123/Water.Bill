using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class UserAttendance
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? AType { get; set; }

    public string? InTime { get; set; }

    public string? OutTime { get; set; }

    public DateOnly? Ddate { get; set; }

    public string? Day { get; set; }

    public string? Hour { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? UpdateDate { get; set; }
}
