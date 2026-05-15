using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalnoidaBankpayMaster
{
    public string Jalrefid { get; set; } = null!;

    public string? Consid { get; set; }

    public string? ConsName { get; set; }

    public string? ConsProperty { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public double? Payamount { get; set; }

    public string? EmailId { get; set; }

    public string? MobileNo { get; set; }

    public string? DepositBank { get; set; }

    public string? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? Disclaimer { get; set; }

    public string? Paymentstatus { get; set; }

    public string? ChallanNo { get; set; }

    public string? DueDate { get; set; }

    public string? BillNdc { get; set; }

    public string? BillNo { get; set; }
}
