namespace Water.Bill.Infrastructure.Data.Entities;

public partial class WorkflowStage
{
    public int Id { get; set; }

    public int WorkflowId { get; set; }

    public string StageName { get; set; } = null!;

    public int StageOrder { get; set; }

    public int? DepartmentId { get; set; }

    public int? ApproverRoleId { get; set; }

    public int? ApproverUserId { get; set; }

    public string ApprovalType { get; set; } = "DepartmentRole";

    public bool CanApprove { get; set; } = true;

    public bool CanReject { get; set; } = true;

    public bool CanSendCorrection { get; set; } = true;

    public bool CanForward { get; set; }

    public bool IsFinalStage { get; set; }

    public int? SlaDays { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual WorkflowMaster Workflow { get; set; } = null!;

    public virtual MasterDeptDetail? Department { get; set; }
}
