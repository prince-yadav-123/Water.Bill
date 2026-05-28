using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PublicNewConnectionOtpVerificationConfiguration : IEntityTypeConfiguration<PublicNewConnectionOtpVerification>
{
    public void Configure(EntityTypeBuilder<PublicNewConnectionOtpVerification> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("public_new_connection_otp_verifications");
        entity.HasIndex(e => new { e.MobileNumber, e.Purpose, e.IsActive }, "IX_PublicNewConnectionOtp_Mobile_Purpose_Active");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.MobileNumber).HasMaxLength(12);
        entity.Property(e => e.OtpHash).HasMaxLength(128);
        entity.Property(e => e.OtpSalt).HasMaxLength(64);
        entity.Property(e => e.Purpose).HasMaxLength(50);
        entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
