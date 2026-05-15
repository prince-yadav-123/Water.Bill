using Microsoft.AspNetCore.Http;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AuditAction action, string? module = null, string? entityId = null, string? details = null, bool success = true, CancellationToken ct = default)
    {
        var http = _httpContextAccessor.HttpContext;
        _db.Auditlogs.Add(new Auditlog
        {
            Action = (int)action,
            Module = module,
            EntityId = entityId,
            Details = details,
            Success = success,
            Username = http?.User?.Identity?.Name,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request.Headers.UserAgent.ToString()
        });

        await _db.SaveChangesAsync(ct);
    }
}
