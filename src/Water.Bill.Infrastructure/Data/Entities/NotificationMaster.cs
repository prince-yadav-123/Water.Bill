namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NotificationMaster
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string NotificationType { get; set; } = "General";
    public string TargetAudience { get; set; } = "Consumer";   // Consumer | Internal
    public string Channels { get; set; } = "InApp";            // InApp | Email | InApp,Email
    public string Priority { get; set; } = "Normal";           // Low | Normal | High | Urgent
    public string Status { get; set; } = "Draft";              // Draft | Sent | Cancelled
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? RedirectUrl { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public virtual ICollection<NotificationTarget> Targets { get; set; } = new List<NotificationTarget>();
}
