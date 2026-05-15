using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.DTOs.User;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IAuditLogService _audit;

    public AuthService(ApplicationDbContext db, IConfiguration config, IAuditLogService audit)
    {
        _db = db;
        _config = config;
        _audit = audit;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var username = dto.Username.Trim();
        var user = await _db.Appusers
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => !x.IsDeleted && (x.Username == username || x.Email == username), ct);

        if (user is null)
        {
            await RecordFailedAttemptAsync(username, null, "InvalidCredentials", ct);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");

        if (user.IsActive != true || !VerifyPassword(dto.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);

            await RecordFailedAttemptAsync(username, user.Id, "InvalidCredentials", ct);
            await _audit.LogAsync(user.FailedLoginCount >= 5 ? AuditAction.AccountLocked : AuditAction.LoginFailed,
                details: $"Failed login for {username}", success: false, ct: ct);
            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        user.FailedLoginCount = 0;
        user.LockoutUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var (token, expiresAt) = GenerateJwt(user);
        await _audit.LogAsync(AuditAction.LoginSuccess, details: $"Login: {user.Username}", ct: ct);

        return new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                RoleId = user.RoleId,
                RoleName = user.Role.Name,
                IsActive = user.IsActive == true
            }
        };
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes("WATER_BILL_SALT_" + password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private static bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;

    private async Task RecordFailedAttemptAsync(string username, Guid? userId, string reason, CancellationToken ct)
    {
        _db.Loginattempts.Add(new Loginattempt
        {
            Username = username,
            UserId = userId,
            Success = false,
            FailureReason = reason
        });
        await _db.SaveChangesAsync(ct);
    }

    private (string token, DateTime expiresAt) GenerateJwt(Appuser user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = jwtSection["Issuer"] ?? "Water.Bill";
        var audience = jwtSection["Audience"] ?? "Water.Bill";
        var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var minutes) ? minutes : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(AppConstants.Claims.UserId, user.Id.ToString()),
            new Claim(AppConstants.Claims.Username, user.Username),
            new Claim(AppConstants.Claims.Email, user.Email),
            new Claim(AppConstants.Claims.RoleName, user.Role.Name),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("RoleId", user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
