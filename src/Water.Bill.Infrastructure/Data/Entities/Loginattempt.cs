using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Loginattempt
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? IpAddress { get; set; }

    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public string? UserAgent { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Appuser? User { get; set; }
}

