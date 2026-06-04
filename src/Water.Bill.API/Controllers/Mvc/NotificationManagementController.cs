using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Models;
using Water.Bill.API.Models.NotificationManagement;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class NotificationManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationDispatchService _dispatch;

    public NotificationManagementController(ApplicationDbContext db, INotificationDispatchService dispatch)
    {
        _db = db;
        _dispatch = dispatch;
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(NotificationListFilterViewModel filter, int page = 1, int pageSize = 0, CancellationToken ct = default)
    {
        ViewData["Title"] = "Notification Management";
        ViewData["ActiveMenu"] = "Notification Management";

        var query = _db.NotificationMasters.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.Title.Contains(filter.Search) || x.Message.Contains(filter.Search));

        if (!string.IsNullOrWhiteSpace(filter.TargetAudience))
            query = query.Where(x => x.TargetAudience == filter.TargetAudience);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(x => x.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.Priority))
            query = query.Where(x => x.Priority == filter.Priority);

        if (!string.IsNullOrWhiteSpace(filter.Channel))
            query = query.Where(x => x.Channels.Contains(filter.Channel));

        if (filter.DateFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.DateTo.Value.AddDays(1));

        var validPageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * validPageSize)
            .Take(validPageSize)
            .Select(x => new NotificationListRowViewModel
            {
                Id = x.Id,
                Title = x.Title,
                TargetAudience = x.TargetAudience,
                Channels = x.Channels,
                Priority = x.Priority,
                Status = x.Status,
                NotificationType = x.NotificationType,
                CreatedByName = x.CreatedByName,
                CreatedAt = x.CreatedAt,
                SentAt = x.SentAt,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        ViewBag.Pagination = PaginationViewModel.Create(new Application.Models.PagedResult<NotificationListRowViewModel>
        {
            Items = items, TotalCount = total, Page = page, PageSize = validPageSize
        });
        return View(new NotificationListViewModel
        {
            Items = items,
            Filter = filter,
            TotalCount = total,
            Page = page,
            PageSize = validPageSize
        });
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Create Notification";
        ViewData["ActiveMenu"] = "Notification Management";
        return View(await BuildCreateViewModelAsync(new NotificationCreateViewModel(), ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotificationCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Notification";
        ViewData["ActiveMenu"] = "Notification Management";

        // Must have at least one channel
        if (model.Channels == null || model.Channels.Count == 0)
            ModelState.AddModelError("Channels", "Select at least one channel.");

        // Must have at least one targeting option
        if (!HasValidTarget(model))
            ModelState.AddModelError("", "Select at least one target (All Users, specific role, department, user, or consumer).");

        if (!ModelState.IsValid)
            return View(await BuildCreateViewModelAsync(model, ct));

        var userId = int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId), out var uid) ? uid : 0;
        var userName = User.Identity?.Name ?? "Admin";

        var notif = new NotificationMaster
        {
            Title = model.Title.Trim(),
            Message = model.Message.Trim(),
            NotificationType = model.NotificationType,
            TargetAudience = model.TargetAudience,
            Channels = string.Join(",", model.Channels.Distinct()),
            Priority = model.Priority,
            Status = "Draft",
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            CreatedByUserId = userId,
            CreatedByName = userName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.NotificationMasters.Add(notif);
        await _db.SaveChangesAsync(ct);

        // Build targets
        var targets = BuildTargets(notif.Id, model);
        if (targets.Count > 0)
        {
            _db.NotificationTargets.AddRange(targets);
            await _db.SaveChangesAsync(ct);
        }

        if (model.Action == "Send")
        {
            var result = await _dispatch.SendAsync(notif.Id, ct);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Send failed.";
                return RedirectToAction(nameof(Details), new { id = notif.Id });
            }
            TempData["SuccessMessage"] = $"Notification sent to {result.TotalTargeted} user(s). In-App: {result.InAppSent}, Email: {result.EmailSent}.";
        }
        else
        {
            TempData["SuccessMessage"] = "Notification saved as draft.";
        }

        return RedirectToAction(nameof(Details), new { id = notif.Id });
    }

    // ── Send (from draft) ─────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(long id, CancellationToken ct)
    {
        var result = await _dispatch.SendAsync(id, ct);
        if (!result.Success)
            TempData["ErrorMessage"] = result.ErrorMessage;
        else
            TempData["SuccessMessage"] = $"Sent to {result.TotalTargeted} user(s). In-App: {result.InAppSent}, Email: {result.EmailSent}.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct)
    {
        var notif = await _db.NotificationMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (notif is not null && notif.Status == "Draft")
        {
            notif.Status = "Cancelled";
            await _db.SaveChangesAsync(ct);
            TempData["SuccessMessage"] = "Notification cancelled.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Details ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Notification Details";
        ViewData["ActiveMenu"] = "Notification Management";

        var notif = await _db.NotificationMasters
            .Include(x => x.Targets)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (notif is null) return NotFound();

        var inAppAll = await _db.InAppNotifications.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ReferenceType == "Notification" && x.ReferenceId == id.ToString())
            .GroupBy(x => x.IsRead)
            .Select(x => new { IsRead = x.Key, Count = x.Count() })
            .ToListAsync(ct);

        var vm = new NotificationDetailsViewModel
        {
            Id = notif.Id,
            Title = notif.Title,
            Message = notif.Message,
            NotificationType = notif.NotificationType,
            TargetAudience = notif.TargetAudience,
            Channels = notif.Channels,
            Priority = notif.Priority,
            Status = notif.Status,
            ValidFrom = notif.ValidFrom,
            ValidTo = notif.ValidTo,
            CreatedByName = notif.CreatedByName,
            CreatedAt = notif.CreatedAt,
            SentAt = notif.SentAt,
            IsActive = notif.IsActive,
            Targets = notif.Targets.Where(t => !t.IsDeleted).Select(t => new NotificationTargetRowViewModel
            {
                TargetType = t.TargetType,
                TargetId = t.TargetId,
                TargetName = t.TargetName
            }).ToList(),
            InAppRead = inAppAll.FirstOrDefault(x => x.IsRead)?.Count ?? 0,
            InAppUnread = inAppAll.FirstOrDefault(x => !x.IsRead)?.Count ?? 0
        };
        vm.InAppTotalSent = vm.InAppRead + vm.InAppUnread;

        return View(vm);
    }

    // ── Bell API: unread count ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = ResolveInternalUserId();
        if (userId == 0) return Json(new { count = 0 });
        var count = await _db.InAppNotifications.AsNoTracking()
            .CountAsync(x => x.UserId == userId && x.UserType == "Internal" && !x.IsRead && !x.IsDeleted, ct);
        return Json(new { count });
    }

    // ── Bell API: latest ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetLatest(CancellationToken ct)
    {
        var userId = ResolveInternalUserId();
        if (userId == 0) return Json(Array.Empty<object>());
        var items = await _db.InAppNotifications.AsNoTracking()
            .Where(x => x.UserId == userId && x.UserType == "Internal" && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new { x.Id, x.Title, x.Message, x.IsRead, x.CreatedAt, x.ReferenceNo })
            .ToListAsync(ct);
        return Json(items);
    }

    // ── Bell API: mark read ───────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
    {
        var userId = ResolveInternalUserId();
        if (userId == 0) return Json(new { ok = false });
        var notif = await _db.InAppNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.UserType == "Internal" && !x.IsDeleted, ct);
        if (notif is not null && !notif.IsRead)
        {
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return Json(new { ok = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = ResolveInternalUserId();
        if (userId == 0)
        {
            TempData["NotificationMessage"] = "Unable to mark notifications as read.";
            TempData["NotificationMessageType"] = "danger";
            return RedirectToAction(nameof(MyNotifications));
        }
        var now = DateTime.UtcNow;
        await _db.InAppNotifications
            .Where(x => x.UserId == userId && x.UserType == "Internal" && !x.IsRead && !x.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now), ct);
        TempData["NotificationMessage"] = "All notifications were marked as read successfully.";
        TempData["NotificationMessageType"] = "success";
        return RedirectToAction(nameof(MyNotifications));
    }

    // ── My Notifications (admin bell list page) ───────────────────────────────

    [HttpGet]
    public async Task<IActionResult> MyNotifications(CancellationToken ct)
    {
        ViewData["Title"] = "My Notifications";
        ViewData["ActiveMenu"] = "Notification Management";
        var userId = ResolveInternalUserId();
        var items = userId == 0
            ? new List<InAppNotification>()
            : await _db.InAppNotifications.AsNoTracking()
                .Where(x => x.UserId == userId && x.UserType == "Internal" && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync(ct);
        return View(items);
    }

    // ── Internal user ID resolution ───────────────────────────────────────────
    // Admin cookie sets ClaimTypes.NameIdentifier = Appuser.Id
    // AppConstants.Claims.UserId ("user_id") is only in JWT — not in the admin cookie.
    // So try "user_id" first, then fall back to NameIdentifier (which IS in the cookie).

    private long ResolveInternalUserId()
    {
        var raw = User.FindFirstValue(AppConstants.Claims.UserId)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out var uid) && uid > 0 ? uid : 0;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<NotificationCreateViewModel> BuildCreateViewModelAsync(NotificationCreateViewModel vm, CancellationToken ct)
    {
        vm.RoleOptions = await _db.Approles.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new SelectOption { Value = x.Id, Text = x.Name })
            .ToListAsync(ct);

        vm.DepartmentOptions = await _db.MasterDeptDetails.AsNoTracking()
            .Select(x => new SelectOption { Value = x.Id, Text = x.DeptName ?? x.Id.ToString() })
            .OrderBy(x => x.Text)
            .ToListAsync(ct);

        vm.UserOptions = await _db.Appusers.AsNoTracking()
            .Where(x => x.IsActive == true && !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .Select(x => new SelectOption { Value = x.Id, Text = x.FullName })
            .ToListAsync(ct);

        return vm;
    }

    private static bool HasValidTarget(NotificationCreateViewModel m)
    {
        if (m.TargetAudience == "Consumer")
            return m.AllConsumers || !string.IsNullOrWhiteSpace(m.ConsumerNo) || !string.IsNullOrWhiteSpace(m.ConsumerUserIds);

        return m.AllInternalUsers || m.SelectedUserIds.Count > 0 ||
               m.SelectedRoleIds.Count > 0 || m.SelectedDepartmentIds.Count > 0;
    }

    private static List<NotificationTarget> BuildTargets(long notifId, NotificationCreateViewModel m)
    {
        var targets = new List<NotificationTarget>();

        if (m.TargetAudience == "Consumer")
        {
            if (m.AllConsumers)
                targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "AllConsumers", TargetName = "All Consumers" });

            if (!string.IsNullOrWhiteSpace(m.ConsumerNo))
                foreach (var cn in m.ConsumerNo.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "ConsumerNo", TargetId = cn.ToUpper(), TargetName = cn });

            if (!string.IsNullOrWhiteSpace(m.ConsumerUserIds))
                foreach (var cu in m.ConsumerUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "ConsumerUser", TargetId = cu });
        }
        else
        {
            if (m.AllInternalUsers)
                targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "AllInternalUsers", TargetName = "All Internal Users" });

            foreach (var uid in m.SelectedUserIds)
                targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "InternalUser", TargetId = uid.ToString() });

            foreach (var rid in m.SelectedRoleIds)
                targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "Role", TargetId = rid.ToString() });

            foreach (var did in m.SelectedDepartmentIds)
                targets.Add(new NotificationTarget { NotificationId = notifId, TargetType = "Department", TargetId = did.ToString() });
        }

        return targets;
    }
}
