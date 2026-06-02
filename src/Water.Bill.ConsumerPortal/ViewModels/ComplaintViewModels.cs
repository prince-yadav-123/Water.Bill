using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerComplaintFormViewModel
{
    [Required(ErrorMessage = "Please select complaint category.")]
    public int? CategoryId { get; set; }

    [Required, StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2500)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string ConsumerNo { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LocationDetails { get; set; }

    [StringLength(50)]
    public string? RelatedBillNo { get; set; }

    [StringLength(50)]
    public string? RelatedApplicationNo { get; set; }

    [StringLength(15)]
    public string? MobileNo { get; set; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string Priority { get; set; } = "Normal";

    public List<SelectListItem> Categories { get; set; } = [];
}

public class ConsumerComplaintListViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? Status { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];

    public IReadOnlyList<ConsumerComplaint> Complaints { get; set; } = [];
}
