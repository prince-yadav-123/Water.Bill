namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApplicationDetailsDto : NewConnectionApplicationSummaryDto
{
    public string? FatherName { get; set; }

    public string? EmailId { get; set; }

    public string? Address { get; set; }

    public decimal PlotSize { get; set; }

    public decimal? PipeSize { get; set; }

    public string? KhasraNo { get; set; }

    public string? VillageName { get; set; }

    public int? VillageId { get; set; }

    public string? ConnectionCategory { get; set; }

    public string? ConnectionType { get; set; }

    public string? FlatType { get; set; }

    public string? PurposeOfConnection { get; set; }

    public string? PreviousConnectionYesNo { get; set; }

    public string? OtherConnection { get; set; }

    public string? Rid { get; set; }

    public int? DevType { get; set; }

    public string? Remarks { get; set; }

    public bool DeclarationAccepted { get; set; }

    public string? SubmittedByConsumerNo { get; set; }

    public IReadOnlyList<NewConnectionDocumentDto> Documents { get; set; } = [];

    public IReadOnlyList<NewConnectionApprovalHistoryDto> Timeline { get; set; } = [];

    public IReadOnlyList<NewConnectionWorkflowStageDto> WorkflowStages { get; set; } = [];
}

public class NewConnectionWorkflowStageDto
{
    public int StageOrder { get; set; }

    public string StageName { get; set; } = string.Empty;

    public string State { get; set; } = "Upcoming";

    public string? Remarks { get; set; }

    public DateTime? AssignedOn { get; set; }

    public DateTime? ActionOn { get; set; }
}
