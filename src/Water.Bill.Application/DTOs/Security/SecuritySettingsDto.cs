namespace Water.Bill.Application.DTOs.Security;

public class SecuritySettingsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 480;
    public int IdleTimeoutMinutes { get; set; } = 30;
    public int PasswordMinLength { get; set; } = 8;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireSpecialChar { get; set; } = true;
    public int PasswordExpiryDays { get; set; } = 90;
    public int PasswordHistoryCount { get; set; } = 5;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool EnableCaptchaAfterFailures { get; set; }
    public int CaptchaAfterAttempts { get; set; } = 3;
    public bool AllowMultipleSessions { get; set; } = true;
    public bool BlockNewLoginOnConflict { get; set; }
}
