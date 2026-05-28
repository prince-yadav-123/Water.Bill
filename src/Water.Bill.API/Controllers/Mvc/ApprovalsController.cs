using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Models.Approvals;
using Water.Bill.Application.DTOs.Workflow;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ApprovalsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWorkflowService _workflowService;

    public ApprovalsController(ApplicationDbContext db, IWorkflowService workflowService)
    {
        _db = db;
        _workflowService = workflowService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Pending));

    public async Task<IActionResult> Pending(
        string tab = "Pending",
        string? search = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? departmentId = null,
        int? stageId = null,
        string? applicationType = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Approval Applications";
        ViewData["ActiveMenu"] = "My Pending Applications";
        await _workflowService.RepairSequentialWorkflowTasksAsync(ct);
        var roleId = int.TryParse(User.FindFirstValue("RoleId"), out var parsedRoleId) ? parsedRoleId : 0;
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId) ? parsedUserId : 0;
        var departmentIds = await _db.AuthorityUserDepartments
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => x.DepartmentId)
            .ToListAsync(ct);

        var normalizedTab = NormalizeTab(tab);
        var query = _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
                .ThenInclude(x => x.Department)
            .Include(x => x.WorkflowInstance)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.WorkflowInstance.IsDeleted);

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

        query = ApplyWorkflowAssignmentFilter(query, userId, roleId, departmentIds);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status || x.WorkflowInstance.CurrentStatus == status);
        if (fromDate.HasValue)
            query = query.Where(x => x.AssignedOn.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.AssignedOn.Date <= toDate.Value.Date);
        if (departmentId.HasValue)
            query = query.Where(x => x.AssignedDepartmentId == departmentId.Value);
        if (stageId.HasValue)
            query = query.Where(x => x.StageId == stageId.Value);
        if (!string.IsNullOrWhiteSpace(applicationType))
            query = query.Where(x => x.WorkflowInstance.ApplicationType == applicationType);

        var rows = await query
            .OrderByDescending(x => x.AssignedOn)
            .ToListAsync(ct);

        var applicationIds = rows
            .Where(x => x.WorkflowInstance.ApplicationType == "NewConnection")
            .Select(x => x.ApplicationId)
            .Distinct()
            .ToList();
        var applications = await _db.NewConnectionApplications
            .AsNoTracking()
            .Where(x => applicationIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            rows = rows.Where(x =>
            {
                applications.TryGetValue(x.ApplicationId, out var app);
                return x.ApplicationNo.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (app?.ApplicantName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
                    || (app?.MobileNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
            }).ToList();
        }

        var userIds = rows.Where(x => x.AssignedUserId.HasValue).Select(x => x.AssignedUserId!.Value).Distinct().ToList();
        var roleIds = rows.Where(x => x.AssignedRoleId.HasValue).Select(x => x.AssignedRoleId!.Value).Distinct().ToList();
        var users = await _db.Appusers.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);
        var roles = await _db.Approles.AsNoTracking().Where(x => roleIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = rows.Select(x =>
        {
            applications.TryGetValue(x.ApplicationId, out var app);
            return new ApprovalListItemViewModel
            {
                TaskId = x.Id,
                ApplicationNo = x.ApplicationNo,
                ApplicationType = x.WorkflowInstance.ApplicationType,
                ApplicantName = app?.ApplicantName,
                MobileNumber = app?.MobileNumber,
                Property = app is null ? "-" : $"{app.Sector} / {app.Block} / {app.FlatNo}",
                CurrentStatus = app?.ApplicationStatus ?? x.WorkflowInstance.CurrentStatus,
                CurrentStage = x.Stage.StageName,
                AssignedTo = ResolveAssignedTo(x, users, roles),
                SubmittedOn = app?.SubmittedOn,
                AssignedOn = x.AssignedOn,
                CanAct = x.IsActive
                    && x.Status == "Pending"
                    && x.WorkflowInstance.CurrentStageId == x.StageId
                    && x.WorkflowInstance.IsActive,
                CanApprove = x.Stage.CanApprove,
                CanReject = x.Stage.CanReject
            };
        }).ToList();

        return View(new ApprovalListViewModel
        {
            ActiveTab = normalizedTab,
            Search = search,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            DepartmentId = departmentId,
            StageId = stageId,
            ApplicationType = applicationType,
            Items = items,
            Departments = await _db.MasterDeptDetails.AsNoTracking().Where(x => x.Status == "1").OrderBy(x => x.DeptName).ToListAsync(ct),
            Stages = await _db.WorkflowStages.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.StageOrder).ToListAsync(ct)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var task = await GetAllowedTaskQuery(pendingOnly: false)
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
            .Include(x => x.Department)
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

        ViewData["Title"] = "Approval Details";
        ViewData["ActiveMenu"] = "My Pending Applications";
        return View(new ApprovalDetailsViewModel
        {
            Task = task,
            NewConnectionApplication = application,
            Fee = fee,
            WorkflowTimeline = timeline,
            StageProgress = stageProgress,
            CanMoveToNextStage = canMoveToNextStage
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Action(long taskId, string actionType, string? remarks, CancellationToken ct)
    {
        if (string.Equals(actionType, "Correction", StringComparison.OrdinalIgnoreCase)
            || string.Equals(actionType, "SendCorrection", StringComparison.OrdinalIgnoreCase)
            || string.Equals(actionType, "CorrectionRequired", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Send for correction is no longer available.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        var taskExists = await GetAllowedPendingTaskQuery().AnyAsync(x => x.Id == taskId, ct);
        if (!taskExists)
            return NotFound();

        try
        {
            await _workflowService.ProcessActionAsync(new WorkflowActionRequest
            {
                TaskId = taskId,
                Action = actionType,
                Remarks = remarks,
                ActorUserId = ResolveUserId(),
                ActorRoleId = ResolveRoleId(),
                ActorName = User.FindFirstValue("FullName") ?? User.Identity?.Name,
                ActorRole = ResolveRoleName(),
                ActorDepartmentIds = await ResolveDepartmentIdsAsync(ct),
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

    private IQueryable<ApplicationWorkflowTask> GetAllowedPendingTaskQuery()
        => GetAllowedTaskQuery(pendingOnly: true);

    private IQueryable<ApplicationWorkflowTask> GetAllowedTaskQuery(bool pendingOnly)
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

        var userId = ResolveUserId() ?? 0;
        var roleId = ResolveRoleId() ?? 0;
        var departmentIds = _db.AuthorityUserDepartments
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => x.DepartmentId);

        return ApplyWorkflowAssignmentFilter(query, userId, roleId, departmentIds);
    }

    private static IQueryable<ApplicationWorkflowTask> ApplyWorkflowAssignmentFilter(
        IQueryable<ApplicationWorkflowTask> query,
        int userId,
        int roleId,
        IEnumerable<int> departmentIds)
        => query.Where(x =>
            (x.AssignedUserId.HasValue && x.AssignedUserId == userId)
            || (!x.AssignedUserId.HasValue
                && x.AssignedDepartmentId.HasValue
                && x.AssignedRoleId.HasValue
                && x.AssignedRoleId == roleId
                && departmentIds.Contains(x.AssignedDepartmentId.Value))
            || (!x.AssignedUserId.HasValue
                && x.AssignedDepartmentId.HasValue
                && !x.AssignedRoleId.HasValue
                && departmentIds.Contains(x.AssignedDepartmentId.Value))
            || (!x.AssignedUserId.HasValue
                && !x.AssignedDepartmentId.HasValue
                && x.AssignedRoleId.HasValue
                && x.AssignedRoleId == roleId));

    private async Task<IReadOnlyList<int>> ResolveDepartmentIdsAsync(CancellationToken ct)
    {
        var userId = ResolveUserId() ?? 0;
        return await _db.AuthorityUserDepartments
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => x.DepartmentId)
            .ToListAsync(ct);
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

        var parts = new List<string>();
        if (task.Stage.Department?.DeptName is { Length: > 0 } departmentName)
            parts.Add(departmentName);
        if (task.AssignedRoleId.HasValue && roles.TryGetValue(task.AssignedRoleId.Value, out var roleName))
            parts.Add(roleName);

        return parts.Count == 0 ? "Unassigned" : string.Join(" / ", parts);
    }
}
