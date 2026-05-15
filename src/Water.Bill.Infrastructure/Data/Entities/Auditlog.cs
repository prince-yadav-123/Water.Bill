using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Auditlog
{
    public Guid Id { get; set; }

    public DateTime Timestamp { get; set; }

    public Guid? UserId { get; set; }

    public string? Username { get; set; }

    public int Action { get; set; }

    public string? Module { get; set; }

    public string? EntityId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Details { get; set; }

    public bool? Success { get; set; }
}
