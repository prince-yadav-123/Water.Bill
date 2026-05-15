using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ProfileController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public ProfileController(IAuditLogService auditLogService) => _auditLogService = auditLogService;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Profile";
        await _auditLogService.LogAsync(AuditAction.ProfileViewed);
        return View();
    }
}
