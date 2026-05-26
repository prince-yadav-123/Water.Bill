using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerOtpVerificationConfiguration : IEntityTypeConfiguration<ConsumerOtpVerification>
{
    public void Configure(EntityTypeBuilder<ConsumerOtpVerification> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity.ToTable("ConsumerOtpVerifications");

        entity.HasIndex(e => new { e.ConsumerNo, e.Purpose, e.CreatedAt }, "IX_ConsumerOtpVerifications_Consumer_Purpose_CreatedAt");
        entity.HasIndex(e => new { e.ConsumerNo, e.Purpose, e.IsActive, e.IsVerified }, "IX_ConsumerOtpVerifications_ActiveLookup");

        entity.Property(e => e.ConsumerNo)
            .IsRequired()
            .HasMaxLength(10);

        entity.Property(e => e.MobileNo)
            .IsRequired()
            .HasMaxLength(12);

        entity.Property(e => e.OtpHash)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.OtpSalt)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.Purpose)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("ConsumerLogin");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        entity.Property(e => e.IsActive)
            .HasDefaultValueSql("'1'");

        entity.Property(e => e.IsDeleted)
            .HasDefaultValueSql("'0'");

        entity.HasOne(e => e.Consumer)
            .WithMany()
            .HasForeignKey(e => e.ConsumerNo)
            .HasPrincipalKey(e => e.ConsNo)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ConsumerOtpVerifications_ConsumerDetailsMaster_ConsumerNo");
    }
}
