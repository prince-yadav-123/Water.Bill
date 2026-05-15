using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class DocumentMasterConfiguration : IEntityTypeConfiguration<DocumentMaster>
{
    public void Configure(EntityTypeBuilder<DocumentMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("document_master");
        
        entity.HasIndex(e => e.AutoId, "IX_DOCUMENT_MASTER_AUTO_ID_AUTO");
        
        entity.Property(e => e.ApplicationId)
            .HasMaxLength(20)
            .HasColumnName("APPLICATION_ID");
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.DocumentName)
            .HasMaxLength(100)
            .HasColumnName("DOCUMENT_NAME");
        entity.Property(e => e.DocumentPath)
            .HasColumnType("text")
            .HasColumnName("DOCUMENT_PATH");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY");
        entity.Property(e => e.UpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("UPDATED_ON");
        
    }
}
