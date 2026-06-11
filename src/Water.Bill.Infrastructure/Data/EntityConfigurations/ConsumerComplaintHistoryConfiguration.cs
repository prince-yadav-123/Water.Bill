using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerComplaintHistoryConfiguration : IEntityTypeConfiguration<ConsumerComplaintHistory>
{
    public void Configure(EntityTypeBuilder<ConsumerComplaintHistory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerComplaintHistories");
        entity.HasIndex(e => e.ComplaintId, "IX_ConsumerComplaintHistories_ComplaintId");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.FromStatus).HasMaxLength(30);
        entity.Property(e => e.ToStatus).HasMaxLength(30);
        entity.Property(e => e.Action).HasMaxLength(50);
        entity.Property(e => e.Remarks).HasMaxLength(1000);
        entity.Property(e => e.ActionByName).HasMaxLength(100);
        entity.Property(e => e.ActionByRole).HasMaxLength(50);
        entity.Property(e => e.ActionAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(e => e.Complaint)
            .WithMany(e => e.Histories)
            .HasForeignKey(e => e.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsumerComplaintHistories_Complaint");
    }
}

