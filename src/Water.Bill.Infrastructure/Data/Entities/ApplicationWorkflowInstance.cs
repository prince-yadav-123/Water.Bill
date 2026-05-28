namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ApplicationWorkflowInstance
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string ApplicationType { get; set; } = null!;

    public int WorkflowId { get; set; }

    public int? CurrentStageId { get; set; }

    public string CurrentStatus { get; set; } = null!;

    public DateTime StartedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual WorkflowMaster Workflow { get; set; } = null!;

    public virtual WorkflowStage? CurrentStage { get; set; }
}
