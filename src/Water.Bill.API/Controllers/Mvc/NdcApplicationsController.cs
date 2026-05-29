using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Ndc;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
[RequirePermission("NDC Applications.view")]
public class NdcApplicationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWorkflowService _workflowService;

    public NdcApplicationsController(ApplicationDbContext db, IWorkflowService workflowService)
    {
        _db = db;
        _workflowService = workflowService;
    }

    [HttpGet("/NdcApplications")]
    public async Task<IActionResult> Index(
        string? search,
        string? status,
        int? divisionType,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = "NDC Applications";
        ViewData["ActiveMenu"] = "NDC Applications";

        search = Normalize(search);
        status = Normalize(status);

        var userId = ResolveUserId() ?? 0;
        var roleId = ResolveRoleId() ?? 0;
        var departmentIds = await _db.AuthorityUserDepartments
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => x.DepartmentId)
            .ToListAsync(ct);

        var assignedTasks = await _db.ApplicationWorkflowTasks
            .Include(x => x.WorkflowInstance)
            .AsNoTracking()
            .Where(x => x.WorkflowInstance.ApplicationType == WorkflowService.ApplicationTypeNdc
                && !x.IsDeleted
                && x.IsActive
                && x.Status == WorkflowService.TaskStatusPending
                && x.WorkflowInstance.IsActive
                && !x.WorkflowInstance.IsDeleted
                && x.WorkflowInstance.CurrentStageId == x.StageId)
            .OrderByDescending(x => x.AssignedOn)
            .ToListAsync(ct);

        assignedTasks = assignedTasks
            .Where(x => IsAssignedToCurrentUser(x, userId, roleId, departmentIds))
            .ToList();

        var assignedApplicationIds = assignedTasks
            .Select(x => (int)x.ApplicationId)
            .Distinct()
            .ToList();

        var taskLookup = assignedTasks
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(x => x.Key, x => x.First().Id);

        var query = _db.ConsumerApplyNdcs
            .AsNoTracking()
            .Where(x => assignedApplicationIds.Contains(x.AutoId));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.ApplicationNo.Contains(search)
                || (x.ConsumerNo != null && x.ConsumerNo.Contains(search))
                || (x.ConsName != null && x.ConsName.Contains(search))
                || (x.MobileNo != null && x.MobileNo.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status || x.CurrentStatus == status || x.FinalStatus == status);
        if (divisionType.HasValue)
            query = query.Where(x => x.DivisionType == divisionType);
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedOn != null && x.CreatedOn.Value.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedOn != null && x.CreatedOn.Value.Date <= toDate.Value.Date);

        var rows = await query
            .OrderByDescending(x => x.CreatedOn)
            .ThenByDescending(x => x.AutoId)
            .Take(300)
            .ToListAsync(ct);

        return View(new NdcApplicationIndexViewModel
        {
            Search = search,
            Status = status,
            DivisionType = divisionType,
            FromDate = fromDate,
            ToDate = toDate,
            Items = rows.Select(x => new NdcApplicationListItemViewModel
            {
                AutoId = x.AutoId,
                ApplicationNo = x.ApplicationNo,
                ConsumerNo = x.ConsumerNo,
                ConsumerName = x.ConsName,
                MobileNo = x.MobileNo,
                PropertyNo = BuildProperty(x.Sector, x.Block, x.PlotNo),
                Division = x.DivisionTypeName,
                Status = DisplayStatus(x.Status, x.CurrentStatus, x.FinalStatus),
                SuccessStatus = x.SuccessStatus,
                Amount = x.Amount,
                CreatedOn = x.CreatedOn,
                WorkflowStarted = taskLookup.ContainsKey(x.AutoId),
                WorkflowTaskId = taskLookup.TryGetValue(x.AutoId, out var taskId) ? taskId : null
            }).ToList()
        });
    }

    [HttpGet("/NdcApplications/Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "NDC Application Details";
        ViewData["ActiveMenu"] = "NDC Applications";

        var application = await _db.ConsumerApplyNdcs.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == id, ct);
        if (application is null)
            return NotFound();

        var workflow = await _db.ApplicationWorkflowInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationType == WorkflowService.ApplicationTypeNdc && x.ApplicationId == id && !x.IsDeleted, ct);

        List<ApplicationWorkflowTask> tasks = workflow is null
            ? []
            : await _db.ApplicationWorkflowTasks
                .Include(x => x.Stage)
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id && !x.IsDeleted)
                .OrderBy(x => x.AssignedOn)
                .ToListAsync(ct);

        List<ApplicationWorkflowHistory> history = workflow is null
            ? []
            : await _db.ApplicationWorkflowHistories
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id)
                .OrderBy(x => x.ActionOn)
                .ToListAsync(ct);

        var documents = await _db.NdcDocuments
            .AsNoTracking()
            .Where(x => x.NdcAutoId == id || x.ConsumerNo == application.ConsumerNo)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync(ct);

        var hasNdcWorkflow = await _db.WorkflowMasters
            .AsNoTracking()
            .AnyAsync(x => x.ApplicationType == WorkflowService.ApplicationTypeNdc && x.IsActive && !x.IsDeleted, ct);

        return View(new NdcApplicationDetailsViewModel
        {
            Application = application,
            Documents = documents,
            WorkflowInstance = workflow,
            WorkflowTasks = tasks,
            WorkflowHistory = history,
            CanStartWorkflow = workflow is null && hasNdcWorkflow
        });
    }

    [HttpPost("/NdcApplications/StartWorkflow/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("NDC Applications.approve")]
    public async Task<IActionResult> StartWorkflow(int id, CancellationToken ct)
    {
        var application = await _db.ConsumerApplyNdcs.FirstOrDefaultAsync(x => x.AutoId == id, ct);
        if (application is null)
            return NotFound();

        var instanceId = await _workflowService.StartWorkflowAsync(
            WorkflowService.ApplicationTypeNdc,
            application.AutoId,
            application.ApplicationNo,
            "Submitted",
            ResolveUserId(),
            User.FindFirstValue("FullName") ?? User.Identity?.Name,
            ResolveRoleName(),
            ct);

        if (instanceId.HasValue)
        {
            application.CurrentStatus = "Submitted";
            application.LastUpdatedBy = ResolveUserId();
            application.LastUpdatedOn = DateTime.Now;
            await _db.SaveChangesAsync(ct);
            TempData["SuccessMessage"] = "NDC workflow started successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "No active NDC workflow is configured.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private int? ResolveUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private int? ResolveRoleId()
        => int.TryParse(User.FindFirstValue("RoleId"), out var roleId) ? roleId : null;

    private string? ResolveRoleName()
        => User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue(AppConstants.Claims.RoleName);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string DisplayStatus(string? status, string? currentStatus, string? finalStatus)
        => !string.IsNullOrWhiteSpace(finalStatus)
            ? finalStatus!
            : !string.IsNullOrWhiteSpace(currentStatus)
                ? currentStatus!
                : !string.IsNullOrWhiteSpace(status)
                    ? status!
                    : "Pending";

    private static string BuildProperty(string? sector, string? block, string? plotNo)
        => string.Join(" / ", new[] { sector, $"{block}-{plotNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static bool IsAssignedToCurrentUser(ApplicationWorkflowTask task, int userId, int roleId, IReadOnlyCollection<int> departmentIds)
    {
        if (task.AssignedUserId.HasValue)
            return task.AssignedUserId.Value == userId;

        if (task.AssignedDepartmentId.HasValue && task.AssignedRoleId.HasValue)
            return task.AssignedRoleId.Value == roleId && departmentIds.Contains(task.AssignedDepartmentId.Value);

        if (task.AssignedDepartmentId.HasValue)
            return departmentIds.Contains(task.AssignedDepartmentId.Value);

        if (task.AssignedRoleId.HasValue)
            return task.AssignedRoleId.Value == roleId;

        return false;
    }
}
