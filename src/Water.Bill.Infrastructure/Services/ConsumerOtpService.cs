using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.Consumer;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class ConsumerOtpService : IConsumerOtpService
{
    private const string Purpose = "ConsumerLogin";
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxAttempts = 5;

    private readonly ApplicationDbContext _db;
    private readonly IConsumerSmsSender _smsSender;
    private readonly string? _configuredDefaultOtp;

    public ConsumerOtpService(ApplicationDbContext db, IConsumerSmsSender smsSender, IConfiguration configuration)
    {
        _db = db;
        _smsSender = smsSender;
        _configuredDefaultOtp = NormalizeConfiguredOtp(configuration["Sms:Otp:DefaultOtp"]);
    }

    public async Task<ConsumerOtpRequestResult> RequestOtpAsync(string consumerNo, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            throw new InvalidOperationException("Enter your consumer number.");

        var consumer = await _db.ConsumerDetailsMasters
            .FirstOrDefaultAsync(x => x.ConsNo == normalizedConsumerNo, ct);

        if (consumer is null)
            throw new InvalidOperationException("Consumer number not found.");

        return await RequestOtpForConsumerAsync(consumer, ct);
    }

    public async Task<ConsumerOtpRequestResult> RequestOtpByMobileAsync(string mobileNo, CancellationToken ct = default)
    {
        var normalizedMobileNo = NormalizeMobileNo(mobileNo);
        if (string.IsNullOrWhiteSpace(normalizedMobileNo))
            throw new InvalidOperationException("Enter a valid 10 digit mobile number.");

        var consumer = await _db.ConsumerDetailsMasters
            .Where(x => x.MobNo != null
                && x.MobNo.Replace("+", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .EndsWith(normalizedMobileNo))
            .OrderByDescending(x => x.Status == 1)
            .ThenByDescending(x => x.EntryDate)
            .ThenBy(x => x.ConsNo)
            .FirstOrDefaultAsync(ct);

        if (consumer is null)
            throw new InvalidOperationException("Mobile number is not registered for any consumer. Please contact support.");

        return await RequestOtpForConsumerAsync(consumer, ct);
    }

    private async Task<ConsumerOtpRequestResult> RequestOtpForConsumerAsync(ConsumerDetailsMaster consumer, CancellationToken ct)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumer.ConsNo);
        var mobileNo = NormalizeMobileNo(consumer.MobNo);
        if (string.IsNullOrWhiteSpace(mobileNo))
            throw new InvalidOperationException("Mobile number is not registered for this consumer. Please contact support.");

        var now = DateTime.UtcNow;
        var activeOtp = await _db.Set<ConsumerOtpVerification>()
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == Purpose
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

        _db.Set<ConsumerOtpVerification>().Add(new ConsumerOtpVerification
        {
            Id = Guid.NewGuid(),
            ConsumerNo = normalizedConsumerNo,
            MobileNo = mobileNo,
            OtpHash = HashOtp(otp, salt),
            OtpSalt = salt,
            Purpose = Purpose,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
        await _smsSender.SendOtpAsync(mobileNo, otp, expiresAt, ct);

        return new ConsumerOtpRequestResult
        {
            ConsumerNo = normalizedConsumerNo,
            MaskedMobileNo = MaskMobileNo(mobileNo),
            ExpiresAt = expiresAt,
            ResendAvailableInSeconds = ResendCooldownSeconds,
            DevelopmentOtp = otp
        };
    }

    public async Task<ConsumerOtpVerifyResult> VerifyOtpAsync(string consumerNo, string otp, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        var normalizedOtp = new string((otp ?? string.Empty).Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            throw new InvalidOperationException("Consumer number is required.");

        if (normalizedOtp.Length != OtpLength)
            throw new InvalidOperationException("Enter the valid 6 digit OTP.");

        var now = DateTime.UtcNow;
        var verification = await _db.Set<ConsumerOtpVerification>()
            .Where(x => x.ConsumerNo == normalizedConsumerNo
                && x.Purpose == Purpose
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

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == normalizedConsumerNo, ct)
            ?? throw new InvalidOperationException("Consumer number not found.");

        var consumerRole = await _db.Approles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Name.ToLower() == AppConstants.Roles.Consumer.ToLower(), ct);

        await _db.SaveChangesAsync(ct);

        return new ConsumerOtpVerifyResult
        {
            ConsumerNo = normalizedConsumerNo,
            ConsumerName = GetConsumerName(consumer),
            Email = consumer.EmailId,
            ConsumerRoleId = consumerRole?.Id
        };
    }

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
        var name = string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim()));

        return string.IsNullOrWhiteSpace(name) ? consumer.ConsNo : name;
    }
}
