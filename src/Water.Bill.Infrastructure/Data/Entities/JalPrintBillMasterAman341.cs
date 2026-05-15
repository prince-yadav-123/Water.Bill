using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalPrintBillMasterAman341
{
    public string? BillNo { get; set; }

    public string? ConsNo { get; set; }

    public DateTime? BillDate { get; set; }

    public DateTime? BillDueDate { get; set; }

    public DateTime? BillDateFrom { get; set; }

    public DateTime? BillDateTo { get; set; }

    public double? MinRate { get; set; }

    public double? MinTotalAmt { get; set; }

    public int? BillRebatePer { get; set; }

    public double? BillRebateAmt { get; set; }

    public double? CessAmt { get; set; }

    public double? Arear { get; set; }

    public string? ArearText { get; set; }

    public double? ArearInt { get; set; }

    public string? ArearIntText { get; set; }

    public double? LastBillExtra { get; set; }

    public double? TotalBillAmt { get; set; }

    public string? BeforeDate { get; set; }

    public string? AfterDate { get; set; }

    public double? AfterDateAmt { get; set; }

    public string? DivType { get; set; }

    public string? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? DueDate { get; set; }

    public double? DueAmt { get; set; }

    public DateTime? PaidDate { get; set; }

    public double? PaidAmt { get; set; }

    public double? Diff { get; set; }

    public string? PaidStatus { get; set; }

    public string? NewRecord { get; set; }

    public string? UpdateRecord { get; set; }

    public double? BillAfterSepAmt { get; set; }

    public double? AdvAmt { get; set; }

    public int? PrintStatus { get; set; }

    public int? OldRate { get; set; }

    public string? BillType { get; set; }

    public double? LastPaidAmt { get; set; }

    public int? BillCount { get; set; }

    public string? SchemeId { get; set; }

    public int? BillPercentage { get; set; }

    public string? Userid { get; set; }

    public int? DevType { get; set; }

    public int? PaymentType { get; set; }

    public string? ChallanNo { get; set; }

    public string? BankCode { get; set; }

    public string? ChallanContent { get; set; }

    public string? Rid { get; set; }

    public int? PaymentMode { get; set; }

    public double? PartAmt { get; set; }
}
