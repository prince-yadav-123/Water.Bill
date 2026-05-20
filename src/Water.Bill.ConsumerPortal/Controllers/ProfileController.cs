using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class ProfileController : Controller
{
    private const string ContactUpdatePurpose = "ContactUpdate";
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxAttempts = 5;

    private readonly IAuditLogService _auditLogService;
    private readonly ApplicationDbContext _db;
    private readonly IConsumerSmsSender _smsSender;
    private readonly IHostEnvironment _environment;
    private readonly string? _configuredDefaultOtp;

    public ProfileController(
        IAuditLogService auditLogService,
        ApplicationDbContext db,
        IConsumerSmsSender smsSender,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _auditLogService = auditLogService;
        _db = db;
        _smsSender = smsSender;
        _environment = environment;
        _configuredDefaultOtp = NormalizeConfiguredOtp(configuration["Sms:Otp:DefaultOtp"]);
    }

    [HttpGet("/Consumer/Profile")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Profile & Connections";
        await _auditLogService.LogAsync(AuditAction.ProfileViewed);

        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return View(new ConsumerProfileViewModel());

        var primaryConsumer = await GetConsumerAsync(consumerNo);
        if (primaryConsumer is null)
            return View(new ConsumerProfileViewModel { ConsumerNo = consumerNo });

        var linkedConsumers = await GetLinkedConsumersAsync(primaryConsumer);
        var linkedConsumerNos = linkedConsumers.Select(x => x.ConsNo).ToList();

        var latestBills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo != null
                && linkedConsumerNos.Contains(x.ConsNo)
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0)
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .ToListAsync();

        var model = MapProfile(primaryConsumer, linkedConsumers, latestBills);
        return View(model);
    }

    [HttpGet("/Consumer/Profile/UpdateContact")]
    public async Task<IActionResult> UpdateContact()
    {
        ViewData["Title"] = "Update Mobile/Email";

        var consumer = await GetLoggedInConsumerForUpdateAsync();
        if (consumer is null)
            return View(new UpdateContactViewModel());

        return View(BuildUpdateContactModel(consumer));
    }

    [HttpPost("/Consumer/Profile/UpdateContact/SendOtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendContactUpdateOtp(UpdateContactViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Update Mobile/Email";

        var consumer = await GetLoggedInConsumerForUpdateAsync(ct);
        if (consumer is null)
            return View("UpdateContact", new UpdateContactViewModel());

        PrepareContactModel(model, consumer);
        NormalizeContactUpdateInput(model);

        if (!ValidateContactUpdateRequest(model, consumer))
            return View("UpdateContact", model);

        try
        {
            var otpResult = await RequestContactUpdateOtpAsync(consumer, ct);
            model.IsOtpSent = true;
            model.MaskedMobileNo = otpResult.MaskedMobileNo;
            model.ExpiresAt = otpResult.ExpiresAt;
            model.ResendAvailableInSeconds = otpResult.ResendAvailableInSeconds;
            model.DevelopmentOtp = _environment.IsDevelopment() ? otpResult.DevelopmentOtp : null;
            TempData["InfoMessage"] = $"OTP sent to {otpResult.MaskedMobileNo}.";
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View("UpdateContact", model);
    }

    [HttpPost("/Consumer/Profile/UpdateContact/Verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyContactUpdate(UpdateContactViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Update Mobile/Email";

        var consumer = await GetLoggedInConsumerForUpdateAsync(ct);
        if (consumer is null)
            return View("UpdateContact", new UpdateContactViewModel());

        PrepareContactModel(model, consumer);
        NormalizeContactUpdateInput(model);
        model.IsOtpSent = true;

        if (!ValidateContactUpdateRequest(model, consumer))
            return View("UpdateContact", model);

        if (string.IsNullOrWhiteSpace(model.Otp))
        {
            ModelState.AddModelError(nameof(model.Otp), "Enter the OTP sent to your registered mobile number.");
            return View("UpdateContact", model);
        }

        try
        {
            await VerifyContactUpdateOtpAsync(consumer.ConsNo, model.Otp, ct);

            if (!string.IsNullOrWhiteSpace(model.NewMobileNo))
                consumer.MobNo = model.NewMobileNo;

            if (!string.IsNullOrWhiteSpace(model.NewEmail))
                consumer.EmailId = model.NewEmail;

            consumer.ModifyDate = DateTime.Now;
            await _db.SaveChangesAsync(ct);

            TempData["SuccessMessage"] = "Mobile/email updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("UpdateContact", model);
        }
    }

    private async Task<ConsumerDetailsMaster?> GetConsumerAsync(string consumerNo)
        => await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo);

    private async Task<ConsumerDetailsMaster?> GetLoggedInConsumerForUpdateAsync(CancellationToken ct = default)
    {
        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return null;

        return await _db.ConsumerDetailsMasters
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
    }

    private async Task<IReadOnlyList<ConsumerDetailsMaster>> GetLinkedConsumersAsync(ConsumerDetailsMaster primary)
    {
        var mobile = primary.MobNo?.Trim();
        var email = primary.EmailId?.Trim();

        return await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == primary.ConsNo
                || (!string.IsNullOrWhiteSpace(mobile) && x.MobNo == mobile)
                || (!string.IsNullOrWhiteSpace(email) && x.EmailId == email))
            .OrderByDescending(x => x.ConsNo == primary.ConsNo)
            .ThenBy(x => x.ConsNo)
            .Take(10)
            .ToListAsync();
    }

    private string? ResolveConsumerNo()
    {
        var claimConsumerNo = User.FindFirstValue("ConsumerNo")?.Trim();
        if (!string.IsNullOrWhiteSpace(claimConsumerNo))
            return claimConsumerNo.ToUpperInvariant();

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim();
        return !string.IsNullOrWhiteSpace(nameIdentifier) && !Guid.TryParse(nameIdentifier, out _)
            ? nameIdentifier.ToUpperInvariant()
            : null;
    }

    private static ConsumerProfileViewModel MapProfile(
        ConsumerDetailsMaster primary,
        IReadOnlyList<ConsumerDetailsMaster> linkedConsumers,
        IReadOnlyList<JalPrintBillMaster> bills)
    {
        var name = BuildConsumerName(primary);

        return new ConsumerProfileViewModel
        {
            ConsumerNo = primary.ConsNo,
            Name = name,
            Initials = GetInitials(name),
            MobileNo = primary.MobNo,
            Email = primary.EmailId,
            CustomerDuration = BuildDuration(primary.ConnDt ?? primary.EntryDate),
            Connections = linkedConsumers.Select((consumer, index) =>
            {
                var latestBill = bills.FirstOrDefault(x => x.ConsNo == consumer.ConsNo);
                var amount = latestBill?.TotalBillAmt ?? latestBill?.DueAmt ?? 0;
                var lastPaid = latestBill?.LastPaidAmt ?? latestBill?.PaidAmt ?? 0;

                return new ConsumerConnectionCardViewModel
                {
                    ConsumerNo = consumer.ConsNo,
                    Title = BuildPropertyTitle(consumer),
                    Address = BuildAddress(consumer),
                    ConnectionType = ResolveConnectionType(consumer.ConTp),
                    SinceDate = consumer.ConnDt ?? consumer.EntryDate,
                    Status = ResolveServiceStatus(consumer.Status),
                    IsPrimary = index == 0 || consumer.ConsNo == primary.ConsNo,
                    LatestBillNo = latestBill?.BillNo,
                    LatestBillAmount = amount,
                    LatestBillDueDate = latestBill?.BillDueDate ?? latestBill?.DueDate,
                    LatestBillStatus = latestBill is null
                        ? "Not available"
                        : ResolveBillPaymentStatus(latestBill, amount, lastPaid)
                };
            }).ToList()
        };
    }

    private static string BuildConsumerName(ConsumerDetailsMaster consumer)
    {
        var name = string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }
            .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        return string.IsNullOrWhiteSpace(name) ? "Consumer" : name;
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return "C";
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return string.Concat(parts.Take(2).Select(x => x[0])).ToUpperInvariant();
    }

    private static string BuildPropertyTitle(ConsumerDetailsMaster consumer)
    {
        var parts = new[] { consumer.Sector, consumer.BlkNo, consumer.FlatNo }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var title = string.Join(" - ", parts);
        return string.IsNullOrWhiteSpace(title) ? consumer.ConsNo : title;
    }

    private static string BuildAddress(ConsumerDetailsMaster consumer)
    {
        if (!string.IsNullOrWhiteSpace(consumer.ConsAddress))
            return consumer.ConsAddress;

        var parts = new[] { consumer.FlatNo, consumer.BlkNo, consumer.Sector, consumer.VillgaeName }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(", ", parts);
    }

    private static string? BuildDuration(DateTime? since)
    {
        if (!since.HasValue)
            return null;

        var years = Math.Max(0, (DateTime.Today - since.Value.Date).Days / 365);
        return years <= 0 ? "New consumer" : $"Loyal consumer - {years} yr{(years == 1 ? "" : "s")}";
    }

    private static string ResolveServiceStatus(int? status) => status switch
    {
        null => "Not available",
        0 => "Inactive",
        1 => "Active",
        _ => "Active"
    };

    private static string ResolveBillPaymentStatus(JalPrintBillMaster bill, double totalPayable, double lastPaid)
    {
        if (string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bill.PaidStatus, "1", StringComparison.OrdinalIgnoreCase))
            return "Paid";

        if (lastPaid > 0 && totalPayable <= 0)
            return "Paid";

        if (lastPaid > 0)
            return "Partially paid";

        return "Due";
    }

    private static string ResolveConnectionType(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "I" => "Institutional",
        "C" => "Commercial",
        "R" or "S" => "Domestic",
        "T" => "Industrial",
        "V" => "Village",
        "G" => "Group Housing",
        "H" => "Housing",
        null or "" => "Not available",
        _ => value!
    };

    private UpdateContactViewModel BuildUpdateContactModel(ConsumerDetailsMaster consumer)
    {
        var name = BuildConsumerName(consumer);
        var mobile = NormalizeMobileNo(consumer.MobNo);

        return new UpdateContactViewModel
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = name,
            CurrentMobileNo = mobile,
            CurrentEmail = consumer.EmailId,
            MaskedMobileNo = string.IsNullOrWhiteSpace(mobile) ? null : MaskMobileNo(mobile)
        };
    }

    private void PrepareContactModel(UpdateContactViewModel model, ConsumerDetailsMaster consumer)
    {
        var currentMobile = NormalizeMobileNo(consumer.MobNo);
        model.ConsumerNo = consumer.ConsNo;
        model.ConsumerName = BuildConsumerName(consumer);
        model.CurrentMobileNo = currentMobile;
        model.CurrentEmail = consumer.EmailId;
        model.MaskedMobileNo = string.IsNullOrWhiteSpace(currentMobile) ? null : MaskMobileNo(currentMobile);
    }

    private static void NormalizeContactUpdateInput(UpdateContactViewModel model)
    {
        model.NewMobileNo = NormalizeMobileNo(model.NewMobileNo);
        model.NewEmail = string.IsNullOrWhiteSpace(model.NewEmail) ? null : model.NewEmail.Trim();
        model.Otp = string.IsNullOrWhiteSpace(model.Otp)
            ? null
            : new string(model.Otp.Where(char.IsDigit).ToArray());
    }

    private bool ValidateContactUpdateRequest(UpdateContactViewModel model, ConsumerDetailsMaster consumer)
    {
        if (string.IsNullOrWhiteSpace(model.CurrentMobileNo))
        {
            ModelState.AddModelError(string.Empty, "Mobile number is not registered for this consumer. Please contact support.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(model.NewMobileNo) && string.IsNullOrWhiteSpace(model.NewEmail))
        {
            ModelState.AddModelError(string.Empty, "Enter new mobile number, new email, or both.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(model.NewMobileNo) && !IsValidMobileNo(model.NewMobileNo))
            ModelState.AddModelError(nameof(model.NewMobileNo), "Enter a valid 10 digit mobile number.");

        if (!string.IsNullOrWhiteSpace(model.NewEmail) && model.NewEmail.Length > 50)
            ModelState.AddModelError(nameof(model.NewEmail), "Email cannot be more than 50 characters.");

        var currentMobile = NormalizeMobileNo(consumer.MobNo);
        var currentEmail = consumer.EmailId?.Trim();
        var isSameMobile = string.IsNullOrWhiteSpace(model.NewMobileNo)
            || string.Equals(model.NewMobileNo, currentMobile, StringComparison.OrdinalIgnoreCase);
        var isSameEmail = string.IsNullOrWhiteSpace(model.NewEmail)
            || string.Equals(model.NewEmail, currentEmail, StringComparison.OrdinalIgnoreCase);

        if (isSameMobile && isSameEmail)
            ModelState.AddModelError(string.Empty, "Entered mobile/email is already saved on your profile.");

        return ModelState.IsValid;
    }

    private async Task<Water.Bill.Application.DTOs.Consumer.ConsumerOtpRequestResult> RequestContactUpdateOtpAsync(
        ConsumerDetailsMaster consumer,
        CancellationToken ct)
    {
        var normalizedConsumerNo = consumer.ConsNo.Trim().ToUpperInvariant();
        var mobileNo = NormalizeMobileNo(consumer.MobNo);

        if (string.IsNullOrWhiteSpace(mobileNo))
            throw new InvalidOperationException("Mobile number is not registered for this consumer. Please contact support.");

        var now = DateTime.UtcNow;
        var activeOtp = await _db.ConsumerOtpVerifications
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == ContactUpdatePurpose
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (activeOtp is not null)
        {
            var secondsSinceLastOtp = (now - activeOtp.CreatedAt).TotalSeconds;
            if (secondsSinceLastOtp < ResendCooldownSeconds && activeOtp.ExpiresAt > now)
            {
                return new Water.Bill.Application.DTOs.Consumer.ConsumerOtpRequestResult
                {
                    ConsumerNo = normalizedConsumerNo,
                    MaskedMobileNo = MaskMobileNo(mobileNo),
                    ExpiresAt = activeOtp.ExpiresAt,
                    ResendAvailableInSeconds = Math.Max(1, ResendCooldownSeconds - (int)secondsSinceLastOtp)
                };
            }

            activeOtp.IsActive = false;
        }

        var otp = _configuredDefaultOtp ?? GenerateOtp();
        var salt = GenerateSalt();
        var expiresAt = now.AddMinutes(OtpExpiryMinutes);

        _db.ConsumerOtpVerifications.Add(new ConsumerOtpVerification
        {
            Id = Guid.NewGuid(),
            ConsumerNo = normalizedConsumerNo,
            MobileNo = mobileNo,
            OtpHash = HashOtp(otp, salt),
            OtpSalt = salt,
            Purpose = ContactUpdatePurpose,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
        await _smsSender.SendOtpAsync(mobileNo, otp, expiresAt, ct);

        return new Water.Bill.Application.DTOs.Consumer.ConsumerOtpRequestResult
        {
            ConsumerNo = normalizedConsumerNo,
            MaskedMobileNo = MaskMobileNo(mobileNo),
            ExpiresAt = expiresAt,
            ResendAvailableInSeconds = ResendCooldownSeconds,
            DevelopmentOtp = otp
        };
    }

    private async Task VerifyContactUpdateOtpAsync(string consumerNo, string otp, CancellationToken ct)
    {
        var normalizedConsumerNo = consumerNo.Trim().ToUpperInvariant();
        var normalizedOtp = new string((otp ?? string.Empty).Where(char.IsDigit).ToArray());

        if (normalizedOtp.Length != OtpLength)
            throw new InvalidOperationException("Enter the valid 6 digit OTP.");

        var now = DateTime.UtcNow;
        var verification = await _db.ConsumerOtpVerifications
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == ContactUpdatePurpose
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (verification is null)
            throw new InvalidOperationException("OTP was not found. Please request a new OTP.");

        if (verification.ExpiresAt <= now)
        {
            verification.IsActive = false;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("OTP has expired. Please request a new OTP.");
        }

        if (verification.AttemptCount >= MaxAttempts)
        {
            verification.IsActive = false;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Too many invalid OTP attempts. Please request a new OTP.");
        }

        verification.AttemptCount++;

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(verification.OtpHash),
                Encoding.UTF8.GetBytes(HashOtp(normalizedOtp, verification.OtpSalt))))
        {
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Invalid OTP. Please try again.");
        }

        verification.IsVerified = true;
        verification.VerifiedAt = now;
        verification.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    private static bool IsValidMobileNo(string mobileNo)
        => mobileNo.Length == 10 && mobileNo[0] is >= '6' and <= '9' && mobileNo.All(char.IsDigit);

    private static string NormalizeMobileNo(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length >= 10 ? digits[^10..] : string.Empty;
    }

    private static string MaskMobileNo(string mobileNo)
        => mobileNo.Length < 10 ? "registered mobile" : $"******{mobileNo[^4..]}";

    private static string GenerateOtp()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return number.ToString("D6");
    }

    private static string? NormalizeConfiguredOtp(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == OtpLength ? digits : null;
    }

    private static string GenerateSalt()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes);
    }

    private static string HashOtp(string otp, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{otp}"));
        return Convert.ToHexString(bytes);
    }
}
