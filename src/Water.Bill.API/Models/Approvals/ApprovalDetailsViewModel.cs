using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Models.Approvals;

public class ApprovalDetailsViewModel
{
    public ApplicationWorkflowTask Task { get; set; } = null!;

    public NewConnectionApplication? NewConnectionApplication { get; set; }

    public NewConnectionApplicationFee? Fee { get; set; }

    public IReadOnlyList<ApplicationWorkflowHistory> WorkflowTimeline { get; set; } = [];

    public IReadOnlyList<WorkflowStageProgressViewModel> StageProgress { get; set; } = [];

    public bool CanMoveToNextStage { get; set; }
}

public class WorkflowStageProgressViewModel
{
    public WorkflowStage Stage { get; set; } = null!;

    public ApplicationWorkflowTask? Task { get; set; }

    public string State { get; set; } = "Upcoming";

    public string RoleName { get; set; } = "-";

    public string UserName { get; set; } = "-";
}

public class ApprovalListViewModel
{
    public string ActiveTab { get; set; } = "Pending";

    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int? DepartmentId { get; set; }

    public int? StageId { get; set; }

    public string? ApplicationType { get; set; }

    public IReadOnlyList<ApprovalListItemViewModel> Items { get; set; } = [];

    public IReadOnlyList<MasterDeptDetail> Departments { get; set; } = [];

    public IReadOnlyList<WorkflowStage> Stages { get; set; } = [];
}

public class ApprovalListItemViewModel
{
    public long TaskId { get; set; }

    public string ApplicationNo { get; set; } = string.Empty;

    public string ApplicationType { get; set; } = string.Empty;

    public string? ApplicantName { get; set; }

    public string? MobileNumber { get; set; }

    public string? Property { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    public string CurrentStage { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public DateTime? SubmittedOn { get; set; }

    public DateTime AssignedOn { get; set; }

    public bool CanAct { get; set; }

    public bool CanApprove { get; set; }

    public bool CanReject { get; set; }
}
