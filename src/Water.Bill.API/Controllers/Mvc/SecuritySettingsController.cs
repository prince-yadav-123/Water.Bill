using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.API.Filters;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.DTOs.Security;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class SecuritySettingsController : Controller
{
    private const int DefaultTenantId = AppConstants.DefaultTenantId;

    private readonly ISecuritySettingsService _securitySettingsService;
    private readonly IAuditLogService _auditLogService;

    public SecuritySettingsController(
        ISecuritySettingsService securitySettingsService,
        IAuditLogService auditLogService)
    {
        _securitySettingsService = securitySettingsService;
        _auditLogService = auditLogService;
    }

    [HttpGet, RequirePermission("Security Settings.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Security Settings";
        ViewData["ActiveMenu"] = "Security Settings";
        var settings = await _securitySettingsService.GetByTenantAsync(DefaultTenantId, ct);
        return View(new SecuritySettingsViewModel { Settings = settings });
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Security Settings.edit")]
    public async Task<IActionResult> Index(SecuritySettingsDto settings, CancellationToken ct)
    {
        ViewData["Title"] = "Security Settings";
        ViewData["ActiveMenu"] = "Security Settings";
        settings.TenantId = settings.TenantId == 0 ? DefaultTenantId : settings.TenantId;

        if (!ModelState.IsValid)
            return View(new SecuritySettingsViewModel { Settings = settings });

        var saved = await _securitySettingsService.SaveAsync(settings, ct);
        await _auditLogService.LogAsync(
            AuditAction.SecuritySettingsChanged,
            AppConstants.Modules.SecuritySettings,
            saved.Id.ToString(),
            "Security settings updated.",
            ct: ct);

        TempData["SuccessMessage"] = "Security settings saved.";
        return RedirectToAction(nameof(Index));
    }
}
