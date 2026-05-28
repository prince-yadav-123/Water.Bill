namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ApplicationWorkflowTask
{
    public long Id { get; set; }

    public long WorkflowInstanceId { get; set; }

    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public int StageId { get; set; }

    public int? AssignedDepartmentId { get; set; }

    public int? AssignedRoleId { get; set; }

    public int? AssignedUserId { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime AssignedOn { get; set; }

    public DateTime? ActionOn { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual ApplicationWorkflowInstance WorkflowInstance { get; set; } = null!;

    public virtual WorkflowStage Stage { get; set; } = null!;
}
