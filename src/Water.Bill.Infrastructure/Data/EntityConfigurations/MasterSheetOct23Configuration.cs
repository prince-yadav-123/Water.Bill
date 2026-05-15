using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterSheetOct23Configuration : IEntityTypeConfiguration<MasterSheetOct23>
{
    public void Configure(EntityTypeBuilder<MasterSheetOct23> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_sheet_oct23");
        
        entity.Property(e => e.A).HasMaxLength(1);
        entity.Property(e => e.AcountNo)
            .HasMaxLength(20)
            .HasColumnName("acount_no");
        entity.Property(e => e.Address)
            .HasMaxLength(100)
            .HasColumnName("ADDRESS");
        entity.Property(e => e.Arrear).HasColumnName("ARREAR");
        entity.Property(e => e.ArrearFrom)
            .HasColumnType("datetime")
            .HasColumnName("ARREAR_FROM");
        entity.Property(e => e.ArrearTo)
            .HasColumnType("datetime")
            .HasColumnName("ARREAR_TO");
        entity.Property(e => e.BankId)
            .HasMaxLength(20)
            .HasColumnName("bank_id");
        entity.Property(e => e.BilFrOld)
            .HasColumnType("datetime")
            .HasColumnName("BIL_FR_OLD");
        entity.Property(e => e.BilToOld)
            .HasColumnType("datetime")
            .HasColumnName("BIL_TO_OLD");
        entity.Property(e => e.BillAmt).HasColumnName("BILL_AMT");
        entity.Property(e => e.BillId)
            .HasMaxLength(20)
            .HasColumnName("BILL_ID");
        entity.Property(e => e.BlPadOld).HasColumnName("BL_PAD_OLD");
        entity.Property(e => e.BlPerFr)
            .HasColumnType("datetime")
            .HasColumnName("BL_PER_FR");
        entity.Property(e => e.BlPerTo)
            .HasColumnType("datetime")
            .HasColumnName("BL_PER_TO");
        entity.Property(e => e.Blk)
            .HasMaxLength(15)
            .HasColumnName("BLK");
        entity.Property(e => e.BnkCd)
            .HasMaxLength(100)
            .HasColumnName("BNK_CD");
        entity.Property(e => e.BrNm)
            .HasMaxLength(100)
            .HasColumnName("BR_NM");
        entity.Property(e => e.ChallanStatus).HasColumnName("CHALLAN_STATUS");
        entity.Property(e => e.ChallanVia)
            .HasMaxLength(20)
            .HasColumnName("CHALLAN_VIA");
        entity.Property(e => e.ConnCharge).HasColumnName("conn_charge");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(15)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Credit).HasColumnName("CREDIT");
        entity.Property(e => e.Css)
            .HasMaxLength(6)
            .HasColumnName("CSS");
        entity.Property(e => e.DeposeterName).HasColumnName("deposeter_name");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.DisCd)
            .HasMaxLength(2)
            .HasColumnName("DIS_CD");
        entity.Property(e => e.Disconnection).HasColumnName("disconnection");
        entity.Property(e => e.DueDt)
            .HasMaxLength(255)
            .HasColumnName("DUE_DT");
        entity.Property(e => e.Dup)
            .HasMaxLength(1)
            .HasColumnName("DUP");
        entity.Property(e => e.EarlDt)
            .HasColumnType("datetime")
            .HasColumnName("EARL_DT");
        entity.Property(e => e.EntryDate)
            .HasMaxLength(255)
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Err)
            .HasMaxLength(10)
            .HasColumnName("ERR");
        entity.Property(e => e.FlatNo)
            .HasMaxLength(15)
            .HasColumnName("FLAT_NO");
        entity.Property(e => e.Gst).HasColumnName("gst");
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.Key)
            .HasMaxLength(16)
            .HasColumnName("KEY");
        entity.Property(e => e.ManLed)
            .HasMaxLength(1)
            .HasColumnName("MAN_LED");
        entity.Property(e => e.Match)
            .HasMaxLength(1)
            .HasColumnName("MATCH");
        entity.Property(e => e.Mst)
            .HasMaxLength(1)
            .HasColumnName("MST");
        entity.Property(e => e.Noc).HasColumnName("NOC");
        entity.Property(e => e.PaidAmt).HasColumnName("PAID_AMT");
        entity.Property(e => e.PanalityCharges).HasColumnName("panality_charges");
        entity.Property(e => e.PayDate)
            .HasMaxLength(255)
            .HasColumnName("PAY_DATE");
        entity.Property(e => e.PaymentMode)
            .HasMaxLength(255)
            .HasColumnName("Payment Mode");
        entity.Property(e => e.RT).HasColumnName("R/T");
        entity.Property(e => e.Reb)
            .HasMaxLength(3)
            .HasColumnName("REB");
        entity.Property(e => e.ReceiptId)
            .HasMaxLength(22)
            .HasColumnName("receipt_id");
        entity.Property(e => e.ReceiptId1)
            .HasMaxLength(50)
            .HasColumnName("RECEIPT_ID1");
        entity.Property(e => e.Reconnection).HasColumnName("reconnection");
        entity.Property(e => e.RecpNo)
            .HasMaxLength(15)
            .HasColumnName("RECP_NO");
        entity.Property(e => e.RevBilFr)
            .HasMaxLength(10)
            .HasColumnName("REV_BIL_FR");
        entity.Property(e => e.RevConDt)
            .HasMaxLength(10)
            .HasColumnName("REV_CON_DT");
        entity.Property(e => e.Rid).HasMaxLength(15);
        entity.Property(e => e.Rmc).HasColumnName("RMC");
        entity.Property(e => e.Sec)
            .HasMaxLength(5)
            .HasColumnName("SEC");
        entity.Property(e => e.Secu).HasColumnName("SECU");
        entity.Property(e => e.Status)
            .HasMaxLength(2)
            .HasColumnName("STATUS");
        entity.Property(e => e.Surcharge).HasColumnName("SURCHARGE");
        entity.Property(e => e.TFee).HasColumnName("T_FEE");
        entity.Property(e => e.Upd)
            .HasMaxLength(2)
            .HasColumnName("UPD");
        entity.Property(e => e.Userid)
            .HasMaxLength(50)
            .HasColumnName("USERID");
        
    }
}
