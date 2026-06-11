using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerAccountAdjustmentConfiguration : IEntityTypeConfiguration<ConsumerAccountAdjustment>
{
    public void Configure(EntityTypeBuilder<ConsumerAccountAdjustment> entity)
    {
        entity.ToTable("ConsumerAccountAdjustments");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.AdjustmentNo).IsUnique();
        entity.HasIndex(e => new { e.ConsumerNo, e.Status, e.IsDeleted }, "IX_ConsumerAccountAdjustments_Consumer_Status");
        entity.HasIndex(e => new { e.EffectiveDate, e.Status, e.IsDeleted }, "IX_ConsumerAccountAdjustments_Effective_Status");

        entity.Property(e => e.AdjustmentNo).HasMaxLength(30).IsRequired();
        entity.Property(e => e.ConsumerNo).HasMaxLength(20).IsRequired();
        entity.Property(e => e.AdjustmentType).HasMaxLength(30).IsRequired();
        entity.Property(e => e.Amount).HasPrecision(18, 2);
        entity.Property(e => e.SourceBillNo).HasMaxLength(30);
        entity.Property(e => e.SourceChallanNo).HasMaxLength(30);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
        entity.Property(e => e.AppliedBillNo).HasMaxLength(30);
        entity.Property(e => e.CreatedByName).HasMaxLength(100);
        entity.Property(e => e.UpdatedByName).HasMaxLength(100);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        entity.HasMany(e => e.Histories)
            .WithOne(e => e.Adjustment)
            .HasForeignKey(e => e.AdjustmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

