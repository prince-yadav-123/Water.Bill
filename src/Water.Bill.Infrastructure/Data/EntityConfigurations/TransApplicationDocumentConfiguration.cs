using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class TransApplicationDocumentConfiguration : IEntityTypeConfiguration<TransApplicationDocument>
{
    public void Configure(EntityTypeBuilder<TransApplicationDocument> entity)
    {
        entity
            .HasNoKey()
            .ToTable("trans_application_document");
        
        entity.HasIndex(e => e.ApplicationId, "FK_TRANS_APPLICATION_DOCUMENT_MasteR_application_detail");
        
        entity.Property(e => e.ApplicationId)
            .HasMaxLength(10)
            .HasColumnName("Application_id");
        entity.Property(e => e.DocumentDate).HasColumnName("document_date");
        entity.Property(e => e.DocumentId).HasColumnName("document_id");
        entity.Property(e => e.DocumentNo)
            .HasMaxLength(100)
            .HasColumnName("Document_no");
        entity.Property(e => e.DocumentUplod).HasColumnName("document_uplod");
        entity.Property(e => e.DocumentsUplod)
            .HasMaxLength(100)
            .HasColumnName("documents_uplod");
        entity.Property(e => e.EntryDate)
            .HasDefaultValueSql("curdate()")
            .HasColumnName("entry_date");
        entity.Property(e => e.Status).HasColumnName("status");
        
        entity.HasOne(d => d.Application).WithMany()
            .HasForeignKey(d => d.ApplicationId)
            .HasConstraintName("FK_TRANS_APPLICATION_DOCUMENT_MasteR_application_detail");
        
    }
}
