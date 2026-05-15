using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalPrintBillMasterConfiguration : IEntityTypeConfiguration<JalPrintBillMaster>
{
    public void Configure(EntityTypeBuilder<JalPrintBillMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_print_bill_master");
        
        entity.HasIndex(e => e.ChallanNo, "INX_JAL_PRINT_BILL_MASTER_CHALLAN_NO");
        
        entity.HasIndex(e => e.ConsNo, "INX_JAL_PRINT_BILL_MASTER_CONS_NO");
        
        entity.HasIndex(e => e.EntryDate, "INX_JAL_PRINT_BILL_MASTER_ENTRY_DATE");
        
        entity.Property(e => e.AdvAmt).HasColumnName("adv_amt");
        entity.Property(e => e.AfterDate)
            .HasMaxLength(100)
            .HasColumnName("AFTER_DATE");
        entity.Property(e => e.AfterDateAmt).HasColumnName("AFTER_DATE_AMT");
        entity.Property(e => e.Arear).HasColumnName("AREAR");
        entity.Property(e => e.ArearInt).HasColumnName("AREAR_INT");
        entity.Property(e => e.ArearIntText)
            .HasMaxLength(100)
            .HasColumnName("AREAR_INT_TEXT");
        entity.Property(e => e.ArearText)
            .HasMaxLength(100)
            .HasColumnName("AREAR_TEXT");
        entity.Property(e => e.BankCode)
            .HasMaxLength(10)
            .HasColumnName("BANK_CODE");
        entity.Property(e => e.BeforeDate)
            .HasMaxLength(100)
            .HasColumnName("BEFORE_DATE");
        entity.Property(e => e.BillAfterSepAmt).HasColumnName("bill_after_sep_amt");
        entity.Property(e => e.BillCount).HasColumnName("BILL_COUNT");
        entity.Property(e => e.BillDate)
            .HasColumnType("datetime")
            .HasColumnName("BILL_DATE");
        entity.Property(e => e.BillDateFrom)
            .HasColumnType("datetime")
            .HasColumnName("BILL_DATE_FROM");
        entity.Property(e => e.BillDateTo)
            .HasColumnType("datetime")
            .HasColumnName("BILL_DATE_TO");
        entity.Property(e => e.BillDueDate)
            .HasColumnType("datetime")
            .HasColumnName("BILL_DUE_DATE");
        entity.Property(e => e.BillNo)
            .HasMaxLength(20)
            .HasColumnName("BILL_NO");
        entity.Property(e => e.BillPercentage).HasColumnName("BILL_PERCENTAGE");
        entity.Property(e => e.BillRebateAmt).HasColumnName("BILL_REBATE_AMT");
        entity.Property(e => e.BillRebatePer).HasColumnName("BILL_REBATE_PER");
        entity.Property(e => e.BillType).HasColumnName("BILL_TYPE");
        entity.Property(e => e.CessAmt).HasColumnName("CESS_AMT");
        entity.Property(e => e.ChallanContent).HasColumnName("Challan_Content");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(10)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Diff).HasColumnName("diff");
        entity.Property(e => e.DivType)
            .HasMaxLength(50)
            .HasColumnName("Div_type");
        entity.Property(e => e.DueAmt).HasColumnName("due_amt");
        entity.Property(e => e.DueDate)
            .HasColumnType("datetime")
            .HasColumnName("Due_date");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.LastBillExtra).HasColumnName("LAST_BILL_EXTRA");
        entity.Property(e => e.LastPaidAmt).HasColumnName("LAST_PAID_AMT");
        entity.Property(e => e.MinRate).HasColumnName("MIN_RATE");
        entity.Property(e => e.MinTotalAmt).HasColumnName("MIN_TOTAL_AMT");
        entity.Property(e => e.NewRecord)
            .HasMaxLength(2)
            .HasColumnName("new_record");
        entity.Property(e => e.OldRate).HasColumnName("OLD_RATE");
        entity.Property(e => e.PaidAmt).HasColumnName("paid_amt");
        entity.Property(e => e.PaidDate)
            .HasColumnType("datetime")
            .HasColumnName("paid_date");
        entity.Property(e => e.PaidStatus)
            .HasMaxLength(2)
            .HasColumnName("PAID_STATUS");
        entity.Property(e => e.PartAmt).HasColumnName("Part_Amt");
        entity.Property(e => e.PaymentType).HasColumnName("PAYMENT_TYPE");
        entity.Property(e => e.PrintStatus).HasColumnName("PRINT_STATUS");
        entity.Property(e => e.Rid).HasMaxLength(15);
        entity.Property(e => e.SchemeId)
            .HasMaxLength(20)
            .HasColumnName("SCHEME_ID");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        entity.Property(e => e.TotalBillAmt).HasColumnName("TOTAL_BILL_AMT");
        entity.Property(e => e.UpdateRecord)
            .HasMaxLength(2)
            .HasColumnName("update_record");
        entity.Property(e => e.Userid)
            .HasMaxLength(10)
            .HasColumnName("USERID");
        
    }
}
