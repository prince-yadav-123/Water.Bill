using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ComplaintCategoryConfiguration : IEntityTypeConfiguration<ComplaintCategory>
{
    public void Configure(EntityTypeBuilder<ComplaintCategory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ComplaintCategories");
        entity.HasIndex(e => e.CategoryName, "UX_ComplaintCategories_CategoryName").IsUnique();

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CategoryName).HasMaxLength(100);
        entity.Property(e => e.Description).HasMaxLength(300);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}

