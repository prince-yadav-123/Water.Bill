using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class RolepermissionConfiguration : IEntityTypeConfiguration<Rolepermission>
{
    public void Configure(EntityTypeBuilder<Rolepermission> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity
            .ToTable("rolepermissions")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => new { e.RoleId, e.Module }, "UX_RolePermissions_RoleId_Module");
        entity.HasIndex(e => e.ModuleId, "IX_RolePermissions_ModuleId");
        entity.HasIndex(e => new { e.RoleId, e.ModuleId }, "UX_RolePermissions_RoleId_ModuleId").IsUnique();
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.Module).HasMaxLength(100);
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        
        entity.HasOne(d => d.Role).WithMany(p => p.Rolepermissions)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RolePermissions_AppRoles_RoleId");

        entity.HasOne(d => d.PermissionModule).WithMany(p => p.Rolepermissions)
            .HasForeignKey(d => d.ModuleId)
            .HasConstraintName("FK_RolePermissions_PermissionModules_ModuleId");
        
    }
}
