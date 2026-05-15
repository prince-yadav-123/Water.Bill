using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AppuserConfiguration : IEntityTypeConfiguration<Appuser>
{
    public void Configure(EntityTypeBuilder<Appuser> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity
            .ToTable("appusers")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => e.RoleId, "FK_AppUsers_AppRoles_RoleId");
        
        entity.HasIndex(e => e.Email, "UX_AppUsers_Email").IsUnique();
        
        entity.HasIndex(e => e.Username, "UX_AppUsers_Username").IsUnique();
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.Email).HasMaxLength(150);
        entity.Property(e => e.FullName).HasMaxLength(150);
        entity.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValueSql("'1'");
        entity.Property(e => e.LastLoginAt).HasMaxLength(6);
        entity.Property(e => e.LastLoginIp).HasMaxLength(64);
        entity.Property(e => e.LockoutUntil).HasMaxLength(6);
        entity.Property(e => e.PasswordChangedAt).HasMaxLength(6);
        entity.Property(e => e.PasswordHash).HasMaxLength(500);
        entity.Property(e => e.PhoneNumber).HasMaxLength(30);
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        entity.Property(e => e.Username).HasMaxLength(100);
        
        entity.HasOne(d => d.Role).WithMany(p => p.Appusers)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_AppUsers_AppRoles_RoleId");
        
    }
}
