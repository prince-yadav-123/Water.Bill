using System.ComponentModel.DataAnnotations;

namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApplicationFormDto
{
    [Required(ErrorMessage = "Applicant name is required.")]
    [StringLength(100)]
    public string? ApplicantName { get; set; }

    [StringLength(100)]
    public string? FatherName { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression("^([6-9]{1})([0-9]{9})$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
    public string? MobileNumber { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(50)]
    public string? EmailId { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(150)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Sector is required.")]
    [StringLength(10)]
    public string? Sector { get; set; }

    [Required(ErrorMessage = "Block is required.")]
    [StringLength(10)]
    public string? Block { get; set; }

    [Required(ErrorMessage = "Flat/plot number is required.")]
    [StringLength(15)]
    public string? FlatNo { get; set; }

    [Required(ErrorMessage = "Plot size is required.")]
    [Range(1, 999999.99, ErrorMessage = "Plot size must be greater than 0.")]
    public decimal? PlotSize { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Pipe size must be greater than 0.")]
    public decimal? PipeSize { get; set; }

    [StringLength(20)]
    public string? KhasraNo { get; set; }

    [StringLength(100)]
    public string? VillageName { get; set; }

    public int? VillageId { get; set; }

    [Required(ErrorMessage = "Connection category is required.")]
    [StringLength(4)]
    public string? ConnectionCategory { get; set; }

    [Required(ErrorMessage = "Connection type is required.")]
    [StringLength(10)]
    public string? ConnectionType { get; set; }

    [Required(ErrorMessage = "Flat type is required.")]
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
}
