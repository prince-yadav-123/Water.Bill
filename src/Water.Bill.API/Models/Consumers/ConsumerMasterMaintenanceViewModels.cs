using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Water.Bill.API.Models.Consumers;

public class ConsumerMasterMaintenanceIndexViewModel
{
    public string? Search { get; set; }

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public int? DevType { get; set; }

    public int? Status { get; set; } = 1;

    public IReadOnlyList<SelectListItem> DivisionOptions { get; set; } = [];

    public IReadOnlyList<ConsumerMasterMaintenanceListItemViewModel> Consumers { get; set; } = [];
}

public class ConsumerMasterMaintenanceListItemViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? FatherName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? PropertyNo { get; set; }

    public string? ConnectionType { get; set; }

    public string? Category { get; set; }

    public int? DevType { get; set; }

    public int? Status { get; set; }

    public DateTime? ConnectionDate { get; set; }

    public DateTime? ModifiedOn { get; set; }
}

public class ConsumerMasterMaintenanceDetailsViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? FatherName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? PropertyNo { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? FlatNo { get; set; }

    public int? PlotSize { get; set; }

    public int? PipeSize { get; set; }

    public string? FlatType { get; set; }

    public string? ConnectionType { get; set; }

    public string? Category { get; set; }

    public int? DevType { get; set; }

    public string? RegistrationNo { get; set; }

    public DateTime? ConnectionDate { get; set; }

    public double? MonthlyRate { get; set; }

    public double? MonthlyCharges { get; set; }

    public double? CessAmount { get; set; }

    public string? EstimateNo { get; set; }

    public int? EstimateAmount { get; set; }

    public int? SecurityAmount { get; set; }

    public string? VillageName { get; set; }

    public string? KhasraNo { get; set; }

    public string? Purpose { get; set; }

    public string? OtherConnection { get; set; }

    public string? Narration { get; set; }

    public int? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public IReadOnlyList<ConsumerMasterBillRowViewModel> RecentBills { get; set; } = [];

    public IReadOnlyList<ConsumerMasterChallanRowViewModel> RecentChallans { get; set; } = [];
}

public class ConsumerMasterMaintenanceFormViewModel
{
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Consumer name is required.")]
    [StringLength(150)]
    [Display(Name = "Consumer Name")]
    public string? ConsumerName { get; set; }

    [StringLength(30)]
    [Display(Name = "Father / Guardian Name")]
    public string? FatherName { get; set; }

    [StringLength(12)]
    [Display(Name = "Mobile No")]
    public string? MobileNo { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(50)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(150)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Division")]
    public int? DevType { get; set; }

    [StringLength(25)]
    [Display(Name = "Sector")]
    public string? Sector { get; set; }

    [StringLength(15)]
    [Display(Name = "Block")]
    public string? Block { get; set; }

    [StringLength(20)]
    [Display(Name = "Plot / Flat No")]
    public string? FlatNo { get; set; }

    [Display(Name = "Plot Size")]
    public int? PlotSize { get; set; }

    [Display(Name = "Pipe Size")]
    public int? PipeSize { get; set; }

    [StringLength(6)]
    [Display(Name = "Flat Type")]
    public string? FlatType { get; set; }

    [StringLength(1)]
    [Display(Name = "Connection Type")]
    public string? ConnectionType { get; set; }

    [StringLength(10)]
    [Display(Name = "Consumer Category")]
    public string? Category { get; set; }

    [StringLength(8)]
    [Display(Name = "Registration No")]
    public string? RegistrationNo { get; set; }

    [Display(Name = "Connection Date")]
    public DateTime? ConnectionDate { get; set; }

    [Display(Name = "Type Change Date")]
    public DateTime? TypeChangeDate { get; set; }

    [StringLength(10)]
    [Display(Name = "Estimate No")]
    public string? EstimateNo { get; set; }

    [Display(Name = "Estimate Amount")]
    public int? EstimateAmount { get; set; }

    [Display(Name = "Security Amount")]
    public int? SecurityAmount { get; set; }

    [Display(Name = "Estimate Date")]
    public DateTime? EstimateDate { get; set; }

    [Display(Name = "Monthly Rate")]
    public double? MonthlyRate { get; set; }

    [Display(Name = "Monthly Charges")]
    public double? MonthlyCharges { get; set; }

    [Display(Name = "Cess Amount")]
    public double? CessAmount { get; set; }

    [StringLength(100)]
    [Display(Name = "Purpose")]
    public string? Purpose { get; set; }

    [StringLength(150)]
    [Display(Name = "Other Connection")]
    public string? OtherConnection { get; set; }

    [StringLength(20)]
    [Display(Name = "Khasra No")]
    public string? KhasraNo { get; set; }

    [StringLength(100)]
    [Display(Name = "Village Name")]
    public string? VillageName { get; set; }

    [Display(Name = "Village Id")]
    public int? VillageId { get; set; }

    [StringLength(50)]
    [Display(Name = "Issue Officer")]
    public string? IssueOfficer { get; set; }

    [StringLength(25)]
    [Display(Name = "Plot Map Id")]
    public string? PlotMapId { get; set; }

    [StringLength(50)]
    [Display(Name = "Kilo Liter")]
    public string? KiloLiter { get; set; }

    [StringLength(250)]
    [Display(Name = "Narration")]
    public string? Narration { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> DivisionOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> ConnectionTypeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> CategoryOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> PipeSizeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> FlatTypeOptions { get; set; } = [];
}

public class ConsumerMasterBillRowViewModel
{
    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public DateTime? BillDateFrom { get; set; }

    public DateTime? BillDateTo { get; set; }

    public double? TotalAmount { get; set; }

    public double? PaidAmount { get; set; }

    public DateTime? PaidDate { get; set; }
}

public class ConsumerMasterChallanRowViewModel
{
    public long Id { get; set; }

    public string? ChallanNo { get; set; }

    public DateTime? GeneratedOn { get; set; }

    public double? Amount { get; set; }

    public DateTime? PaidDate { get; set; }

    public string? Status { get; set; }
}
