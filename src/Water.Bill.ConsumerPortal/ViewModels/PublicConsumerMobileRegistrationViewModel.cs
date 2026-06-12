using System.ComponentModel.DataAnnotations;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class PublicConsumerMobileRegistrationViewModel : IValidatableObject
{
    [Display(Name = "Consumer Number")]
    public string? ConsumerNo { get; set; }

    [Display(Name = "Mobile Number")]
    public string? MobileNo { get; set; }

    [Display(Name = "OTP")]
    public string? Otp { get; set; }

    public bool IsOtpSent { get; set; }

    public string? MaskedMobileNo { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int ResendAvailableInSeconds { get; set; }

    public string? DevelopmentOtp { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConsumerNo))
            yield return new ValidationResult("Please enter Consumer Number.", [nameof(ConsumerNo)]);

        if (string.IsNullOrWhiteSpace(MobileNo))
            yield return new ValidationResult("Please enter Mobile Number.", [nameof(MobileNo)]);
        else if (!IsValidMobileNo(MobileNo))
            yield return new ValidationResult("Please enter a valid 10 digit mobile number.", [nameof(MobileNo)]);

        if (IsOtpSent)
        {
            var digits = new string((Otp ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(Otp))
                yield return new ValidationResult("Please enter OTP.", [nameof(Otp)]);
            else if (digits.Length != 6)
                yield return new ValidationResult("Please enter the valid 6 digit OTP.", [nameof(Otp)]);
        }
    }

    private static bool IsValidMobileNo(string value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 10 && digits[0] is >= '6' and <= '9';
    }
}
