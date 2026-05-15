using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class BankCounterpayDetail
{
    public string? TransactionFor { get; set; }

    public string? BillNo { get; set; }

    public string? ConsNo { get; set; }

    public string? MobileNo { get; set; }

    public DateTime? Date { get; set; }

    public string? Bank { get; set; }

    public double? Amount { get; set; }

    public string? PaymentMode { get; set; }

    public string? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? BankTransNo { get; set; }

    public string? ChqDdCash { get; set; }

    public DateTime? RealisationDate { get; set; }

    public string? Info1 { get; set; }

    public string? Info2 { get; set; }

    public string? Id { get; set; }

    public string? ChallanNo { get; set; }

    public string? Rid { get; set; }
}
