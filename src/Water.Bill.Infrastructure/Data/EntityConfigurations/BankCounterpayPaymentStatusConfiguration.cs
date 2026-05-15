using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class BankCounterpayPaymentStatusConfiguration : IEntityTypeConfiguration<BankCounterpayPaymentStatus>
{
    public void Configure(EntityTypeBuilder<BankCounterpayPaymentStatus> entity)
    {
        entity
            .HasNoKey()
            .ToTable("bank_counterpay_payment_status");
        
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Id)
            .HasMaxLength(50)
            .HasColumnName("ID");
        entity.Property(e => e.PaymentStatus)
            .HasMaxLength(50)
            .HasColumnName("PAYMENT_STATUS");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasDefaultValueSql("'1'")
            .HasColumnName("status");
        
    }
}
