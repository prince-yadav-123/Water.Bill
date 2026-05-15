using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalnoidaBankpayMasterConfiguration : IEntityTypeConfiguration<JalnoidaBankpayMaster>
{
    public void Configure(EntityTypeBuilder<JalnoidaBankpayMaster> entity)
    {
        entity.HasKey(e => e.Jalrefid).HasName("PRIMARY");
        
        entity.ToTable("jalnoida_bankpay_master");
        
        entity.Property(e => e.Jalrefid)
            .HasMaxLength(12)
            .HasColumnName("JALREFID");
        entity.Property(e => e.BillNdc)
            .HasMaxLength(4)
            .HasColumnName("Bill_Ndc");
        entity.Property(e => e.BillNo)
            .HasMaxLength(15)
            .HasColumnName("Bill_No");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(20)
            .HasColumnName("Challan_No");
        entity.Property(e => e.ConsName)
            .HasMaxLength(150)
            .HasColumnName("CONS_NAME");
        entity.Property(e => e.ConsProperty)
            .HasMaxLength(50)
            .HasColumnName("CONS_PROPERTY");
        entity.Property(e => e.Consid)
            .HasMaxLength(8)
            .HasColumnName("CONSID");
        entity.Property(e => e.DateFrom)
            .HasColumnType("datetime")
            .HasColumnName("DATE_FROM");
        entity.Property(e => e.DateTo)
            .HasColumnType("datetime")
            .HasColumnName("DATE_TO");
        entity.Property(e => e.DepositBank)
            .HasMaxLength(20)
            .HasColumnName("DEPOSIT_BANK");
        entity.Property(e => e.Disclaimer)
            .HasMaxLength(1)
            .HasColumnName("disclaimer");
        entity.Property(e => e.DueDate).HasMaxLength(11);
        entity.Property(e => e.EmailId)
            .HasMaxLength(30)
            .HasColumnName("EMAIL_ID");
        entity.Property(e => e.EntryDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(15)
            .HasColumnName("MOBILE_NO");
        entity.Property(e => e.Payamount).HasColumnName("PAYAMOUNT");
        entity.Property(e => e.Paymentstatus)
            .HasMaxLength(1)
            .HasColumnName("PAYMENTSTATUS");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
