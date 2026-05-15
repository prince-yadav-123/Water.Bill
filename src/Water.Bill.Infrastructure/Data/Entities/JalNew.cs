using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalNew
{
    public string? ReceiptId { get; set; }

    public string? DeposeterName { get; set; }

    public string? AlloteName { get; set; }

    public string? Userid { get; set; }

    public string? BankId { get; set; }

    public string? BankName { get; set; }

    public string? Branch { get; set; }

    public string? AccountNo { get; set; }

    public string? ChallanId { get; set; }

    public DateTime? DepositDate { get; set; }

    public double? AmountPaid { get; set; }

    public double? ReceiptHeadId { get; set; }

    public string? RecieptHeadName { get; set; }

    public double? ReceiptSubheadId { get; set; }

    public string? ReceiptSubHead { get; set; }

    public string? DeptName { get; set; }

    public int? DeptId { get; set; }

    public string? PropRegId { get; set; }

    public string? PropertyNumber { get; set; }

    public string? Sector { get; set; }

    public string? BlkNo { get; set; }

    public string? FlatNo { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? DevType { get; set; }
}
