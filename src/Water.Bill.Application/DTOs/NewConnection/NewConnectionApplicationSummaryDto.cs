namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApplicationSummaryDto
{
    public long Id { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string ApplicationStatus { get; set; } = null!;

    public string ApplicantName { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? FlatNo { get; set; }

    public DateTime? SubmittedOn { get; set; }

    public bool IsPublicApplication { get; set; }
}
