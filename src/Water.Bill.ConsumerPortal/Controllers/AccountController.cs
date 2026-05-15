using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;

namespace Water.Bill.ConsumerPortal.Controllers;

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

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new ConsumerLoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(ConsumerLoginViewModel model, string? returnUrl = null)
    {
        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        if (model.LoginMethod != ConsumerLoginMethods.UsernameEmail)
        {
            ModelState.AddModelError(string.Empty, "OTP login is ready for integration. Please use Username/Email sign in for now.");
            return View(model);
        }

        try
        {
            var request = new LoginRequestDto
            {
                Username = model.UsernameOrEmail?.Trim() ?? string.Empty,
                Password = model.Password ?? string.Empty,
                RememberMe = model.RememberMe
            };

            var result = await _authService.LoginAsync(request);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();
            var sessionToken = await _sessionService.CreateSessionAsync(result.User.Id, ip, userAgent);

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

            var identity = new ClaimsIdentity(claims, AppConstants.CookieScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AppConstants.CookieScheme, principal, new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
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
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var sessionToken = User.FindFirstValue("SessionToken");
        if (!string.IsNullOrWhiteSpace(sessionToken))
            await _sessionService.RevokeSessionAsync(sessionToken, "ManualLogout");

        await _auditLogService.LogAsync(AuditAction.Logout);
        await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}
