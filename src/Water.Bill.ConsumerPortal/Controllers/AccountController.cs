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
    private readonly ISessionService _sessionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IConsumerOtpService _consumerOtpService;
    private readonly IConsumerAccountService _consumerAccountService;
    private readonly IHostEnvironment _environment;

    public AccountController(
        ISessionService sessionService,
        IAuditLogService auditLogService,
        IConsumerOtpService consumerOtpService,
        IConsumerAccountService consumerAccountService,
        IHostEnvironment environment)
    {
        _sessionService = sessionService;
        _auditLogService = auditLogService;
        _consumerOtpService = consumerOtpService;
        _consumerAccountService = consumerAccountService;
        _environment = environment;
    }

    [HttpGet("/Account/Login")]
    [HttpGet("/Consumer/Login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!User.IsInRole(AppConstants.Roles.Consumer))
            {
                await HttpContext.SignOutAsync(AppConstants.CookieScheme);
                ViewData["Title"] = "Login";
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "You are not allowed to access Consumer Login.");
                return View(new ConsumerLoginViewModel());
            }

            return LocalRedirect("/Consumer/Dashboard");
        }

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new ConsumerLoginViewModel());
    }

    [HttpPost("/Account/Login")]
    [HttpPost("/Consumer/Login")]
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
            var result = await _consumerAccountService.LoginAsync(
                model.UsernameOrEmail?.Trim() ?? string.Empty,
                model.Password ?? string.Empty);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Id.ToString()),
                new(ClaimTypes.Name, result.Username ?? result.ConsumerName),
                new(ClaimTypes.Role, AppConstants.Roles.Consumer),
                new("FullName", result.ConsumerName),
                new("ConsumerNo", result.ConsumerNo),
                new("RoleId", (result.ConsumerRoleId ?? Guid.Empty).ToString())
            };

            if (!string.IsNullOrWhiteSpace(result.Email))
                claims.Add(new Claim(ClaimTypes.Email, result.Email));

            if (!string.IsNullOrWhiteSpace(result.MobileNo))
                claims.Add(new Claim("MobileNo", result.MobileNo));

            var identity = new ClaimsIdentity(claims, AppConstants.CookieScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AppConstants.CookieScheme, principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

            return LocalRedirect(ResolvePostLoginRedirect(returnUrl));
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("/Account/VerifyOtp")]
    [HttpGet("/Consumer/VerifyOtp")]
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

    [HttpPost("/Account/VerifyOtp")]
    [HttpPost("/Consumer/VerifyOtp")]
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

            return LocalRedirect(ResolvePostLoginRedirect(returnUrl));
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

    private string ResolvePostLoginRedirect(string? returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
            return "/Consumer/Dashboard";

        var path = returnUrl!
            .Split('?', '#')[0]
            .TrimEnd('/');

        if (string.IsNullOrWhiteSpace(path) ||
            path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/Consumer/Login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/Account/VerifyOtp", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/Consumer/VerifyOtp", StringComparison.OrdinalIgnoreCase))
        {
            return "/Consumer/Dashboard";
        }

        return returnUrl!;
    }
}
