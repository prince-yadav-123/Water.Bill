using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Approvals;
using Water.Bill.Application.DTOs.Workflow;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Security;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ApprovalsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWorkflowService _workflowService;
    private readonly IPermissionService _permissionService;

    public ApprovalsController(ApplicationDbContext db, IConfiguration configuration, IWorkflowService workflowService, IPermissionService permissionService)
    {
        _db = db;
        _configuration = configuration;
        _workflowService = workflowService;
        _permissionService = permissionService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Pending));

    [RequirePermission("My Pending Applications.view")]
    public async Task<IActionResult> Pending(
        string? tab = null,
        string? search = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? stageId = null,
        string? applicationType = null,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Approval Applications";
        ViewData["ActiveMenu"] = "My Pending Applications";
        await _workflowService.RepairSequentialWorkflowTasksAsync(ct);

        var isAdmin = IsAdminUser();
        var roleId = int.TryParse(User.FindFirstValue("RoleId"), out var parsedRoleId) ? parsedRoleId : 0;
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId) ? parsedUserId : 0;

        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);

        // Admin defaults to "All" tab so they see everything; others default to "Pending"
        var defaultTab = isAdmin ? "All" : "Pending";
        var normalizedTab = NormalizeTab(tab ?? defaultTab);
        ViewData["IsAdmin"] = isAdmin;

        var query = _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Include(x => x.WorkflowInstance)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.WorkflowInstance.IsDeleted);

        var divisionDevType = await ResolveDivisionDevTypeAsync(ct);
        query = ApplyDivisionFilter(query, divisionDevType);

        if (!isAdmin)
        {
            query = normalizedTab switch
            {
                "Approved" => query.Where(x => x.Status == "Approved"),
                "Rejected" => query.Where(x => x.Status == "Rejected"),
                "All" => query,
                _ => query.Where(x => x.IsActive
                    && x.Status == "Pending"
                    && x.WorkflowInstance.CurrentStageId == x.StageId
                    && x.WorkflowInstance.IsActive)
            };
        }

        // Admin sees ALL applications — no assignment filter
        // Non-admin sees only applications assigned to their user or role
        if (!isAdmin)
        {
            query = ApplyWorkflowAssignmentFilter(query, userId, roleId);
        }

        if (!string.IsNullOrWhiteSpace(status) && !isAdmin)
            query = query.Where(x => x.Status == status
                || x.WorkflowInstance.CurrentStatus == status
                || (status == "SentBackToApplicant" && x.WorkflowInstance.CurrentStatus == "SentBackToApplicant")
                || (status == "Forwarded" && x.Status == "Forwarded")
                || (status == "SentBackToPrevious" && x.Status == "SentBackToPrevious"));
        if (fromDate.HasValue && !isAdmin)
            query = query.Where(x => x.AssignedOn.Date >= fromDate.Value.Date);
        if (toDate.HasValue && !isAdmin)
            query = query.Where(x => x.AssignedOn.Date <= toDate.Value.Date);
        if (stageId.HasValue && !isAdmin)
            query = query.Where(x => x.StageId == stageId.Value);
        if (!string.IsNullOrWhiteSpace(applicationType))
            query = query.Where(x => x.WorkflowInstance.ApplicationType == applicationType);

        var rows = await query
            .OrderByDescending(x => x.AssignedOn)
            .ToListAsync(ct);

        if (rows.Count == 0)
        {
            return View(new ApprovalListViewModel
            {
                ActiveTab = normalizedTab,
                Search = search,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                StageId = stageId,
                ApplicationType = applicationType,
                Items = [],
                Stages = await _db.WorkflowStages.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.StageOrder).ToListAsync(ct)
            });
        }

        Dictionary<long, ApplicationWorkflowHistory> latestHistoryByInstance = [];
        if (rows.Count > 0)
        {
            var workflowInstanceIds = rows.Select(x => x.WorkflowInstanceId).Distinct().ToList();
            var histories = await _db.ApplicationWorkflowHistories
                .AsNoTracking()
                .Where(x => workflowInstanceIds.Contains(x.WorkflowInstanceId))
                .OrderByDescending(x => x.ActionOn)
                .ThenByDescending(x => x.Id)
                .ToListAsync(ct);

            latestHistoryByInstance = histories
                .GroupBy(x => x.WorkflowInstanceId)
                .ToDictionary(x => x.Key, x => x.First());
        }

        if (isAdmin)
        {
            rows = rows
                .GroupBy(x => x.WorkflowInstanceId)
                .Select(group =>
                {
                    var currentTask = group.FirstOrDefault(x =>
                        x.WorkflowInstance.CurrentStageId == x.StageId
                        && x.WorkflowInstance.IsActive
                        && x.IsActive
                        && string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase));

                    return currentTask
                        ?? group.OrderByDescending(x => x.ActionOn ?? x.AssignedOn)
                            .ThenByDescending(x => x.Id)
                            .First();
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                rows = rows.Where(x =>
                    string.Equals(x.WorkflowInstance.CurrentStatus, status, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase)
                    || (status == "SentBackToPrevious" && string.Equals(x.WorkflowInstance.CurrentStatus, "SentBackToPreviousStage", StringComparison.OrdinalIgnoreCase))
                    || (status == "SentBackToApplicant" && string.Equals(x.WorkflowInstance.CurrentStatus, "SentBackToApplicant", StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (stageId.HasValue)
                rows = rows.Where(x => x.StageId == stageId.Value).ToList();

            if (fromDate.HasValue)
            {
                rows = rows.Where(x =>
                {
                    var lastUpdatedOn = latestHistoryByInstance.TryGetValue(x.WorkflowInstanceId, out var history)
                        ? history.ActionOn
                        : x.ActionOn ?? x.AssignedOn;
                    return lastUpdatedOn.Date >= fromDate.Value.Date;
                }).ToList();
            }

            if (toDate.HasValue)
            {
                rows = rows.Where(x =>
                {
                    var lastUpdatedOn = latestHistoryByInstance.TryGetValue(x.WorkflowInstanceId, out var history)
                        ? history.ActionOn
                        : x.ActionOn ?? x.AssignedOn;
                    return lastUpdatedOn.Date <= toDate.Value.Date;
                }).ToList();
            }

            rows = rows
                .OrderByDescending(x => latestHistoryByInstance.TryGetValue(x.WorkflowInstanceId, out var history)
                    ? history.ActionOn
                    : x.ActionOn ?? x.AssignedOn)
                .ThenByDescending(x => x.Id)
                .ToList();
        }

        var applicationIds = rows
            .Where(x => x.WorkflowInstance.ApplicationType == "NewConnection")
            .Select(x => x.ApplicationId)
            .Distinct()
            .ToList();
        var applications = await _db.NewConnectionApplications
            .AsNoTracking()
            .Where(x => applicationIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);
        var ndcApplicationIds = rows
            .Where(x => x.WorkflowInstance.ApplicationType == WorkflowService.ApplicationTypeNdc)
            .Select(x => (int)x.ApplicationId)
            .Distinct()
            .ToList();
        var ndcApplications = await _db.ConsumerApplyNdcs
            .AsNoTracking()
            .Where(x => ndcApplicationIds.Contains(x.AutoId))
            .ToDictionaryAsync(x => x.AutoId, ct);
        var legacyApplicationNos = rows
            .Where(x => IsLegacyConsumerChange(x.WorkflowInstance.ApplicationType))
            .Select(x => x.ApplicationNo)
            .Distinct()
            .ToList();
        var legacyApplications = await _db.MasterApplicationDetails
            .AsNoTracking()
            .Where(x => legacyApplicationNos.Contains(x.ApplicationId))
            .ToDictionaryAsync(x => x.ApplicationId, ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            rows = rows.Where(x =>
            {
                applications.TryGetValue(x.ApplicationId, out var app);
                ndcApplications.TryGetValue((int)x.ApplicationId, out var ndc);
                legacyApplications.TryGetValue(x.ApplicationNo, out var legacy);
                return x.ApplicationNo.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (app?.ApplicantName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (app?.MobileNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (ndc?.ConsName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (ndc?.MobileNo?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (ndc?.ConsumerNo?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (legacy?.ConName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (legacy?.ConPhoneMobile?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (legacy?.ConsNo?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
            }).ToList();
        }

        // Total count is after in-memory search (rows already filtered above)
        var totalCount = rows.Count;

        // Page the filtered rows, then build dictionaries only for the paged subset
        var pagedRows = rows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userIds = pagedRows.Where(x => x.AssignedUserId.HasValue).Select(x => x.AssignedUserId!.Value).Distinct().ToList();
        var roleIds = pagedRows.Where(x => x.AssignedRoleId.HasValue).Select(x => x.AssignedRoleId!.Value).Distinct().ToList();
        var users = await _db.Appusers.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);
        var roles = await _db.Approles.AsNoTracking().Where(x => roleIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = pagedRows.Select(x =>
        {
            applications.TryGetValue(x.ApplicationId, out var app);
            ndcApplications.TryGetValue((int)x.ApplicationId, out var ndc);
            legacyApplications.TryGetValue(x.ApplicationNo, out var legacy);
            latestHistoryByInstance.TryGetValue(x.WorkflowInstanceId, out var latestHistory);
            return new ApprovalListItemViewModel
            {
                TaskId = x.Id,
                WorkflowInstanceId = x.WorkflowInstanceId,
                ApplicationNo = x.ApplicationNo,
                ApplicationType = x.WorkflowInstance.ApplicationType,
                ApplicantName = app?.ApplicantName ?? ndc?.ConsName ?? legacy?.ConName,
                MobileNumber = app?.MobileNumber ?? ndc?.MobileNo ?? legacy?.ConPhoneMobile,
                Property = app is not null
                    ? $"{app.Sector} / {app.Block} / {app.FlatNo}"
                    : ndc is not null
                        ? $"{ndc.Sector} / {ndc.Block} / {ndc.PlotNo}"
                        : legacy is not null
                            ? $"{legacy.SectorVill} / {legacy.Block} / {legacy.PlotNo}"
                            : "-",
                CurrentStatus = app?.ApplicationStatus ?? ndc?.CurrentStatus ?? ndc?.FinalStatus ?? ndc?.Status ?? legacy?.ApplicationStatus ?? x.WorkflowInstance.CurrentStatus,
                CurrentStage = x.Stage.StageName,
                AssignedTo = ResolveAssignedTo(x, users, roles),
                LastAction = latestHistory?.Action,
                LastUpdatedOn = latestHistory?.ActionOn ?? x.ActionOn ?? x.AssignedOn,
                SubmittedOn = app?.SubmittedOn ?? ndc?.CreatedOn ?? legacy?.EnterDate?.ToDateTime(TimeOnly.MinValue),
                AssignedOn = x.AssignedOn,
                StageSlaDays = x.Stage.SlaDays,
                StageDueOn = x.Stage.SlaDays.HasValue && x.Stage.SlaDays.Value > 0 ? x.AssignedOn.Date.AddDays(x.Stage.SlaDays.Value) : null,
                DaysSinceAssigned = Math.Max(0, (int)Math.Floor((DateTime.Now.Date - x.AssignedOn.Date).TotalDays)),
                SlaState = ResolveSlaState(x, DateTime.Now),
                CanAct = x.IsActive
                    && x.Status == "Pending"
                    && x.WorkflowInstance.CurrentStageId == x.StageId
                    && x.WorkflowInstance.IsActive,
                CanApprove = x.Stage.CanApprove,
                CanReject = x.Stage.CanReject
            };
        }).ToList();

        ViewBag.Pagination = PaginationViewModel.Create(new PagedResult<ApprovalListItemViewModel>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });

        return View(new ApprovalListViewModel
        {
            ActiveTab = normalizedTab,
            Search = search,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            StageId = stageId,
            ApplicationType = applicationType,
            Items = items,
            Stages = await _db.WorkflowStages.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.StageOrder).ToListAsync(ct)
        });
    }

    [HttpGet]
    [RequirePermission("My Pending Applications.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var taskQuery = await GetAllowedTaskQueryAsync(pendingOnly: false, ct);
        var task = await taskQuery
            .Include(x => x.WorkflowInstance)
            .Include(x => x.Stage)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (task is null)
            return NotFound();

        var application = task.WorkflowInstance.ApplicationType == "NewConnection"
            ? await _db.NewConnectionApplications
                .Include(x => x.Documents.Where(d => !d.IsDeleted))
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == task.ApplicationId && !x.IsDeleted, ct)
            : null;
        var ndcApplication = task.WorkflowInstance.ApplicationType == WorkflowService.ApplicationTypeNdc
            ? await _db.ConsumerApplyNdcs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AutoId == task.ApplicationId, ct)
            : null;
        var legacyApplication = IsLegacyConsumerChange(task.WorkflowInstance.ApplicationType)
            ? await _db.MasterApplicationDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationId == task.ApplicationNo && x.AppType == ResolveLegacyAppType(task.WorkflowInstance.ApplicationType), ct)
            : null;
        List<NdcDocument> ndcDocuments = ndcApplication is null
            ? []
            : await _db.NdcDocuments
                .AsNoTracking()
                .Where(x => x.NdcAutoId == ndcApplication.AutoId || x.ConsumerNo == ndcApplication.ConsumerNo)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync(ct);

        var fee = application is null
            ? null
            : await _db.NewConnectionApplicationFees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationId == application.Id, ct);

        var timeline = await _db.ApplicationWorkflowHistories
            .AsNoTracking()
            .Where(x => x.WorkflowInstanceId == task.WorkflowInstanceId)
            .OrderBy(x => x.ActionOn)
            .ToListAsync(ct);

        var stages = await _db.WorkflowStages
            .AsNoTracking()
            .Where(x => x.WorkflowId == task.WorkflowInstance.WorkflowId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.StageOrder)
            .ToListAsync(ct);

        var tasks = await _db.ApplicationWorkflowTasks
            .AsNoTracking()
            .Where(x => x.WorkflowInstanceId == task.WorkflowInstanceId && !x.IsDeleted)
            .OrderBy(x => x.AssignedOn)
            .ToListAsync(ct);

        var stageRoleIds = stages
            .Where(x => x.ApproverRoleId.HasValue)
            .Select(x => x.ApproverRoleId!.Value)
            .Distinct()
            .ToList();
        var stageUserIds = stages
            .Where(x => x.ApproverUserId.HasValue)
            .Select(x => x.ApproverUserId!.Value)
            .Distinct()
            .ToList();
        var stageRoles = await _db.Approles
            .AsNoTracking()
            .Where(x => stageRoleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var stageUsers = await _db.Appusers
            .AsNoTracking()
            .Where(x => stageUserIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, ct);

        var taskByStage = tasks
            .GroupBy(x => x.StageId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(t => t.AssignedOn).First());
        var stageProgress = stages.Select(stage =>
        {
            taskByStage.TryGetValue(stage.Id, out var stageTask);
            var state = string.Equals(stageTask?.Status, "Skipped", StringComparison.OrdinalIgnoreCase)
                ? "Upcoming"
                : stage.Id == task.WorkflowInstance.CurrentStageId && stageTask?.Status == "Pending"
                ? "Current"
                : stageTask is null
                    ? "Upcoming"
                    : stageTask.Status;

            return new WorkflowStageProgressViewModel
            {
                Stage = stage,
                Task = stageTask,
                State = state,
                RoleName = stage.ApproverRoleId.HasValue && stageRoles.TryGetValue(stage.ApproverRoleId.Value, out var roleName)
                    ? roleName
                    : "-",
                UserName = stage.ApproverUserId.HasValue && stageUsers.TryGetValue(stage.ApproverUserId.Value, out var userName)
                    ? userName
                    : "-"
            };
        }).ToList();
        var canMoveToNextStage = stages.Any(x => x.StageOrder > task.Stage.StageOrder);
        var isFirstStage = !stages.Any(x => x.StageOrder < task.Stage.StageOrder);
        // InternalUsers not loaded — Forward to Specific User is disabled for current phase

        ViewData["Title"] = "Approval Details";
        ViewData["ActiveMenu"] = "My Pending Applications";
        ViewData["IsAdmin"] = IsAdminUser();
        return View(new ApprovalDetailsViewModel
        {
            Task = task,
            NewConnectionApplication = application,
            NdcApplication = ndcApplication,
            NdcDocuments = ndcDocuments,
            LegacyApplication = legacyApplication,
            LegacyDetailValues = DecodeDetail(legacyApplication?.ApplcationStatusDetail),
            Fee = fee,
            WorkflowTimeline = timeline,
            StageProgress = stageProgress,
            CanMoveToNextStage = canMoveToNextStage,
            IsFirstStage = isFirstStage,
            InternalUsers = []   // Forward to Specific User disabled for current phase
        });
    }

    [HttpGet]
    [RequirePermission("My Pending Applications.view")]
    public async Task<IActionResult> OpenCurrent(long id, CancellationToken ct)
    {
        var taskQuery = await GetAllowedTaskQueryAsync(pendingOnly: false, ct);
        var task = await taskQuery
            .Include(x => x.WorkflowInstance)
            .Where(x => x.WorkflowInstanceId == id)
            .OrderByDescending(x => x.WorkflowInstance.CurrentStageId == x.StageId && x.IsActive && x.Status == "Pending" ? 1 : 0)
            .ThenByDescending(x => x.ActionOn ?? x.AssignedOn)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (task is null)
            return RedirectToAction(nameof(Pending));

        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    [HttpGet]
    [RequirePermission("My Pending Applications.view")]
    public async Task<IActionResult> Document(long taskId, long documentId, CancellationToken ct)
    {
        var taskQuery = await GetAllowedTaskQueryAsync(pendingOnly: false, ct);
        var task = await taskQuery
            .FirstOrDefaultAsync(x => x.Id == taskId, ct);
        if (task is null || !string.Equals(task.WorkflowInstance.ApplicationType, "NewConnection", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        var document = await _db.NewConnectionApplicationDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId
                && x.ApplicationId == task.ApplicationId
                && !x.IsDeleted, ct);
        if (document is null || string.IsNullOrWhiteSpace(document.FilePath))
            return NotFound();

        if (!FileUploadSecurityHelper.TryResolveSafeStoredFilePath(GetDocumentStorageBasePath(), document.FilePath, out var fullPath))
            return NotFound();

        return System.IO.File.Exists(fullPath)
            ? PhysicalFile(fullPath, FileUploadSecurityHelper.ResolveSafeContentType(document.FilePath), document.FileName)
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Action(long taskId, string actionType, string? remarks, int? forwardToUserId, CancellationToken ct)
    {
        var permissionAction = IsForwardAction(actionType) ? "forward" : "approve";
        if (!await HasPermissionAsync(AppConstants.Modules.MyPendingApplications, permissionAction, ct))
            return PermissionDenied(AppConstants.Modules.MyPendingApplications, permissionAction);

        var pendingTaskQuery = await GetAllowedPendingTaskQuery(ct);
        var taskExists = await pendingTaskQuery.AnyAsync(x => x.Id == taskId, ct);
        if (!taskExists)
            return NotFound();

        try
        {
            await _workflowService.ProcessActionAsync(new WorkflowActionRequest
            {
                TaskId = taskId,
                Action = actionType,
                Remarks = remarks,
                ForwardToUserId = forwardToUserId,
                ActorUserId = ResolveUserId(),
                ActorRoleId = ResolveRoleId(),
                ActorName = User.FindFirstValue("FullName") ?? User.Identity?.Name,
                ActorRole = ResolveRoleName(),
                ActorDepartmentIds = [],
                IsAdmin = IsAdminUser(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            }, ct);

            var application = await _db.NewConnectionApplications
                .AsNoTracking()
                .Where(x => _db.ApplicationWorkflowTasks.Any(t => t.Id == taskId && t.ApplicationId == x.Id))
                .Select(x => new { x.ApplicationStatus, x.FinalConsumerNo })
                .FirstOrDefaultAsync(ct);

            TempData["SuccessMessage"] = !string.IsNullOrWhiteSpace(application?.FinalConsumerNo)
                ? $"Application approved successfully. Consumer Number generated: {application.FinalConsumerNo}"
                : "Workflow action completed successfully.";
            if (!string.IsNullOrWhiteSpace(application?.FinalConsumerNo))
                TempData["FinalConsumerNo"] = application.FinalConsumerNo;
            return RedirectToAction(nameof(Details), new { id = taskId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = taskId });
        }
    }

    private Task<IQueryable<ApplicationWorkflowTask>> GetAllowedPendingTaskQuery(CancellationToken ct)
        => GetAllowedTaskQueryAsync(pendingOnly: true, ct);

    private async Task<IQueryable<ApplicationWorkflowTask>> GetAllowedTaskQueryAsync(bool pendingOnly, CancellationToken ct)
    {
        var query = _db.ApplicationWorkflowTasks
            .Include(x => x.WorkflowInstance)
            .Where(x => !x.IsDeleted);

        if (pendingOnly)
            query = query.Where(x => x.IsActive
                && x.Status == "Pending"
                && x.WorkflowInstance.CurrentStageId == x.StageId
                && x.WorkflowInstance.IsActive
                && !x.WorkflowInstance.IsDeleted);

        query = query.Where(x => x.WorkflowInstance.ApplicationType != "NewConnection"
            || _db.NewConnectionApplications.Any(app => app.Id == x.ApplicationId && !app.IsDeleted));

        var divisionDevType = await ResolveDivisionDevTypeAsync(ct);
        query = ApplyDivisionFilter(query, divisionDevType);

        // Admin can view any task; other roles limited to assigned tasks
        if (IsAdminUser()) return query;

        var userId = ResolveUserId() ?? 0;
        var roleId = ResolveRoleId() ?? 0;

        return ApplyWorkflowAssignmentFilter(query, userId, roleId);
    }

    private static IQueryable<ApplicationWorkflowTask> ApplyWorkflowAssignmentFilter(
        IQueryable<ApplicationWorkflowTask> query,
        int userId,
        int roleId)
        => query.Where(x =>
            (x.AssignedUserId.HasValue && x.AssignedUserId == userId)
            || (!x.AssignedUserId.HasValue
                && x.AssignedRoleId.HasValue
                && x.AssignedRoleId == roleId)
            || (!x.AssignedUserId.HasValue
                && !x.AssignedRoleId.HasValue));

    private IQueryable<ApplicationWorkflowTask> ApplyDivisionFilter(
        IQueryable<ApplicationWorkflowTask> query,
        int? divisionDevType)
    {
        if (!divisionDevType.HasValue
            || divisionDevType.Value == AppConstants.Divisions.AllDivision.DevType)
            return query;

        return query.Where(x =>
            x.WorkflowInstance.ApplicationType != WorkflowService.ApplicationTypeNewConnection
            || _db.NewConnectionApplications.Any(app =>
                app.Id == x.ApplicationId
                && !app.IsDeleted
                && app.DevType == divisionDevType.Value));
    }

    private async Task<int?> ResolveDivisionDevTypeAsync(CancellationToken ct)
    {
        var userId = ResolveUserId();
        if (!userId.HasValue)
            return null;

        return await _db.Appusers
            .AsNoTracking()
            .Where(x => x.Id == userId.Value && !x.IsDeleted)
            .Select(x => x.DivisionDevType)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<bool> HasPermissionAsync(string module, string action, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue("RoleId"), out var roleId))
            return false;

        return await _permissionService.HasPermissionAsync(roleId, module, action, ct);
    }

    private IActionResult PermissionDenied(string module, string action)
    {
        var acceptHeader = Request.Headers.Accept.ToString();
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
            || (acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                && !acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase));

        if (isAjax)
        {
            return new JsonResult(new
            {
                error = "Forbidden",
                message = "You do not have permission to access this module."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return RedirectToAction("Index", "Unauthorized", new
        {
            permission = $"{module}.{action}",
            returnUrl = $"{Request.Path}{Request.QueryString}"
        });
    }

    private static bool IsForwardAction(string? actionType)
        => string.Equals(actionType, "ForwardToUser", StringComparison.OrdinalIgnoreCase)
            || string.Equals(actionType, "ForwardUser", StringComparison.OrdinalIgnoreCase)
            || string.Equals(actionType, "Forward", StringComparison.OrdinalIgnoreCase);

    private bool IsAdminUser()
    {
        var role = User.FindFirstValue(ClaimTypes.Role)
                ?? User.FindFirstValue(AppConstants.Claims.RoleName)
                ?? string.Empty;
        return role.Contains("admin", StringComparison.OrdinalIgnoreCase);
    }

    private int? ResolveUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private int? ResolveRoleId()
        => int.TryParse(User.FindFirstValue("RoleId"), out var roleId) ? roleId : null;

    private string? ResolveRoleName()
        => User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue(AppConstants.Claims.RoleName);

    private static string NormalizeTab(string? tab)
        => tab?.Trim() switch
        {
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            "All" => "All",
            _ => "Pending"
        };

    private static string ResolveAssignedTo(ApplicationWorkflowTask task, IDictionary<int, string> users, IDictionary<int, string> roles)
    {
        if (task.AssignedUserId.HasValue && users.TryGetValue(task.AssignedUserId.Value, out var userName))
            return userName;

        if (task.AssignedRoleId.HasValue && roles.TryGetValue(task.AssignedRoleId.Value, out var roleName))
            return roleName;

        return "Unassigned";
    }

    private static string ResolveSlaState(ApplicationWorkflowTask task, DateTime now)
    {
        var isCurrentPendingTask = task.IsActive
            && string.Equals(task.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            && task.WorkflowInstance.CurrentStageId == task.StageId
            && task.WorkflowInstance.IsActive;

        if (!isCurrentPendingTask || !task.Stage.SlaDays.HasValue || task.Stage.SlaDays.Value <= 0)
            return "None";

        var elapsedDays = Math.Max(0, (int)Math.Floor((now.Date - task.AssignedOn.Date).TotalDays));
        var slaDays = task.Stage.SlaDays.Value;
        if (elapsedDays >= slaDays)
            return "Expired";

        var remainingDays = slaDays - elapsedDays;
        var nearExpiryThreshold = Math.Max(2, (int)Math.Ceiling(slaDays * 0.30));
        return remainingDays <= nearExpiryThreshold ? "NearExpiry" : "Normal";
    }

    private static bool IsLegacyConsumerChange(string? applicationType)
        => string.Equals(applicationType, WorkflowService.ApplicationTypeNameTransfer, StringComparison.OrdinalIgnoreCase)
            || string.Equals(applicationType, WorkflowService.ApplicationTypeConnectionChange, StringComparison.OrdinalIgnoreCase);

    private static string ResolveLegacyAppType(string applicationType)
        => string.Equals(applicationType, WorkflowService.ApplicationTypeNameTransfer, StringComparison.OrdinalIgnoreCase) ? "TRN" : "CTC";

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

    private string GetDocumentStorageBasePath()
        => _configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads";
}
