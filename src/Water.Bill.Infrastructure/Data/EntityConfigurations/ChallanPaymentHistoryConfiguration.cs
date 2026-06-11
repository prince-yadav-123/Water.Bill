using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ChallanPaymentHistoryConfiguration : IEntityTypeConfiguration<ChallanPaymentHistory>
{
    public void Configure(EntityTypeBuilder<ChallanPaymentHistory> entity)
    {
        entity.ToTable("ChallanPaymentHistories");

        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ChallanNo).HasMaxLength(30);
        entity.Property(e => e.ConsumerNo).HasMaxLength(15);
        entity.Property(e => e.SourceBillNo).HasMaxLength(30);
        entity.Property(e => e.PaymentDate).HasColumnType("datetime");
        entity.Property(e => e.PaymentMode).HasMaxLength(50);
        entity.Property(e => e.BankCode).HasMaxLength(100);
        entity.Property(e => e.BankName).HasMaxLength(150);
        entity.Property(e => e.TransactionReferenceNo).HasMaxLength(100);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.PostedByName).HasMaxLength(150);
        entity.Property(e => e.PostedOn).HasColumnType("datetime");

        entity.HasIndex(e => e.ChallanId, "IX_ChallanPaymentHistories_ChallanId");
        entity.HasIndex(e => e.ChallanNo, "IX_ChallanPaymentHistories_ChallanNo");
        entity.HasIndex(e => e.ConsumerNo, "IX_ChallanPaymentHistories_ConsumerNo");
        entity.HasIndex(e => e.PaymentDate, "IX_ChallanPaymentHistories_PaymentDate");
    }
}

