namespace Water.Bill.Infrastructure.Data.Entities;

public class NoticeTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public string NoticeType { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
