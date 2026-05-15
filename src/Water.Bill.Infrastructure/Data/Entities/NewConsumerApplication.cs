using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConsumerApplication
{
    public int AutoId { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public int? PlotNo { get; set; }

    public int? Size { get; set; }

    public int? PipeSize { get; set; }

    public string? CategoryId { get; set; }

    public string? Category { get; set; }

    public int? SubCategoryId { get; set; }

    public string? SubCategory { get; set; }

    public int? ConnectionTypeId { get; set; }

    public string? ConnectionType { get; set; }

    public string? PurposeOfConnection { get; set; }

    public string? ApplicantName { get; set; }

    public string? FatherName { get; set; }

    public string? MobileNumber { get; set; }

    public string? EmailId { get; set; }

    public string? Address { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public int? LastUpdatedBy { get; set; }
}
