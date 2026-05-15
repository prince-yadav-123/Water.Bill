using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalNewConfiguration : IEntityTypeConfiguration<JalNew>
{
    public void Configure(EntityTypeBuilder<JalNew> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_new");
        
        entity.Property(e => e.AccountNo)
            .HasMaxLength(16)
            .HasColumnName("ACCOUNT_NO");
        entity.Property(e => e.AlloteName)
            .HasMaxLength(100)
            .HasColumnName("ALLOTE_NAME");
        entity.Property(e => e.AmountPaid).HasColumnName("AMOUNT_PAID");
        entity.Property(e => e.BankId)
            .HasMaxLength(20)
            .HasColumnName("BANK_ID");
        entity.Property(e => e.BankName)
            .HasMaxLength(100)
            .HasColumnName("BANK_NAME");
        entity.Property(e => e.BlkNo)
            .HasMaxLength(20)
            .HasColumnName("BLK_NO");
        entity.Property(e => e.Branch)
            .HasMaxLength(100)
            .HasColumnName("BRANCH");
        entity.Property(e => e.ChallanId)
            .HasMaxLength(12)
            .HasColumnName("CHALLAN_ID");
        entity.Property(e => e.DeposeterName)
            .HasMaxLength(50)
            .HasColumnName("DEPOSETER_NAME");
        entity.Property(e => e.DepositDate)
            .HasColumnType("datetime")
            .HasColumnName("DEPOSIT_DATE");
        entity.Property(e => e.DeptId).HasColumnName("DEPT_ID");
        entity.Property(e => e.DeptName)
            .HasMaxLength(100)
            .HasColumnName("DEPT_NAME");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.FlatNo)
            .HasMaxLength(50)
            .HasColumnName("FLAT_NO");
        entity.Property(e => e.PropRegId)
            .HasMaxLength(50)
            .HasColumnName("PROP_REG_ID");
        entity.Property(e => e.PropertyNumber)
            .HasMaxLength(150)
            .HasColumnName("PROPERTY_NUMBER");
        entity.Property(e => e.ReceiptHeadId).HasColumnName("RECEIPT_HEAD_ID");
        entity.Property(e => e.ReceiptId)
            .HasMaxLength(15)
            .HasColumnName("RECEIPT_ID");
        entity.Property(e => e.ReceiptSubHead)
            .HasMaxLength(100)
            .HasColumnName("RECEIPT_SUB_HEAD");
        entity.Property(e => e.ReceiptSubheadId).HasColumnName("RECEIPT_SUBHEAD_ID");
        entity.Property(e => e.RecieptHeadName)
            .HasMaxLength(50)
            .HasColumnName("RECIEPT_HEAD_NAME");
        entity.Property(e => e.Sector)
            .HasMaxLength(50)
            .HasColumnName("SECTOR");
        entity.Property(e => e.Userid)
            .HasMaxLength(20)
            .HasColumnName("USERID");
        
    }
}
