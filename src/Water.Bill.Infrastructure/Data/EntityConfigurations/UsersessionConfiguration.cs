using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class UsersessionConfiguration : IEntityTypeConfiguration<Usersession>
{
    public void Configure(EntityTypeBuilder<Usersession> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity
            .ToTable("usersessions")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => new { e.UserId, e.IsActive }, "IX_UserSessions_UserId_IsActive");
        
        entity.HasIndex(e => e.SessionToken, "UX_UserSessions_SessionToken").IsUnique();
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.DeviceFingerprint).HasMaxLength(200);
        entity.Property(e => e.ExpiresAt).HasMaxLength(6);
        entity.Property(e => e.IpAddress).HasMaxLength(64);
        entity.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValueSql("'1'");
        entity.Property(e => e.LastActivityAt).HasMaxLength(6);
        entity.Property(e => e.RevokedAt).HasMaxLength(6);
        entity.Property(e => e.RevokedReason).HasMaxLength(100);
        entity.Property(e => e.SessionToken).HasMaxLength(200);
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        entity.Property(e => e.UserAgent).HasMaxLength(500);
        
        entity.HasOne(d => d.User).WithMany(p => p.Usersessions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserSessions_AppUsers_UserId");
        
    }
}
