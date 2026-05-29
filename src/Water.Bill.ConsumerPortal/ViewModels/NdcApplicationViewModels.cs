using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerNdcIndexViewModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public IReadOnlyList<ConsumerApplyNdc> Applications { get; set; } = [];
}

public class ConsumerNdcApplyViewModel
{
    [Required(ErrorMessage = "Select consumer number.")]
    public string ConsumerNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [StringLength(15)]
    public string MobileNo { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? Email { get; set; }

    public bool DeclarationAccepted { get; set; }

    public List<SelectListItem> Consumers { get; set; } = [];

    public ConsumerNdcConsumerDetailsViewModel? SelectedConsumer { get; set; }

    public List<ConsumerNdcDocumentInputViewModel> Documents { get; set; } = [];
}

public class ConsumerNdcDocumentInputViewModel
{
    public int DocumentId { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DocumentNo { get; set; }

    public DateTime? DocumentDate { get; set; }
}

public class ConsumerNdcConsumerDetailsViewModel
{
    public string ConsumerNo { get; set; } = string.Empty;

    public string? ConsumerName { get; set; }

    public string? FatherName { get; set; }

    public string? ConnectionCategory { get; set; }

    public string? ConnectionType { get; set; }

    public string? FlatType { get; set; }

    public string? Address { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public string? PlotSize { get; set; }

    public string? PipeSize { get; set; }

    public string? RegistrationNo { get; set; }

    public string? EstimationNo { get; set; }

    public DateTime? EstimationDate { get; set; }

    public DateTime? ConnectionDate { get; set; }

    public int? DivisionType { get; set; }

    public string? DivisionName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? Rid { get; set; }
}

public class ConsumerNdcDetailsViewModel
{
    public ConsumerApplyNdc Application { get; set; } = null!;

    public IReadOnlyList<NdcDocument> Documents { get; set; } = [];

    public IReadOnlyList<ApplicationWorkflowHistory> WorkflowHistory { get; set; } = [];

    public IReadOnlyList<ApplicationWorkflowTask> WorkflowTasks { get; set; } = [];
}
