using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.DTOs.Security;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class AuthorityLoginOtpService : IAuthorityLoginOtpService
{
    private const string Purpose = "AuthorityLogin";
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxAttempts = 5;

    private readonly ApplicationDbContext _db;
    private readonly ISecuritySettingsService _securitySettingsService;
    private readonly IOtpThrottleService _otpThrottleService;
    private readonly IConfiguration _configuration;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AuthorityLoginOtpService> _logger;

    public AuthorityLoginOtpService(
        ApplicationDbContext db,
        ISecuritySettingsService securitySettingsService,
        IOtpThrottleService otpThrottleService,
        IConfiguration configuration,
        ITemplateRenderer templateRenderer,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IWhatsAppSender whatsAppSender,
        IHostEnvironment hostEnvironment,
        ILogger<AuthorityLoginOtpService> logger)
    {
        _db = db;
        _securitySettingsService = securitySettingsService;
        _otpThrottleService = otpThrottleService;
        _configuration = configuration;
        _templateRenderer = templateRenderer;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _whatsAppSender = whatsAppSender;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<AuthorityLoginOtpChallengeResult> RequestOtpAsync(AuthorityLoginValidationResult user, CancellationToken ct = default)
    {
        var normalizedUsername = (user.Username ?? string.Empty).Trim();
        if (user.UserId <= 0 || string.IsNullOrWhiteSpace(normalizedUsername))
            throw new InvalidOperationException("Authority user details are not available for 2FA verification.");

        var settings = await _securitySettingsService.GetByTenantAsync(AppConstants.DefaultTenantId, ct);
        EnsureTwoFactorConfiguration(settings);

        if (!_otpThrottleService.TryConsumeRequest(Purpose, $"{user.UserId}:{normalizedUsername}", out _))
            throw new InvalidOperationException("Too many OTP requests. Please try again after some time.");

        var now = DateTime.UtcNow;
        var activeChallenge = await _db.AuthorityLoginOtpVerifications
            .FirstOrDefaultAsync(x => x.UserId == user.UserId
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified, ct);

        if (activeChallenge is not null)
        {
            var secondsSinceLastOtp = (now - activeChallenge.CreatedAt).TotalSeconds;
            if (secondsSinceLastOtp < ResendCooldownSeconds && activeChallenge.ExpiresAt > now)
            {
                return MapChallenge(activeChallenge, Math.Max(1, ResendCooldownSeconds - (int)secondsSinceLastOtp));
            }
        }

        return await CreateOrRefreshChallengeAsync(user, settings, activeChallenge, now, ct);
    }

    public async Task<AuthorityLoginOtpChallengeResult?> GetChallengeAsync(string challengeToken, CancellationToken ct = default)
    {
        var token = NormalizeChallengeToken(challengeToken);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var challenge = await _db.AuthorityLoginOtpVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChallengeToken == token
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified, ct);

        if (challenge is null)
            return null;

        var now = DateTime.UtcNow;
        var secondsSinceLastOtp = (now - challenge.CreatedAt).TotalSeconds;
        var cooldown = secondsSinceLastOtp < ResendCooldownSeconds
            ? Math.Max(1, ResendCooldownSeconds - (int)secondsSinceLastOtp)
            : 0;

        return MapChallenge(challenge, cooldown);
    }

    public async Task<AuthorityLoginOtpChallengeResult> ResendOtpAsync(string challengeToken, CancellationToken ct = default)
    {
        var token = NormalizeChallengeToken(challengeToken);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("2FA verification session is not valid. Please login again.");

        var challenge = await _db.AuthorityLoginOtpVerifications
            .FirstOrDefaultAsync(x => x.ChallengeToken == token
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified, ct);

        if (challenge is null)
            throw new InvalidOperationException("2FA verification session has expired. Please login again.");

        var user = await _db.Appusers
            .Include(x => x.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == challenge.UserId && !x.IsDeleted, ct);

        if (user is null || user.IsActive != true)
            throw new InvalidOperationException("Authority user account is not available for 2FA verification.");

        if (!_otpThrottleService.TryConsumeRequest(Purpose, $"{user.Id}:{user.Username}", out _))
            throw new InvalidOperationException("Too many OTP requests. Please try again after some time.");

        var now = DateTime.UtcNow;
        var secondsSinceLastOtp = (now - challenge.CreatedAt).TotalSeconds;
        if (secondsSinceLastOtp < ResendCooldownSeconds && challenge.ExpiresAt > now)
        {
            return MapChallenge(challenge, Math.Max(1, ResendCooldownSeconds - (int)secondsSinceLastOtp));
        }

        var settings = await _securitySettingsService.GetByTenantAsync(AppConstants.DefaultTenantId, ct);
        EnsureTwoFactorConfiguration(settings);

        var authorityUser = new AuthorityLoginValidationResult
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            MobileNo = user.PhoneNumber,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive == true
        };

        return await CreateOrRefreshChallengeAsync(authorityUser, settings, challenge, now, ct);
    }

    public async Task<AuthorityLoginOtpVerifyResult> VerifyOtpAsync(string challengeToken, string otp, CancellationToken ct = default)
    {
        var token = NormalizeChallengeToken(challengeToken);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("2FA verification session is not valid. Please login again.");

        var normalizedOtp = new string((otp ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedOtp.Length != OtpLength)
            throw new InvalidOperationException("Enter the valid 6 digit OTP.");

        var challenge = await _db.AuthorityLoginOtpVerifications
            .FirstOrDefaultAsync(x => x.ChallengeToken == token
                && x.IsActive
                && !x.IsDeleted
                && !x.IsVerified, ct);

        if (challenge is null)
            throw new InvalidOperationException("2FA verification session has expired. Please login again.");

        var now = DateTime.UtcNow;
        if (challenge.ExpiresAt <= now)
        {
            challenge.IsActive = false;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("OTP has expired. Please login again to request a new OTP.");
        }

        challenge.AttemptCount++;
        challenge.LastAttemptAt = now;

        if (challenge.AttemptCount > MaxAttempts)
        {
            challenge.IsActive = false;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Too many invalid OTP attempts. Please login again to request a new OTP.");
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(challenge.OtpHash),
                Encoding.UTF8.GetBytes(HashOtp(normalizedOtp, challenge.OtpSalt))))
        {
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Invalid OTP. Please try again.");
        }

        challenge.IsVerified = true;
        challenge.VerifiedAt = now;
        challenge.IsActive = false;
        await _db.SaveChangesAsync(ct);

        return new AuthorityLoginOtpVerifyResult
        {
            UserId = challenge.UserId,
            Username = challenge.Username
        };
    }

    private async Task<AuthorityLoginOtpChallengeResult> CreateOrRefreshChallengeAsync(
        AuthorityLoginValidationResult user,
        SecuritySettingsDto settings,
        AuthorityLoginOtpVerification? challenge,
        DateTime now,
        CancellationToken ct)
    {
        var templates = await LoadTemplatesAsync(ct);
        var deliveries = BuildDeliveries(user, settings, templates).ToList();
        if (deliveries.Count == 0)
            throw new InvalidOperationException("Two-factor authentication is enabled, but no valid verification channel is available for your account. Please contact the administrator.");

        var otp = GenerateOtp();
        var expiresAt = now.AddMinutes(OtpExpiryMinutes);
        var successfulDeliveries = new List<ChannelDelivery>();
        var failureReasons = new List<string>();

        foreach (var delivery in deliveries)
        {
            var result = await SendDeliveryAsync(delivery, otp, expiresAt, ct);
            if (string.Equals(result.Status, "Sent", StringComparison.OrdinalIgnoreCase))
            {
                successfulDeliveries.Add(delivery);
                continue;
            }

            failureReasons.Add($"{delivery.Channel}: {result.ErrorMessage ?? result.Status}");
        }

        if (successfulDeliveries.Count == 0)
        {
            _logger.LogWarning(
                "Authority login 2FA OTP could not be delivered for user {UserId}/{Username}. Reasons: {Reasons}",
                user.UserId,
                user.Username,
                string.Join(" | ", failureReasons));

            if (!TryGetDevelopmentFallbackOtp(out otp))
                throw new InvalidOperationException("Two-factor authentication is enabled, but the selected verification channel is not available right now. Please contact the administrator.");

            successfulDeliveries.Add(new ChannelDelivery(
                "DevelopmentFallback",
                "local",
                "development fallback OTP",
                (_, _, _) => Task.FromResult(CommunicationSendResult.Sent())));

            _logger.LogWarning(
                "Authority login 2FA fell back to development OTP for user {UserId}/{Username}.",
                user.UserId,
                user.Username);
        }

        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        challenge ??= new AuthorityLoginOtpVerification
        {
            UserId = user.UserId,
            Username = user.Username,
            ChallengeToken = Guid.NewGuid().ToString("N"),
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        };

        challenge.UserId = user.UserId;
        challenge.Username = user.Username;
        challenge.Channels = string.Join(",", successfulDeliveries.Select(x => x.Channel));
        challenge.DeliverySummary = string.Join(", ", successfulDeliveries.Select(x => $"{x.Channel}: {x.MaskedTarget}"));
        challenge.OtpHash = HashOtp(otp, salt);
        challenge.OtpSalt = salt;
        challenge.ExpiresAt = expiresAt;
        challenge.AttemptCount = 0;
        challenge.LastAttemptAt = null;
        challenge.CreatedAt = now;
        challenge.IsVerified = false;
        challenge.VerifiedAt = null;
        challenge.IsActive = true;
        challenge.IsDeleted = false;

        if (challenge.Id == 0)
            _db.AuthorityLoginOtpVerifications.Add(challenge);

        await _db.SaveChangesAsync(ct);
        return MapChallenge(challenge, ResendCooldownSeconds);
    }

    private IEnumerable<ChannelDelivery> BuildDeliveries(
        AuthorityLoginValidationResult user,
        SecuritySettingsDto settings,
        IReadOnlyDictionary<string, TemplatePayload> templates)
    {
        if (settings.AuthorityLoginTwoFactorEmail)
        {
            var email = (user.Email ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (templates.TryGetValue(CommunicationChannels.Email, out var emailTemplate))
                {
                    yield return new ChannelDelivery(
                        CommunicationChannels.Email,
                        email,
                        MaskEmail(email),
                        (otp, expiresAt, ct) =>
                        {
                            var values = BuildTemplateValues(user, otp, expiresAt);
                            var subject = _templateRenderer.Render(emailTemplate.Subject, values);
                            var body = _templateRenderer.Render(emailTemplate.Body, values);
                            return _emailSender.SendAsync(
                                email,
                                string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
                                subject,
                                body,
                                ct);
                        });
                }
                else
                {
                    yield return new ChannelDelivery(
                        CommunicationChannels.Email,
                        email,
                        MaskEmail(email),
                        (_, _, _) => Task.FromResult(CommunicationSendResult.Failed("Authority login OTP email template is not configured.")));
                }
            }
            else
            {
                yield return new ChannelDelivery(
                    CommunicationChannels.Email,
                    string.Empty,
                    "no registered email",
                    (_, _, _) => Task.FromResult(CommunicationSendResult.Skipped("Registered email address is not available for this user.")));
            }
        }

        var mobile = NormalizeMobileNo(user.MobileNo);
        if (settings.AuthorityLoginTwoFactorSms && !string.IsNullOrWhiteSpace(mobile))
        {
            if (templates.TryGetValue(CommunicationChannels.Sms, out var smsTemplate))
            {
                yield return new ChannelDelivery(
                    CommunicationChannels.Sms,
                    mobile,
                    MaskMobile(mobile),
                    (otp, expiresAt, ct) =>
                    {
                        var values = BuildTemplateValues(user, otp, expiresAt);
                        var body = _templateRenderer.Render(smsTemplate.Body, values);
                        return _smsSender.SendAsync(mobile, body, smsTemplate.ExternalTemplateId, ct);
                    });
            }
            else
            {
                yield return new ChannelDelivery(
                    CommunicationChannels.Sms,
                    mobile,
                    MaskMobile(mobile),
                    (_, _, _) => Task.FromResult(CommunicationSendResult.Failed("Authority login OTP SMS template is not configured.")));
            }
        }
        else if (settings.AuthorityLoginTwoFactorSms)
        {
            yield return new ChannelDelivery(
                CommunicationChannels.Sms,
                string.Empty,
                "no registered mobile",
                (_, _, _) => Task.FromResult(CommunicationSendResult.Skipped("Registered mobile number is not available for this user.")));
        }

        if (settings.AuthorityLoginTwoFactorWhatsApp && !string.IsNullOrWhiteSpace(mobile))
        {
            if (templates.TryGetValue(CommunicationChannels.WhatsApp, out var whatsappTemplate))
            {
                yield return new ChannelDelivery(
                    CommunicationChannels.WhatsApp,
                    mobile,
                    MaskMobile(mobile),
                    (otp, expiresAt, ct) =>
                    {
                        var values = BuildTemplateValues(user, otp, expiresAt);
                        var body = _templateRenderer.Render(whatsappTemplate.Body, values);
                        return _whatsAppSender.SendAsync(mobile, body, whatsappTemplate.ExternalTemplateId, ct);
                    });
            }
            else
            {
                yield return new ChannelDelivery(
                    CommunicationChannels.WhatsApp,
                    mobile,
                    MaskMobile(mobile),
                    (_, _, _) => Task.FromResult(CommunicationSendResult.Failed("Authority login OTP WhatsApp template is not configured.")));
            }
        }
        else if (settings.AuthorityLoginTwoFactorWhatsApp)
        {
            yield return new ChannelDelivery(
                CommunicationChannels.WhatsApp,
                string.Empty,
                "no registered mobile",
                (_, _, _) => Task.FromResult(CommunicationSendResult.Skipped("Registered mobile number is not available for this user.")));
        }
    }

    private static void EnsureTwoFactorConfiguration(SecuritySettingsDto settings)
    {
        if (!settings.AuthorityLoginTwoFactorEnabled)
            throw new InvalidOperationException("Authority login 2FA is currently disabled.");

        if (!settings.AuthorityLoginTwoFactorEmail
            && !settings.AuthorityLoginTwoFactorSms
            && !settings.AuthorityLoginTwoFactorWhatsApp)
        {
            throw new InvalidOperationException("Authority login 2FA is enabled, but no verification channel is selected in Security Settings.");
        }
    }

    private async Task<CommunicationSendResult> SendDeliveryAsync(ChannelDelivery delivery, string otp, DateTime expiresAt, CancellationToken ct)
    {
        try
        {
            return await delivery.SendAsync(otp, expiresAt, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authority login 2FA send failed for channel {Channel} and target {Target}.", delivery.Channel, delivery.Target);
            return CommunicationSendResult.Failed(ex.Message);
        }
    }

    private static AuthorityLoginOtpChallengeResult MapChallenge(AuthorityLoginOtpVerification challenge, int resendAvailableInSeconds)
        => new()
        {
            ChallengeToken = challenge.ChallengeToken,
            DeliverySummary = challenge.DeliverySummary ?? string.Empty,
            ExpiresAt = challenge.ExpiresAt,
            ResendAvailableInSeconds = Math.Max(0, resendAvailableInSeconds)
        };

    private static string GenerateOtp()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private Dictionary<string, string?> BuildTemplateValues(AuthorityLoginValidationResult user, string otp, DateTime expiresAt)
    {
        var expiryMinutes = Math.Max(1, (int)Math.Ceiling((expiresAt - DateTime.UtcNow).TotalMinutes));
        var displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;

        return new Dictionary<string, string?>
        {
            ["UserName"] = user.Username,
            ["FullName"] = displayName,
            ["ApplicationName"] = "Noida Jal Authority Portal",
            ["Otp"] = otp,
            ["Date"] = AppTime.IndiaNow.ToString("dd MMM yyyy"),
            ["ExpiryMinutes"] = expiryMinutes.ToString()
        };
    }

    private async Task<Dictionary<string, TemplatePayload>> LoadTemplatesAsync(CancellationToken ct)
    {
        var templates = await _db.CommunicationTemplates
            .AsNoTracking()
            .Where(x => x.PurposeKey == CommunicationPurposes.AuthorityLoginOtp
                && x.IsActive
                && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(ct);

        var allowedJson = await _db.CommunicationPurposes
            .AsNoTracking()
            .Where(x => x.PurposeKey == CommunicationPurposes.AuthorityLoginOtp && x.IsActive)
            .Select(x => x.AllowedPlaceholders)
            .FirstOrDefaultAsync(ct);

        var allowed = ParseAllowedPlaceholders(allowedJson);
        var result = new Dictionary<string, TemplatePayload>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            if (result.ContainsKey(template.Channel))
                continue;

            var placeholders = _templateRenderer.ExtractPlaceholders($"{template.Subject} {template.Body}");
            if (placeholders.Any(x => !allowed.Contains(x, StringComparer.OrdinalIgnoreCase)))
                continue;

            result[template.Channel] = new TemplatePayload(template.Subject ?? string.Empty, template.Body, template.ExternalTemplateId);
        }

        return result;
    }

    private bool TryGetDevelopmentFallbackOtp(out string otp)
    {
        otp = string.Empty;
        if (!_hostEnvironment.IsDevelopment())
            return false;

        var configured = _configuration["AuthorityLogin2FA:DevelopmentFallbackOtp"];
        var digits = new string((configured ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length != OtpLength)
            return false;

        otp = digits;
        return true;
    }

    private static IReadOnlyList<string> ParseAllowedPlaceholders(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string HashOtp(string otp, string salt)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{otp}")));
    }

    private static string NormalizeChallengeToken(string? challengeToken)
        => (challengeToken ?? string.Empty).Trim();

    private static string NormalizeMobileNo(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length > 10 ? digits[^10..] : digits;
    }

    private static string MaskMobile(string value)
    {
        var digits = NormalizeMobileNo(value);
        return digits.Length < 4 ? "**********" : $"******{digits[^4..]}";
    }

    private static string MaskEmail(string email)
    {
        var value = (email ?? string.Empty).Trim();
        var atIndex = value.IndexOf('@');
        if (atIndex <= 1)
            return value;

        var local = value[..atIndex];
        var domain = value[atIndex..];
        var visible = local.Length <= 2 ? local[..1] : local[..2];
        return $"{visible}***{domain}";
    }

    private sealed record ChannelDelivery(
        string Channel,
        string Target,
        string MaskedTarget,
        Func<string, DateTime, CancellationToken, Task<CommunicationSendResult>> SendAsync);

    private sealed record TemplatePayload(string Subject, string Body, string? ExternalTemplateId);
}
