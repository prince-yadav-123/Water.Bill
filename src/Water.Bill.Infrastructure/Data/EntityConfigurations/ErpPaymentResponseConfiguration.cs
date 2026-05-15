using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ErpPaymentResponseConfiguration : IEntityTypeConfiguration<ErpPaymentResponse>
{
    public void Configure(EntityTypeBuilder<ErpPaymentResponse> entity)
    {
        entity
            .HasNoKey()
            .ToTable("erp_payment_response");
        
        entity.Property(e => e.BankTransactionNo).HasMaxLength(50);
        entity.Property(e => e.ChallanNo).HasMaxLength(50);
        entity.Property(e => e.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.Of1)
            .HasColumnType("text")
            .HasColumnName("OF1");
        entity.Property(e => e.Of2)
            .HasColumnType("text")
            .HasColumnName("OF2");
        entity.Property(e => e.Of3)
            .HasColumnType("text")
            .HasColumnName("OF3");
        entity.Property(e => e.StatusCode).HasMaxLength(10);
        entity.Property(e => e.StatusMessage).HasColumnType("text");
        entity.Property(e => e.VoucherNumber).HasMaxLength(100);
        
    }
}
