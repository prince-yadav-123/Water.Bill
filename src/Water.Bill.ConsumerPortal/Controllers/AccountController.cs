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

public class AccountController : ConsumerPortalControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IConsumerOtpService _consumerOtpService;
    private readonly IConsumerAccountService _consumerAccountService;
    private readonly ISecuritySettingsService _securitySettingsService;

    public AccountController(
        ISessionService sessionService,
        IAuditLogService auditLogService,
        IConsumerOtpService consumerOtpService,
        IConsumerAccountService consumerAccountService,
        ISecuritySettingsService securitySettingsService,
        IErrorLogService errorLogService)
        : base(errorLogService)
    {
        _sessionService = sessionService;
        _auditLogService = auditLogService;
        _consumerOtpService = consumerOtpService;
        _consumerAccountService = consumerAccountService;
        _securitySettingsService = securitySettingsService;
    }

    [HttpGet("/Account/Login")]
    [HttpGet("/Consumer/Login")]
    public async Task<IActionResult> Login(string? returnUrl = null, string? consumerId = null, string? loginMethod = null)
    {
        var securitySettings = await _securitySettingsService.GetByTenantAsync(AppConstants.DefaultTenantId);

        if (User.Identity?.IsAuthenticated == true)
        {
            if (!User.IsInRole(AppConstants.Roles.Consumer))
            {
                await HttpContext.SignOutAsync(AppConstants.CookieScheme);
                ViewData["Title"] = "Login";
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "You are not allowed to access Consumer Login.");
                return View(new ConsumerLoginViewModel
                {
                    ConsumerId = consumerId,
                    LoginMethod = NormalizeLoginMethod(loginMethod),
                    ConsumerNumberOtpEnabled = securitySettings.ConsumerLoginOtpEnabled
                });
            }

            return LocalRedirect("/Consumer/Dashboard");
        }

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new ConsumerLoginViewModel
        {
            ConsumerId = consumerId,
            LoginMethod = NormalizeLoginMethod(loginMethod),
            ConsumerNumberOtpEnabled = securitySettings.ConsumerLoginOtpEnabled
        });
    }

    [HttpPost("/Account/Login")]
    [HttpPost("/Consumer/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(ConsumerLoginViewModel model, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppConstants.Roles.Consumer))
            return LocalRedirect("/Consumer/Dashboard");

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;

        var securitySettings = await _securitySettingsService.GetByTenantAsync(AppConstants.DefaultTenantId);
        model.ConsumerNumberOtpEnabled = securitySettings.ConsumerLoginOtpEnabled;

        if (!ModelState.IsValid)
            return View(model);

        if (model.LoginMethod == ConsumerLoginMethods.ConsumerId)
        {
            if (!securitySettings.ConsumerLoginOtpEnabled)
            {
                try
                {
                    var result = await _consumerAccountService.LoginByConsumerNoAsync(model.ConsumerId ?? string.Empty);

                    await BuildConsumerPrincipalAsync(
                        result.Id > 0 ? result.Id : null,
                        result.Username ?? result.ConsumerName,
                        result.ConsumerName,
                        result.ConsumerNo,
                        result.ConsumerRoleId,
                        result.Email,
                        result.MobileNo,
                        model.RememberMe);

                    await _auditLogService.LogAsync(
                        AuditAction.LoginSuccess,
                        AuditLogDisplayHelper.ConsumerAuthenticationModule,
                        entityId: result.Id > 0 ? result.Id.ToString() : result.ConsumerNo,
                        details: $"Consumer number login: {result.ConsumerNo}");

                    TempData["SuccessMessage"] = "Login successful.";
                    return LocalRedirect(ResolvePostLoginRedirect(returnUrl));
                }
                catch (UnauthorizedAccessException ex)
                {
                    await LogHandledErrorAsync(ex, StatusCodes.Status403Forbidden);
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
            }

            try
            {
                var otpResult = await _consumerOtpService.RequestOtpAsync(model.ConsumerId ?? string.Empty, usePimsMobileLookup: true);
                SetOtpTempData(otpResult);
                return RedirectToAction(nameof(VerifyOtp), new { consumerNo = otpResult.ConsumerNo, returnUrl });
            }
            catch (InvalidOperationException ex)
            {
                await LogHandledErrorAsync(ex);
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
                await LogHandledErrorAsync(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        try
        {
            var result = await _consumerAccountService.LoginAsync(
                model.UsernameOrEmail?.Trim() ?? string.Empty,
                model.Password ?? string.Empty);

            var principal = await BuildConsumerPrincipalAsync(
                result.Id,
                result.Username ?? result.ConsumerName,
                result.ConsumerName,
                result.ConsumerNo,
                result.ConsumerRoleId,
                result.Email,
                result.MobileNo,
                model.RememberMe);

            await _auditLogService.LogAsync(
                AuditAction.LoginSuccess,
                AuditLogDisplayHelper.ConsumerAuthenticationModule,
                entityId: result.Id.ToString(),
                details: $"Consumer login: {result.ConsumerNo}");

            return LocalRedirect(ResolvePostLoginRedirect(returnUrl));
        }
        catch (UnauthorizedAccessException ex)
        {
            await LogHandledErrorAsync(ex, StatusCodes.Status403Forbidden);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("/Account/VerifyOtp")]
    [HttpGet("/Consumer/VerifyOtp")]
    public IActionResult VerifyOtp(string consumerNo, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppConstants.Roles.Consumer))
            return LocalRedirect("/Consumer/Dashboard");

        ViewData["Title"] = "Verify OTP";
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction(nameof(Login));

        return View(new ConsumerOtpViewModel
        {
            ConsumerNo = consumerNo.Trim().ToUpperInvariant(),
            MaskedMobileNo = TempData["OtpMaskedMobile"] as string,
            ExpiresAt = TryGetTempDate("OtpExpiresAt"),
            ResendAvailableInSeconds = TryGetTempInt("OtpResendSeconds")
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

            await BuildConsumerPrincipalAsync(
                result.ConsumerUserId,
                result.ConsumerName,
                result.ConsumerName,
                result.ConsumerNo,
                result.ConsumerRoleId,
                result.Email,
                result.MobileNo,
                true);

            await _auditLogService.LogAsync(
                AuditAction.LoginSuccess,
                AuditLogDisplayHelper.ConsumerAuthenticationModule,
                entityId: result.ConsumerUserId?.ToString(),
                details: $"Consumer OTP login verified: {result.ConsumerNo}");

            return LocalRedirect(ResolvePostLoginRedirect(returnUrl));
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
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
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpPost("/Account/Logout")]
    [HttpPost("/Consumer/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var sessionToken = User.FindFirstValue("SessionToken");
        if (!string.IsNullOrWhiteSpace(sessionToken))
            await _sessionService.RevokeSessionAsync(sessionToken, "ManualLogout");

        await _auditLogService.LogAsync(AuditAction.Logout);
        await HttpContext.SignOutAsync(AppConstants.CookieScheme);
        HttpContext.Session.Clear();
        Response.Cookies.Delete("WaterBill.ConsumerPortal.Auth");
        Response.Cookies.Delete("WaterBill.PublicNewConnection.Session");
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

    private void SetOtpTempData(Water.Bill.Application.DTOs.Consumer.ConsumerOtpRequestResult otpResult)
    {
        TempData["OtpMaskedMobile"] = otpResult.MaskedMobileNo;
        TempData["OtpExpiresAt"] = otpResult.ExpiresAt.ToString("O");
        TempData["OtpResendSeconds"] = otpResult.ResendAvailableInSeconds.ToString();
    }

    private DateTime? TryGetTempDate(string key)
        => DateTime.TryParse(TempData[key] as string, out var value) ? value : null;

    private int TryGetTempInt(string key)
        => int.TryParse(TempData[key] as string, out var value) ? value : 0;

    private static string NormalizeLoginMethod(string? loginMethod)
    {
        return loginMethod switch
        {
            ConsumerLoginMethods.MobileOtp => ConsumerLoginMethods.MobileOtp,
            ConsumerLoginMethods.UsernameEmail => ConsumerLoginMethods.UsernameEmail,
            _ => ConsumerLoginMethods.ConsumerId
        };
    }

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

    private async Task<ClaimsPrincipal> BuildConsumerPrincipalAsync(
        int? consumerUserId,
        string name,
        string fullName,
        string consumerNo,
        int? consumerRoleId,
        string? email,
        string? mobileNo,
        bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, consumerUserId?.ToString() ?? consumerNo),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, AppConstants.Roles.Consumer),
            new("FullName", fullName),
            new("ConsumerNo", consumerNo),
            new("RoleId", (consumerRoleId ?? 0).ToString())
        };

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(ClaimTypes.Email, email));

        if (!string.IsNullOrWhiteSpace(mobileNo))
            claims.Add(new Claim("MobileNo", mobileNo));

        if (consumerUserId.HasValue && consumerUserId.Value > 0)
        {
            var sessionToken = await _sessionService.CreateSessionAsync(
                consumerUserId.Value,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

            claims.Add(new Claim("SessionToken", sessionToken));
        }

        var identity = new ClaimsIdentity(claims, AppConstants.CookieScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(AppConstants.CookieScheme, principal, new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

        return principal;
    }
}
