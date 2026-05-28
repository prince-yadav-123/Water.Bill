using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.ViewModels;

public class ConsumerSupportQueryFormViewModel
{
    [Required(ErrorMessage = "Select query category.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Enter subject.")]
    [StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter query description.")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Consumer number is required.")]
    [StringLength(20)]
    public string ConsumerNo { get; set; } = string.Empty;

    [StringLength(50)]
    public string? RelatedBillNo { get; set; }

    [StringLength(50)]
    public string? RelatedApplicationNo { get; set; }

    [StringLength(15)]
    public string? MobileNo { get; set; }

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string Priority { get; set; } = "Normal";

    public List<SelectListItem> Categories { get; set; } = [];
}

public class ConsumerSupportQueryListViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? Status { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];

    public IReadOnlyList<ConsumerSupportQuery> Queries { get; set; } = [];
}
