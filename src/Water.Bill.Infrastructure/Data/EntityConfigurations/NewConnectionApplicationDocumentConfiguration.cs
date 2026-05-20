using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConnectionApplicationDocumentConfiguration : IEntityTypeConfiguration<NewConnectionApplicationDocument>
{
    public void Configure(EntityTypeBuilder<NewConnectionApplicationDocument> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("new_connection_application_documents");

        entity.HasIndex(e => e.ApplicationId, "IX_NewConnectionApplicationDocuments_ApplicationId");
        entity.HasIndex(e => e.DocumentType, "IX_NewConnectionApplicationDocuments_DocumentType");

        entity.Property(e => e.Id).HasColumnName("Id");
        entity.Property(e => e.ApplicationId).HasColumnName("ApplicationId");
        entity.Property(e => e.DocumentType).HasMaxLength(50).HasColumnName("DocumentType");
        entity.Property(e => e.DocumentNo).HasMaxLength(100).HasColumnName("DocumentNo");
        entity.Property(e => e.DocumentDate).HasColumnName("DocumentDate");
        entity.Property(e => e.FileName).HasMaxLength(200).HasColumnName("FileName");
        entity.Property(e => e.FilePath).HasMaxLength(500).HasColumnName("FilePath");
        entity.Property(e => e.ContentType).HasMaxLength(100).HasColumnName("ContentType");
        entity.Property(e => e.FileSize).HasColumnName("FileSize");
        entity.Property(e => e.UploadedBy).HasColumnName("UploadedBy");
        entity.Property(e => e.UploadedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("UploadedOn");
        entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("IsActive");
        entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");

        entity.HasOne(e => e.Application)
            .WithMany(e => e.Documents)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_NewConnectionApplicationDocuments_Applications");
    }
}
