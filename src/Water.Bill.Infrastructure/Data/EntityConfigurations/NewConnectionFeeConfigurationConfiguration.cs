using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConnectionFeeConfigurationConfiguration : IEntityTypeConfiguration<NewConnectionFeeConfiguration>
{
    public void Configure(EntityTypeBuilder<NewConnectionFeeConfiguration> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("NewConnectionFeeConfigurations");
        entity.HasIndex(e => new { e.ConnectionCategory, e.ConnectionType, e.PipeSize, e.IsActive }, "IX_NewConnectionFeeConfigurations_Lookup");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ConnectionCategory).HasMaxLength(20);
        entity.Property(e => e.ConnectionType).HasMaxLength(50);
        entity.Property(e => e.PipeSize).HasPrecision(8, 2);
        entity.Property(e => e.PlotSizeFrom).HasPrecision(12, 2);
        entity.Property(e => e.PlotSizeTo).HasPrecision(12, 2);
        entity.Property(e => e.ApplicationFee).HasPrecision(12, 2);
        entity.Property(e => e.ProcessingFee).HasPrecision(12, 2);
        entity.Property(e => e.SecurityAmount).HasPrecision(12, 2);
        entity.Property(e => e.MeterInstallationFee).HasPrecision(12, 2);
        entity.Property(e => e.OtherCharges).HasPrecision(12, 2);
        entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
        entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");
        entity.Property(e => e.EffectiveTo).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
    }
}
