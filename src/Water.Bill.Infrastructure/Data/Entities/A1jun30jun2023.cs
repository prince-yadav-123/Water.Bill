using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class A1jun30jun2023
{
    public string ReceiptId1 { get; set; } = null!;

    public string ConsNo { get; set; } = null!;

    public string FlatNo { get; set; } = null!;

    public string Blk { get; set; } = null!;

    public string Sec { get; set; } = null!;

    public DateTime BlPerFr { get; set; }

    public DateTime BlPerTo { get; set; }

    public DateTime DueDt { get; set; }

    public double BillAmt { get; set; }

    public double Surcharge { get; set; }

    public double PaidAmt { get; set; }

    public DateTime PayDate { get; set; }

    public double Arrear { get; set; }

    public double Credit { get; set; }

    public string RecpNo { get; set; } = null!;

    public string DisCd { get; set; } = null!;

    public double Noc { get; set; }

    public double Rmc { get; set; }

    public double Secu { get; set; }

    public double TFee { get; set; }

    public string Css { get; set; } = null!;

    public string BnkCd { get; set; } = null!;

    public string BrNm { get; set; } = null!;

    public string RevBilFr { get; set; } = null!;

    public string Match { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Err { get; set; } = null!;

    public DateTime EarlDt { get; set; }

    public string Upd { get; set; } = null!;

    public DateTime BilFrOld { get; set; }

    public DateTime BilToOld { get; set; }

    public int BlPadOld { get; set; }

    public string Dup { get; set; } = null!;

    public string Mst { get; set; } = null!;

    public string RevConDt { get; set; } = null!;

    public string A { get; set; } = null!;

    public string Reb { get; set; } = null!;

    public string ManLed { get; set; } = null!;

    public double DevType { get; set; }

    public double Gst { get; set; }

    public double ConnCharge { get; set; }

    public double PanalityCharges { get; set; }

    public string AcountNo { get; set; } = null!;

    public string BankId { get; set; } = null!;

    public string DeposeterName { get; set; } = null!;

    public string ReceiptId { get; set; } = null!;

    public string BillId { get; set; } = null!;

    public DateTime ArrearFrom { get; set; }

    public DateTime ArrearTo { get; set; }

    public string Status { get; set; } = null!;

    public DateTime EntryDate { get; set; }

    public string ChallanVia { get; set; } = null!;

    public int ChallanStatus { get; set; }

    public string Userid { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double RT { get; set; }

    public double Disconnection { get; set; }

    public double Reconnection { get; set; }

    public string Rid { get; set; } = null!;

    public long? Id { get; set; }

    public string PaymentMode { get; set; } = null!;
}
