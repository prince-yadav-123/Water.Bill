namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerUser
{
    public int Id { get; set; }

    public string ConsumerNo { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ConsumerDetailsMaster Consumer { get; set; } = null!;
}
