namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionDocumentInputDto
{
    public string DocumentType { get; set; } = null!;

    public string? DocumentNo { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }
}
