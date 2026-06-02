using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerMeterReadingConfiguration : IEntityTypeConfiguration<ConsumerMeterReading>
{
    public void Configure(EntityTypeBuilder<ConsumerMeterReading> entity)
    {
        entity.ToTable("ConsumerMeterReadings");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.ReadingNo).IsUnique();
        entity.HasIndex(e => new { e.ConsumerNo, e.ReadingDate, e.IsDeleted }, "IX_ConsumerMeterReadings_Consumer_Date");
        entity.HasIndex(e => new { e.MeterStatus, e.IsDeleted }, "IX_ConsumerMeterReadings_Status");

        entity.Property(e => e.ReadingNo).HasMaxLength(30).IsRequired();
        entity.Property(e => e.ConsumerNo).HasMaxLength(20).IsRequired();
        entity.Property(e => e.ReadingDate).HasColumnType("datetime");
        entity.Property(e => e.PeriodFrom).HasColumnType("datetime");
        entity.Property(e => e.PeriodTo).HasColumnType("datetime");
        entity.Property(e => e.PreviousReading).HasPrecision(18, 2);
        entity.Property(e => e.CurrentReading).HasPrecision(18, 2);
        entity.Property(e => e.Consumption).HasPrecision(18, 2);
        entity.Property(e => e.MeterStatus).HasMaxLength(30).IsRequired();
        entity.Property(e => e.MeterNo).HasMaxLength(50);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.Source).HasMaxLength(30).IsRequired();
        entity.Property(e => e.RecordedByName).HasMaxLength(100);
        entity.Property(e => e.RecordedAt).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
