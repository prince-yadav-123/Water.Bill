using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerNoticeConfiguration : IEntityTypeConfiguration<ConsumerNotice>
{
    public void Configure(EntityTypeBuilder<ConsumerNotice> entity)
    {
        entity.ToTable("ConsumerNotices");

        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.NoticeNo).IsUnique();
        entity.HasIndex(e => new { e.ConsumerNo, e.Status, e.IsDeleted }, "IX_ConsumerNotices_Consumer_Status");
        entity.HasIndex(e => new { e.NoticeDate, e.NoticeType, e.IsDeleted }, "IX_ConsumerNotices_Date_Type");

        entity.Property(e => e.NoticeNo).HasMaxLength(30).IsRequired();
        entity.Property(e => e.ConsumerNo).HasMaxLength(20).IsRequired();
        entity.Property(e => e.NoticeType).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Body).HasColumnType("text").IsRequired();
        entity.Property(e => e.NoticeDate).HasColumnType("datetime");
        entity.Property(e => e.DueDate).HasColumnType("datetime");
        entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
        entity.Property(e => e.RelatedBillNo).HasMaxLength(30);
        entity.Property(e => e.RelatedChallanNo).HasMaxLength(30);
        entity.Property(e => e.AmountDue).HasPrecision(18, 2);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.CreatedByName).HasMaxLength(100);
        entity.Property(e => e.UpdatedByName).HasMaxLength(100);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        entity.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasMany(e => e.Histories)
            .WithOne(e => e.Notice)
            .HasForeignKey(e => e.NoticeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

