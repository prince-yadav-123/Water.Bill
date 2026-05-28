using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerSupportQueryDocumentConfiguration : IEntityTypeConfiguration<ConsumerSupportQueryDocument>
{
    public void Configure(EntityTypeBuilder<ConsumerSupportQueryDocument> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ConsumerSupportQueryDocuments");
        entity.HasIndex(e => e.QueryId, "IX_ConsumerSupportQueryDocuments_QueryId");

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.DocumentType).HasMaxLength(50).HasDefaultValue("Support Document");
        entity.Property(e => e.FileName).HasMaxLength(255);
        entity.Property(e => e.FilePath).HasMaxLength(500);
        entity.Property(e => e.ContentType).HasMaxLength(100);
        entity.Property(e => e.UploadedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(e => e.Query)
            .WithMany(e => e.Documents)
            .HasForeignKey(e => e.QueryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConsumerSupportQueryDocuments_Query");
    }
}
