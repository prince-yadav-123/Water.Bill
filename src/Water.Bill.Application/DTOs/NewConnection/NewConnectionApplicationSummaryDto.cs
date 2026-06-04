namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApplicationSummaryDto
{
    public long Id { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string ApplicationStatus { get; set; } = null!;

    public string? FinalConsumerNo { get; set; }

    public string ApplicantName { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? FlatNo { get; set; }

    public DateTime? SubmittedOn { get; set; }

    public bool IsPublicApplication { get; set; }

    public decimal? TotalFee { get; set; }

    public string? PaymentStatus { get; set; }

    public bool CanContinue { get; set; }

    /// <summary>True when the application is sent back to the applicant for correction and can be resubmitted.</summary>
    public bool CanResubmit { get; set; }

    /// <summary>Authority remarks entered when the application was sent back.</summary>
    public string? SentBackRemarks { get; set; }

    /// <summary>Date/time when the application was sent back.</summary>
    public DateTime? SentBackAt { get; set; }
}
