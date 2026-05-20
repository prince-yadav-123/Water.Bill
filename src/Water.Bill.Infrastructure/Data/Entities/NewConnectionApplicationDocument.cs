namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConnectionApplicationDocument
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string? DocumentNo { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public Guid? UploadedBy { get; set; }

    public DateTime UploadedOn { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual NewConnectionApplication Application { get; set; } = null!;
}
