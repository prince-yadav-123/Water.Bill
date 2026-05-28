using Water.Bill.Application.DTOs.Workflow;

namespace Water.Bill.Application.Interfaces;

public interface IWorkflowService
{
    Task<long?> StartWorkflowAsync(string applicationType, long applicationId, string applicationNo, string currentStatus, int? actorUserId, string? actorName, string? actorRole, CancellationToken ct = default);

    Task ProcessActionAsync(WorkflowActionRequest request, CancellationToken ct = default);

    Task RepairSequentialWorkflowTasksAsync(CancellationToken ct = default);
}
