namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NotificationTarget
{
    public long Id { get; set; }
    public long NotificationId { get; set; }

    /// <summary>
    /// AllConsumers | ConsumerUser | ConsumerNo |
    /// AllInternalUsers | InternalUser | Role | Department | Division
    /// </summary>
    public string TargetType { get; set; } = null!;

    /// <summary>UserId / RoleId / DeptId / ConsumerNo / ConsumerUserId as string.</summary>
    public string? TargetId { get; set; }

    public string? TargetName { get; set; }
    public bool IsDeleted { get; set; }

    public virtual NotificationMaster Notification { get; set; } = null!;
}
