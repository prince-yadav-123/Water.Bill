using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class WorkflowsController : Controller
{
    private readonly ApplicationDbContext _db;

    public WorkflowsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Workflow Master";
        ViewData["ActiveMenu"] = "Workflow Master";
        var rows = await _db.WorkflowMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.ApplicationType).ThenBy(x => x.WorkflowName).ToListAsync(ct);
        return View(rows);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Workflow";
        ViewData["ActiveMenu"] = "Workflow Master";
        return View("Form", new WorkflowMaster { ApplicationType = "NewConnection", IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkflowMaster model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Workflow";
        ViewData["ActiveMenu"] = "Workflow Master";
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);
        model.WorkflowName = model.WorkflowName.Trim();
        model.ApplicationType = model.ApplicationType.Trim();
        model.CreatedOn = DateTime.Now;
        _db.WorkflowMasters.Add(model);
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Stages), new { workflowId = model.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Workflow";
        ViewData["ActiveMenu"] = "Workflow Master";
        var model = await _db.WorkflowMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkflowMaster model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Workflow";
        ViewData["ActiveMenu"] = "Workflow Master";
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);
        var entity = await _db.WorkflowMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();
        entity.WorkflowName = model.WorkflowName.Trim();
        entity.ApplicationType = model.ApplicationType.Trim();
        entity.IsActive = model.IsActive;
        entity.UpdatedOn = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Stages(int workflowId, CancellationToken ct)
    {
        ViewData["Title"] = "Workflow Stages";
        ViewData["ActiveMenu"] = "Workflow Master";
        var workflow = await _db.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workflowId && !x.IsDeleted, ct);
        if (workflow is null) return NotFound();
        ViewBag.Workflow = workflow;
        var rows = await _db.WorkflowStages
            .Include(x => x.Department)
            .AsNoTracking()
            .Where(x => x.WorkflowId == workflowId && !x.IsDeleted)
            .OrderBy(x => x.StageOrder)
            .ToListAsync(ct);
        ViewBag.Notifications = await _db.WorkflowStageNotifications
            .AsNoTracking()
            .Where(x => rows.Select(stage => stage.Id).Contains(x.WorkflowStageId) && x.EventType == "StageAssigned" && !x.IsDeleted)
            .ToDictionaryAsync(x => x.WorkflowStageId, ct);
        var roleIds = rows.Where(x => x.ApproverRoleId.HasValue).Select(x => x.ApproverRoleId!.Value).Distinct().ToList();
        var userIds = rows.Where(x => x.ApproverUserId.HasValue).Select(x => x.ApproverUserId!.Value).Distinct().ToList();
        ViewBag.RoleNames = await _db.Approles.AsNoTracking().Where(x => roleIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        ViewBag.UserNames = await _db.Appusers.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.FullName ?? x.Username ?? x.Id.ToString(), ct);
        return View(rows);
    }

    public async Task<IActionResult> CreateStage(int workflowId, CancellationToken ct)
    {
        ViewData["Title"] = "Create Stage";
        ViewData["ActiveMenu"] = "Workflow Master";
        await LoadStageLists(null, null, null, ct);
        var nextOrder = await _db.WorkflowStages
            .Where(x => x.WorkflowId == workflowId && !x.IsDeleted)
            .MaxAsync(x => (int?)x.StageOrder, ct) is { } maxOrder
                ? maxOrder + 1
                : 1;
        LoadNotificationDefaults(null);
        return View("StageForm", new WorkflowStage { WorkflowId = workflowId, StageOrder = nextOrder, IsActive = true, CanApprove = true, CanReject = true, CanSendCorrection = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStage(WorkflowStage model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Stage";
        ViewData["ActiveMenu"] = "Workflow Master";
        await LoadStageLists(model.DepartmentId, model.ApproverRoleId, model.ApproverUserId, ct);
        SuppressStageNavigationValidation();
        ValidateStage(model);
        if (!ModelState.IsValid) return View("StageForm", model);
        model.StageName = model.StageName.Trim();
        model.CreatedOn = DateTime.Now;
        _db.WorkflowStages.Add(model);
        await _db.SaveChangesAsync(ct);
        await SaveStageNotificationAsync(model.Id, ct);
        return RedirectToAction(nameof(Stages), new { workflowId = model.WorkflowId });
    }

    public async Task<IActionResult> EditStage(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Stage";
        ViewData["ActiveMenu"] = "Workflow Master";
        var model = await _db.WorkflowStages.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (model is not null)
        {
            await LoadStageLists(model.DepartmentId, model.ApproverRoleId, model.ApproverUserId, ct);
            var notification = await _db.WorkflowStageNotifications.AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowStageId == id && x.EventType == "StageAssigned" && !x.IsDeleted, ct);
            LoadNotificationDefaults(notification);
        }
        return model is null ? NotFound() : View("StageForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStage(int id, WorkflowStage model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Stage";
        ViewData["ActiveMenu"] = "Workflow Master";
        await LoadStageLists(model.DepartmentId, model.ApproverRoleId, model.ApproverUserId, ct);
        SuppressStageNavigationValidation();
        ValidateStage(model);
        if (!ModelState.IsValid) return View("StageForm", model);
        var entity = await _db.WorkflowStages.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();
        entity.StageName = model.StageName.Trim();
        entity.StageOrder = model.StageOrder;
        entity.DepartmentId = model.DepartmentId;
        entity.ApproverRoleId = model.ApproverRoleId;
        entity.ApproverUserId = model.ApproverUserId;
        entity.ApprovalType = model.ApprovalType;
        entity.CanApprove = model.CanApprove;
        entity.CanReject = model.CanReject;
        entity.CanSendCorrection = false;
        entity.CanForward = model.CanForward;
        entity.IsFinalStage = model.IsFinalStage;
        entity.SlaDays = model.SlaDays;
        entity.IsActive = model.IsActive;
        entity.UpdatedOn = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        await SaveStageNotificationAsync(entity.Id, ct);
        return RedirectToAction(nameof(Stages), new { workflowId = entity.WorkflowId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveStage(int id, string direction, CancellationToken ct)
    {
        var stage = await _db.WorkflowStages.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (stage is null) return NotFound();

        var sibling = string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase)
            ? await _db.WorkflowStages
                .Where(x => x.WorkflowId == stage.WorkflowId && !x.IsDeleted && x.StageOrder < stage.StageOrder)
                .OrderByDescending(x => x.StageOrder)
                .FirstOrDefaultAsync(ct)
            : await _db.WorkflowStages
                .Where(x => x.WorkflowId == stage.WorkflowId && !x.IsDeleted && x.StageOrder > stage.StageOrder)
                .OrderBy(x => x.StageOrder)
                .FirstOrDefaultAsync(ct);

        if (sibling is not null)
        {
            (stage.StageOrder, sibling.StageOrder) = (sibling.StageOrder, stage.StageOrder);
            stage.UpdatedOn = DateTime.Now;
            sibling.UpdatedOn = DateTime.Now;
            await _db.SaveChangesAsync(ct);
        }

        return RedirectToAction(nameof(Stages), new { workflowId = stage.WorkflowId });
    }

    [HttpGet]
    public async Task<IActionResult> FilterUsers(int? departmentId, int? roleId, CancellationToken ct)
    {
        var users = await GetFilteredUsersQuery(departmentId, roleId)
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Username)
            .Select(x => new
            {
                id = x.Id,
                name = string.IsNullOrWhiteSpace(x.FullName) ? x.Username : x.FullName
            })
            .ToListAsync(ct);

        return Json(users);
    }

    private async Task LoadStageLists(int? departmentId, int? roleId, int? selectedUserId, CancellationToken ct)
    {
        ViewBag.Departments = new SelectList(await _db.MasterDeptDetails.AsNoTracking().Where(x => x.Status == "1").OrderBy(x => x.DeptName).ToListAsync(ct), "Id", "DeptName");
        ViewBag.Roles = new SelectList(await _db.Approles.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct), "Id", "Name");
        var users = await GetFilteredUsersQuery(departmentId, roleId)
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Username)
            .ToListAsync(ct);

        if (selectedUserId.HasValue && users.All(x => x.Id != selectedUserId.Value))
        {
            var selectedUser = await _db.Appusers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == selectedUserId.Value && x.IsActive != false && !x.IsDeleted, ct);
            if (selectedUser is not null)
                users.Add(selectedUser);
        }

        ViewBag.Users = new SelectList(users.Select(x => new
        {
            x.Id,
            Name = string.IsNullOrWhiteSpace(x.FullName) ? x.Username : x.FullName
        }), "Id", "Name", selectedUserId);
    }

    private IQueryable<Appuser> GetFilteredUsersQuery(int? departmentId, int? roleId)
    {
        var query = _db.Appusers
            .AsNoTracking()
            .Where(x => x.IsActive != false && !x.IsDeleted);

        if (roleId.HasValue)
            query = query.Where(x => x.RoleId == roleId.Value);

        if (departmentId.HasValue)
        {
            var departmentUserIds = _db.AuthorityUserDepartments
                .Where(x => x.DepartmentId == departmentId.Value && x.IsActive && !x.IsDeleted)
                .Select(x => x.UserId);
            query = query.Where(x => departmentUserIds.Contains(x.Id));
        }

        return query;
    }

    private void LoadNotificationDefaults(WorkflowStageNotification? notification)
    {
        ViewBag.SendEmail = notification?.SendEmail ?? false;
        ViewBag.SendSms = notification?.SendSms ?? false;
        ViewBag.SendWhatsApp = notification?.SendWhatsApp ?? false;
        ViewBag.SendInAppNotification = notification?.SendInAppNotification ?? true;
    }

    private async Task SaveStageNotificationAsync(int stageId, CancellationToken ct)
    {
        var notification = await _db.WorkflowStageNotifications.FirstOrDefaultAsync(x => x.WorkflowStageId == stageId && x.EventType == "StageAssigned" && !x.IsDeleted, ct);
        if (notification is null)
        {
            notification = new WorkflowStageNotification
            {
                WorkflowStageId = stageId,
                EventType = "StageAssigned",
                IsActive = true
            };
            _db.WorkflowStageNotifications.Add(notification);
        }

        notification.SendEmail = Request.Form.ContainsKey("SendEmail");
        notification.SendSms = Request.Form.ContainsKey("SendSms");
        notification.SendWhatsApp = Request.Form.ContainsKey("SendWhatsApp");
        notification.SendInAppNotification = Request.Form.ContainsKey("SendInAppNotification");
        notification.IsActive = notification.SendEmail || notification.SendSms || notification.SendWhatsApp || notification.SendInAppNotification;
        await _db.SaveChangesAsync(ct);
    }

    private void Validate(WorkflowMaster model)
    {
        if (string.IsNullOrWhiteSpace(model.WorkflowName)) ModelState.AddModelError(nameof(model.WorkflowName), "Workflow name is required.");
        if (string.IsNullOrWhiteSpace(model.ApplicationType)) ModelState.AddModelError(nameof(model.ApplicationType), "Application type is required.");
    }

    private void ValidateStage(WorkflowStage model)
    {
        if (string.IsNullOrWhiteSpace(model.StageName)) ModelState.AddModelError(nameof(model.StageName), "Stage name is required.");
        if (model.StageOrder <= 0) ModelState.AddModelError(nameof(model.StageOrder), "Stage order is required.");
        var approvalType = model.ApprovalType?.Trim();

        if (string.Equals(approvalType, "SpecificUser", StringComparison.OrdinalIgnoreCase) && !model.ApproverUserId.HasValue)
            ModelState.AddModelError(nameof(model.ApproverUserId), "Specific user is required when approval type is Specific User.");

        if (string.Equals(approvalType, "DepartmentRole", StringComparison.OrdinalIgnoreCase)
            && !model.DepartmentId.HasValue
            && !model.ApproverRoleId.HasValue)
            ModelState.AddModelError(nameof(model.ApprovalType), "Department or role is required for department role-based approval.");

        if (string.Equals(approvalType, "AllApprovers", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.ApprovalType), "All Approvers workflow is not implemented yet. Please use Any One Approver, Specific User, or Department Role Based.");

        if (!model.ApproverUserId.HasValue && !model.DepartmentId.HasValue && !model.ApproverRoleId.HasValue)
            ModelState.AddModelError(nameof(model.ApproverUserId), "Select at least one assignment target: department, role, or specific user.");
    }

    private void SuppressStageNavigationValidation()
    {
        ModelState.Remove(nameof(WorkflowStage.Workflow));
        ModelState.Remove(nameof(WorkflowStage.Department));
    }
}
