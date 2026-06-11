using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerDisconnectionCaseHistoryConfiguration : IEntityTypeConfiguration<ConsumerDisconnectionCaseHistory>
{
    public void Configure(EntityTypeBuilder<ConsumerDisconnectionCaseHistory> entity)
    {
        entity.ToTable("ConsumerDisconnectionCaseHistories");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => new { e.CaseId, e.ActionAt }, "IX_ConsumerDisconnectionCaseHistories_Case_ActionAt");

        entity.Property(e => e.FromStatus).HasMaxLength(30);
        entity.Property(e => e.ToStatus).HasMaxLength(30).IsRequired();
        entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.ActionByName).HasMaxLength(100);
        entity.Property(e => e.ActionAt).HasColumnType("datetime");
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}

