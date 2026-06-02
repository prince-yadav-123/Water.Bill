using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerAccountAdjustmentHistoryConfiguration : IEntityTypeConfiguration<ConsumerAccountAdjustmentHistory>
{
    public void Configure(EntityTypeBuilder<ConsumerAccountAdjustmentHistory> entity)
    {
        entity.ToTable("ConsumerAccountAdjustmentHistories");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => new { e.AdjustmentId, e.ActionAt }, "IX_ConsumerAccountAdjustmentHistories_Adjustment");

        entity.Property(e => e.FromStatus).HasMaxLength(20);
        entity.Property(e => e.ToStatus).HasMaxLength(20).IsRequired();
        entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.ActionByName).HasMaxLength(100);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
