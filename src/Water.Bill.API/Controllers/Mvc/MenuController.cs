using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class MenuController : Controller
{
    private const int DefaultTenantId = AppConstants.DefaultTenantId;

    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;

    public MenuController(
        ApplicationDbContext db,
        IPermissionService permissionService,
        IAuditLogService auditLogService)
    {
        _db = db;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
    }

    [RequirePermission("Menu Management.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Menu Management";
        ViewData["ActiveMenu"] = "Menu Management";
        var items = await _db.Menuitems
            .Include(x => x.Parent)
            .Include(x => x.PermissionModule)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Label)
            .ToListAsync(ct);

        return View(new MenuIndexViewModel { Items = items });
    }

    [HttpGet, RequirePermission("Menu Management.add")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Create Menu Item";
        ViewData["ActiveMenu"] = "Menu Management";
        return View(new MenuFormViewModel
        {
            Item = new Menuitem { TenantId = DefaultTenantId, IsActive = true, ShowInSidebar = true },
            ParentItems = await GetParentItemsAsync(ct),
            PermissionModules = await GetPermissionModulesAsync(ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.add")]
    public async Task<IActionResult> Create(MenuFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Menu Management";
        ValidateMenuItem(model.Item);
        if (!ModelState.IsValid)
        {
            model.ParentItems = await GetParentItemsAsync(ct);
            model.PermissionModules = await GetPermissionModulesAsync(ct);
            return View(model);
        }

        await ApplyModuleNameAsync(model.Item, ct);
        model.Item.TenantId = model.Item.TenantId == 0 ? DefaultTenantId : model.Item.TenantId;
        model.Item.CreatedAt = DateTime.UtcNow;
        model.Item.IsDeleted = false;
        _db.Menuitems.Add(model.Item);
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(AuditAction.MenuChanged, AppConstants.Modules.MenuManagement, model.Item.Id.ToString(), "Menu item created.", ct: ct);
        TempData["SuccessMessage"] = "Menu item created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequirePermission("Menu Management.edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Menu Item";
        ViewData["ActiveMenu"] = "Menu Management";
        var item = await _db.Menuitems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (item is null) return NotFound();

        return View(new MenuFormViewModel
        {
            Item = item,
            ParentItems = await GetParentItemsAsync(ct, id),
            PermissionModules = await GetPermissionModulesAsync(ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.edit")]
    public async Task<IActionResult> Edit(int id, MenuFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Menu Management";
        ValidateMenuItem(model.Item);
        if (!ModelState.IsValid)
        {
            model.ParentItems = await GetParentItemsAsync(ct, id);
            model.PermissionModules = await GetPermissionModulesAsync(ct);
            return View(model);
        }

        var item = await _db.Menuitems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (item is null) return NotFound();

        await ApplyModuleNameAsync(model.Item, ct);
        item.ParentId = model.Item.ParentId == 0 ? null : model.Item.ParentId;
        item.Label = model.Item.Label;
        item.Icon = model.Item.Icon;
        item.Url = model.Item.Url;
        item.SectionLabel = model.Item.SectionLabel;
        item.ModuleId = model.Item.ModuleId;
        item.Module = model.Item.Module;
        item.Order = model.Item.Order;
        item.ShowInSidebar = model.Item.ShowInSidebar;
        item.IsActive = model.Item.IsActive ?? true;
        item.OpenInNewTab = model.Item.OpenInNewTab;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(AuditAction.MenuChanged, AppConstants.Modules.MenuManagement, id.ToString(), "Menu item updated.", ct: ct);
        TempData["SuccessMessage"] = "Menu item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _db.Menuitems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (item is not null)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await _auditLogService.LogAsync(AuditAction.MenuChanged, AppConstants.Modules.MenuManagement, id.ToString(), "Menu item deleted.", ct: ct);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.edit")]
    public async Task<IActionResult> Reorder([FromBody] IReadOnlyList<MenuReorderViewModel> items, CancellationToken ct)
    {
        if (items.Count == 0)
            return Json(new { success = true });

        var ids = items.Select(x => x.Id).ToHashSet();
        var records = await _db.Menuitems.Where(x => ids.Contains(x.Id) && !x.IsDeleted).ToListAsync(ct);

        foreach (var item in items)
        {
            var record = records.FirstOrDefault(x => x.Id == item.Id);
            if (record is null) continue;

            record.Order = item.Order;
            record.ParentId = item.ParentId == 0 || item.ParentId == record.Id ? null : item.ParentId;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return Json(new { success = true });
    }

    [HttpGet, RequirePermission("Menu Management.view")]
    public async Task<IActionResult> Tree(CancellationToken ct)
        => Json(await _permissionService.GetMenuTreeAsync(DefaultTenantId, ct));

    private async Task<IReadOnlyList<Menuitem>> GetParentItemsAsync(CancellationToken ct, int? excludeId = null)
        => await _db.Menuitems
            .Where(x => !x.IsDeleted && (!excludeId.HasValue || x.Id != excludeId.Value))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Label)
            .ToListAsync(ct);

    private async Task<IReadOnlyList<PermissionModule>> GetPermissionModulesAsync(CancellationToken ct)
        => await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    private async Task ApplyModuleNameAsync(Menuitem item, CancellationToken ct)
    {
        if (!item.ModuleId.HasValue)
        {
            item.Module = null;
            return;
        }

        item.Module = await _db.PermissionModules
            .Where(x => x.Id == item.ModuleId.Value && x.IsActive && !x.IsDeleted)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(ct);
    }

    private void ValidateMenuItem(Menuitem item)
    {
        if (string.IsNullOrWhiteSpace(item.Label))
            ModelState.AddModelError("Item.Label", "Menu label is required.");
        if (item.ParentId == item.Id && item.Id != 0)
            ModelState.AddModelError("Item.ParentId", "A menu item cannot be its own parent.");
    }
}
