using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.PublicNewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class PublicNewConnectionOtpService : IPublicNewConnectionOtpService
{
    private const string Purpose = "PublicNewConnection";
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxAttempts = 5;

    private readonly ApplicationDbContext _db;
    private readonly IConsumerSmsSender _smsSender;
    private readonly string? _configuredDefaultOtp;

    public PublicNewConnectionOtpService(ApplicationDbContext db, IConsumerSmsSender smsSender, IConfiguration configuration)
    {
        _db = db;
        _smsSender = smsSender;
        _configuredDefaultOtp = NormalizeConfiguredOtp(configuration["Sms:Otp:DefaultOtp"]) ?? "123456";
    }

    public async Task<PublicOtpRequestResult> RequestOtpAsync(string mobileNumber, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var now = DateTime.UtcNow;
        var activeOtp = await _db.PublicNewConnectionOtpVerifications
            .Where(x => x.MobileNumber == mobile
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
                return new PublicOtpRequestResult
                {
                    MobileNumber = mobile,
                    MaskedMobileNumber = MaskMobile(mobile),
                    ExpiresAt = activeOtp.ExpiresAt,
                    ResendAvailableInSeconds = Math.Max(1, ResendCooldownSeconds - (int)secondsSinceLastOtp)
                };
            }

            activeOtp.IsActive = false;
        }

        var otp = _configuredDefaultOtp ?? GenerateOtp();
        var salt = GenerateSalt();
        var expiresAt = now.AddMinutes(OtpExpiryMinutes);

        _db.PublicNewConnectionOtpVerifications.Add(new PublicNewConnectionOtpVerification
        {
            MobileNumber = mobile,
            OtpHash = HashOtp(otp, salt),
            OtpSalt = salt,
            Purpose = Purpose,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
        await _smsSender.SendOtpAsync(mobile, otp, expiresAt, ct);

        return new PublicOtpRequestResult
        {
            MobileNumber = mobile,
            MaskedMobileNumber = MaskMobile(mobile),
            ExpiresAt = expiresAt,
            ResendAvailableInSeconds = ResendCooldownSeconds,
            DevelopmentOtp = otp
        };
    }

    public async Task<PublicOtpVerifyResult> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var normalizedOtp = new string((otp ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedOtp.Length != OtpLength)
            throw new InvalidOperationException("Enter the valid 6 digit OTP.");

        var now = DateTime.UtcNow;
        var verification = await _db.PublicNewConnectionOtpVerifications
            .Where(x => x.MobileNumber == mobile
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
        await _db.SaveChangesAsync(ct);

        return new PublicOtpVerifyResult { MobileNumber = mobile, VerifiedAt = now };
    }

    private static string NormalizeMobile(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length > 10) digits = digits[^10..];
        if (digits.Length != 10) throw new InvalidOperationException("Enter a valid 10 digit mobile number.");
        return digits;
    }

    private static string MaskMobile(string mobile) => $"{mobile[..2]}******{mobile[^2..]}";

    private static string GenerateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string GenerateSalt()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes);
    }

    private static string HashOtp(string otp, string salt)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{otp}")));

    private static string? NormalizeConfiguredOtp(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == OtpLength ? digits : null;
    }
}
