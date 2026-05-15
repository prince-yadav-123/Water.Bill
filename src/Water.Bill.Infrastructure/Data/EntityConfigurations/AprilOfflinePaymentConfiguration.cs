using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AprilOfflinePaymentConfiguration : IEntityTypeConfiguration<AprilOfflinePayment>
{
    public void Configure(EntityTypeBuilder<AprilOfflinePayment> entity)
    {
        entity
            .HasNoKey()
            .ToTable("april_offline_payment");
        
        entity.Property(e => e.AcountNo).HasColumnName("acount_no");
        entity.Property(e => e.Address).HasColumnName("ADDRESS");
        entity.Property(e => e.Arrear).HasColumnName("ARREAR");
        entity.Property(e => e.ArrearFrom).HasColumnName("ARREAR_FROM");
        entity.Property(e => e.ArrearTo).HasColumnName("ARREAR_TO");
        entity.Property(e => e.BankId)
            .HasMaxLength(50)
            .HasColumnName("bank_id");
        entity.Property(e => e.BilFrOld).HasColumnName("BIL_FR_OLD");
        entity.Property(e => e.BilToOld).HasColumnName("BIL_TO_OLD");
        entity.Property(e => e.BillAmt).HasColumnName("BILL_AMT");
        entity.Property(e => e.BillId).HasColumnName("BILL_ID");
        entity.Property(e => e.BlPadOld).HasColumnName("BL_PAD_OLD");
        entity.Property(e => e.BlPerFr).HasColumnName("BL_PER_FR");
        entity.Property(e => e.BlPerTo).HasColumnName("BL_PER_TO");
        entity.Property(e => e.Blk)
            .HasMaxLength(50)
            .HasColumnName("BLK");
        entity.Property(e => e.BnkCd)
            .HasMaxLength(100)
            .HasColumnName("BNK_CD");
        entity.Property(e => e.BrNm)
            .HasMaxLength(100)
            .HasColumnName("BR_NM");
        entity.Property(e => e.ChallanStatus).HasColumnName("CHALLAN_STATUS");
        entity.Property(e => e.ChallanVia)
            .HasMaxLength(50)
            .HasColumnName("CHALLAN_VIA");
        entity.Property(e => e.ConnCharge).HasColumnName("conn_charge");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Credit).HasColumnName("CREDIT");
        entity.Property(e => e.Css).HasColumnName("CSS");
        entity.Property(e => e.DeposeterName).HasColumnName("deposeter_name");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.DisCd).HasColumnName("DIS_CD");
        entity.Property(e => e.Disconnection).HasColumnName("disconnection");
        entity.Property(e => e.DueDt).HasColumnName("DUE_DT");
        entity.Property(e => e.Dup).HasColumnName("DUP");
        entity.Property(e => e.EarlDt).HasColumnName("EARL_DT");
        entity.Property(e => e.EntryDate).HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Err).HasColumnName("ERR");
        entity.Property(e => e.FlatNo)
            .HasMaxLength(50)
            .HasColumnName("FLAT_NO");
        entity.Property(e => e.Gst).HasColumnName("gst");
        entity.Property(e => e.Id)
            .HasMaxLength(1)
            .HasColumnName("ID");
        entity.Property(e => e.Key).HasColumnName("KEY");
        entity.Property(e => e.ManLed).HasColumnName("MAN_LED");
        entity.Property(e => e.Match).HasColumnName("MATCH");
        entity.Property(e => e.Mst).HasColumnName("MST");
        entity.Property(e => e.Noc).HasColumnName("NOC");
        entity.Property(e => e.PaidAmt).HasColumnName("PAID_AMT");
        entity.Property(e => e.PanalityCharges).HasColumnName("panality_charges");
        entity.Property(e => e.PayDate).HasColumnName("PAY_DATE");
        entity.Property(e => e.RT).HasColumnName("R_T");
        entity.Property(e => e.Reb).HasColumnName("REB");
        entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
        entity.Property(e => e.ReceiptId1).HasColumnName("RECEIPT_ID1");
        entity.Property(e => e.Reconnection).HasColumnName("reconnection");
        entity.Property(e => e.RecpNo).HasColumnName("RECP_NO");
        entity.Property(e => e.RevBilFr).HasColumnName("REV_BIL_FR");
        entity.Property(e => e.RevConDt).HasColumnName("REV_CON_DT");
        entity.Property(e => e.Rmc).HasColumnName("RMC");
        entity.Property(e => e.Sec)
            .HasMaxLength(50)
            .HasColumnName("SEC");
        entity.Property(e => e.Secu).HasColumnName("SECU");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.Surcharge).HasColumnName("SURCHARGE");
        entity.Property(e => e.TFee).HasColumnName("T_FEE");
        entity.Property(e => e.Upd).HasColumnName("UPD");
        entity.Property(e => e.Userid).HasColumnName("USERID");
        
    }
}
