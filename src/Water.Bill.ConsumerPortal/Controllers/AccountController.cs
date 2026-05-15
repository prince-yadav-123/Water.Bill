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
    private readonly IConsumerOtpService _consumerOtpService;
    private readonly IHostEnvironment _environment;

    public AccountController(
        IAuthService authService,
        ISessionService sessionService,
        IAuditLogService auditLogService,
        IConsumerOtpService consumerOtpService,
        IHostEnvironment environment)
    {
        _authService = authService;
        _sessionService = sessionService;
        _auditLogService = auditLogService;
        _consumerOtpService = consumerOtpService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!User.IsInRole(AppConstants.Roles.Consumer))
            {
                await HttpContext.SignOutAsync(AppConstants.CookieScheme);
                ViewData["Title"] = "Login";
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "You are not allowed to access the Consumer Portal.");
                return View(new ConsumerLoginViewModel());
            }

            return RedirectToAction("Index", "Dashboard");
        }

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

        if (model.LoginMethod == ConsumerLoginMethods.ConsumerId)
        {
            try
            {
                var otpResult = await _consumerOtpService.RequestOtpAsync(model.ConsumerId ?? string.Empty);
                SetOtpTempData(otpResult);
                return RedirectToAction(nameof(VerifyOtp), new { consumerNo = otpResult.ConsumerNo, returnUrl });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        if (model.LoginMethod == ConsumerLoginMethods.MobileOtp)
        {
            try
            {
                var otpResult = await _consumerOtpService.RequestOtpByMobileAsync(model.MobileNumber ?? string.Empty);
                SetOtpTempData(otpResult);
                return RedirectToAction(nameof(VerifyOtp), new { consumerNo = otpResult.ConsumerNo, returnUrl });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
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
            if (!string.Equals(result.User.RoleName, AppConstants.Roles.Consumer, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "You are not allowed to access the Consumer Portal.");
                return View(model);
            }

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

    [HttpGet]
    public IActionResult VerifyOtp(string consumerNo, string? returnUrl = null)
    {
        ViewData["Title"] = "Verify OTP";
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction(nameof(Login));

        return View(new ConsumerOtpViewModel
        {
            ConsumerNo = consumerNo.Trim().ToUpperInvariant(),
            MaskedMobileNo = TempData["OtpMaskedMobile"] as string,
            ExpiresAt = TryGetTempDate("OtpExpiresAt"),
            ResendAvailableInSeconds = TryGetTempInt("OtpResendSeconds"),
            DevelopmentOtp = _environment.IsDevelopment() ? TempData["OtpDevelopmentCode"] as string : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(ConsumerOtpViewModel model, string? returnUrl = null)
    {
        ViewData["Title"] = "Verify OTP";
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _consumerOtpService.VerifyOtpAsync(model.ConsumerNo, model.Otp ?? string.Empty);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.ConsumerNo),
                new(ClaimTypes.Name, result.ConsumerName),
                new(ClaimTypes.Role, AppConstants.Roles.Consumer),
                new("FullName", result.ConsumerName),
                new("ConsumerNo", result.ConsumerNo),
                new("RoleId", (result.ConsumerRoleId ?? Guid.Empty).ToString())
            };

            if (!string.IsNullOrWhiteSpace(result.Email))
                claims.Add(new Claim(ClaimTypes.Email, result.Email));

            var identity = new ClaimsIdentity(claims, AppConstants.CookieScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AppConstants.CookieScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Dashboard");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string consumerNo)
    {
        try
        {
            var otpResult = await _consumerOtpService.RequestOtpAsync(consumerNo);
            SetOtpTempData(otpResult);
        }
        catch (InvalidOperationException ex)
        {
            TempData["OtpError"] = ex.Message;
        }

        return RedirectToAction(nameof(VerifyOtp), new { consumerNo });
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

    private void SetOtpTempData(Water.Bill.Application.DTOs.Consumer.ConsumerOtpRequestResult otpResult)
    {
        TempData["OtpMaskedMobile"] = otpResult.MaskedMobileNo;
        TempData["OtpExpiresAt"] = otpResult.ExpiresAt.ToString("O");
        TempData["OtpResendSeconds"] = otpResult.ResendAvailableInSeconds.ToString();

        if (_environment.IsDevelopment())
            TempData["OtpDevelopmentCode"] = otpResult.DevelopmentOtp;
    }

    private DateTime? TryGetTempDate(string key)
        => DateTime.TryParse(TempData[key] as string, out var value) ? value : null;

    private int TryGetTempInt(string key)
        => int.TryParse(TempData[key] as string, out var value) ? value : 0;
}
