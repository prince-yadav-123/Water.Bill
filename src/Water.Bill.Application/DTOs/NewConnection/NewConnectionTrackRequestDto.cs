using System.ComponentModel.DataAnnotations;

namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionTrackRequestDto
{
    [Required(ErrorMessage = "Application number is required.")]
    public string? ApplicationNo { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression("^([6-9]{1})([0-9]{9})$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
    public string? MobileNumber { get; set; }

    public NewConnectionApplicationDetailsDto? Result { get; set; }
}
