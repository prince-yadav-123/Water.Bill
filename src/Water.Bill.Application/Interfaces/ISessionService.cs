using Water.Bill.Application.DTOs.Session;

namespace Water.Bill.Application.Interfaces;

public interface ISessionService
{
    Task<string> CreateSessionAsync(int userId, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task<IReadOnlyList<UserSessionDto>> GetActiveSessionsAsync(int userId, CancellationToken ct = default);
    Task RevokeSessionAsync(string sessionToken, string reason, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(int userId, string reason, CancellationToken ct = default);
}
