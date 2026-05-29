using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Models.Ndc;

public class NdcApplicationIndexViewModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public int? DivisionType { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public IReadOnlyList<NdcApplicationListItemViewModel> Items { get; set; } = [];
}

public class NdcApplicationListItemViewModel
{
    public int AutoId { get; set; }

    public string ApplicationNo { get; set; } = string.Empty;

    public string? ConsumerNo { get; set; }

    public string? ConsumerName { get; set; }

    public string? MobileNo { get; set; }

    public string? PropertyNo { get; set; }

    public string? Division { get; set; }

    public string? Status { get; set; }

    public string? SuccessStatus { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool WorkflowStarted { get; set; }

    public long? WorkflowTaskId { get; set; }
}

public class NdcApplicationDetailsViewModel
{
    public ConsumerApplyNdc Application { get; set; } = null!;

    public IReadOnlyList<NdcDocument> Documents { get; set; } = [];

    public ApplicationWorkflowInstance? WorkflowInstance { get; set; }

    public IReadOnlyList<ApplicationWorkflowTask> WorkflowTasks { get; set; } = [];

    public IReadOnlyList<ApplicationWorkflowHistory> WorkflowHistory { get; set; } = [];

    public bool CanStartWorkflow { get; set; }
}

public class NdcCertificateIndexViewModel
{
    public string? Search { get; set; }

    public IReadOnlyList<NdcApplicationListItemViewModel> Items { get; set; } = [];
}
