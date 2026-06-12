using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.DTOs.Consumer;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class ConsumerMobileRegistrationService : IConsumerMobileRegistrationService
{
    private const string Purpose = "ConsumerMobileRegistration";
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxAttempts = 5;

    private readonly ApplicationDbContext _db;
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<ConsumerMobileRegistrationService> _logger;
    private readonly string? _configuredDefaultOtp;

    public ConsumerMobileRegistrationService(
        ApplicationDbContext db,
        ICommunicationService communicationService,
        IConfiguration configuration,
        ILogger<ConsumerMobileRegistrationService> logger)
    {
        _db = db;
        _communicationService = communicationService;
        _configuredDefaultOtp = NormalizeConfiguredOtp(configuration["Communication:Sms:DefaultOtp"])
            ?? NormalizeConfiguredOtp(configuration["Sms:Otp:DefaultOtp"]);
        _logger = logger;
    }

    public async Task<ConsumerMobileRegistrationEligibilityResult> CheckEligibilityAsync(string consumerNo, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            return new ConsumerMobileRegistrationEligibilityResult();

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == normalizedConsumerNo, ct);

        if (consumer is null)
        {
            return new ConsumerMobileRegistrationEligibilityResult
            {
                ConsumerNo = normalizedConsumerNo,
                ConsumerExists = false
            };
        }

        return new ConsumerMobileRegistrationEligibilityResult
        {
            ConsumerNo = normalizedConsumerNo,
            ConsumerExists = true,
            IsActiveConsumer = IsActiveConsumer(consumer),
            HasRegisteredMobile = !string.IsNullOrWhiteSpace(NormalizeMobileNo(consumer.MobNo))
        };
    }

    public async Task<ConsumerOtpRequestResult> RequestOtpAsync(string consumerNo, string mobileNo, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        var normalizedMobileNo = NormalizeMobileNo(mobileNo);

        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            throw new InvalidOperationException("Please enter Consumer Number.");

        if (!IsValidMobileNo(normalizedMobileNo))
            throw new InvalidOperationException("Please enter a valid 10 digit mobile number.");

        var consumer = await _db.ConsumerDetailsMasters
            .FirstOrDefaultAsync(x => x.ConsNo == normalizedConsumerNo, ct);

        if (consumer is null)
            throw new InvalidOperationException("Consumer Number not found.");

        if (!IsActiveConsumer(consumer))
            throw new InvalidOperationException("Only active consumers can update/register mobile number.");

        if (!string.IsNullOrWhiteSpace(NormalizeMobileNo(consumer.MobNo)))
            throw new InvalidOperationException("Mobile number is already registered for this Consumer Number.");

        var now = DateTime.UtcNow;
        var activeOtp = await _db.ConsumerOtpVerifications
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == Purpose
                && x.MobileNo == normalizedMobileNo
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
                return new ConsumerOtpRequestResult
                {
                    ConsumerNo = normalizedConsumerNo,
                    MaskedMobileNo = MaskMobileNo(normalizedMobileNo),
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
            ConsumerNo = normalizedConsumerNo,
            MobileNo = normalizedMobileNo,
            OtpHash = HashOtp(otp, salt),
            OtpSalt = salt,
            Purpose = Purpose,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
        await SendOtpAsync(consumer, normalizedMobileNo, otp, expiresAt, ct);

        _logger.LogInformation("Public mobile registration OTP requested for consumer {ConsumerNo}.", normalizedConsumerNo);

        return new ConsumerOtpRequestResult
        {
            ConsumerNo = normalizedConsumerNo,
            MaskedMobileNo = MaskMobileNo(normalizedMobileNo),
            ExpiresAt = expiresAt,
            ResendAvailableInSeconds = ResendCooldownSeconds,
            DevelopmentOtp = otp
        };
    }

    public async Task UpdateMobileAsync(string consumerNo, string mobileNo, string otp, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        var normalizedMobileNo = NormalizeMobileNo(mobileNo);
        var normalizedOtp = new string((otp ?? string.Empty).Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            throw new InvalidOperationException("Consumer Number is required.");

        if (!IsValidMobileNo(normalizedMobileNo))
            throw new InvalidOperationException("Please enter a valid 10 digit mobile number.");

        if (normalizedOtp.Length != OtpLength)
            throw new InvalidOperationException("Please enter the valid 6 digit OTP.");

        var consumer = await _db.ConsumerDetailsMasters
            .FirstOrDefaultAsync(x => x.ConsNo == normalizedConsumerNo, ct);

        if (consumer is null)
            throw new InvalidOperationException("Consumer Number not found.");

        if (!IsActiveConsumer(consumer))
            throw new InvalidOperationException("Only active consumers can update/register mobile number.");

        if (!string.IsNullOrWhiteSpace(NormalizeMobileNo(consumer.MobNo)))
            throw new InvalidOperationException("Mobile number is already registered for this Consumer Number.");

        var now = DateTime.UtcNow;
        var verification = await _db.ConsumerOtpVerifications
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == Purpose
                && x.MobileNo == normalizedMobileNo
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

        consumer.MobNo = normalizedMobileNo;
        consumer.ModifyDate = DateTime.Now;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Public mobile registration completed for consumer {ConsumerNo}.", normalizedConsumerNo);
    }

    private async Task SendOtpAsync(ConsumerDetailsMaster consumer, string mobileNo, string otp, DateTime expiresAt, CancellationToken ct)
    {
        var values = new Dictionary<string, string?>
        {
            ["ConsumerName"] = GetConsumerName(consumer),
            ["ConsumerNo"] = NormalizeConsumerNo(consumer.ConsNo),
            ["Otp"] = otp,
            ["Date"] = DateTime.Now.ToString("dd MMM yyyy"),
            ["ExpiryMinutes"] = Math.Max(1, (int)Math.Ceiling((expiresAt - DateTime.UtcNow).TotalMinutes)).ToString()
        };

        await _communicationService.SendAsync(
            CommunicationPurposes.ConsumerOtp,
            new NotificationRecipient
            {
                Name = GetConsumerName(consumer),
                Mobile = mobileNo,
                Email = consumer.EmailId
            },
            values,
            NotificationChannelOptions.For(CommunicationChannels.Sms, CommunicationChannels.Email),
            referenceType: "ConsumerMobileRegistrationOtp",
            referenceId: NormalizeConsumerNo(consumer.ConsNo),
            referenceNo: NormalizeConsumerNo(consumer.ConsNo),
            ct: ct);
    }

    private static bool IsActiveConsumer(ConsumerDetailsMaster consumer)
        => consumer.DeleteDate == null && (consumer.Status == null || consumer.Status == 1);

    private static bool IsValidMobileNo(string mobileNo)
        => mobileNo.Length == 10 && mobileNo[0] is >= '6' and <= '9' && mobileNo.All(char.IsDigit);

    private static string NormalizeConsumerNo(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeMobileNo(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length >= 10 ? digits[^10..] : string.Empty;
    }

    private static string MaskMobileNo(string mobileNo)
        => mobileNo.Length < 10 ? "registered mobile" : $"{mobileNo[..2]}******{mobileNo[^2..]}";

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

    private static string GetConsumerName(ConsumerDetailsMaster consumer)
    {
        var name = (consumer.ConsNm1 ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(name) ? consumer.ConsNo : name;
    }
}
