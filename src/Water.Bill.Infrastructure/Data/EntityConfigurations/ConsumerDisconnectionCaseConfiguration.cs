using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerDisconnectionCaseConfiguration : IEntityTypeConfiguration<ConsumerDisconnectionCase>
{
    public void Configure(EntityTypeBuilder<ConsumerDisconnectionCase> entity)
    {
        entity.ToTable("ConsumerDisconnectionCases");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.CaseNo).IsUnique();
        entity.HasIndex(e => new { e.ConsumerNo, e.Status, e.IsDeleted }, "IX_ConsumerDisconnectionCases_Consumer_Status");
        entity.HasIndex(e => new { e.NoticeDate, e.Status, e.IsDeleted }, "IX_ConsumerDisconnectionCases_Notice_Status");

        entity.Property(e => e.CaseNo).HasMaxLength(30).IsRequired();
        entity.Property(e => e.ConsumerNo).HasMaxLength(20).IsRequired();
        entity.Property(e => e.CaseType).HasMaxLength(30).IsRequired();
        entity.Property(e => e.Reason).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
        entity.Property(e => e.NoticeDate).HasColumnType("datetime");
        entity.Property(e => e.DueDate).HasColumnType("datetime");
        entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(e => e.DisconnectionFee).HasPrecision(18, 2);
        entity.Property(e => e.ReconnectionFee).HasPrecision(18, 2);
        entity.Property(e => e.DisconnectedOn).HasColumnType("datetime");
        entity.Property(e => e.ReconnectionRequestedOn).HasColumnType("datetime");
        entity.Property(e => e.ReconnectedOn).HasColumnType("datetime");
        entity.Property(e => e.ChallanNo).HasMaxLength(30);
        entity.Property(e => e.FieldOfficerName).HasMaxLength(100);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.PreviousConsumerCategory).HasMaxLength(20);
        entity.Property(e => e.CreatedByName).HasMaxLength(100);
        entity.Property(e => e.UpdatedByName).HasMaxLength(100);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        entity.HasMany(e => e.Histories)
            .WithOne(e => e.Case)
            .HasForeignKey(e => e.CaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
