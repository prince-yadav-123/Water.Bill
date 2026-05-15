namespace Water.Bill.Core.Entities;

public class LoginAttempt : BaseEntity
{
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }

    public AppUser? User { get; set; }
}
