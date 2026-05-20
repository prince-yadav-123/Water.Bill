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

    public string? ConnectionCategory { get; set; }

    public string? ConnectionType { get; set; }

    public string? FlatType { get; set; }

    public string? PurposeOfConnection { get; set; }

    public string? PreviousConnectionYesNo { get; set; }

    public string? OtherConnection { get; set; }

    public string? SubmittedByConsumerNo { get; set; }

    public IReadOnlyList<NewConnectionDocumentDto> Documents { get; set; } = [];

    public IReadOnlyList<NewConnectionApprovalHistoryDto> Timeline { get; set; } = [];
}
