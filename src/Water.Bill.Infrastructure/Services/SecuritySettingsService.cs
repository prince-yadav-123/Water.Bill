using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Security;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class SecuritySettingsService : ISecuritySettingsService
{
    private readonly ApplicationDbContext _db;

    public SecuritySettingsService(ApplicationDbContext db) => _db = db;

    public async Task<SecuritySettingsDto> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var entity = await _db.Securitysettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);

        return entity is null ? new SecuritySettingsDto { TenantId = tenantId } : Map(entity);
    }

    public async Task<SecuritySettingsDto> SaveAsync(SecuritySettingsDto settings, CancellationToken ct = default)
    {
        var entity = await _db.Securitysettings
            .FirstOrDefaultAsync(x => x.TenantId == settings.TenantId && !x.IsDeleted, ct);

        if (entity is null)
        {
            entity = new Securitysetting { Id = Guid.NewGuid(), TenantId = settings.TenantId, CreatedAt = DateTime.UtcNow };
            _db.Securitysettings.Add(entity);
        }

        entity.SessionTimeoutMinutes = settings.SessionTimeoutMinutes;
        entity.IdleTimeoutMinutes = settings.IdleTimeoutMinutes;
        entity.PasswordMinLength = settings.PasswordMinLength;
        entity.PasswordRequireUppercase = settings.PasswordRequireUppercase;
        entity.PasswordRequireLowercase = settings.PasswordRequireLowercase;
        entity.PasswordRequireDigit = settings.PasswordRequireDigit;
        entity.PasswordRequireSpecialChar = settings.PasswordRequireSpecialChar;
        entity.PasswordExpiryDays = settings.PasswordExpiryDays;
        entity.PasswordHistoryCount = settings.PasswordHistoryCount;
        entity.MaxFailedLoginAttempts = settings.MaxFailedLoginAttempts;
        entity.LockoutDurationMinutes = settings.LockoutDurationMinutes;
        entity.EnableCaptchaAfterFailures = settings.EnableCaptchaAfterFailures;
        entity.CaptchaAfterAttempts = settings.CaptchaAfterAttempts;
        entity.AllowMultipleSessions = settings.AllowMultipleSessions;
        entity.BlockNewLoginOnConflict = settings.BlockNewLoginOnConflict;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static SecuritySettingsDto Map(Securitysetting entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        SessionTimeoutMinutes = entity.SessionTimeoutMinutes,
        IdleTimeoutMinutes = entity.IdleTimeoutMinutes,
        PasswordMinLength = entity.PasswordMinLength,
        PasswordRequireUppercase = entity.PasswordRequireUppercase,
        PasswordRequireLowercase = entity.PasswordRequireLowercase,
        PasswordRequireDigit = entity.PasswordRequireDigit,
        PasswordRequireSpecialChar = entity.PasswordRequireSpecialChar,
        PasswordExpiryDays = entity.PasswordExpiryDays,
        PasswordHistoryCount = entity.PasswordHistoryCount,
        MaxFailedLoginAttempts = entity.MaxFailedLoginAttempts,
        LockoutDurationMinutes = entity.LockoutDurationMinutes,
        EnableCaptchaAfterFailures = entity.EnableCaptchaAfterFailures,
        CaptchaAfterAttempts = entity.CaptchaAfterAttempts,
        AllowMultipleSessions = entity.AllowMultipleSessions,
        BlockNewLoginOnConflict = entity.BlockNewLoginOnConflict
    };
}
