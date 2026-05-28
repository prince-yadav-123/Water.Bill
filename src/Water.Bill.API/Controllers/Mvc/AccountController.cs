using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AccountController(IAuthService authService, ISessionService sessionService, IAuditLogService auditLogService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _auditLogService = auditLogService;
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
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Authority Login";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _authService.LoginAsync(model);
            if (string.Equals(result.User.RoleName, AppConstants.Roles.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Consumers are not allowed to access Authority Login.");
                return View(model);
            }

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
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Dashboard");
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var sessionToken = User.FindFirstValue("SessionToken");
        if (!string.IsNullOrWhiteSpace(sessionToken))
            await _sessionService.RevokeSessionAsync(sessionToken, "ManualLogout");

        await _auditLogService.LogAsync(AuditAction.Logout);
        await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        Response.Cookies.Delete("WaterBill.Authority.Auth");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
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
}
