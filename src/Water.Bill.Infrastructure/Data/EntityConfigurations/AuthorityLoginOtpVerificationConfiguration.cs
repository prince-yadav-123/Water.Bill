using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AuthorityLoginOtpVerificationConfiguration : IEntityTypeConfiguration<AuthorityLoginOtpVerification>
{
    public void Configure(EntityTypeBuilder<AuthorityLoginOtpVerification> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.ToTable("AuthorityLoginOtpVerifications");

        entity.HasIndex(e => e.ChallengeToken, "UX_AuthorityLoginOtpVerifications_ChallengeToken").IsUnique();
        entity.HasIndex(e => new { e.UserId, e.IsActive, e.IsVerified }, "IX_AuthorityLoginOtpVerifications_ActiveLookup");

        entity.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.ChallengeToken)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.Channels)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.DeliverySummary)
            .HasMaxLength(300);

        entity.Property(e => e.OtpHash)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.OtpSalt)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSDATETIME()");

        entity.Property(e => e.IsActive)
            .HasDefaultValueSql("'1'");

        entity.Property(e => e.IsDeleted)
            .HasDefaultValueSql("'0'");

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_AuthorityLoginOtpVerifications_AppUsers_UserId");
    }
}
