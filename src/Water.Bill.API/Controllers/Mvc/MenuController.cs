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
        private const int ConsumerTenantId = AppConstants.ConsumerTenantId;

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
        return View(await BuildMenuFormViewModelAsync(new Menuitem
        {
            TenantId = DefaultTenantId,
            IsActive = true,
            ShowInSidebar = true
        }, ct));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.add")]
    public async Task<IActionResult> Create(MenuFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Menu Management";
        ValidateMenuItem(model.Item);
        if (!ModelState.IsValid)
        {
            return View(await BuildMenuFormViewModelAsync(model.Item, ct));
        }

        model.Item.TenantId = model.Item.TenantId == 0 ? DefaultTenantId : model.Item.TenantId;
        model.Item.Icon = NormalizeMenuIcon(model.Item.Label, model.Item.Icon);
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

        return View(await BuildMenuFormViewModelAsync(item, ct, id));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Menu Management.edit")]
    public async Task<IActionResult> Edit(int id, MenuFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Menu Management";
        ValidateMenuItem(model.Item);
        if (!ModelState.IsValid)
        {
            return View(await BuildMenuFormViewModelAsync(model.Item, ct, id));
        }

        var item = await _db.Menuitems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (item is null) return NotFound();

        item.ParentId = model.Item.ParentId == 0 ? null : model.Item.ParentId;
        item.TenantId = model.Item.TenantId == 0 ? DefaultTenantId : model.Item.TenantId;
        item.Label = model.Item.Label;
        item.Icon = NormalizeMenuIcon(model.Item.Label, model.Item.Icon);
        item.Url = model.Item.Url;
        item.SectionLabel = model.Item.SectionLabel;
        item.ModuleId = model.Item.ModuleId;
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

    private async Task<IReadOnlyList<Menuitem>> GetParentItemsAsync(int tenantId, CancellationToken ct, int? excludeId = null)
        => await _db.Menuitems
            .Where(x => !x.IsDeleted && x.TenantId == tenantId && (!excludeId.HasValue || x.Id != excludeId.Value))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Label)
            .ToListAsync(ct);

    private async Task<IReadOnlyList<PermissionModule>> GetPermissionModulesAsync(int tenantId, CancellationToken ct)
    {
        var portalScope = tenantId == ConsumerTenantId
            ? AppConstants.PortalScopes.Consumer
            : AppConstants.PortalScopes.Authority;

        return await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted && EF.Property<string>(x, "PortalScope") == portalScope)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    private void ValidateMenuItem(Menuitem item)
    {
        if (string.IsNullOrWhiteSpace(item.Label))
            ModelState.AddModelError("Item.Label", "Menu label is required.");
        if (item.ParentId == item.Id && item.Id != 0)
            ModelState.AddModelError("Item.ParentId", "A menu item cannot be its own parent.");
    }

    private static string NormalizeMenuIcon(string? label, string? icon)
    {
        if (!string.IsNullOrWhiteSpace(icon))
            return icon.Trim();

        var firstVisibleChar = (label ?? string.Empty)
            .Trim()
            .FirstOrDefault(char.IsLetterOrDigit);

        return firstVisibleChar == default
            ? "M"
            : char.ToUpperInvariant(firstVisibleChar).ToString();
    }

    private async Task<MenuFormViewModel> BuildMenuFormViewModelAsync(Menuitem item, CancellationToken ct, int? excludeId = null)
    {
        item.TenantId = item.TenantId == 0 ? DefaultTenantId : item.TenantId;
        var parentItems = await GetParentItemsAsync(item.TenantId, ct, excludeId);
        var permissionModules = await GetPermissionModulesAsync(item.TenantId, ct);
        var allParentOptions = await _db.Menuitems
            .Where(x => !x.IsDeleted && (!excludeId.HasValue || x.Id != excludeId.Value))
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Label)
            .Select(x => new MenuParentOptionViewModel
            {
                Id = x.Id,
                Label = x.Label,
                TenantId = x.TenantId
            })
            .ToListAsync(ct);

        return new MenuFormViewModel
        {
            Item = item,
            ParentItems = parentItems,
            PermissionModules = permissionModules,
            PortalOptions = GetPortalOptions(),
            ParentOptions = allParentOptions,
            PermissionModuleOptions = await _db.PermissionModules
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new MenuPermissionModuleOptionViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PortalScope = EF.Property<string>(x, "PortalScope")
                })
                .ToListAsync(ct),
            IconCategories = GetIconCategories()
        };
    }

    private static IReadOnlyList<SelectOptionViewModel> GetPortalOptions()
        =>
        [
            new() { Value = DefaultTenantId.ToString(), Text = "Authority / Admin Portal" },
            new() { Value = ConsumerTenantId.ToString(), Text = "Consumer Portal" }
        ];

    private static IReadOnlyList<MenuIconCategoryViewModel> GetIconCategories()
        =>
        [
            new()
            {
                Key = "navigation",
                Label = "Navigation",
                Icons =
                [
                    new() { Value = "bi-grid-1x2", Label = "Dashboard" },
                    new() { Value = "bi-list", Label = "Menu" },
                    new() { Value = "bi-diagram-3", Label = "Hierarchy" },
                    new() { Value = "bi-collection", Label = "Modules" },
                    new() { Value = "bi-card-list", Label = "Listing" },
                    new() { Value = "bi-kanban", Label = "Boards" }
                ]
            },
            new()
            {
                Key = "users",
                Label = "Users & Roles",
                Icons =
                [
                    new() { Value = "bi-people", Label = "Users" },
                    new() { Value = "bi-person-badge", Label = "Authority User" },
                    new() { Value = "bi-person-gear", Label = "User Settings" },
                    new() { Value = "bi-shield-lock", Label = "Permissions" },
                    new() { Value = "bi-shield-check", Label = "Security" },
                    new() { Value = "bi-person-vcard", Label = "Profile" }
                ]
            },
            new()
            {
                Key = "billing",
                Label = "Billing & Payments",
                Icons =
                [
                    new() { Value = "bi-receipt", Label = "Bill" },
                    new() { Value = "bi-printer", Label = "Print" },
                    new() { Value = "bi-credit-card", Label = "Payment" },
                    new() { Value = "bi-cash-coin", Label = "Cash" },
                    new() { Value = "bi-wallet2", Label = "Challan" },
                    new() { Value = "bi-bar-chart", Label = "Reports" }
                ]
            },
            new()
            {
                Key = "consumer",
                Label = "Consumers & Requests",
                Icons =
                [
                    new() { Value = "bi-house-door", Label = "Connection" },
                    new() { Value = "bi-droplet", Label = "Water" },
                    new() { Value = "bi-building", Label = "Department" },
                    new() { Value = "bi-file-earmark-text", Label = "Application" },
                    new() { Value = "bi-chat-dots", Label = "Support" },
                    new() { Value = "bi-bell", Label = "Notification" }
                ]
            },
            new()
            {
                Key = "workflow",
                Label = "Workflow & Actions",
                Icons =
                [
                    new() { Value = "bi-diagram-2", Label = "Workflow" },
                    new() { Value = "bi-arrow-left-right", Label = "Transitions" },
                    new() { Value = "bi-check2-square", Label = "Approval" },
                    new() { Value = "bi-exclamation-triangle", Label = "Alert" },
                    new() { Value = "bi-clock-history", Label = "History" },
                    new() { Value = "bi-gear-wide-connected", Label = "Settings" }
                ]
            }
        ];
}
