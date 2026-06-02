namespace Water.Bill.Infrastructure.Data.Entities;

public partial class InAppNotification
{
    public long Id { get; set; }

    public string UserType { get; set; } = null!;

    public long UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string PurposeKey { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public string? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}

