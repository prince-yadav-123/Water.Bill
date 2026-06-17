using Microsoft.AspNetCore.Http;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using System.Security.Claims;

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
        var user = http?.User;
        var userId = ResolveUserId(user);
        var isConsumer = string.Equals(user?.FindFirstValue(ClaimTypes.Role), AppConstants.Roles.Consumer, StringComparison.OrdinalIgnoreCase);
        var resolvedModule = string.IsNullOrWhiteSpace(module)
            ? AuditLogDisplayHelper.InferModuleFromPath(http?.Request.Path.Value, isConsumer)
            : module;

        _db.Auditlogs.Add(new Auditlog
        {
            Action = (int)action,
            UserId = userId,
            Module = resolvedModule,
            EntityId = entityId,
            Details = details,
            Success = success,
            Username = user?.FindFirstValue(AppConstants.Claims.Username) ?? user?.Identity?.Name,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request.Headers.UserAgent.ToString()
        });

        await _db.SaveChangesAsync(ct);
    }

    private static int? ResolveUserId(ClaimsPrincipal? user)
    {
        var raw = user?.FindFirstValue(AppConstants.Claims.UserId)
                  ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : null;
    }
}
