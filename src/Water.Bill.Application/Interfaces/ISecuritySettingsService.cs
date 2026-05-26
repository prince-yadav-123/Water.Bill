using Water.Bill.Application.DTOs.Security;

namespace Water.Bill.Application.Interfaces;

public interface ISecuritySettingsService
{
    Task<SecuritySettingsDto> GetByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<SecuritySettingsDto> SaveAsync(SecuritySettingsDto settings, CancellationToken ct = default);
}
