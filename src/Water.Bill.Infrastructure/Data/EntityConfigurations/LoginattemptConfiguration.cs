using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class LoginattemptConfiguration : IEntityTypeConfiguration<Loginattempt>
{
    public void Configure(EntityTypeBuilder<Loginattempt> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity
            .ToTable("loginattempts")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => e.UserId, "FK_LoginAttempts_AppUsers_UserId");
        
        entity.HasIndex(e => new { e.Username, e.CreatedAt }, "IX_LoginAttempts_Username_CreatedAt");
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.FailureReason).HasMaxLength(100);
        entity.Property(e => e.IpAddress).HasMaxLength(64);
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        entity.Property(e => e.UserAgent).HasMaxLength(500);
        entity.Property(e => e.Username).HasMaxLength(100);
        
        entity.HasOne(d => d.User).WithMany(p => p.Loginattempts)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_LoginAttempts_AppUsers_UserId");
        
    }
}
