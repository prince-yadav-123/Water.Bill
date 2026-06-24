using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.DTOs.Workflow;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    public const string ApplicationTypeNewConnection = "NewConnection";
    public const string ApplicationTypeNdc = "NDC";
    public const string ApplicationTypeNameTransfer = "NameTransfer";
    public const string ApplicationTypeConnectionChange = "ConnectionChange";
    public const string TaskStatusPending = "Pending";
    public const string TaskStatusApproved = "Approved";
    public const string TaskStatusRejected = "Rejected";
    public const string TaskStatusCorrectionRequired = "CorrectionRequired";
    public const string TaskStatusForwarded = "Forwarded";
    public const string TaskStatusSentBackToApplicant = "SentBackToApplicant";
    public const string TaskStatusSentBackToPrevious = "SentBackToPrevious";

    public const string ActionWorkflowStarted = "WorkflowStarted";
    public const string ActionApproved = "Approved";           // legacy compat
    public const string ActionMoveNext = "MoveNext";           // legacy compat
    public const string ActionRejected = "Rejected";
    public const string ActionCorrectionRequired = "CorrectionRequired"; // legacy compat
    public const string ActionStageAssigned = "StageAssigned";
    public const string ActionFinalConsumerCreated = "FinalConsumerCreated";
    public const string StatusFinalConsumerCreated = "FinalConsumerCreated";

    // ── New action constants ──────────────────────────────────────────────────
    public const string ActionAcceptMoveNext = "AcceptMoveNext";
    public const string ActionFinalApproval = "FinalApproval";
    public const string ActionForwardToUser = "ForwardToUser";
    public const string ActionSendBackToApplicant = "SendBackToApplicant";
    public const string ActionSendBackToPrevious = "SendBackToPrevious";
    public const string StatusSentBackToApplicant = "SentBackToApplicant";

    private readonly ApplicationDbContext _db;
    private readonly INewConnectionFinalizationService _finalizationService;
    private readonly ICommunicationService _communicationService;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IErrorLogService _errorLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkflowService(
        ApplicationDbContext db,
        INewConnectionFinalizationService finalizationService,
        ICommunicationService communicationService,
        ITemplateRenderer templateRenderer,
        IErrorLogService errorLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _finalizationService = finalizationService;
        _communicationService = communicationService;
        _templateRenderer = templateRenderer;
        _errorLogService = errorLogService;
        _httpContextAccessor = httpContextAccessor;
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
            CurrentStatusCode = WorkflowCodes.InstanceStatus.UnderReview,
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
            AssignedDepartmentId = null,
            AssignedRoleId = firstStage.ApproverRoleId,
            AssignedUserId = firstStage.ApproverUserId,
            StatusCode = WorkflowCodes.TaskStatus.Pending,
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
            FromStatusCode = null,
            ToStatusCode = WorkflowCodes.InstanceStatus.UnderReview,
            FromStatus = null,
            ToStatus = currentStatus,
            ActionCode = WorkflowCodes.ActionCode.WorkflowStarted,
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
                ?? "Unassigned",
            Message = $"Application {applicationNo} assigned to {firstStage.StageName}.",
            Status = "PendingIntegration",
            CreatedOn = now
        });

        // In-App notification to Stage 1 assignees
        await SendStageAssignmentInAppAsync(instance.Id, applicationId, applicationNo, firstStage, now, ct);

        await _db.SaveChangesAsync(ct);
        return instance.Id;
    }

    public async Task ProcessActionAsync(WorkflowActionRequest request, CancellationToken ct = default)
    {
        var normalizedAction = NormalizeAction(request.Action);

        // ── Forward to Specific User is disabled for this phase ──────────────
        if (normalizedAction == ActionForwardToUser)
            throw new InvalidOperationException("Forward to Specific User is not available in the current phase.");

        // ── Server-side validation (always enforced, not UI-only) ────────────
        if (string.IsNullOrWhiteSpace(request.Remarks)
            && normalizedAction is ActionRejected or ActionSendBackToApplicant or ActionSendBackToPrevious
                                or ActionAcceptMoveNext or ActionFinalApproval)
            throw new InvalidOperationException("Please enter remarks before taking this action.");
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
                    && (x.StatusCode == WorkflowCodes.TaskStatus.Pending || x.Status == TaskStatusPending)
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
            var ndcApplication = task.WorkflowInstance.ApplicationType == ApplicationTypeNdc
                ? await _db.ConsumerApplyNdcs.FirstOrDefaultAsync(x => x.AutoId == task.ApplicationId, ct)
                : null;
            var legacyApplication = IsLegacyConsumerChange(task.WorkflowInstance.ApplicationType)
                ? await _db.MasterApplicationDetails.FirstOrDefaultAsync(x =>
                    x.ApplicationId == task.ApplicationNo
                    && x.AppType == ResolveLegacyAppType(task.WorkflowInstance.ApplicationType), ct)
                : null;

            if (task.WorkflowInstance.ApplicationType == ApplicationTypeNewConnection && application is null)
                throw new InvalidOperationException("New connection application not found.");
            if (task.WorkflowInstance.ApplicationType == ApplicationTypeNdc && ndcApplication is null)
                throw new InvalidOperationException("NDC application not found.");
            if (IsLegacyConsumerChange(task.WorkflowInstance.ApplicationType) && legacyApplication is null)
                throw new InvalidOperationException("Consumer service request not found.");

            var applicantRecipient = application is null ? null : BuildApplicantRecipient(application);
            string? applicantNotificationPurpose = null;
            string? applicantNotificationStage = null;
            string? applicantNotificationRemarks = null;
            string? applicantNotificationConsumerNo = null;

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
                        && (x.StatusCode == WorkflowCodes.TaskStatus.Approved || x.Status == TaskStatusApproved))
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
                    && (x.StatusCode == WorkflowCodes.TaskStatus.Pending || x.Status == TaskStatusPending))
                .ToListAsync(ct);
            foreach (var staleTask in stalePendingTasks)
            {
                staleTask.IsActive = false;
                staleTask.StatusCode = WorkflowCodes.TaskStatus.Skipped;
                staleTask.Status = "Skipped";
                staleTask.ActionOn = now;
                staleTask.Remarks = "Closed automatically because workflow is sequential and only the current stage can remain pending.";
            }

            task.StatusCode = ResolveTaskStatusCode(normalizedAction);
            task.Status = ResolveTaskStatus(normalizedAction);
            task.ActionOn = now;
            task.Remarks = Normalize(request.Remarks);
            task.IsActive = false;

            WorkflowStage? nextStage = null;
            WorkflowStage? previousStage = null;

            // ── AcceptMoveNext / legacy MoveNext / legacy Approve (non-final) ──
            var isMovingNext = normalizedAction is ActionAcceptMoveNext or ActionMoveNext
                || (normalizedAction == ActionApproved && !task.Stage.IsFinalStage);

            // ── FinalApproval / legacy Approve (final) ──
            var isFinalizing = normalizedAction is ActionFinalApproval
                || (normalizedAction == ActionApproved && task.Stage.IsFinalStage);

            if (isMovingNext)
            {
                nextStage = await _db.WorkflowStages
                    .Where(x => x.WorkflowId == task.WorkflowInstance.WorkflowId
                        && x.IsActive && !x.IsDeleted
                        && x.StageOrder > task.Stage.StageOrder)
                    .OrderBy(x => x.StageOrder)
                    .FirstOrDefaultAsync(ct);

                if (normalizedAction == ActionAcceptMoveNext && nextStage is null)
                    throw new InvalidOperationException("No next workflow stage is configured. Use 'Final Approval' for the last stage.");
            }
            else if (normalizedAction == ActionSendBackToPrevious)
            {
                previousStage = await _db.WorkflowStages
                    .Where(x => x.WorkflowId == task.WorkflowInstance.WorkflowId
                        && x.IsActive && !x.IsDeleted
                        && x.StageOrder < task.Stage.StageOrder)
                    .OrderByDescending(x => x.StageOrder)
                    .FirstOrDefaultAsync(ct);

                if (previousStage is null)
                    throw new InvalidOperationException("No previous stage exists. Cannot send back on the first stage.");
            }

            // ── Update instance and create follow-up tasks ────────────────────

            if (isMovingNext && nextStage is not null)
            {
                // Move to next configured stage
                nextStatus = "UnderReview";
                task.WorkflowInstance.CurrentStageId = nextStage.Id;
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.UnderReview;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                if (application is not null)
                {
                    applicantNotificationPurpose = CommunicationPurposes.NewConnectionForwarded;
                    applicantNotificationStage = nextStage.StageName;
                    applicantNotificationRemarks = Normalize(request.Remarks);
                }
                _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId      = task.ApplicationId,
                    ApplicationNo      = task.ApplicationNo,
                    StageId            = nextStage.Id,
                    AssignedDepartmentId = null,
                    AssignedRoleId     = nextStage.ApproverRoleId,
                    AssignedUserId     = nextStage.ApproverUserId,
                    StatusCode = WorkflowCodes.TaskStatus.Pending,
                    Status    = TaskStatusPending,
                    AssignedOn = now,
                    IsActive  = true,
                    IsDeleted = false
                });

                // InApp notification to next stage assignees
                await SendStageAssignmentInAppAsync(
                    task.WorkflowInstanceId, task.ApplicationId, task.ApplicationNo, nextStage, now, ct);
            }
            else if (isMovingNext && nextStage is null)
            {
                // No more stages — treat as implicit final approval
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.Completed;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
            }
            else if (isFinalizing)
            {
                // Final approval — workflow completes after finalization
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.Approved;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
            }
            else if (normalizedAction == ActionRejected)
            {
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.Rejected;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
                if (application is not null)
                {
                    applicantNotificationPurpose = CommunicationPurposes.NewConnectionRejected;
                    applicantNotificationStage = task.Stage.StageName;
                    applicantNotificationRemarks = Normalize(request.Remarks);
                }
            }
            else if (normalizedAction == ActionForwardToUser)
            {
                // Forward to a specific user — same stage, different assignee
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.UnderReview;
                task.WorkflowInstance.CurrentStatus = "UnderReview";
                _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId      = task.ApplicationId,
                    ApplicationNo      = task.ApplicationNo,
                    StageId            = task.StageId,        // same stage
                    AssignedDepartmentId = null,
                    AssignedRoleId     = null,
                    AssignedUserId     = request.ForwardToUserId,
                    StatusCode = WorkflowCodes.TaskStatus.Pending,
                    Status    = TaskStatusPending,
                    AssignedOn = now,
                    IsActive  = true,
                    IsDeleted = false
                });
            }
            else if (normalizedAction is ActionSendBackToApplicant or ActionCorrectionRequired)
            {
                // Return to applicant — workflow stays active, application needs correction
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.SentBackToApplicant;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.IsActive = true;
                task.WorkflowInstance.CompletedOn = null;
                if (application is not null)
                {
                    applicantNotificationPurpose = CommunicationPurposes.NewConnectionSentBack;
                    applicantNotificationStage = task.Stage.StageName;
                    applicantNotificationRemarks = Normalize(request.Remarks);
                }
            }
            else if (normalizedAction == ActionSendBackToPrevious && previousStage is not null)
            {
                // Send back to previous stage
                task.WorkflowInstance.CurrentStageId = previousStage.Id;
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.SentBackToPreviousStage;
                task.WorkflowInstance.CurrentStatus = "UnderReview";
                _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId      = task.ApplicationId,
                    ApplicationNo      = task.ApplicationNo,
                    StageId            = previousStage.Id,
                    AssignedDepartmentId = null,
                    AssignedRoleId     = previousStage.ApproverRoleId,
                    AssignedUserId     = previousStage.ApproverUserId,
                    StatusCode = WorkflowCodes.TaskStatus.Pending,
                    Status    = TaskStatusPending,
                    AssignedOn = now,
                    IsActive  = true,
                    IsDeleted = false
                });
                nextStage = previousStage; // reuse for history/notification below
            }
            else
            {
                task.WorkflowInstance.CurrentStatus = nextStatus;
            }

            var effectiveNextStage = normalizedAction == ActionSendBackToPrevious ? null : nextStage;

            // Compute here (before application status blocks) so it's in scope everywhere below
            var isFinalApprovalAction = isFinalizing
                || (normalizedAction is ActionAcceptMoveNext or ActionMoveNext && nextStage is null);

            if (application is not null)
            {
                var oldApplicationStatus = application.ApplicationStatus;
                application.ApplicationStatus = nextStatus;
                application.UpdatedBy = request.ActorUserId;
                application.UpdatedOn = now;
                if (isFinalApprovalAction)
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
            else if (ndcApplication is not null)
            {
                ApplyNdcWorkflowStatus(ndcApplication, nextStatus, normalizedAction, request.ActorUserId, now, request.Remarks);
            }
            else if (legacyApplication is not null)
            {
                ApplyLegacyWorkflowStatus(legacyApplication, nextStatus, normalizedAction, request.Remarks, now);
                AddMasterApplicationHistory(
                    legacyApplication.ApplicationId,
                    legacyApplication.DivName,
                    $"{historyAction}. {Normalize(request.Remarks)}",
                    normalizedAction == ActionRejected ? "3" : "1");
            }

            var workflowActionToStatus = nextStatus;
            string? finalConsumerNo = null;

            if (application is not null && isFinalApprovalAction)
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
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.FinalConsumerCreated;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
                applicantNotificationPurpose = CommunicationPurposes.FinalConsumerCreated;
                applicantNotificationStage = task.Stage.StageName;
                applicantNotificationConsumerNo = finalConsumerNo;
            }
            else if (ndcApplication is not null && isFinalApprovalAction)
            {
                ndcApplication.Status = "A";
                ndcApplication.FinalStatus = "A";
                ndcApplication.CurrentStatus = "Approved";
                ndcApplication.CompletedDate = now;
                ndcApplication.LastUpdatedBy = request.ActorUserId;
                ndcApplication.LastUpdatedOn = now;
                ndcApplication.CertificateUrl = $"/NdcCertificates/Print/{ndcApplication.AutoId}";
                nextStatus = "Approved";
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.Approved;
                task.WorkflowInstance.CurrentStatus = nextStatus;
                task.WorkflowInstance.CompletedOn = now;
                task.WorkflowInstance.IsActive = false;
            }
            else if (legacyApplication is not null && isFinalApprovalAction)
            {
                if (task.WorkflowInstance.ApplicationType == ApplicationTypeNameTransfer)
                    await FinalizeNameTransferAsync(legacyApplication, request, now, ct);
                else if (task.WorkflowInstance.ApplicationType == ApplicationTypeConnectionChange)
                    await FinalizeConnectionChangeAsync(legacyApplication, request, now, ct);

                nextStatus = "Approved";
                task.WorkflowInstance.CurrentStatusCode = WorkflowCodes.InstanceStatus.Approved;
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
                FromStatusCode = ResolveInstanceStatusCode(fromStatus),
                FromStatus = fromStatus,
                ToStatusCode = ResolveInstanceStatusCode(workflowActionToStatus),
                ToStatus = workflowActionToStatus,
                ActionCode = ResolveActionCode(normalizedAction),
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
                    FromStatusCode = WorkflowCodes.InstanceStatus.Approved,
                    FromStatus = "Approved",
                    ToStatusCode = WorkflowCodes.InstanceStatus.FinalConsumerCreated,
                    ToStatus = StatusFinalConsumerCreated,
                    ActionCode = WorkflowCodes.ActionCode.FinalConsumerCreated,
                    Action = ActionFinalConsumerCreated,
                    Remarks = $"Consumer number generated: {finalConsumerNo}",
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now
                });
            }

            // Determine which stage got a new pending task for history/notifications
            var assignedStage = normalizedAction == ActionForwardToUser
                ? null   // forwarded to user at same stage — no separate stage history
                : nextStage ?? (normalizedAction == ActionSendBackToPrevious ? previousStage : null);

            if (assignedStage is not null)
            {
                var assignedLabel = normalizedAction == ActionSendBackToPrevious
                    ? $"Sent back to {assignedStage.StageName}."
                    : $"Assigned to {assignedStage.StageName}.";

                _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId = task.ApplicationId,
                    ApplicationNo = task.ApplicationNo,
                    StageId = assignedStage.Id,
                    FromStatusCode = ResolveInstanceStatusCode(nextStatus),
                    ToStatusCode = ResolveInstanceStatusCode(nextStatus),
                    FromStatus = nextStatus,
                    ToStatus = nextStatus,
                    ActionCode = WorkflowCodes.ActionCode.StageAssigned,
                    Action = ActionStageAssigned,
                    Remarks = assignedLabel,
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now
                });
            }

            // ForwardToUser — history entry with target user
            if (normalizedAction == ActionForwardToUser && request.ForwardToUserId.HasValue)
            {
                var targetUserName = await _db.Appusers.AsNoTracking()
                    .Where(x => x.Id == request.ForwardToUserId.Value)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync(ct) ?? request.ForwardToUserId.ToString();

                _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    ApplicationId = task.ApplicationId,
                    ApplicationNo = task.ApplicationNo,
                    StageId = task.StageId,
                    FromStatusCode = ResolveInstanceStatusCode(nextStatus),
                    ToStatusCode = ResolveInstanceStatusCode(nextStatus),
                    FromStatus = nextStatus,
                    ToStatus = nextStatus,
                    ActionCode = WorkflowCodes.ActionCode.StageAssigned,
                    Action = ActionStageAssigned,
                    Remarks = $"Forwarded to {targetUserName}.",
                    ActionBy = request.ActorUserId,
                    ActionByName = Normalize(request.ActorName),
                    ActionByRole = Normalize(request.ActorRole),
                    ActionOn = now
                });

                // In-App notification to the forwarded-to user
                var notification = await BuildTemplatedInAppNotificationAsync(
                    purposeKey: CommunicationPurposes.WorkflowForward,
                    userType: "Internal",
                    userId: request.ForwardToUserId.Value,
                    values: BuildWorkflowTemplateValues(task.ApplicationNo, nextStage?.StageName, request.Remarks, now),
                    referenceType: "WorkflowTask",
                    referenceId: task.Id.ToString(),
                    referenceNo: task.ApplicationNo,
                    redirectUrl: BuildInternalWorkflowTaskUrl(task.Id),
                    createdAt: now,
                    ct: ct);
                if (notification is not null)
                    _db.InAppNotifications.Add(notification);
            }

            // SendBackToApplicant — In-App notification to the consumer
            if (normalizedAction is ActionSendBackToApplicant or ActionCorrectionRequired)
            {
                var consumerUserId = await ResolveConsumerUserIdAsync(task, ct);
                if (consumerUserId > 0)
                {
                    var notification = await BuildTemplatedInAppNotificationAsync(
                        purposeKey: CommunicationPurposes.WorkflowSentBack,
                        userType: "Consumer",
                        userId: consumerUserId,
                        values: BuildWorkflowTemplateValues(task.ApplicationNo, task.Stage?.StageName, request.Remarks, now),
                        referenceType: "WorkflowTask",
                        referenceId: task.Id.ToString(),
                        referenceNo: task.ApplicationNo,
                        redirectUrl: BuildConsumerApplicationUrl(task.WorkflowInstance.ApplicationType, task.ApplicationId),
                        createdAt: now,
                        ct: ct);
                    if (notification is not null)
                        _db.InAppNotifications.Add(notification);
                }
            }

            // SendBackToPrevious — In-App notification to previous stage user
            if (normalizedAction == ActionSendBackToPrevious && previousStage?.ApproverUserId.HasValue == true)
            {
                var notification = await BuildTemplatedInAppNotificationAsync(
                    purposeKey: CommunicationPurposes.WorkflowSentBackPrevious,
                    userType: "Internal",
                    userId: previousStage.ApproverUserId.Value,
                    values: BuildWorkflowTemplateValues(task.ApplicationNo, previousStage.StageName, request.Remarks, now),
                    referenceType: "WorkflowTask",
                    referenceId: task.Id.ToString(),
                    referenceNo: task.ApplicationNo,
                    redirectUrl: BuildInternalWorkflowInstanceUrl(task.WorkflowInstanceId),
                    createdAt: now,
                    ct: ct);
                if (notification is not null)
                    _db.InAppNotifications.Add(notification);
            }

            await QueueConfiguredNotificationsAsync(task, normalizedAction, effectiveNextStage, now, ct);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            if (application is not null
                && applicantRecipient is not null
                && !string.IsNullOrWhiteSpace(applicantNotificationPurpose))
            {
                var portalUrl = BuildApplicantPortalUrl(application);
                var applicantValues = BuildApplicantTemplateValues(
                    application.ApplicationNo,
                    application.ApplicationStatus,
                    applicantNotificationStage,
                    applicantNotificationRemarks,
                    request.ActorName,
                    applicantNotificationConsumerNo ?? application.FinalConsumerNo,
                    portalUrl,
                    now);

                await SendApplicantCommunicationAsync(
                    applicantNotificationPurpose,
                    applicantRecipient,
                    applicantValues,
                    referenceType: "NewConnectionApplication",
                    referenceId: application.Id.ToString(),
                    referenceNo: application.ApplicationNo,
                    redirectUrl: portalUrl,
                    ct: ct);
            }
        });
    }

    public async Task RepairSequentialWorkflowTasksAsync(CancellationToken ct = default)
    {
        var instanceIds = await _db.ApplicationWorkflowTasks
            .AsNoTracking()
            .Where(x => !x.IsDeleted && (x.StatusCode == WorkflowCodes.TaskStatus.Pending || x.Status == TaskStatusPending))
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
            ?? "Unassigned";

    private async Task EnsureSingleCurrentPendingTaskAsync(ApplicationWorkflowInstance instance, CancellationToken ct)
    {
            var pendingTasks = await _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Where(x => x.WorkflowInstanceId == instance.Id
                && !x.IsDeleted
                && (x.StatusCode == WorkflowCodes.TaskStatus.Pending || x.Status == TaskStatusPending))
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
            pendingTask.StatusCode = WorkflowCodes.TaskStatus.Skipped;
            pendingTask.Status = "Skipped";
            pendingTask.ActionOn ??= DateTime.Now;
            pendingTask.Remarks ??= "Closed automatically because workflow is sequential and only the current stage can remain pending.";
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool CanAct(ApplicationWorkflowTask task, WorkflowActionRequest request)
    {
        // Admin/Super Admin can take action on any pending task
        if (request.IsAdmin) return true;

        if (task.AssignedUserId.HasValue)
            return request.ActorUserId.HasValue && task.AssignedUserId == request.ActorUserId;

        if (task.AssignedRoleId.HasValue)
            return request.ActorRoleId.HasValue && task.AssignedRoleId == request.ActorRoleId;

        return true;
    }

    private static void ValidateStagePermission(WorkflowStage stage, string action)
    {
        var allowed = action switch
        {
            ActionAcceptMoveNext or ActionFinalApproval
                                => stage.CanApprove,
            ActionApproved or ActionMoveNext
                                => stage.CanApprove,   // legacy
            ActionRejected      => stage.CanReject,
            ActionForwardToUser => stage.CanForwardToUser,
            ActionSendBackToApplicant or ActionCorrectionRequired
                                => stage.CanSendBackToApplicant || stage.CanSendCorrection,
            ActionSendBackToPrevious
                                => stage.CanSendBackToPrevious,
            _                   => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"The current workflow stage '{stage.StageName}' does not allow the action '{action}'.");
    }

    private static void ValidateApprovalTypeAssignment(WorkflowStage stage)
    {
        if (string.Equals(stage.ApprovalType, "SpecificUser", StringComparison.OrdinalIgnoreCase)
            && !stage.ApproverUserId.HasValue)
            throw new InvalidOperationException("Specific user is required for this workflow stage.");

        if (IsRoleBasedApprovalType(stage.ApprovalType)
            && !stage.ApproverRoleId.HasValue)
            throw new InvalidOperationException("Approver role is required for role-based approval.");
    }

    private static string ResolveTaskStatus(string action)
        => action switch
        {
            ActionAcceptMoveNext or ActionFinalApproval
                or ActionApproved or ActionMoveNext => TaskStatusApproved,
            ActionRejected                          => TaskStatusRejected,
            ActionForwardToUser                     => TaskStatusForwarded,
            ActionSendBackToApplicant
                or ActionCorrectionRequired         => TaskStatusSentBackToApplicant,
            ActionSendBackToPrevious                => TaskStatusSentBackToPrevious,
            _ => throw new InvalidOperationException($"Unsupported workflow action: {action}")
        };

    private static bool IsRoleBasedApprovalType(string? approvalType)
        => string.Equals(approvalType, "DepartmentRole", StringComparison.OrdinalIgnoreCase)
           || string.Equals(approvalType, "RoleBased", StringComparison.OrdinalIgnoreCase);

    private static string ResolveApplicationStatus(string action, WorkflowStage stage)
        => action switch
        {
            ActionAcceptMoveNext                    => "UnderReview",
            ActionFinalApproval                     => "Approved",
            ActionApproved => stage.IsFinalStage ? "Approved" : "UnderReview",
            ActionMoveNext                          => "UnderReview",
            ActionRejected                          => "Rejected",
            ActionForwardToUser                     => "UnderReview",
            ActionSendBackToApplicant
                or ActionCorrectionRequired         => StatusSentBackToApplicant,
            ActionSendBackToPrevious                => "UnderReview",
            _ => throw new InvalidOperationException($"Unsupported workflow action: {action}")
        };

    private static int ResolveTaskStatusCode(string action)
        => action switch
        {
            ActionAcceptMoveNext or ActionFinalApproval
                or ActionApproved or ActionMoveNext => WorkflowCodes.TaskStatus.Approved,
            ActionRejected                          => WorkflowCodes.TaskStatus.Rejected,
            ActionForwardToUser                     => WorkflowCodes.TaskStatus.Forwarded,
            ActionSendBackToApplicant
                or ActionCorrectionRequired         => WorkflowCodes.TaskStatus.SentBackToApplicant,
            ActionSendBackToPrevious                => WorkflowCodes.TaskStatus.SentBackToPreviousStage,
            _ => throw new InvalidOperationException($"Unsupported workflow action: {action}")
        };

    private static int ResolveInstanceStatusCode(string? status)
        => status switch
        {
            "UnderReview" => WorkflowCodes.InstanceStatus.UnderReview,
            "Approved" => WorkflowCodes.InstanceStatus.Approved,
            "Rejected" => WorkflowCodes.InstanceStatus.Rejected,
            StatusSentBackToApplicant => WorkflowCodes.InstanceStatus.SentBackToApplicant,
            "FinalConsumerCreated" => WorkflowCodes.InstanceStatus.FinalConsumerCreated,
            "Completed" => WorkflowCodes.InstanceStatus.Completed,
            "Pending" => WorkflowCodes.InstanceStatus.Pending,
            null => WorkflowCodes.InstanceStatus.Pending,
            _ => WorkflowCodes.InstanceStatus.Pending
        };

    private static int ResolveActionCode(string action)
        => action switch
        {
            ActionWorkflowStarted => WorkflowCodes.ActionCode.WorkflowStarted,
            ActionAcceptMoveNext  => WorkflowCodes.ActionCode.AcceptMoveNext,
            ActionFinalApproval   => WorkflowCodes.ActionCode.FinalApproval,
            ActionRejected        => WorkflowCodes.ActionCode.Reject,
            ActionSendBackToApplicant or ActionCorrectionRequired => WorkflowCodes.ActionCode.SendBackToApplicant,
            ActionSendBackToPrevious => WorkflowCodes.ActionCode.SendBackToPreviousStage,
            ActionForwardToUser   => WorkflowCodes.ActionCode.ForwardToUser,
            ActionStageAssigned   => WorkflowCodes.ActionCode.StageAssigned,
            ActionFinalConsumerCreated => WorkflowCodes.ActionCode.FinalConsumerCreated,
            _ => WorkflowCodes.ActionCode.StageAssigned
        };

    private static string NormalizeAction(string action)
    {
        var normalized = action?.Trim();
        return normalized switch
        {
            // ── New canonical action names ─────────────────────────────────────
            "AcceptMoveNext" or "Accept & Move Next" or "AcceptAndMoveNext"
                => ActionAcceptMoveNext,
            "FinalApproval" or "Final Approval" or "FinalApprove"
                => ActionFinalApproval,
            "ForwardToUser" or "Forward to Specific User" or "ForwardUser"
                => ActionForwardToUser,
            "SendBackToApplicant" or "Send Back to Applicant" or "SendBack"
                => ActionSendBackToApplicant,
            "SendBackToPrevious" or "Send Back to Previous Stage" or "SendBackPrevious"
                => ActionSendBackToPrevious,

            // ── Legacy / backward-compatible names ────────────────────────────
            "Approve" or "Approved"                       => ActionApproved,
            "MoveNext" or "MoveToNext" or "ForwardToNext" => ActionMoveNext,
            "Reject" or "Rejected"                        => ActionRejected,
            "CorrectionRequired" or "SendCorrection" or "Correction"
                => ActionSendBackToApplicant,

            _ => throw new InvalidOperationException($"Unsupported workflow action: '{normalized}'.")
        };
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsLegacyConsumerChange(string? applicationType)
        => string.Equals(applicationType, ApplicationTypeNameTransfer, StringComparison.OrdinalIgnoreCase)
            || string.Equals(applicationType, ApplicationTypeConnectionChange, StringComparison.OrdinalIgnoreCase);

    private static string ResolveLegacyAppType(string applicationType)
        => string.Equals(applicationType, ApplicationTypeNameTransfer, StringComparison.OrdinalIgnoreCase) ? "TRN" : "CTC";

    private void ApplyLegacyWorkflowStatus(
        MasterApplicationDetail application,
        string nextStatus,
        string action,
        string? remarks,
        DateTime now)
    {
        application.ApplicationStatus = nextStatus;
        application.StatusDate = DateOnly.FromDateTime(now);
        if (action == ActionRejected)
            application.ApplcationStatusDetail = AppendDetail(application.ApplcationStatusDetail, "RejectionRemarks", remarks);
        else if (!string.IsNullOrWhiteSpace(remarks))
            application.ApplcationStatusDetail = AppendDetail(application.ApplcationStatusDetail, "WorkflowRemarks", remarks);
    }

    private async Task FinalizeNameTransferAsync(
        MasterApplicationDetail application,
        WorkflowActionRequest request,
        DateTime now,
        CancellationToken ct)
    {
        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == application.ConsNo, ct)
            ?? throw new InvalidOperationException("Linked consumer was not found.");

        var detail = DecodeDetail(application.ApplcationStatusDetail);
        var oldName = consumer.ConsNm1;
        var oldFather = consumer.ConsNm2;
        consumer.ConsNm1 = application.ConName;
        consumer.ConsNm2 = DetailValue(detail, "NewFather") ?? consumer.ConsNm2;
        consumer.MobNo = application.ConPhoneMobile ?? consumer.MobNo;
        consumer.ModifyDate = now;
        consumer.Userid = UserIdText(request.ActorUserId);

        _db.ConsumerTransfers.Add(new ConsumerTransfer
        {
            ConsNo = consumer.ConsNo,
            ConsNm = application.ConName,
            ConsFnm = consumer.ConsNm2,
            TransDate = now,
            TransAmt = ToDouble(ExtractDecimal(detail, "TransferFee")),
            ChallanNo = DetailValue(detail, "ChallanNo"),
            ChallanDate = ExtractDate(detail, "ChallanDate") ?? now,
            Secu = ToDouble(ExtractDecimal(detail, "SecurityAmount")) ?? 0,
            Status = 1,
            Userid = UserIdText(request.ActorUserId),
            EntryDate = now,
            DevType = consumer.DevType
        });

        application.ApplicationStatus = "Approved";
        application.StatusDate = DateOnly.FromDateTime(now);
        application.ApplcationStatusDetail = AppendDetail(application.ApplcationStatusDetail, "ApprovalRemarks", request.Remarks);
        AddMasterApplicationHistory(
            application.ApplicationId,
            application.DivName,
            $"Approved and applied to consumer master. Old name: {oldName}; old father/name2: {oldFather}. {Normalize(request.Remarks)}",
            "2");
    }

    private async Task FinalizeConnectionChangeAsync(
        MasterApplicationDetail application,
        WorkflowActionRequest request,
        DateTime now,
        CancellationToken ct)
    {
        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == application.ConsNo, ct)
            ?? throw new InvalidOperationException("Linked consumer was not found.");

        var detail = DecodeDetail(application.ApplcationStatusDetail);
        consumer.ConTp = NormalizeConnectionTypeCode(DetailValue(detail, "NewConnectionType")) ?? consumer.ConTp;
        consumer.ConsCtg = NormalizeConsumerCategoryCode(DetailValue(detail, "NewCategory")) ?? consumer.ConsCtg;
        consumer.TypeChangeDate = ExtractDate(detail, "TypeChangeDate") ?? now;
        consumer.EstiNo = DetailValue(detail, "EstimationNo") ?? consumer.EstiNo;
        consumer.EstiAmt = ToInt(ExtractDecimal(detail, "EstimationAmount")) ?? consumer.EstiAmt;
        consumer.Secu = ToInt(ExtractDecimal(detail, "SecurityAmount")) ?? consumer.Secu;
        consumer.MonthlyRate = ToDouble(ExtractDecimal(detail, "MonthlyRate")) ?? consumer.MonthlyRate;
        consumer.ModifyDate = now;
        consumer.Userid = UserIdText(request.ActorUserId);

        application.ApplicationStatus = "Approved";
        application.StatusDate = DateOnly.FromDateTime(now);
        application.ApplcationStatusDetail = AppendDetail(application.ApplcationStatusDetail, "ApprovalRemarks", request.Remarks);
        AddMasterApplicationHistory(application.ApplicationId, application.DivName, "Approved and applied to consumer master. " + Normalize(request.Remarks), "2");
    }

    private void AddMasterApplicationHistory(string appId, string? division, string remark, string status)
    {
        var persistedMax = (_db.MasterApplicationDetailHistories
            .Where(x => x.ApplicationId == appId)
            .Select(x => x.SerialNumber ?? 0)
            .DefaultIfEmpty()
            .Max());
        var localMax = _db.ChangeTracker.Entries<MasterApplicationDetailHistory>()
            .Where(x => x.Entity.ApplicationId == appId)
            .Select(x => x.Entity.SerialNumber ?? 0)
            .DefaultIfEmpty()
            .Max();
        var next = Math.Max(persistedMax, localMax) + 1;

        _db.MasterApplicationDetailHistories.Add(new MasterApplicationDetailHistory
        {
            ApplicationId = appId,
            SerialNumber = next,
            Division = division,
            CurrentHoldingPer = division,
            ForwardDate = DateOnly.FromDateTime(DateTime.Today),
            Remark = Truncate(remark, 200),
            Flag = "N",
            CurentStatus = status,
            Status = "1"
        });
    }

    private static Dictionary<string, string> DecodeDetail(string? text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
            return result;

        foreach (var item in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = item.Split('=', 2);
            if (parts.Length == 2)
                result[parts[0]] = parts[1];
        }

        return result;
    }

    private static string? DetailValue(IReadOnlyDictionary<string, string> detail, string key)
        => detail.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static decimal? ExtractDecimal(IReadOnlyDictionary<string, string> detail, string key)
        => decimal.TryParse(DetailValue(detail, key), out var value) ? value : null;

    private static DateTime? ExtractDate(IReadOnlyDictionary<string, string> detail, string key)
        => DateTime.TryParse(DetailValue(detail, key), out var value) ? value : null;

    private static int? ToInt(decimal? value)
        => value.HasValue ? Convert.ToInt32(value.Value) : null;

    private static double? ToDouble(decimal? value)
        => value.HasValue ? Convert.ToDouble(value.Value) : null;

    private static string UserIdText(int? userId)
        => userId.HasValue ? userId.Value.ToString() : "0";

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private static string AppendDetail(string? existing, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return existing ?? string.Empty;

        var sanitized = value.Replace(";", ",").Replace("=", "-").Trim();
        return string.IsNullOrWhiteSpace(existing) ? $"{key}={sanitized}" : $"{existing};{key}={sanitized}";
    }

    private static string? NormalizeConnectionTypeCode(string? value)
    {
        var normalized = NormalizeToken(value);
        return normalized switch
        {
            "R" or "RESIDENTIAL" => "R",
            "C" or "COMMERCIAL" => "C",
            "I" or "INSTITUTIONAL" => "I",
            "T" or "INDUSTRIAL" or "INDUSTRY" => "T",
            "S" or "STAFF" => "S",
            "V" or "VILLAGE" => "V",
            "H" or "HOUSING" => "H",
            "G" or "GROUPHOUSING" or "GROUP HOUSING" => "G",
            "CC" or "COURTCASE" or "COURT CASE" => "CC",
            "D" or "DISCONNECTION" or "DISCONNECTED" => "D",
            _ => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 1)]
        };
    }

    private static string? NormalizeConsumerCategoryCode(string? value)
    {
        var normalized = NormalizeToken(value);
        return normalized switch
        {
            "R" or "REGULAR" => "R",
            "T" or "TEMPORARY" => "T",
            "S" or "STAFF" => "S",
            "M" or "RMC" => "M",
            "CC" or "COURTCASE" or "COURT CASE" => "CC",
            "D" or "DISCONNECTION" or "DISCONNECTED" => "D",
            _ => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 10)]
        };
    }

    private async Task SendStageAssignmentInAppAsync(
        long instanceId, long applicationId, string applicationNo,
        WorkflowStage stage, DateTime now, CancellationToken ct)
    {
        var batch = new List<InAppNotification>();
        var applicationDevType = await ResolveWorkflowDivisionDevTypeAsync(instanceId, ct);

        // Specific user
        if (stage.ApproverUserId.HasValue)
        {
            var notification = await MakeStageInAppAsync(stage.ApproverUserId.Value, instanceId, applicationNo, stage.StageName, ct);
            if (notification is not null)
                batch.Add(notification);
        }
        // Role-based users
        else if (stage.ApproverRoleId.HasValue)
        {
            var usersQuery = _db.Appusers.AsNoTracking()
                .Where(x => x.IsActive == true && !x.IsDeleted);

            if (stage.ApproverRoleId.HasValue)
                usersQuery = usersQuery.Where(x => x.RoleId == stage.ApproverRoleId.Value);

            usersQuery = ApplyDivisionRecipientFilter(usersQuery, applicationDevType);

            var userIds = await usersQuery.Select(x => x.Id).ToListAsync(ct);
            foreach (var uid in userIds)
            {
                var notification = await MakeStageInAppAsync(uid, instanceId, applicationNo, stage.StageName, ct);
                if (notification is not null)
                    batch.Add(notification);
            }
        }

        if (batch.Count > 0)
            await _db.InAppNotifications.AddRangeAsync(batch, ct);
    }

    private IQueryable<Appuser> ApplyDivisionRecipientFilter(IQueryable<Appuser> query, int? applicationDevType)
    {
        if (!applicationDevType.HasValue)
            return query;

        return query.Where(x =>
            !x.DivisionDevType.HasValue
            || x.DivisionDevType == applicationDevType.Value
            || x.DivisionDevType == AppConstants.Divisions.AllDivision.DevType);
    }

    private async Task<int?> ResolveWorkflowDivisionDevTypeAsync(long workflowInstanceId, CancellationToken ct)
    {
        return await _db.ApplicationWorkflowInstances
            .AsNoTracking()
            .Where(x => x.Id == workflowInstanceId
                && !x.IsDeleted
                && x.ApplicationType == ApplicationTypeNewConnection)
            .Join(
                _db.NewConnectionApplications.AsNoTracking().Where(x => !x.IsDeleted),
                instance => instance.ApplicationId,
                app => app.Id,
                (_, app) => app.DevType)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<InAppNotification?> MakeStageInAppAsync(
        long userId, long instanceId, string applicationNo, string? stageName, CancellationToken ct)
        => await BuildTemplatedInAppNotificationAsync(
            purposeKey: CommunicationPurposes.WorkflowAssigned,
            userType: "Internal",
            userId: userId,
            values: BuildWorkflowTemplateValues(applicationNo, stageName, null, AppTime.IndiaNow),
            referenceType: "WorkflowInstance",
            referenceId: instanceId.ToString(),
            referenceNo: applicationNo,
            redirectUrl: BuildInternalWorkflowInstanceUrl(instanceId),
            createdAt: AppTime.IndiaNow,
            ct: ct);

    private async Task<InAppNotification?> BuildTemplatedInAppNotificationAsync(
        string purposeKey,
        string userType,
        long userId,
        IReadOnlyDictionary<string, string?> values,
        string? referenceType,
        string? referenceId,
        string? referenceNo,
        string? redirectUrl,
        DateTime createdAt,
        CancellationToken ct)
    {
        var template = await _db.CommunicationTemplates
            .AsNoTracking()
            .Where(x => x.PurposeKey == purposeKey
                && x.Channel == CommunicationChannels.InApp
                && x.IsActive
                && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (template is null)
        {
            await LogNotificationTemplateIssueAsync(
                purposeKey,
                "Active in-app communication template was not found for workflow notification.",
                referenceType,
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        if (string.IsNullOrWhiteSpace(template.Body))
        {
            await LogNotificationTemplateIssueAsync(
                purposeKey,
                "In-app communication template body is empty for workflow notification.",
                referenceType,
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        string title;
        string message;
        try
        {
            title = _templateRenderer.Render(template.Subject ?? template.TemplateName ?? purposeKey, values);
            message = _templateRenderer.Render(template.Body, values);
        }
        catch (Exception ex)
        {
            await LogNotificationTemplateIssueAsync(
                purposeKey,
                $"Failed to render workflow notification template. {ex.Message}",
                referenceType,
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        return new InAppNotification
        {
            UserType = userType,
            UserId = userId,
            Title = title,
            Message = message,
            PurposeKey = purposeKey,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNo = referenceNo,
            RedirectUrl = redirectUrl,
            IsRead = false,
            CreatedAt = createdAt,
            IsDeleted = false
        };
    }

    private async Task LogNotificationTemplateIssueAsync(
        string purposeKey,
        string message,
        string? referenceType,
        string? referenceId,
        string? referenceNo,
        CancellationToken ct)
    {
        await _errorLogService.TryLogAsync(new ErrorLogWriteModel
        {
            ExceptionType = "NotificationTemplateIssue",
            Message = message,
            RequestPath = "WorkflowService/InAppNotification",
            HttpMethod = "INTERNAL",
            QueryString = $"referenceType={referenceType}&referenceId={referenceId}&referenceNo={referenceNo}",
            StatusCode = 500,
            PortalType = "Admin",
            ControllerName = "WorkflowService",
            ActionName = purposeKey,
            TraceId = referenceNo ?? referenceId,
            IsHandled = true
        }, ct);
    }

    private static Dictionary<string, string?> BuildWorkflowTemplateValues(string applicationNo, string? stageName, string? remarks, DateTime when)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApplicationNo"] = applicationNo,
            ["StageName"] = stageName,
            ["Remarks"] = Normalize(remarks),
            ["Date"] = when.ToString("dd MMM yyyy hh:mm tt")
        };

    private static string BuildInternalWorkflowTaskUrl(long taskId) => $"/Approvals/Details/{taskId}";

    private static string BuildInternalWorkflowInstanceUrl(long workflowInstanceId) => $"/Approvals/OpenCurrent/{workflowInstanceId}";

    private static string? BuildConsumerApplicationUrl(string? applicationType, long applicationId)
        => applicationType switch
        {
            ApplicationTypeNewConnection => $"/Consumer/NewConnection/Details/{applicationId}",
            ApplicationTypeNdc => $"/Consumer/Ndc/Details/{applicationId}",
            _ => null
        };

    private async Task SendApplicantCommunicationAsync(
        string purposeKey,
        NotificationRecipient recipient,
        IReadOnlyDictionary<string, string?> values,
        string? referenceType,
        string? referenceId,
        string? referenceNo,
        string? redirectUrl,
        CancellationToken ct)
    {
        try
        {
            await _communicationService.SendAsync(
                purposeKey,
                recipient,
                values,
                NotificationChannelOptions.For(
                    CommunicationChannels.InApp,
                    CommunicationChannels.Email,
                    CommunicationChannels.Sms,
                    CommunicationChannels.WhatsApp),
                referenceType,
                referenceId,
                referenceNo,
                redirectUrl,
                ct);
        }
        catch (Exception ex)
        {
            await _errorLogService.TryLogAsync(new ErrorLogWriteModel
            {
                CreatedAt = DateTime.Now,
                ExceptionType = "ApplicantNotificationDispatchException",
                Message = $"Applicant notification dispatch failed for purpose {purposeKey}. {ex.Message}",
                StackTrace = ex.ToString(),
                RequestPath = "WorkflowService/SendApplicantCommunication",
                HttpMethod = "INTERNAL",
                QueryString = $"purposeKey={purposeKey}&referenceType={referenceType}&referenceId={referenceId}&referenceNo={referenceNo}",
                StatusCode = 500,
                PortalType = recipient.UserType ?? AppConstants.PortalTypes.Unknown,
                ControllerName = "WorkflowService",
                ActionName = purposeKey,
                TraceId = referenceNo ?? referenceId,
                IsHandled = true
            }, ct);
        }
    }

    private static Dictionary<string, string?> BuildApplicantTemplateValues(
        string applicationNo,
        string applicationStatus,
        string? stageName,
        string? remarks,
        string? actionBy,
        string? consumerNo,
        string portalUrl,
        DateTime when)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApplicantName"] = null,
            ["ConsumerName"] = null,
            ["ApplicationNo"] = applicationNo,
            ["ApplicationNumber"] = applicationNo,
            ["ApplicationStatus"] = NormalizeApplicantStatusLabel(applicationStatus),
            ["StageName"] = stageName,
            ["ActionBy"] = actionBy,
            ["ActionDate"] = when.ToString("dd MMM yyyy hh:mm tt"),
            ["Date"] = when.ToString("dd MMM yyyy hh:mm tt"),
            ["Remarks"] = Normalize(remarks),
            ["ConsumerNumber"] = consumerNo,
            ["ConsumerNo"] = consumerNo,
            ["PortalUrl"] = portalUrl
        };

    private static string NormalizeApplicantStatusLabel(string? status)
        => status switch
        {
            "Submitted" => "Submitted",
            "PendingPayment" => "Payment Pending",
            "PaymentFailed" => "Payment Failed",
            "FeePending" => "Fee Pending",
            "UnderReview" => "Under Review",
            "SentBackToApplicant" => "Sent Back to Applicant",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            "FinalConsumerCreated" => "Final Consumer Created",
            _ when string.IsNullOrWhiteSpace(status) => "Updated",
            _ => status!
        };

    private static NotificationRecipient BuildApplicantRecipient(NewConnectionApplication application)
        => new()
        {
            Name = application.ApplicantName,
            Email = application.EmailId,
            Mobile = application.MobileNumber,
            UserType = application.SubmittedByConsumerUserId.HasValue ? AppConstants.Roles.Consumer : null,
            UserId = application.SubmittedByConsumerUserId.HasValue ? application.SubmittedByConsumerUserId : null
        };

    private string BuildApplicantPortalUrl(NewConnectionApplication application)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var path = application.IsPublicApplication || !application.SubmittedByConsumerUserId.HasValue
            ? $"/NewConnection/Track?applicationNo={Uri.EscapeDataString(application.ApplicationNo)}&mobileNumber={Uri.EscapeDataString(application.MobileNumber ?? string.Empty)}"
            : $"/Consumer/NewConnection/Details/{application.Id}";

        if (request is null)
            return path;

        return $"{request.Scheme}://{request.Host}{request.PathBase}{path}";
    }

    private async Task<long> ResolveConsumerUserIdAsync(ApplicationWorkflowTask task, CancellationToken ct)
    {
        if (task.WorkflowInstance.ApplicationType == ApplicationTypeNewConnection)
        {
            var consumerUserId = await _db.NewConnectionApplications.AsNoTracking()
                .Where(x => x.Id == task.ApplicationId && !x.IsDeleted && x.SubmittedByConsumerUserId.HasValue)
                .Select(x => (long?)x.SubmittedByConsumerUserId)
                .FirstOrDefaultAsync(ct);
            return consumerUserId ?? 0;
        }

        if (task.WorkflowInstance.ApplicationType == ApplicationTypeNdc)
        {
            var consumerNo = await _db.ConsumerApplyNdcs.AsNoTracking()
                .Where(x => x.AutoId == task.ApplicationId)
                .Select(x => x.ConsumerNo)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(consumerNo))
            {
                var uid = await _db.ConsumerUsers.AsNoTracking()
                    .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted)
                    .Select(x => (long?)x.Id)
                    .FirstOrDefaultAsync(ct);
                return uid ?? 0;
            }
        }

        return 0;
    }

    private static string NormalizeToken(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeNdcStatus(ConsumerApplyNdc application)
        => !string.IsNullOrWhiteSpace(application.FinalStatus)
            ? application.FinalStatus!
            : !string.IsNullOrWhiteSpace(application.CurrentStatus)
                ? application.CurrentStatus!
                : !string.IsNullOrWhiteSpace(application.Status)
                    ? application.Status!
                    : "Pending";

    private static void ApplyNdcWorkflowStatus(
        ConsumerApplyNdc application,
        string nextStatus,
        string action,
        int? actorUserId,
        DateTime now,
        string? remarks)
    {
        application.LastUpdatedBy = actorUserId;
        application.LastUpdatedOn = now;
        application.CurrentStatus = nextStatus;

        if (action == ActionRejected)
        {
            application.Status = "R";
            application.FinalStatus = "C";
            application.Level2Remark2 = Normalize(remarks);
            application.Level2ActionDate2 = now;
            application.Level2Action2 = "Rejected";
            application.CompletedDate = now;
            return;
        }

        if (action is ActionApproved or ActionMoveNext)
        {
            application.Status = "A";
            application.Level = (application.Level ?? 0) + 1;
        }
    }
}
