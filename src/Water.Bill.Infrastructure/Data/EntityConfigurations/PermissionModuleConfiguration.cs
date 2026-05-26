using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PermissionModuleConfiguration : IEntityTypeConfiguration<PermissionModule>
{
    public void Configure(EntityTypeBuilder<PermissionModule> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity.ToTable("PermissionModules");

        entity.HasIndex(e => new { e.Name, e.IsDeleted }, "UX_PermissionModules_Name_IsDeleted").IsUnique();

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.IsActive)
            .HasDefaultValueSql("'1'");

        entity.Property(e => e.IsDeleted)
            .HasDefaultValueSql("'0'");
    }
}
