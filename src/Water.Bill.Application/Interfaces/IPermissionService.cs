using Water.Bill.Application.DTOs.Menu;

namespace Water.Bill.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<MenuItemDto>> GetMenuTreeAsync(int tenantId, CancellationToken ct = default);
    Task<HashSet<string>> GetViewableModulesAsync(int roleId, CancellationToken ct = default);
    Task<HashSet<int>> GetMenuVisibleModuleIdsAsync(int roleId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int roleId, string module, string action, CancellationToken ct = default);
    Task SavePermissionsAsync(
        int roleId,
        IEnumerable<RolePermissionDto> permissions,
        IReadOnlyCollection<int>? scopedModuleIds = null,
        CancellationToken ct = default);
}
