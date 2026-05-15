using Water.Bill.Core.Enums;

namespace Water.Bill.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(AuditAction action, string? module = null, string? entityId = null, string? details = null, bool success = true, CancellationToken ct = default);
}
