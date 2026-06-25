using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Models.ConsumerWorkflows;

public class ConsumerChangeListViewModel
{
    public string ModuleTitle { get; set; } = string.Empty;

    public string ModuleDescription { get; set; } = string.Empty;

    public string Search { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public IReadOnlyList<MasterApplicationDetail> Applications { get; set; } = [];
}

public class ConsumerSearchViewModel
{
    public string ModuleTitle { get; set; } = string.Empty;

    public string CreateAction { get; set; } = "Create";

    public string Search { get; set; } = string.Empty;

    public IReadOnlyList<ConsumerDetailsMaster> Consumers { get; set; } = [];
}

public class NameTransferCreateViewModel
{
    [Required]
    public string ConsumerNo { get; set; } = string.Empty;

    public ConsumerDetailsMaster? Consumer { get; set; }

    [Required, StringLength(100)]
    public string NewConsumerName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NewFatherName { get; set; }

    [StringLength(12)]
    public string? MobileNo { get; set; }

    [StringLength(50)]
    public string? ChallanNo { get; set; }

    public DateTime? ChallanDate { get; set; }

    public decimal? TransferFee { get; set; }

    public decimal? SecurityAmount { get; set; }

    [Required, StringLength(500)]
    public string Remarks { get; set; } = string.Empty;
}

public class ConnectionChangeCreateViewModel
{
    [Required]
    public string ConsumerNo { get; set; } = string.Empty;

    public ConsumerDetailsMaster? Consumer { get; set; }

    [StringLength(2)]
    public string? NewConnectionType { get; set; }

    [Required, StringLength(10)]
    public string NewConsumerCategory { get; set; } = string.Empty;

    public DateTime TypeChangeDate { get; set; } = DateTime.Today;

    [StringLength(10)]
    public string? EstimationNo { get; set; }

    public decimal? EstimationAmount { get; set; }

    public decimal? SecurityAmount { get; set; }

    public decimal? MonthlyRate { get; set; }

    [Required, StringLength(500)]
    public string Remarks { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> ConnectionTypeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> ConsumerCategoryOptions { get; set; } = [];
}

public class ConsumerChangeDetailsViewModel
{
    public string ModuleTitle { get; set; } = string.Empty;

    public string BackAction { get; set; } = "Index";

    public MasterApplicationDetail Application { get; set; } = null!;

    public ConsumerDetailsMaster? Consumer { get; set; }

    public IReadOnlyList<MasterApplicationDetailHistory> Histories { get; set; } = [];

    public IReadOnlyDictionary<string, string> DetailValues { get; set; } = new Dictionary<string, string>();

    public bool CanApprove => string.Equals(Application.ApplicationStatus, "Pending", StringComparison.OrdinalIgnoreCase);

    public bool CanReject => string.Equals(Application.ApplicationStatus, "Pending", StringComparison.OrdinalIgnoreCase);
}

public class ConsumerChangeActionViewModel
{
    public string ApplicationId { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Remarks { get; set; } = string.Empty;
}
