using Water.Bill.Application.DTOs.Menu;

namespace Water.Bill.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<MenuItemDto>> GetMenuTreeAsync(Guid tenantId, CancellationToken ct = default);
    Task<HashSet<string>> GetViewableModulesAsync(Guid roleId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetMenuVisibleModuleIdsAsync(Guid roleId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid roleId, string module, string action, CancellationToken ct = default);
    Task SavePermissionsAsync(Guid roleId, IEnumerable<RolePermissionDto> permissions, CancellationToken ct = default);
}
