namespace Water.Bill.Application.DTOs.Workflow;

public class WorkflowActionRequest
{
    public long TaskId { get; set; }

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActorUserId { get; set; }

    public int? ActorRoleId { get; set; }

    public string? ActorName { get; set; }

    public string? ActorRole { get; set; }

    public IReadOnlyCollection<int> ActorDepartmentIds { get; set; } = [];

    public bool IsAdmin { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}
