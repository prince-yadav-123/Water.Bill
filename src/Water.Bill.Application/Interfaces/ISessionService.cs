using Water.Bill.Application.DTOs.Session;

namespace Water.Bill.Application.Interfaces;

public interface ISessionService
{
    Task<string> CreateSessionAsync(Guid userId, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task<IReadOnlyList<UserSessionDto>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeSessionAsync(string sessionToken, string reason, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(Guid userId, string reason, CancellationToken ct = default);
}
