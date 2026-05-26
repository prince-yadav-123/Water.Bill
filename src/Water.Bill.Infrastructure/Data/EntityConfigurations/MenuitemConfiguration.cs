using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MenuitemConfiguration : IEntityTypeConfiguration<Menuitem>
{
    public void Configure(EntityTypeBuilder<Menuitem> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity
            .ToTable("menuitems")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => e.ParentId, "FK_MenuItems_MenuItems_ParentId");
        
        entity.HasIndex(e => new { e.TenantId, e.ParentId, e.Order }, "IX_MenuItems_TenantId_ParentId_Order");
        entity.HasIndex(e => e.ModuleId, "IX_MenuItems_ModuleId");
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.Icon).HasMaxLength(100);
        entity.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValueSql("'1'");
        entity.Property(e => e.Label).HasMaxLength(100);
        entity.Property(e => e.Module).HasMaxLength(100);
        entity.Property(e => e.SectionLabel).HasMaxLength(100);
        entity.Property(e => e.ShowInSidebar)
            .HasDefaultValueSql("'1'");
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        entity.Property(e => e.Url).HasMaxLength(300);
        
        entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName("FK_MenuItems_MenuItems_ParentId");

        entity.HasOne(d => d.PermissionModule).WithMany(p => p.Menuitems)
            .HasForeignKey(d => d.ModuleId)
            .HasConstraintName("FK_MenuItems_PermissionModules_ModuleId");
        
    }
}
