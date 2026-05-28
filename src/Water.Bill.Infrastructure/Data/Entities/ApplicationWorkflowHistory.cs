namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ApplicationWorkflowHistory
{
    public long Id { get; set; }

    public long WorkflowInstanceId { get; set; }

    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public int? StageId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActionBy { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public DateTime ActionOn { get; set; }
}
