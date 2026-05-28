using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Workflow;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    public const string ApplicationTypeNewConnection = "NewConnection";
    public const string TaskStatusPending = "Pending";
    public const string TaskStatusApproved = "Approved";
    public const string TaskStatusRejected = "Rejected";
    public const string TaskStatusCorrectionRequired = "CorrectionRequired";
    public const string ActionWorkflowStarted = "WorkflowStarted";
    public const string ActionApproved = "Approved";
    public const string ActionMoveNext = "MoveNext";
    public const string ActionRejected = "Rejected";
    public const string ActionCorrectionRequired = "CorrectionRequired";
    public const string ActionStageAssigned = "StageAssigned";
    public const string ActionFinalConsumerCreated = "FinalConsumerCreated";
    public const string StatusFinalConsumerCreated = "FinalConsumerCreated";

    private readonly ApplicationDbContext _db;
    private readonly INewConnectionFinalizationService _finalizationService;

    public WorkflowService(ApplicationDbContext db, INewConnectionFinalizationService finalizationService)
    {
        _db = db;
        _finalizationService = finalizationService;
    }

    public async Task<long?> StartWorkflowAsync(
        string applicationType,
        long applicationId,
        string applicationNo,
        string currentStatus,
        int? actorUserId,
        string? actorName,
        string? actorRole,
        CancellationToken ct = default)
    {
        var existing = await _db.ApplicationWorkflowInstances
            .FirstOrDefaultAsync(x => x.ApplicationType == applicationType
                && x.ApplicationId == applicationId
                && !x.IsDeleted, ct);
        if (existing is not null)
        {
            await EnsureSingleCurrentPendingTaskAsync(existing, ct);
            return existing.Id;
        }

        var workflow = await _db.WorkflowMasters
            .Include(x => x.Stages.Where(s => s.IsActive && !s.IsDeleted))
            .Where(x => x.ApplicationType == applicationType && x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        var firstStage = workflow?.Stages.OrderBy(x => x.StageOrder).FirstOrDefault();
        if (workflow is null || firstStage is null)
        {
            _db.NotificationLogs.Add(new NotificationLog
            {
                ApplicationId = applicationId,
                ApplicationNo = applicationNo,
                Channel = "InApp",
                Recipient = "Admin",
                Message = $"No active workflow configured for {applicationType}.",
                Status = "Skipped",
                CreatedOn = DateTime.Now
            });
            await _db.SaveChangesAsync(ct);
            return null;
        }

        var now = DateTime.Now;
        var instance = new ApplicationWorkflowInstance
        {
            ApplicationId = applicationId,
            ApplicationNo = applicationNo,
            ApplicationType = applicationType,
            WorkflowId = workflow.Id,
            CurrentStageId = firstStage.Id,
            CurrentStatus = currentStatus,
            StartedOn = now,
            IsActive = true,
            IsDeleted = false
        };
        _db.ApplicationWorkflowInstances.Add(instance);
        await _db.SaveChangesAsync(ct);

        _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
        {
            WorkflowInstanceId = instance.Id,
            ApplicationId = applicationId,
            ApplicationNo = applicationNo,
            StageId = firstStage.Id,
            AssignedDepartmentId = firstStage.DepartmentId,
            AssignedRoleId = firstStage.ApproverRoleId,
            AssignedUserId = firstStage.ApproverUserId,
            Status = TaskStatusPending,
            AssignedOn = now,
            IsActive = true,
            IsDeleted = false
        });

        _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            ApplicationId = applicationId,
            ApplicationNo = applicationNo,
            StageId = firstStage.Id,
            FromStatus = null,
            ToStatus = currentStatus,
            Action = ActionWorkflowStarted,
            Remarks = $"Assigned to {firstStage.StageName}.",
            ActionBy = actorUserId,
            ActionByName = actorName,
            ActionByRole = actorRole,
            ActionOn = now
        });

        _db.NotificationLogs.Add(new NotificationLog
        {
            ApplicationId = applicationId,
            ApplicationNo = applicationNo,
            WorkflowInstanceId = instance.Id,
            StageId = firstStage.Id,
            Channel = "InApp",
            Recipient = firstStage.ApproverUserId?.ToString()
                ?? firstStage.ApproverRoleId?.ToString()
                ?? firstStage.DepartmentId?.ToString()
                ?? "Unassigned",
            Message = $"Application {applicationNo} assigned to {firstStage.StageName}.",
            Status = "PendingIntegration",
            CreatedOn = now
        });

        await _db.SaveChangesAsync(ct);
        return instance.Id;
    }

    public async Task ProcessActionAsync(WorkflowActionRequest request, CancellationToken ct = default)
    {
        var normalizedAction = NormalizeAction(request.Action);
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            var now = DateTime.Now;
            await RepairSequentialWorkflowTasksAsync(ct);

            var task = await _db.ApplicationWorkflowTasks
                .Include(x => x.WorkflowInstance)
                .Include(x => x.Stage)
                .FirstOrDefaultAsync(x => x.Id == request.TaskId
                    && !x.IsDeleted
                    && x.IsActive
                    && x.Status == TaskStatusPending
                    && x.WorkflowInstance.IsActive
                    && !x.WorkflowInstance.IsDeleted
                    && x.WorkflowInstance.CurrentStageId == x.StageId, ct)
                ?? throw new InvalidOperationException("Pending workflow task not found.");

            if (!CanAct(task, request))
                throw new InvalidOperationException("This application is not assigned to you for action.");

            ValidateApprovalTypeAssignment(task.Stage);
            ValidateStagePermission(task.Stage, normalizedAction);

            var application = task.WorkflowInstance.ApplicationType == ApplicationTypeNewConnection
                ? await _db.NewConnectionApplications.FirstOrDefaultAsync(x => x.Id == task.ApplicationId && !x.IsDeleted, ct)
                : null;

            if (task.WorkflowInstance.ApplicationType == ApplicationTypeNewConnection && application is null)
                throw new InvalidOperationException("New connection application not found.");

            var fromStatus = task.WorkflowInstance.CurrentStatus;
            var nextStatus = ResolveApplicationStatus(normalizedAction, task.Stage);
            var historyAction = normalizedAction;
            if (task.WorkflowInstance.CurrentStageId != task.StageId)
                throw new InvalidOperationException("Previous approval stage is not completed yet.");

            var previousStageIds = await _db.WorkflowStages
                .AsNoTracking()
                .Where(x => x.WorkflowId == task.WorkflowInstance.WorkflowId
                    && x.IsActive
                    && !x.IsDeleted
                    && x.StageOrder < task.Stage.StageOrder)
                .Select(x => x.Id)
                .ToListAsync(ct);
            if (previousStageIds.Count > 0)
            {
                var completedPreviousStageIds = await _db.ApplicationWorkflowTasks
                    .AsNoTracking()
                    .Where(x => x.WorkflowInstanceId == task.WorkflowInstanceId
                        && previousStageIds.Contains(x.StageId)
                        && !x.IsDeleted
                        && x.Status == TaskStatusApproved)
                    .Select(x => x.StageId)
                    .Distinct()
                    .ToListAsync(ct);

                if (completedPreviousStageIds.Count != previousStageIds.Count)
                    throw new InvalidOperationException("Previous approval stage is not completed yet.");
            }

            var stalePendingTasks = await _db.ApplicationWorkflowTasks
                .Where(x => x.WorkflowInstanceId == task.WorkflowInstanceId
                    && x.Id != task.Id
                    && !x.IsDeleted
                    && x.Status == TaskStatusPending)
                .ToListAsync(ct);
            foreach (var staleTask in stalePendingTasks)
            {
                staleTask.IsActive = false;
                staleTask.Status = "Skipped";
                staleTask.ActionOn = now;
                staleTask.Remarks = "Closed automatically because workflow is sequential and only the current stage can remain pending.";
            }

            task.Status = ResolveTaskStatus(normalizedAction);
            task.ActionOn = now;
            task.Remarks = Normalize(request.Remarks);
            task.IsActive = false;

            WorkflowStage? nextStage = null;
            if (normalizedAction == ActionMoveNext || (normalizedAction == ActionApproved && !task.Stage.IsFinalStage))
            {
                nextStage = await _db.WorkflowStages
                    .Where(x => x.WorkflowId == task.WorkflowInstance.WorkflowId
                        && x.IsActive
                        && !x.IsDeleted
                        && x.StageOrder > task.Stage.StageOrder)
                    .OrderBy(x => x.StageOrder)
                    .FirstOrDefaultAsync(ct);

                if (normalizedAction == ActionMoveNext && nextStage is null)
                    throw new InvalidOperationException("No next workflow stage is configured.");
            }

            if ((normalizedAction == ActionApproved || normalizedAction == ActionMoveNext) && nextStage is not null)
            {
                nextStatus = "UnderReview";
                task.WorkflowInstance.CurrentStageId = nextStage.Id;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId = task.ApplicationId,
                    ApplicationNo = task.ApplicationNo,
                    StageId = nextStage.Id,
                    AssignedDepartmentId = nextStage.DepartmentId,
                    AssignedRoleId = nextStage.ApproverRoleId,
                    AssignedUserId = nextStage.ApproverUserId,
                    Status = TaskStatusPending,
                    AssignedOn = now,
                    IsActive = true,
                    IsDeleted = false
                });
            }
            else
            {
                task.WorkflowInstance.CurrentStatus = nextStatus;
                if (normalizedAction is ActionApproved or ActionRejected or ActionCorrectionRequired)
                {
                    task.WorkflowInstance.CompletedOn = normalizedAction == ActionApproved || normalizedAction == ActionRejected ? now : null;
                    task.WorkflowInstance.IsActive = normalizedAction == ActionCorrectionRequired;
                }
            }

            if (application is not null)
            {
                var oldApplicationStatus = application.ApplicationStatus;
                application.ApplicationStatus = nextStatus;
                application.UpdatedBy = request.ActorUserId;
                application.UpdatedOn = now;
                if (normalizedAction == ActionApproved && nextStage is null)
                {
                    application.ApprovedBy = request.ActorUserId;
                    application.ApprovedOn = now;
                }
                else if (normalizedAction == ActionRejected)
                {
                    application.RejectedBy = request.ActorUserId;
                    application.RejectedOn = now;
                    application.RejectionReason = Normalize(request.Remarks);
                }

                _db.NewConnectionApprovalHistories.Add(new NewConnectionApprovalHistory
                {
                    ApplicationId = application.Id,
                    ApplicationNo = application.ApplicationNo,
                    FromStatus = oldApplicationStatus,
                    ToStatus = application.ApplicationStatus,
                    Action = historyAction,
                    Remarks = Normalize(request.Remarks),
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now,
                    IpAddress = Normalize(request.IpAddress),
                    UserAgent = Normalize(request.UserAgent),
                    IsActive = true,
                    IsDeleted = false
                });
            }

            var workflowActionToStatus = nextStatus;
            string? finalConsumerNo = null;
            if (application is not null && normalizedAction == ActionApproved && nextStage is null)
            {
                finalConsumerNo = await _finalizationService.CreateFinalConsumerAsync(
                    application.Id,
                    request.ActorUserId,
                    request.ActorName,
                    request.ActorRole,
                    request.IpAddress,
                    request.UserAgent,
                    ct);

                nextStatus = StatusFinalConsumerCreated;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
            }

            _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                ApplicationId = task.ApplicationId,
                ApplicationNo = task.ApplicationNo,
                StageId = task.StageId,
                FromStatus = fromStatus,
                ToStatus = workflowActionToStatus,
                Action = historyAction,
                Remarks = Normalize(request.Remarks),
                ActionBy = request.ActorUserId,
                ActionByName = Normalize(request.ActorName),
                ActionByRole = Normalize(request.ActorRole),
                ActionOn = now
            });

            if (!string.IsNullOrWhiteSpace(finalConsumerNo))
            {
                _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId = task.ApplicationId,
                    ApplicationNo = task.ApplicationNo,
                    StageId = task.StageId,
                    FromStatus = "Approved",
                    ToStatus = StatusFinalConsumerCreated,
                    Action = ActionFinalConsumerCreated,
                    Remarks = $"Consumer number generated: {finalConsumerNo}",
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now
                });
            }

            if (nextStage is not null)
            {
                _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId = task.ApplicationId,
                    ApplicationNo = task.ApplicationNo,
                    StageId = nextStage.Id,
                    FromStatus = nextStatus,
                    ToStatus = nextStatus,
                    Action = ActionStageAssigned,
                    Remarks = $"Assigned to {nextStage.StageName}.",
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now
                });
            }

            await QueueConfiguredNotificationsAsync(task, normalizedAction, nextStage, now, ct);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });
    }

    public async Task RepairSequentialWorkflowTasksAsync(CancellationToken ct = default)
    {
        var instanceIds = await _db.ApplicationWorkflowTasks
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == TaskStatusPending)
            .GroupBy(x => x.WorkflowInstanceId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToListAsync(ct);

        foreach (var instanceId in instanceIds)
        {
            var instance = await _db.ApplicationWorkflowInstances
                .FirstOrDefaultAsync(x => x.Id == instanceId && !x.IsDeleted, ct);
            if (instance is not null)
                await EnsureSingleCurrentPendingTaskAsync(instance, ct);
        }
    }

    private async Task QueueConfiguredNotificationsAsync(ApplicationWorkflowTask task, string action, WorkflowStage? nextStage, DateTime now, CancellationToken ct)
    {
        var eventTypes = new List<string> { action };
        if (nextStage is not null)
            eventTypes.Add(ActionStageAssigned);
        if (action == ActionApproved && nextStage is null)
            eventTypes.Add("FinalApproved");

        var stageIds = new[] { task.StageId, nextStage?.Id }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        var configs = await _db.WorkflowStageNotifications
            .AsNoTracking()
            .Where(x => stageIds.Contains(x.WorkflowStageId)
                && eventTypes.Contains(x.EventType)
                && x.IsActive
                && !x.IsDeleted)
            .ToListAsync(ct);

        foreach (var config in configs)
        {
            var recipient = ResolveNotificationRecipient(config.WorkflowStageId == nextStage?.Id ? nextStage : task.Stage);
            AddNotificationIfEnabled(config.SendEmail, "Email", recipient, task, config.EventType, now);
            AddNotificationIfEnabled(config.SendSms, "SMS", recipient, task, config.EventType, now);
            AddNotificationIfEnabled(config.SendWhatsApp, "WhatsApp", recipient, task, config.EventType, now);
            AddNotificationIfEnabled(config.SendInAppNotification, "InApp", recipient, task, config.EventType, now);
        }
    }

    private void AddNotificationIfEnabled(bool enabled, string channel, string recipient, ApplicationWorkflowTask task, string eventType, DateTime now)
    {
        if (!enabled)
            return;

        _db.NotificationLogs.Add(new NotificationLog
        {
            ApplicationId = task.ApplicationId,
            ApplicationNo = task.ApplicationNo,
            WorkflowInstanceId = task.WorkflowInstanceId,
            StageId = task.StageId,
            Channel = channel,
            Recipient = recipient,
            Message = $"{eventType}: Application {task.ApplicationNo}.",
            Status = "PendingIntegration",
            CreatedOn = now
        });
    }

    private static string ResolveNotificationRecipient(WorkflowStage stage)
        => stage.ApproverUserId?.ToString()
            ?? stage.ApproverRoleId?.ToString()
            ?? stage.DepartmentId?.ToString()
            ?? "Unassigned";

    private async Task EnsureSingleCurrentPendingTaskAsync(ApplicationWorkflowInstance instance, CancellationToken ct)
    {
        var pendingTasks = await _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Where(x => x.WorkflowInstanceId == instance.Id
                && !x.IsDeleted
                && x.Status == TaskStatusPending)
            .OrderBy(x => x.Stage.StageOrder)
            .ThenBy(x => x.AssignedOn)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        if (pendingTasks.Count == 0)
            return;

        var currentTask = pendingTasks.First();

        if (instance.CurrentStageId != currentTask.StageId)
            instance.CurrentStageId = currentTask.StageId;

        foreach (var pendingTask in pendingTasks)
        {
            if (pendingTask.Id == currentTask.Id)
            {
                pendingTask.IsActive = true;
                continue;
            }

            pendingTask.IsActive = false;
            pendingTask.Status = "Skipped";
            pendingTask.ActionOn ??= DateTime.Now;
            pendingTask.Remarks ??= "Closed automatically because workflow is sequential and only the current stage can remain pending.";
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool CanAct(ApplicationWorkflowTask task, WorkflowActionRequest request)
    {
        if (task.AssignedUserId.HasValue)
            return request.ActorUserId.HasValue && task.AssignedUserId == request.ActorUserId;

        if (task.AssignedDepartmentId.HasValue && task.AssignedRoleId.HasValue)
        {
            return request.ActorRoleId.HasValue
                && task.AssignedRoleId == request.ActorRoleId
                && request.ActorDepartmentIds.Contains(task.AssignedDepartmentId.Value);
        }

        if (task.AssignedDepartmentId.HasValue)
            return request.ActorDepartmentIds.Contains(task.AssignedDepartmentId.Value);

        if (task.AssignedRoleId.HasValue)
            return request.ActorRoleId.HasValue && task.AssignedRoleId == request.ActorRoleId;

        return false;
    }

    private static void ValidateStagePermission(WorkflowStage stage, string action)
    {
        var allowed = action switch
        {
            ActionApproved or ActionMoveNext => stage.CanApprove,
            ActionRejected => stage.CanReject,
            ActionCorrectionRequired => stage.CanSendCorrection,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"The current workflow stage does not allow {action}.");
    }

    private static void ValidateApprovalTypeAssignment(WorkflowStage stage)
    {
        if (string.Equals(stage.ApprovalType, "AllApprovers", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("All Approvers workflow is not implemented yet.");

        if (string.Equals(stage.ApprovalType, "SpecificUser", StringComparison.OrdinalIgnoreCase)
            && !stage.ApproverUserId.HasValue)
            throw new InvalidOperationException("Specific user is required for this workflow stage.");

        if (string.Equals(stage.ApprovalType, "DepartmentRole", StringComparison.OrdinalIgnoreCase)
            && !stage.DepartmentId.HasValue
            && !stage.ApproverRoleId.HasValue)
            throw new InvalidOperationException("Department or role is required for this workflow stage.");
    }

    private static string ResolveTaskStatus(string action)
        => action switch
        {
            ActionApproved or ActionMoveNext => TaskStatusApproved,
            ActionRejected => TaskStatusRejected,
            ActionCorrectionRequired => TaskStatusCorrectionRequired,
            _ => throw new InvalidOperationException("Unsupported workflow action.")
        };

    private static string ResolveApplicationStatus(string action, WorkflowStage stage)
        => action switch
        {
            ActionApproved => stage.IsFinalStage ? "Approved" : "UnderReview",
            ActionMoveNext => "UnderReview",
            ActionRejected => "Rejected",
            ActionCorrectionRequired => "CorrectionRequired",
            _ => throw new InvalidOperationException("Unsupported workflow action.")
        };

    private static string NormalizeAction(string action)
    {
        var normalized = action?.Trim();
        return normalized switch
        {
            "Approve" or "Approved" => ActionApproved,
            "MoveNext" or "MoveToNext" or "ForwardToNext" => ActionMoveNext,
            "Reject" or "Rejected" => ActionRejected,
            _ => throw new InvalidOperationException("Unsupported workflow action.")
        };
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
