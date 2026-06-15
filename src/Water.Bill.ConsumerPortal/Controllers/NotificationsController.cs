using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.ConsumerPortal.Controllers;

/// <summary>
/// Serves In-App notifications for the Consumer Portal.
/// Only returns notifications where UserType = "Consumer" and UserId matches
/// the current consumer — consumers can never see Internal notifications.
/// </summary>
[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _db;

    public NotificationsController(ApplicationDbContext db) => _db = db;

    // ── Full list page ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "My Notifications";
        ViewData["ActiveMenu"] = "Notifications";

        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0) return View(new List<Infrastructure.Data.Entities.InAppNotification>());

        var items = await _db.InAppNotifications.AsNoTracking()
            .Where(x => x.UserId == userId
                     && x.UserType == "Consumer"
                     && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        return View(items);
    }

    // ── Bell API: unread count (called every 60 s by bell JS) ─────────────────

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0) return Json(new { count = 0 });

        var count = await _db.InAppNotifications.AsNoTracking()
            .CountAsync(x => x.UserId == userId
                          && x.UserType == "Consumer"
                          && !x.IsRead
                          && !x.IsDeleted, ct);

        return Json(new { count });
    }

    // ── Bell API: latest 10 ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetLatest(CancellationToken ct)
    {
        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0) return Json(Array.Empty<object>());

        var items = await _db.InAppNotifications.AsNoTracking()
            .Where(x => x.UserId == userId
                     && x.UserType == "Consumer"
                     && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Message,
                x.IsRead,
                x.CreatedAt,
                x.ReferenceNo,
                x.RedirectUrl,
                x.ReferenceType,
                x.ReferenceId
            })
            .ToListAsync(ct);

        return Json(items.Select(x =>
        {
            var nav = NotificationNavigationHelper.ResolveForConsumer(x.RedirectUrl, x.ReferenceType, x.ReferenceId, x.ReferenceNo);
            return new
            {
                x.Id,
                x.Title,
                x.Message,
                x.IsRead,
                x.CreatedAt,
                x.ReferenceNo,
                HasUrl = nav.HasTarget,
                OpenUrl = nav.HasTarget ? Url.Action(nameof(Open), new { id = x.Id }) : null,
                OpenInNewTab = nav.OpenInNewTab
            };
        }));
    }

    // ── Mark single notification as read ─────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
    {
        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0) return Json(new { ok = false, reason = "unauthenticated" });

        // Security: always filter by UserId + UserType — cannot access another user's notification
        var notif = await _db.InAppNotifications
            .FirstOrDefaultAsync(x => x.Id == id
                                   && x.UserId == userId
                                   && x.UserType == "Consumer"
                                   && !x.IsDeleted, ct);

        if (notif is not null && !notif.IsRead)
        {
            notif.IsRead  = true;
            notif.ReadAt  = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return Json(new { ok = true });
    }

    [HttpGet]
    public async Task<IActionResult> Open(long id, CancellationToken ct)
    {
        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0)
        {
            TempData["NotificationMessage"] = "Please sign in again to open this notification.";
            TempData["NotificationMessageType"] = "warning";
            return RedirectToAction(nameof(Index));
        }

        var notif = await _db.InAppNotifications
            .FirstOrDefaultAsync(x => x.Id == id
                                   && x.UserId == userId
                                   && x.UserType == "Consumer"
                                   && !x.IsDeleted, ct);

        if (notif is null)
        {
            TempData["NotificationMessage"] = "This notification is not available anymore.";
            TempData["NotificationMessageType"] = "warning";
            return RedirectToAction(nameof(Index));
        }

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        var nav = NotificationNavigationHelper.ResolveForConsumer(notif.RedirectUrl, notif.ReferenceType, notif.ReferenceId, notif.ReferenceNo);
        if (!nav.HasTarget || string.IsNullOrWhiteSpace(nav.Url))
        {
            TempData["NotificationMessage"] = "This notification does not have a valid destination.";
            TempData["NotificationMessageType"] = "info";
            return RedirectToAction(nameof(Index));
        }

        if (Url.IsLocalUrl(nav.Url))
            return LocalRedirect(nav.Url);

        if (Uri.TryCreate(nav.Url, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return Redirect(nav.Url);
        }

        TempData["NotificationMessage"] = "This notification link is not allowed.";
        TempData["NotificationMessageType"] = "warning";
        return RedirectToAction(nameof(Index));
    }

    // ── Mark all as read ─────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = await ResolveConsumerUserIdAsync(ct);
        if (userId == 0)
        {
            TempData["NotificationMessage"] = "Unable to mark notifications as read.";
            TempData["NotificationMessageType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        await _db.InAppNotifications
            .Where(x => x.UserId == userId
                     && x.UserType == "Consumer"
                     && !x.IsRead
                     && !x.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now), ct);

        TempData["NotificationMessage"] = "All notifications were marked as read successfully.";
        TempData["NotificationMessageType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UserId resolution — handles both consumer login paths:
    //
    //   Path A (direct login):  NameIdentifier = ConsumerUser.Id  (integer string)
    //   Path B (OTP login):     NameIdentifier = ConsumerNo       (e.g. "C0001001")
    //                           "ConsumerNo" claim also set
    //
    // ConsumerNo is always stored UPPERCASED (NormalizeConsumerNo in OTP service).
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<long> ResolveConsumerUserIdAsync(CancellationToken ct)
    {
        var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Path A: NameIdentifier is the integer ConsumerUser.Id
        if (long.TryParse(nameId, out var parsed) && parsed > 0)
            return parsed;

        // Path B: NameIdentifier is a ConsumerNo — look up via "ConsumerNo" claim
        var consumerNoClaim = User.FindFirstValue("ConsumerNo")?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(consumerNoClaim))
        {
            var id = await _db.ConsumerUsers.AsNoTracking()
                .Where(x => x.ConsumerNo == consumerNoClaim && !x.IsDeleted)
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (id is > 0) return id.Value;
        }

        // Path B fallback: try NameIdentifier itself as ConsumerNo
        if (!string.IsNullOrWhiteSpace(nameId))
        {
            var upper = nameId.Trim().ToUpperInvariant();
            var id = await _db.ConsumerUsers.AsNoTracking()
                .Where(x => x.ConsumerNo == upper && !x.IsDeleted)
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (id is > 0) return id.Value;
        }

        return 0;
    }
}
