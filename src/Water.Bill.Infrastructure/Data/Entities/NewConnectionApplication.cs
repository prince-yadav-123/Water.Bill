namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConnectionApplication
{
    public long Id { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string ApplicationStatus { get; set; } = "Draft";

    public string? FinalConsumerNo { get; set; }

    public bool IsPublicApplication { get; set; }

    public string ApplicantName { get; set; } = null!;

    public string? FatherName { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string? EmailId { get; set; }

    public string Address { get; set; } = null!;

    public string Sector { get; set; } = null!;

    public string Block { get; set; } = null!;

    public string FlatNo { get; set; } = null!;

    public decimal PlotSize { get; set; }

    public decimal? PipeSize { get; set; }

    public string? KhasraNo { get; set; }

    public string? VillageName { get; set; }

    public int? VillageId { get; set; }

    public string ConnectionCategory { get; set; } = null!;

    public string ConnectionType { get; set; } = null!;

    public string FlatType { get; set; } = null!;

    public string? PurposeOfConnection { get; set; }

    public string? PreviousConnectionYesNo { get; set; }

    public string? OtherConnection { get; set; }

    public string? Rid { get; set; }

    public int? DevType { get; set; }

    public string? RegNo { get; set; }

    public DateTime? ConnectionDate { get; set; }

    public string? EstimationNo { get; set; }

    public decimal? EstimationAmount { get; set; }

    public decimal? SecurityAmount { get; set; }

    public DateTime? EstimationDate { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? MonthlyCharges { get; set; }

    public string? IssueOfficer { get; set; }

    public DateOnly? AllotmentDate { get; set; }

    public DateOnly? PossessionDate { get; set; }

    public DateOnly? ComplianceDate { get; set; }

    public DateOnly? SsiDate { get; set; }

    public string? AffidavitYn { get; set; }

    public string? SubmittedByConsumerNo { get; set; }

    public int? SubmittedByConsumerUserId { get; set; }

    public DateTime? SubmittedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public int? RejectedBy { get; set; }

    public DateTime? RejectedOn { get; set; }

    public string? RejectionReason { get; set; }

    public string? Remarks { get; set; }

    public bool DeclarationAccepted { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual ICollection<NewConnectionApplicationDocument> Documents { get; set; } = new List<NewConnectionApplicationDocument>();

    public virtual ICollection<NewConnectionApprovalHistory> ApprovalHistory { get; set; } = new List<NewConnectionApprovalHistory>();
}

