using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class PermissionModulesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public PermissionModulesController(ApplicationDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    [RequirePermission("Permission Modules.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Permission Modules";
        var modules = await _db.PermissionModules
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return View(modules);
    }

    [RequirePermission("Permission Modules.view")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        ViewData["Title"] = "View Permission Module";
        var module = await _db.PermissionModules.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return module is null ? NotFound() : View(module);
    }

    [HttpGet, RequirePermission("Permission Modules.add")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Permission Module";
        return View(new PermissionModule());
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Permission Modules.add")]
    public async Task<IActionResult> Create(PermissionModule model, CancellationToken ct)
    {
        await ValidateModuleAsync(model, ct: ct);
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.Name = model.Name.Trim();
        model.IsDeleted = false;
        _db.PermissionModules.Add(model);
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(AuditAction.PermissionChanged, AppConstants.Modules.PermissionModules, model.Id.ToString(), "Permission module created.", ct: ct);
        TempData["SuccessMessage"] = "Permission module created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequirePermission("Permission Modules.edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Permission Module";
        var module = await _db.PermissionModules.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return module is null ? NotFound() : View(module);
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Permission Modules.edit")]
    public async Task<IActionResult> Edit(Guid id, PermissionModule model, CancellationToken ct)
    {
        var module = await _db.PermissionModules.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (module is null) return NotFound();

        await ValidateModuleAsync(model, id, ct);
        if (module.IsActive && !model.IsActive && await IsModuleInUseAsync(id, ct))
        {
            ModelState.AddModelError(nameof(model.IsActive), "This module is used by active menus or permissions and cannot be deactivated.");
        }

        if (!ModelState.IsValid) return View(model);

        module.Name = model.Name.Trim();
        module.IsActive = model.IsActive;
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(AuditAction.PermissionChanged, AppConstants.Modules.PermissionModules, id.ToString(), "Permission module updated.", ct: ct);
        TempData["SuccessMessage"] = "Permission module updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Permission Modules.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var module = await _db.PermissionModules.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (module is null) return RedirectToAction(nameof(Index));

        if (await IsModuleInUseAsync(id, ct))
        {
            TempData["ErrorMessage"] = "This module is used by active menus or permissions and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        module.IsDeleted = true;
        module.IsActive = false;
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(AuditAction.PermissionChanged, AppConstants.Modules.PermissionModules, id.ToString(), "Permission module deleted.", ct: ct);
        TempData["SuccessMessage"] = "Permission module deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateModuleAsync(PermissionModule model, Guid? currentId = null, CancellationToken ct = default)
    {
        var name = model.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(nameof(model.Name), "Module name is required.");
            return;
        }

        var normalized = name.ToLower();
        var exists = await _db.PermissionModules.AnyAsync(x =>
            !x.IsDeleted
            && (!currentId.HasValue || x.Id != currentId.Value)
            && x.Name.Trim().ToLower() == normalized,
            ct);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "A permission module with this name already exists.");
        }
    }

    private async Task<bool> IsModuleInUseAsync(Guid moduleId, CancellationToken ct)
    {
        var usedByMenu = await _db.Menuitems.AnyAsync(x => x.ModuleId == moduleId && x.IsActive == true && !x.IsDeleted, ct);
        if (usedByMenu) return true;

        return await _db.Rolepermissions.AnyAsync(x => x.ModuleId == moduleId && !x.IsDeleted, ct);
    }
}
