using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;

namespace Water.Bill.ConsumerPortal.Controllers;

public class PublicConsumerMobileRegistrationController : Controller
{
    private readonly IConsumerMobileRegistrationService _mobileRegistrationService;

    public PublicConsumerMobileRegistrationController(IConsumerMobileRegistrationService mobileRegistrationService)
    {
        _mobileRegistrationService = mobileRegistrationService;
    }

    [HttpGet("/Consumer/Public/UpdateMobile")]
    public IActionResult Index(string? consumerNo = null)
    {
        ViewData["Title"] = "Register Mobile Number";
        ViewData["IsPublicCompactFlow"] = true;

        return View(new PublicConsumerMobileRegistrationViewModel
        {
            ConsumerNo = consumerNo?.Trim().ToUpperInvariant()
        });
    }

    [HttpPost("/Consumer/Public/UpdateMobile/SendOtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOtp(PublicConsumerMobileRegistrationViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Register Mobile Number";
        ViewData["IsPublicCompactFlow"] = true;

        NormalizeInput(model);
        ClearOtpValidation(model);

        if (!TryValidateModel(model))
            return View("Index", model);

        try
        {
            var eligibility = await _mobileRegistrationService.CheckEligibilityAsync(model.ConsumerNo ?? string.Empty, ct);
            if (!eligibility.ConsumerExists)
            {
                ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer Number not found.");
                return View("Index", model);
            }

            if (!eligibility.IsActiveConsumer)
            {
                ModelState.AddModelError(nameof(model.ConsumerNo), "Only active consumers can update/register mobile number.");
                return View("Index", model);
            }

            if (eligibility.HasRegisteredMobile)
            {
                ModelState.AddModelError(nameof(model.ConsumerNo), "Mobile number is already registered for this Consumer Number.");
                return View("Index", model);
            }

            var otpResult = await _mobileRegistrationService.RequestOtpAsync(model.ConsumerNo ?? string.Empty, model.MobileNo ?? string.Empty, ct);
            model.IsOtpSent = true;
            model.ConsumerNo = otpResult.ConsumerNo;
            model.MaskedMobileNo = otpResult.MaskedMobileNo;
            model.ExpiresAt = otpResult.ExpiresAt;
            model.ResendAvailableInSeconds = otpResult.ResendAvailableInSeconds;
            TempData["InfoMessage"] = $"OTP sent to {otpResult.MaskedMobileNo}.";
            return View("Index", model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", model);
        }
    }

    [HttpPost("/Consumer/Public/UpdateMobile/Verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(PublicConsumerMobileRegistrationViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Register Mobile Number";
        ViewData["IsPublicCompactFlow"] = true;

        NormalizeInput(model);
        model.IsOtpSent = true;
        PopulateOtpDisplay(model);
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
            return View("Index", model);

        try
        {
            await _mobileRegistrationService.UpdateMobileAsync(model.ConsumerNo ?? string.Empty, model.MobileNo ?? string.Empty, model.Otp ?? string.Empty, ct);
            TempData["SuccessMessage"] = "Mobile number updated successfully. You can now continue with consumer login.";
            return RedirectToAction("Login", "Account", new { consumerId = model.ConsumerNo, loginMethod = ConsumerLoginMethods.ConsumerId });
        }
        catch (InvalidOperationException ex)
        {
            PopulateOtpDisplay(model);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", model);
        }
    }

    private static void NormalizeInput(PublicConsumerMobileRegistrationViewModel model)
    {
        model.ConsumerNo = (model.ConsumerNo ?? string.Empty).Trim().ToUpperInvariant();
        model.MobileNo = NormalizeMobileNo(model.MobileNo);
        model.Otp = new string((model.Otp ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private void ClearOtpValidation(PublicConsumerMobileRegistrationViewModel model)
    {
        model.IsOtpSent = false;
        ModelState.Remove(nameof(model.Otp));
    }

    private static string NormalizeMobileNo(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length >= 10 ? digits[^10..] : digits;
    }

    private void PopulateOtpDisplay(PublicConsumerMobileRegistrationViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MobileNo))
            return;

        var normalizedMobile = NormalizeMobileNo(model.MobileNo);
        if (normalizedMobile.Length == 10)
            model.MaskedMobileNo = $"{normalizedMobile[..2]}******{normalizedMobile[^2..]}";
    }
}
