using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerSupportQueryHistoryConfiguration : IEntityTypeConfiguration<ConsumerSupportQueryHistory>
{
    public void Configure(EntityTypeBuilder<ConsumerSupportQueryHistory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerSupportQueryHistories");
        entity.HasIndex(e => e.QueryId, "IX_ConsumerSupportQueryHistories_QueryId");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.FromStatus).HasMaxLength(30);
        entity.Property(e => e.ToStatus).HasMaxLength(30);
        entity.Property(e => e.Action).HasMaxLength(50);
        entity.Property(e => e.Remarks).HasMaxLength(1000);
        entity.Property(e => e.ActionByName).HasMaxLength(150);
        entity.Property(e => e.ActionByRole).HasMaxLength(50);
        entity.Property(e => e.ActionAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(e => e.Query)
            .WithMany(e => e.Histories)
            .HasForeignKey(e => e.QueryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsumerSupportQueryHistories_Query");
    }
}
