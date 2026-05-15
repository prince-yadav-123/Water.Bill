using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NameChangeDetail
{
    public int AutoId { get; set; }

    public string? ConsumerNo { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string? Rid { get; set; }

    public string? TrackingId { get; set; }

    public string? OrderId { get; set; }

    public string? ChallanNo { get; set; }

    public decimal? Amount { get; set; }

    public string? ConsName { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? DivisionTypeName { get; set; }

    public int? DivisionType { get; set; }

    public string? Sector { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public string? PlotArea { get; set; }

    public string? PipeSize { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? CurrentStatus { get; set; }

    public string? FinalStatus { get; set; }

    public string? Level1Remark1 { get; set; }

    public string? Level1Remark2 { get; set; }

    public string? Level2Remark { get; set; }

    public DateTime? Level1ActionDate1 { get; set; }

    public DateTime? Level1ActionDate2 { get; set; }

    public DateTime? Level2ActionDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public string? Attribute1 { get; set; }

    public string? Attribute2 { get; set; }

    public string? Attribute3 { get; set; }

    public string? Attribute4 { get; set; }

    public string? Attribute5 { get; set; }

    public string? ConsTp { get; set; }

    public string? ChallanFile { get; set; }

    public string? SuccessStatus { get; set; }

    public DateOnly? BillFromDate { get; set; }

    public DateOnly? BillToDate { get; set; }

    public string? Attachment1 { get; set; }

    public string? Attachment2 { get; set; }

    public string? Attachment3 { get; set; }

    public string? BillNo { get; set; }

    public string? CertificateUrl { get; set; }

    public int? Level { get; set; }

    public string? Level2Remark2 { get; set; }

    public DateTime? Level2ActionDate2 { get; set; }

    public int? SecurityAmount { get; set; }

    public DateTime? PaymentSuccessDate { get; set; }
}
