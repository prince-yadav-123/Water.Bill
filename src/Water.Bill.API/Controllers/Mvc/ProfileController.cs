using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public ProfileController(ApplicationDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "My Profile";
        var userId = GetUserId();
        var user = await _db.Appusers.Include(x => x.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, ct);
        if (user is null) return RedirectToAction("Login", "Account");
        await _auditLogService.LogAsync(AuditAction.ProfileViewed, "Profile", entityId: userId.ToString(), ct: ct);
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string fullName, string? phoneNumber, CancellationToken ct)
    {
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == GetUserId() && !x.IsDeleted, ct);
        if (user is null) return RedirectToAction("Login", "Account");
        user.FullName = fullName?.Trim() ?? user.FullName;
        user.PhoneNumber = phoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }

    private Guid GetUserId()
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
}
