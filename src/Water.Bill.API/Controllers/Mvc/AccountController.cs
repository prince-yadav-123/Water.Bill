using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;

namespace Water.Bill.API.Controllers.Mvc;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly IAuditLogService _auditLogService;
    private readonly ISecuritySettingsService _securitySettingsService;
    private readonly IAuthorityLoginOtpService _authorityLoginOtpService;

    public AccountController(
        IAuthService authService,
        ISessionService sessionService,
        IAuditLogService auditLogService,
        ISecuritySettingsService securitySettingsService,
        IAuthorityLoginOtpService authorityLoginOtpService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _auditLogService = auditLogService;
        _securitySettingsService = securitySettingsService;
        _authorityLoginOtpService = authorityLoginOtpService;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var cookieAuth = await HttpContext.AuthenticateAsync(AppConstants.CookieScheme);
        if (cookieAuth.Succeeded)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Authority Login";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequestDto());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null, CancellationToken ct = default)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Authority Login";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) return View(model);

        try
        {
            var validatedUser = await _authService.ValidateAuthorityCredentialsAsync(model, ct);
            if (string.Equals(validatedUser.RoleName, AppConstants.Roles.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Consumers are not allowed to access Authority Login.");
                return View(model);
            }

            var securitySettings = await _securitySettingsService.GetByTenantAsync(AppConstants.DefaultTenantId, ct);
            if (!securitySettings.AuthorityLoginTwoFactorEnabled)
            {
                var result = await _authService.CompleteAuthorityLoginAsync(validatedUser.UserId, ct);
                await SignInAuthorityAsync(result, model.RememberMe);
                return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Dashboard");
            }

            var challenge = await _authorityLoginOtpService.RequestOtpAsync(validatedUser, ct);
            return RedirectToAction(nameof(VerifyTwoFactor), new
            {
                challengeToken = challenge.ChallengeToken,
                rememberMe = model.RememberMe,
                returnUrl
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet]
    public async Task<IActionResult> VerifyTwoFactor(string challengeToken, bool rememberMe = true, string? returnUrl = null, CancellationToken ct = default)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Authority Login Verification";
        ViewData["ReturnUrl"] = returnUrl;

        var challenge = await _authorityLoginOtpService.GetChallengeAsync(challengeToken, ct);
        if (challenge is null)
        {
            TempData["ErrorMessage"] = "2FA verification session has expired. Please login again.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        return View(new AuthorityLoginOtpViewModel
        {
            ChallengeToken = challenge.ChallengeToken,
            DeliverySummary = challenge.DeliverySummary,
            ExpiresAt = challenge.ExpiresAt,
            ResendAvailableInSeconds = challenge.ResendAvailableInSeconds,
            RememberMe = rememberMe
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTwoFactor(AuthorityLoginOtpViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Authority Login Verification";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            var currentChallenge = await _authorityLoginOtpService.GetChallengeAsync(model.ChallengeToken, ct);
            if (currentChallenge is not null)
            {
                model.DeliverySummary = currentChallenge.DeliverySummary;
                model.ExpiresAt = currentChallenge.ExpiresAt;
                model.ResendAvailableInSeconds = currentChallenge.ResendAvailableInSeconds;
            }

            return View(model);
        }

        try
        {
            var verified = await _authorityLoginOtpService.VerifyOtpAsync(model.ChallengeToken, model.Otp, ct);
            var result = await _authService.CompleteAuthorityLoginAsync(verified.UserId, ct);
            await SignInAuthorityAsync(result, model.RememberMe);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Dashboard");
        }
        catch (InvalidOperationException ex)
        {
            var currentChallenge = await _authorityLoginOtpService.GetChallengeAsync(model.ChallengeToken, ct);
            if (currentChallenge is not null)
            {
                model.DeliverySummary = currentChallenge.DeliverySummary;
                model.ExpiresAt = currentChallenge.ExpiresAt;
                model.ResendAvailableInSeconds = currentChallenge.ResendAvailableInSeconds;
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendTwoFactorOtp(string challengeToken, bool rememberMe = true, string? returnUrl = null, CancellationToken ct = default)
    {
        try
        {
            await _authorityLoginOtpService.ResendOtpAsync(challengeToken, ct);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(VerifyTwoFactor), new { challengeToken, rememberMe, returnUrl });
    }

    [Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var sessionToken = User.FindFirstValue("SessionToken");
        if (!string.IsNullOrWhiteSpace(sessionToken))
            await _sessionService.RevokeSessionAsync(sessionToken, "ManualLogout");

        await _auditLogService.LogAsync(AuditAction.Logout);
        await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        Response.Cookies.Delete("WaterBill.Authority.Auth");
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied(string? permission = null, string? returnUrl = null)
    {
        return RedirectToAction("Index", "Unauthorized", new { permission, returnUrl });
    }

    [HttpGet("/Account/AuthStatus")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AuthStatus()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        var cookieAuth = await HttpContext.AuthenticateAsync(AppConstants.CookieScheme);
        if (!cookieAuth.Succeeded)
            return Unauthorized(new
            {
                isAuthenticated = false,
                redirectUrl = "/Account/Login"
            });

        return Json(new
        {
            isAuthenticated = true,
            redirectUrl = "/Dashboard"
        });
    }

    private async Task SignInAuthorityAsync(LoginResponseDto result, bool rememberMe)
    {
        var sessionToken = await _sessionService.CreateSessionAsync(
            result.User.Id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new(ClaimTypes.Name, result.User.Username),
            new(ClaimTypes.Email, result.User.Email),
            new(ClaimTypes.Role, result.User.RoleName),
            new("FullName", result.User.FullName),
            new("RoleId", result.User.RoleId.ToString()),
            new("JwtToken", result.AccessToken),
            new("SessionToken", sessionToken)
        };

        await HttpContext.SignInAsync(
            AppConstants.CookieScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, AppConstants.CookieScheme)),
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
    }
}
