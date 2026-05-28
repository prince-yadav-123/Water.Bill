using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConnectionApplicationFeeConfiguration : IEntityTypeConfiguration<NewConnectionApplicationFee>
{
    public void Configure(EntityTypeBuilder<NewConnectionApplicationFee> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("NewConnectionApplicationFees");
        entity.HasIndex(e => e.ApplicationId, "IX_NewConnectionApplicationFees_ApplicationId").IsUnique();
        entity.HasIndex(e => e.ApplicationNo, "IX_NewConnectionApplicationFees_ApplicationNo");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30);
        entity.Property(e => e.ApplicationFee).HasPrecision(12, 2);
        entity.Property(e => e.ProcessingFee).HasPrecision(12, 2);
        entity.Property(e => e.SecurityAmount).HasPrecision(12, 2);
        entity.Property(e => e.MeterInstallationFee).HasPrecision(12, 2);
        entity.Property(e => e.OtherCharges).HasPrecision(12, 2);
        entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
        entity.Property(e => e.PaymentStatus).HasMaxLength(30).HasDefaultValue("Pending");
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

        entity.HasOne(e => e.Application)
            .WithOne()
            .HasForeignKey<NewConnectionApplicationFee>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.FeeConfiguration)
            .WithMany(e => e.ApplicationFees)
            .HasForeignKey(e => e.FeeConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
