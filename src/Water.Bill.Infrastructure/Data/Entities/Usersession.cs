using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class Usersession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string SessionToken { get; set; } = null!;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? DeviceFingerprint { get; set; }

    public bool? IsActive { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Appuser User { get; set; } = null!;
}

