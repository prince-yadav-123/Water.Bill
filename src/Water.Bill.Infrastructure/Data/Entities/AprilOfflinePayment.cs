using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class AprilOfflinePayment
{
    public double ReceiptId1 { get; set; }

    public string ConsNo { get; set; } = null!;

    public string FlatNo { get; set; } = null!;

    public string Blk { get; set; } = null!;

    public string Sec { get; set; } = null!;

    public DateOnly BlPerFr { get; set; }

    public DateOnly BlPerTo { get; set; }

    public DateOnly DueDt { get; set; }

    public int BillAmt { get; set; }

    public byte Surcharge { get; set; }

    public int PaidAmt { get; set; }

    public DateOnly? PayDate { get; set; }

    public byte Arrear { get; set; }

    public byte Credit { get; set; }

    public int RecpNo { get; set; }

    public byte DisCd { get; set; }

    public byte Noc { get; set; }

    public byte Rmc { get; set; }

    public byte Secu { get; set; }

    public byte TFee { get; set; }

    public byte Css { get; set; }

    public string BnkCd { get; set; } = null!;

    public string BrNm { get; set; } = null!;

    public byte RevBilFr { get; set; }

    public byte Match { get; set; }

    public byte Key { get; set; }

    public byte Err { get; set; }

    public DateOnly EarlDt { get; set; }

    public byte Upd { get; set; }

    public DateOnly BilFrOld { get; set; }

    public DateOnly BilToOld { get; set; }

    public byte BlPadOld { get; set; }

    public byte Dup { get; set; }

    public byte Mst { get; set; }

    public byte RevConDt { get; set; }

    public byte A { get; set; }

    public byte Reb { get; set; }

    public byte ManLed { get; set; }

    public byte DevType { get; set; }

    public byte Gst { get; set; }

    public byte ConnCharge { get; set; }

    public byte PanalityCharges { get; set; }

    public byte AcountNo { get; set; }

    public string BankId { get; set; } = null!;

    public string DeposeterName { get; set; } = null!;

    public double ReceiptId { get; set; }

    public byte BillId { get; set; }

    public DateOnly ArrearFrom { get; set; }

    public DateOnly ArrearTo { get; set; }

    public byte Status { get; set; }

    public DateOnly? EntryDate { get; set; }

    public string ChallanVia { get; set; } = null!;

    public byte ChallanStatus { get; set; }

    public byte Userid { get; set; }

    public byte Address { get; set; }

    public byte RT { get; set; }

    public byte Disconnection { get; set; }

    public byte Reconnection { get; set; }

    public int? Rid { get; set; }

    public string? Id { get; set; }
}
