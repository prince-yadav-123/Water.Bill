using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterDocumentUploadConfiguration : IEntityTypeConfiguration<MasterDocumentUpload>
{
    public void Configure(EntityTypeBuilder<MasterDocumentUpload> entity)
    {
        entity.HasKey(e => e.DocumentId).HasName("PRIMARY");
        
        entity.ToTable("master_document_upload");
        
        entity.Property(e => e.DocumentId)
            .ValueGeneratedNever()
            .HasColumnName("Document_id");
        entity.Property(e => e.DocFor)
            .HasMaxLength(3)
            .HasColumnName("Doc_for");
        entity.Property(e => e.DocumentName)
            .HasMaxLength(100)
            .HasColumnName("Document_Name");
        entity.Property(e => e.Status).HasColumnName("status");
        
    }
}
