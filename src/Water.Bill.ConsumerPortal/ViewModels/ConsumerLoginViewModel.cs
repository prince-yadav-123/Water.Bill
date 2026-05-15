using System.ComponentModel.DataAnnotations;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerLoginViewModel : IValidatableObject
{
    public string LoginMethod { get; set; } = ConsumerLoginMethods.MobileOtp;

    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }

    [Display(Name = "Consumer ID")]
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
                yield return new ValidationResult("Enter your consumer ID.", [nameof(ConsumerId)]);
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

public static class ConsumerLoginMethods
{
    public const string MobileOtp = "mobile-otp";
    public const string ConsumerId = "consumer-id";
    public const string UsernameEmail = "username-email";
}
