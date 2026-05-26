using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class SecuritysettingConfiguration : IEntityTypeConfiguration<Securitysetting>
{
    public void Configure(EntityTypeBuilder<Securitysetting> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity.ToTable("securitysettings");

        entity.HasIndex(e => e.TenantId, "UX_SecuritySettings_TenantId").IsUnique();

        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.SessionTimeoutMinutes).HasDefaultValue(480);
        entity.Property(e => e.IdleTimeoutMinutes).HasDefaultValue(30);
        entity.Property(e => e.PasswordMinLength).HasDefaultValue(8);
        entity.Property(e => e.PasswordRequireUppercase).HasDefaultValue(true);
        entity.Property(e => e.PasswordRequireLowercase).HasDefaultValue(true);
        entity.Property(e => e.PasswordRequireDigit).HasDefaultValue(true);
        entity.Property(e => e.PasswordRequireSpecialChar).HasDefaultValue(true);
        entity.Property(e => e.PasswordExpiryDays).HasDefaultValue(90);
        entity.Property(e => e.PasswordHistoryCount).HasDefaultValue(5);
        entity.Property(e => e.MaxFailedLoginAttempts).HasDefaultValue(5);
        entity.Property(e => e.LockoutDurationMinutes).HasDefaultValue(15);
        entity.Property(e => e.CaptchaAfterAttempts).HasDefaultValue(3);
    }
}
