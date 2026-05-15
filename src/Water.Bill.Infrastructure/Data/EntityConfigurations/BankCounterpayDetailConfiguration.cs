using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class BankCounterpayDetailConfiguration : IEntityTypeConfiguration<BankCounterpayDetail>
{
    public void Configure(EntityTypeBuilder<BankCounterpayDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("bank_counterpay_detail");
        
        entity.Property(e => e.Amount).HasColumnName("AMOUNT");
        entity.Property(e => e.Bank)
            .HasMaxLength(10)
            .HasColumnName("BANK");
        entity.Property(e => e.BankTransNo)
            .HasMaxLength(100)
            .HasColumnName("BANK_TRANS_NO");
        entity.Property(e => e.BillNo)
            .HasMaxLength(10)
            .HasColumnName("BILL_NO");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(10)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.ChqDdCash)
            .HasMaxLength(20)
            .HasColumnName("CHQ_DD_CASH");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(10)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Date)
            .HasColumnType("datetime")
            .HasColumnName("DATE");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Id)
            .HasMaxLength(50)
            .HasColumnName("ID");
        entity.Property(e => e.Info1)
            .HasMaxLength(200)
            .HasColumnName("INFO_1");
        entity.Property(e => e.Info2)
            .HasMaxLength(200)
            .HasColumnName("INFO_2");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(10)
            .HasColumnName("MOBILE_NO");
        entity.Property(e => e.PaymentMode)
            .HasMaxLength(5)
            .HasColumnName("PAYMENT_MODE");
        entity.Property(e => e.RealisationDate)
            .HasColumnType("datetime")
            .HasColumnName("REALISATION_DATE");
        entity.Property(e => e.Rid).HasMaxLength(15);
        entity.Property(e => e.Status)
            .HasMaxLength(5)
            .HasColumnName("STATUS");
        entity.Property(e => e.TransactionFor)
            .HasMaxLength(10)
            .HasColumnName("TRANSACTION_FOR");
        
    }
}
