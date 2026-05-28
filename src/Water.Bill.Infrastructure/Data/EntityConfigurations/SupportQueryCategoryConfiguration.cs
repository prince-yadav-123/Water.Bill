using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class SupportQueryCategoryConfiguration : IEntityTypeConfiguration<SupportQueryCategory>
{
    public void Configure(EntityTypeBuilder<SupportQueryCategory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("SupportQueryCategories");
        entity.HasIndex(e => e.CategoryName, "UX_SupportQueryCategories_CategoryName").IsUnique();

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CategoryName).HasMaxLength(100);
        entity.Property(e => e.Description).HasMaxLength(250);
        entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
    }
}
