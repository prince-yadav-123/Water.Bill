using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class BankMaster
{
    public int Id { get; set; }

    public double? BankId { get; set; }

    public string? BankName { get; set; }

    public double? IsActive { get; set; }

    public double? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public string? ModifiedOn { get; set; }

    public string? BankType { get; set; }

    public string? IsOnline { get; set; }

    public string? IsOffline { get; set; }

    public string? IsNgt { get; set; }

    public double? BankId1 { get; set; }

    public double? BranchId { get; set; }

    public string? BranchName { get; set; }

    public double? AccountNumber { get; set; }

    public string? Ifsccode { get; set; }

    public double? IsActive1 { get; set; }

    public double? CreatedBy1 { get; set; }

    public DateTime? CreatedOn1 { get; set; }

    public double? ModifiedBy1 { get; set; }

    public DateTime? ModifiedOn1 { get; set; }

    public string? BankBranchCode { get; set; }

    public double? BranchLedgerId { get; set; }

    public string? BranchType { get; set; }

    public string? Prefix { get; set; }

    public string? FixedAccountNumber { get; set; }

    public string? IsNgt1 { get; set; }

    public string? OnlineChallanText { get; set; }

    public string? OfflineChallanText { get; set; }

    public int? Status { get; set; }

    public bool? IsAppOnline { get; set; }
}
