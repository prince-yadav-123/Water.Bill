using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerComplaintConfiguration : IEntityTypeConfiguration<ConsumerComplaint>
{
    public void Configure(EntityTypeBuilder<ConsumerComplaint> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerComplaints");
        entity.HasIndex(e => e.ComplaintNo, "UX_ConsumerComplaints_ComplaintNo").IsUnique();
        entity.HasIndex(e => e.ConsumerNo, "IX_ConsumerComplaints_ConsumerNo");
        entity.HasIndex(e => e.Status, "IX_ConsumerComplaints_Status");
        entity.HasIndex(e => e.CategoryId, "IX_ConsumerComplaints_CategoryId");
        entity.HasIndex(e => e.CreatedAt, "IX_ConsumerComplaints_CreatedAt");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ComplaintNo).HasMaxLength(30);
        entity.Property(e => e.ConsumerNo).HasMaxLength(20);
        entity.Property(e => e.ConsumerName).HasMaxLength(150);
        entity.Property(e => e.MobileNo).HasMaxLength(15);
        entity.Property(e => e.Email).HasMaxLength(100);
        entity.Property(e => e.CategoryName).HasMaxLength(100);
        entity.Property(e => e.Subject).HasMaxLength(150);
        entity.Property(e => e.Description).HasMaxLength(2500);
        entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Normal");
        entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("Open");
        entity.Property(e => e.LocationDetails).HasMaxLength(500);
        entity.Property(e => e.RelatedBillNo).HasMaxLength(50);
        entity.Property(e => e.RelatedApplicationNo).HasMaxLength(50);
        entity.Property(e => e.AdminRemarks).HasMaxLength(1000);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
        entity.Property(e => e.ClosedAt).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);

        entity.HasOne(e => e.Category)
            .WithMany(e => e.Complaints)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ConsumerComplaints_Category");
    }
}

