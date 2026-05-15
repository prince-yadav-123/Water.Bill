using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ChallanMisc
{
    public string? ReceiptId1 { get; set; }

    public string? ConsNo { get; set; }

    public string? FlatNo { get; set; }

    public string? Blk { get; set; }

    public string? Sec { get; set; }

    public DateTime? BlPerFr { get; set; }

    public DateTime? BlPerTo { get; set; }

    public DateTime? DueDt { get; set; }

    public double? BillAmt { get; set; }

    public double? Surcharge { get; set; }

    public double? PaidAmt { get; set; }

    public DateTime? PayDate { get; set; }

    public double? Arrear { get; set; }

    public double? Credit { get; set; }

    public string? RecpNo { get; set; }

    public string? DisCd { get; set; }

    public double? Noc { get; set; }

    public double? Rmc { get; set; }

    public double? Secu { get; set; }

    public double? TFee { get; set; }

    public string? Css { get; set; }

    public string? BnkCd { get; set; }

    public string? BrNm { get; set; }

    public string? RevBilFr { get; set; }

    public string? Match { get; set; }

    public string? Key { get; set; }

    public string? Err { get; set; }

    public DateTime? EarlDt { get; set; }

    public string? Upd { get; set; }

    public DateTime? BilFrOld { get; set; }

    public DateTime? BilToOld { get; set; }

    public int? BlPadOld { get; set; }

    public string? Dup { get; set; }

    public string? Mst { get; set; }

    public string? RevConDt { get; set; }

    public string? A { get; set; }

    public string? Reb { get; set; }

    public string? ManLed { get; set; }

    public double? DevType { get; set; }

    public double? Gst { get; set; }

    public double? ConnCharge { get; set; }

    public double? PanalityCharges { get; set; }

    public string? AcountNo { get; set; }

    public string? BankId { get; set; }

    public string? DeposeterName { get; set; }

    public string? ReceiptId { get; set; }

    public string? BillId { get; set; }

    public DateTime? ArrearFrom { get; set; }

    public DateTime? ArrearTo { get; set; }

    public string? Status { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? ChallanVia { get; set; }

    public int? ChallanStatus { get; set; }

    public string? Userid { get; set; }

    public string? Address { get; set; }
}
