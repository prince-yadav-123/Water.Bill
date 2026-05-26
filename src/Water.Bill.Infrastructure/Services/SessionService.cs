using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Session;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;

    public SessionService(ApplicationDbContext db) => _db = db;

    public async Task<string> CreateSessionAsync(int userId, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        _db.Usersessions.Add(new Usersession
        {
            UserId = userId,
            SessionToken = token,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            LastActivityAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return token;
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetActiveSessionsAsync(int userId, CancellationToken ct = default)
    {
        var sessions = await _db.Usersessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive == true && !x.IsDeleted && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.LastActivityAt)
            .ToListAsync(ct);

        return sessions.Select(x => new UserSessionDto
        {
            Id = x.Id,
            UserId = x.UserId,
            SessionToken = x.SessionToken,
            IpAddress = x.IpAddress,
            UserAgent = x.UserAgent,
            IsActive = x.IsActive == true,
            ExpiresAt = x.ExpiresAt,
            LastActivityAt = x.LastActivityAt,
            RevokedAt = x.RevokedAt,
            RevokedReason = x.RevokedReason
        }).ToList();
    }

    public async Task RevokeSessionAsync(string sessionToken, string reason, CancellationToken ct = default)
    {
        var session = await _db.Usersessions.FirstOrDefaultAsync(x => x.SessionToken == sessionToken && x.IsActive == true, ct);
        if (session is null) return;

        session.IsActive = false;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = reason;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllSessionsAsync(int userId, string reason, CancellationToken ct = default)
    {
        var sessions = await _db.Usersessions.Where(x => x.UserId == userId && x.IsActive == true).ToListAsync(ct);
        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = reason;
        }

        await _db.SaveChangesAsync(ct);
    }
}
