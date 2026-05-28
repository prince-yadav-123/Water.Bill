using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerSupportQueryConfiguration : IEntityTypeConfiguration<ConsumerSupportQuery>
{
    public void Configure(EntityTypeBuilder<ConsumerSupportQuery> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerSupportQueries");
        entity.HasIndex(e => e.QueryNo, "UX_ConsumerSupportQueries_QueryNo").IsUnique();
        entity.HasIndex(e => e.ConsumerNo, "IX_ConsumerSupportQueries_ConsumerNo");
        entity.HasIndex(e => e.Status, "IX_ConsumerSupportQueries_Status");
        entity.HasIndex(e => e.CategoryId, "IX_ConsumerSupportQueries_CategoryId");
        entity.HasIndex(e => e.CreatedAt, "IX_ConsumerSupportQueries_CreatedAt");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.QueryNo).HasMaxLength(30);
        entity.Property(e => e.ConsumerNo).HasMaxLength(20);
        entity.Property(e => e.ConsumerName).HasMaxLength(150);
        entity.Property(e => e.MobileNo).HasMaxLength(15);
        entity.Property(e => e.Email).HasMaxLength(100);
        entity.Property(e => e.CategoryName).HasMaxLength(100);
        entity.Property(e => e.Subject).HasMaxLength(150);
        entity.Property(e => e.Description).HasMaxLength(2000);
        entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Normal");
        entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("Open");
        entity.Property(e => e.RelatedBillNo).HasMaxLength(50);
        entity.Property(e => e.RelatedApplicationNo).HasMaxLength(50);
        entity.Property(e => e.AdminRemarks).HasMaxLength(1000);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
        entity.Property(e => e.ClosedAt).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);

        entity.HasOne(e => e.Category)
            .WithMany(e => e.Queries)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ConsumerSupportQueries_Category");
    }
}
