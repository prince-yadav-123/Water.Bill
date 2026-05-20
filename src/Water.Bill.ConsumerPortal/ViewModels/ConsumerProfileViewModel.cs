using System.ComponentModel.DataAnnotations;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerProfileViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string Name { get; set; } = "Consumer";
    public string Initials { get; set; } = "C";
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public bool IsMobileVerified => !string.IsNullOrWhiteSpace(MobileNo);
    public bool IsEmailVerified => !string.IsNullOrWhiteSpace(Email);
    public string? CustomerDuration { get; set; }
    public IReadOnlyList<ConsumerConnectionCardViewModel> Connections { get; set; } = [];
}

public class ConsumerConnectionCardViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;
    public string Title { get; set; } = "Connection";
    public string? Address { get; set; }
    public string? ConnectionType { get; set; }
    public DateTime? SinceDate { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsPrimary { get; set; }
    public string? LatestBillNo { get; set; }
    public double LatestBillAmount { get; set; }
    public DateTime? LatestBillDueDate { get; set; }
    public string LatestBillStatus { get; set; } = "Not available";
}

public class UpdateContactViewModel : IValidatableObject
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string ConsumerName { get; set; } = "Consumer";

    public string? CurrentMobileNo { get; set; }

    public string? CurrentEmail { get; set; }

    public string? MaskedMobileNo { get; set; }

    [Display(Name = "New mobile number")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
    public string? NewMobileNo { get; set; }

    [Display(Name = "New email")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(50, ErrorMessage = "Email cannot be more than 50 characters.")]
    public string? NewEmail { get; set; }

    [Display(Name = "OTP")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Enter the valid 6 digit OTP.")]
    public string? Otp { get; set; }

    public bool IsOtpSent { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int ResendAvailableInSeconds { get; set; }

    public string? DevelopmentOtp { get; set; }

    public bool CanSendOtp => !string.IsNullOrWhiteSpace(CurrentMobileNo);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(NewMobileNo) && string.IsNullOrWhiteSpace(NewEmail))
        {
            yield return new ValidationResult(
                "Enter new mobile number, new email, or both.",
                new[] { nameof(NewMobileNo), nameof(NewEmail) });
        }
    }
}
