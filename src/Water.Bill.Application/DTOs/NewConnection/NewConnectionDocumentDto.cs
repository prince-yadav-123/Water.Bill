namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionDocumentDto
{
    public long Id { get; set; }

    public string DocumentType { get; set; } = null!;

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public DateTime UploadedOn { get; set; }
}
