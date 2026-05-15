using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Menu;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _db;

    public PermissionService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<MenuItemDto>> GetMenuTreeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var items = await LoadMenuTreeAsync(tenantId, ct);

        if (items.Count == 0 && tenantId != Guid.Empty)
        {
            items = await LoadMenuTreeAsync(Guid.Empty, ct);
        }

        return items.Select(MapMenuItem).ToList();
    }

    private async Task<List<Menuitem>> LoadMenuTreeAsync(Guid tenantId, CancellationToken ct)
    {
        return await _db.Menuitems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentId == null && x.IsActive == true && !x.IsDeleted)
            .Include(x => x.InverseParent.Where(c => c.IsActive == true && !c.IsDeleted).OrderBy(c => c.Order))
            .OrderBy(x => x.Order)
            .ToListAsync(ct);
    }

    public async Task<HashSet<string>> GetViewableModulesAsync(Guid roleId, CancellationToken ct = default)
    {
        var modules = await _db.Rolepermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && x.CanView && !x.IsDeleted)
            .Select(x => x.Module)
            .ToListAsync(ct);

        return [.. modules];
    }

    public async Task<bool> HasPermissionAsync(Guid roleId, string module, string action, CancellationToken ct = default)
    {
        var permission = await _db.Rolepermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.Module == module && !x.IsDeleted, ct);

        if (permission is null) return false;

        return action.ToLowerInvariant() switch
        {
            "view" => permission.CanView,
            "add" => permission.CanAdd,
            "edit" => permission.CanEdit,
            "delete" => permission.CanDelete,
            "download" => permission.CanDownload,
            "export" => permission.CanExport,
            "approve" => permission.CanApprove,
            "forward" => permission.CanForward,
            "print" => permission.CanPrint,
            _ => false
        };
    }

    public async Task SavePermissionsAsync(Guid roleId, IEnumerable<RolePermissionDto> permissions, CancellationToken ct = default)
    {
        var incoming = permissions.ToList();
        var existing = await _db.Rolepermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(ct);

        foreach (var item in incoming)
        {
            var record = existing.FirstOrDefault(x => x.Module == item.Module);
            if (record is null)
            {
                _db.Rolepermissions.Add(new Rolepermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    Module = item.Module,
                    CanView = item.CanView,
                    CanAdd = item.CanAdd,
                    CanEdit = item.CanEdit,
                    CanDelete = item.CanDelete,
                    CanDownload = item.CanDownload,
                    CanExport = item.CanExport,
                    CanApprove = item.CanApprove,
                    CanForward = item.CanForward,
                    CanPrint = item.CanPrint,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                continue;
            }

            record.CanView = item.CanView;
            record.CanAdd = item.CanAdd;
            record.CanEdit = item.CanEdit;
            record.CanDelete = item.CanDelete;
            record.CanDownload = item.CanDownload;
            record.CanExport = item.CanExport;
            record.CanApprove = item.CanApprove;
            record.CanForward = item.CanForward;
            record.CanPrint = item.CanPrint;
            record.IsDeleted = false;
            record.UpdatedAt = DateTime.UtcNow;
        }

        var incomingModules = incoming.Select(x => x.Module).ToHashSet();
        foreach (var orphan in existing.Where(x => !incomingModules.Contains(x.Module)))
        {
            orphan.IsDeleted = true;
            orphan.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static MenuItemDto MapMenuItem(Menuitem item) => new()
    {
        Id = item.Id,
        TenantId = item.TenantId,
        ParentId = item.ParentId,
        Label = item.Label,
        Icon = item.Icon,
        Url = item.Url,
        SectionLabel = item.SectionLabel,
        Module = item.Module,
        Order = item.Order,
        IsActive = item.IsActive == true,
        OpenInNewTab = item.OpenInNewTab,
        Children = item.InverseParent.OrderBy(x => x.Order).Select(MapMenuItem).ToList()
    };
}
