using System.ComponentModel.DataAnnotations;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerLoginViewModel : IValidatableObject
{
    public string LoginMethod { get; set; } = ConsumerLoginMethods.ConsumerId;

    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }

    [Display(Name = "Consumer Number")]
    public string? ConsumerId { get; set; }

    [Display(Name = "Username/Email")]
    public string? UsernameOrEmail { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public bool RememberMe { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LoginMethod == ConsumerLoginMethods.MobileOtp)
        {
            if (string.IsNullOrWhiteSpace(MobileNumber))
                yield return new ValidationResult("Enter your mobile number.", [nameof(MobileNumber)]);
            else if (!IsValidMobileNumber(MobileNumber))
                yield return new ValidationResult("Enter a valid 10 digit mobile number.", [nameof(MobileNumber)]);
        }
        else if (LoginMethod == ConsumerLoginMethods.ConsumerId)
        {
            if (string.IsNullOrWhiteSpace(ConsumerId))
                yield return new ValidationResult("Enter your consumer number.", [nameof(ConsumerId)]);
        }
        else if (LoginMethod == ConsumerLoginMethods.UsernameEmail)
        {
            if (string.IsNullOrWhiteSpace(UsernameOrEmail))
                yield return new ValidationResult("Enter your username or email.", [nameof(UsernameOrEmail)]);

            if (string.IsNullOrWhiteSpace(Password))
                yield return new ValidationResult("Enter your password.", [nameof(Password)]);
        }
    }

    private static bool IsValidMobileNumber(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 10;
    }
}

public class ConsumerOtpViewModel : IValidatableObject
{
    [Display(Name = "Consumer Number")]
    public string ConsumerNo { get; set; } = string.Empty;

    public string? MaskedMobileNo { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int ResendAvailableInSeconds { get; set; }

    public string? DevelopmentOtp { get; set; }

    [Display(Name = "OTP")]
    public string? Otp { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConsumerNo))
            yield return new ValidationResult("Consumer number is required.", [nameof(ConsumerNo)]);

        var digits = new string((Otp ?? string.Empty).Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(Otp))
            yield return new ValidationResult("Enter the OTP sent to your registered mobile number.", [nameof(Otp)]);
        else if (digits.Length != 6)
            yield return new ValidationResult("Enter the valid 6 digit OTP.", [nameof(Otp)]);
    }
}

public static class ConsumerLoginMethods
{
    public const string MobileOtp = "mobile-otp";
    public const string ConsumerId = "consumer-id";
    public const string UsernameEmail = "username-email";
}
