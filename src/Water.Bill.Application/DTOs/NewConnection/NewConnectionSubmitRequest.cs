namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionSubmitRequest
{
    public NewConnectionApplicationFormDto Form { get; set; } = new();

    public IReadOnlyList<NewConnectionDocumentInputDto> Documents { get; set; } = [];

    public string? ApplicationNo { get; set; }

    public bool IsPublicApplication { get; set; }

    public string? SubmittedByConsumerNo { get; set; }

    public int? SubmittedByConsumerUserId { get; set; }

    public int? ActionBy { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}

