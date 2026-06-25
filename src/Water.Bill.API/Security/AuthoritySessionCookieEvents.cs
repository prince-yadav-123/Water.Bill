using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Security;

public sealed class AuthoritySessionCookieEvents : CookieAuthenticationEvents
{
    private readonly ApplicationDbContext _db;

    public AuthoritySessionCookieEvents(ApplicationDbContext db)
    {
        _db = db;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (principal?.Identity?.IsAuthenticated != true)
            return;

        var userIdText = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionToken = principal.FindFirstValue("SessionToken");

        if (!int.TryParse(userIdText, out var userId) || string.IsNullOrWhiteSpace(sessionToken))
        {
            await RejectAsync(context, "Your session is no longer valid. Please login again.");
            return;
        }

        var sessionInfo = await _db.Usersessions
            .AsNoTracking()
            .Where(x => x.SessionToken == sessionToken)
            .Select(x => new
            {
                x.UserId,
                x.IsActive,
                x.IsDeleted,
                x.ExpiresAt,
                x.RevokedReason,
                UserIsActive = x.User.IsActive,
                UserIsDeleted = x.User.IsDeleted
            })
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (sessionInfo is null
            || sessionInfo.UserId != userId
            || sessionInfo.IsDeleted
            || sessionInfo.IsActive != true
            || sessionInfo.ExpiresAt <= DateTime.UtcNow
            || sessionInfo.UserIsDeleted
            || sessionInfo.UserIsActive != true)
        {
            var message = string.Equals(sessionInfo?.RevokedReason, "PasswordChanged", StringComparison.OrdinalIgnoreCase)
                ? "Your password was changed. Please login again."
                : "Your session is no longer valid. Please login again.";

            await RejectAsync(context, message);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context, string message)
    {
        context.RejectPrincipal();
        context.HttpContext.Response.Cookies.Append(
            "WaterBill.Authority.AuthMessage",
            Uri.EscapeDataString(message),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            });

        await context.HttpContext.SignOutAsync(AppConstants.CookieScheme);
    }
}
