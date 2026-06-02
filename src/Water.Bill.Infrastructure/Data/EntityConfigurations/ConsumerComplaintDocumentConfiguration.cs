using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerComplaintDocumentConfiguration : IEntityTypeConfiguration<ConsumerComplaintDocument>
{
    public void Configure(EntityTypeBuilder<ConsumerComplaintDocument> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerComplaintDocuments");
        entity.HasIndex(e => e.ComplaintId, "IX_ConsumerComplaintDocuments_ComplaintId");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.DocumentType).HasMaxLength(100);
        entity.Property(e => e.FileName).HasMaxLength(255);
        entity.Property(e => e.FilePath).HasMaxLength(500);
        entity.Property(e => e.ContentType).HasMaxLength(100);
        entity.Property(e => e.UploadedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(e => e.Complaint)
            .WithMany(e => e.Documents)
            .HasForeignKey(e => e.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsumerComplaintDocuments_Complaint");
    }
}
