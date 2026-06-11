using System.ComponentModel.DataAnnotations;

namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApplicationFormDto : IValidatableObject
{
    [Required(ErrorMessage = "Please enter Applicant Name.")]
    [StringLength(100)]
    public string? ApplicantName { get; set; }

    [StringLength(100)]
    public string? FatherName { get; set; }

    [Required(ErrorMessage = "Please enter Mobile Number.")]
    [RegularExpression("^([6-9]{1})([0-9]{9})$", ErrorMessage = "Please enter a valid 10 digit Mobile Number.")]
    public string? MobileNumber { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid Email address.")]
    [StringLength(50)]
    public string? EmailId { get; set; }

    [Required(ErrorMessage = "Please enter Property Address.")]
    [StringLength(150)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Please select Sector.")]
    [StringLength(10)]
    public string? Sector { get; set; }

    [Required(ErrorMessage = "Please select Block.")]
    [StringLength(10)]
    public string? Block { get; set; }

    [Required(ErrorMessage = "Please enter Flat/Plot No.")]
    [StringLength(15)]
    public string? FlatNo { get; set; }

    [Required(ErrorMessage = "Please enter Plot Size.")]
    [Range(1, 999999.99, ErrorMessage = "Plot Size must be greater than 0.")]
    public decimal? PlotSize { get; set; }

    [Required(ErrorMessage = "Please select Pipe Size.")]
    [Range(0.01, 999999.99, ErrorMessage = "Pipe Size must be greater than 0.")]
    public decimal? PipeSize { get; set; }

    [StringLength(20)]
    public string? KhasraNo { get; set; }

    [StringLength(100)]
    public string? VillageName { get; set; }

    public int? VillageId { get; set; }

    [Required(ErrorMessage = "Please select Connection Category.")]
    [StringLength(4)]
    public string? ConnectionCategory { get; set; }

    [StringLength(10)]
    public string? ConnectionType { get; set; }

    [Required(ErrorMessage = "Please select Flat Type.")]
    [StringLength(50)]
    public string? FlatType { get; set; }

    [StringLength(50)]
    public string? PurposeOfConnection { get; set; }

    [StringLength(1)]
    public string? PreviousConnectionYesNo { get; set; } = "N";

    [StringLength(150)]
    public string? OtherConnection { get; set; }

    [StringLength(15)]
    public string? Rid { get; set; }

    public int? DevType { get; set; }

    public bool DeclarationAccepted { get; set; }

    public string? Remarks { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(PreviousConnectionYesNo?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(OtherConnection))
        {
            yield return new ValidationResult(
                "Please enter Previous/Other Connection Details.",
                [nameof(OtherConnection)]);
        }
    }
}
