using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ChallanHistoryConfiguration : IEntityTypeConfiguration<ChallanHistory>
{
    public void Configure(EntityTypeBuilder<ChallanHistory> entity)
    {
        entity.ToTable("ChallanHistories");

        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ChallanNo).HasMaxLength(30);
        entity.Property(e => e.ConsumerNo).HasMaxLength(15);
        entity.Property(e => e.FromStatus).HasMaxLength(30);
        entity.Property(e => e.ToStatus).HasMaxLength(30);
        entity.Property(e => e.Action).HasMaxLength(50);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.ActionByName).HasMaxLength(150);
        entity.Property(e => e.ActionOn).HasColumnType("datetime");

        entity.HasIndex(e => e.ChallanId, "IX_ChallanHistories_ChallanId");
        entity.HasIndex(e => e.ChallanNo, "IX_ChallanHistories_ChallanNo");
    }
}
