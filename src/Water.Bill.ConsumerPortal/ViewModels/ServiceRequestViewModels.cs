using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerServiceRequestListViewModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public IReadOnlyList<MasterApplicationDetail> Applications { get; set; } = [];
}

public class ConsumerServiceRequestDetailsViewModel
{
    public MasterApplicationDetail Application { get; set; } = null!;

    public IReadOnlyDictionary<string, string> DetailValues { get; set; } = new Dictionary<string, string>();

    public IReadOnlyList<MasterApplicationDetailHistory> Histories { get; set; } = [];

    public IReadOnlyList<ApplicationWorkflowHistory> WorkflowHistory { get; set; } = [];

    public IReadOnlyList<ApplicationWorkflowTask> WorkflowTasks { get; set; } = [];
}

public abstract class ConsumerServiceRequestFormViewModel
{
    [Required]
    [Display(Name = "Consumer No")]
    public string ConsumerNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Mobile Number")]
    public string MobileNo { get; set; } = string.Empty;

    [Display(Name = "Remarks")]
    [StringLength(500)]
    public string? Remarks { get; set; }

    [Display(Name = "Declaration")]
    public bool DeclarationAccepted { get; set; }

    public IReadOnlyList<SelectListItem> Consumers { get; set; } = [];

    public ConsumerDetailsMaster? SelectedConsumer { get; set; }
}

public class NameTransferRequestViewModel : ConsumerServiceRequestFormViewModel
{
    [Required]
    [Display(Name = "New Consumer Name")]
    [StringLength(100)]
    public string NewConsumerName { get; set; } = string.Empty;

    [Display(Name = "New Father / Name2")]
    [StringLength(100)]
    public string? NewFatherName { get; set; }

    [Display(Name = "Transfer Fee")]
    public decimal? TransferFee { get; set; }

    [Display(Name = "Security Amount")]
    public decimal? SecurityAmount { get; set; }

    [Display(Name = "Challan No")]
    [StringLength(50)]
    public string? ChallanNo { get; set; }

    [Display(Name = "Challan Date")]
    public DateTime? ChallanDate { get; set; }
}

public class ConnectionChangeRequestViewModel : ConsumerServiceRequestFormViewModel
{
    [Display(Name = "New Connection Type")]
    public string? NewConnectionType { get; set; }

    [Display(Name = "New Consumer Category")]
    public string? NewConsumerCategory { get; set; }

    [Display(Name = "Type Change Date")]
    public DateTime TypeChangeDate { get; set; } = DateTime.Today;

    [Display(Name = "Estimation No")]
    [StringLength(50)]
    public string? EstimationNo { get; set; }

    [Display(Name = "Estimation Amount")]
    public decimal? EstimationAmount { get; set; }

    [Display(Name = "Security Amount")]
    public decimal? SecurityAmount { get; set; }

    [Display(Name = "Monthly Rate")]
    public decimal? MonthlyRate { get; set; }

    public IReadOnlyList<SelectListItem> ConnectionTypeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> ConsumerCategoryOptions { get; set; } = [];
}
