using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NdcDocumentConfiguration : IEntityTypeConfiguration<NdcDocument>
{
    public void Configure(EntityTypeBuilder<NdcDocument> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");

        entity.ToTable("ndc_document");
        
        entity.HasIndex(e => e.AutoId, "IX_NDC_DOCUMENT_AUTO_ID_AUTO");
        
        entity.Property(e => e.AttachmentName)
            .HasColumnType("text")
            .HasColumnName("ATTACHMENT_NAME");
        entity.Property(e => e.AttachmentPath)
            .HasColumnType("text")
            .HasColumnName("ATTACHMENT_PATH");
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.ConsumerNo)
            .HasMaxLength(50)
            .HasColumnName("CONSUMER_NO");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.NdcAutoId).HasColumnName("NDC_AUTO_ID");
        entity.Property(e => e.Status)
            .HasDefaultValueSql("'1'")
            .HasColumnName("STATUS");
        
    }
}
