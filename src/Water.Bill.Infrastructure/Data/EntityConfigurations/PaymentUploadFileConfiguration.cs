using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentUploadFileConfiguration : IEntityTypeConfiguration<PaymentUploadFile>
{
    public void Configure(EntityTypeBuilder<PaymentUploadFile> entity)
    {
        entity
            .HasNoKey()
            .ToTable("payment_upload_file");
        
        entity.HasIndex(e => e.FileId, "IX_PAYMENT_UPLOAD_FILE_FILE_ID_AUTO");
        
        entity.Property(e => e.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.FailureCount).HasColumnName("FAILURE_COUNT");
        entity.Property(e => e.FileId)
            .ValueGeneratedOnAdd()
            .HasColumnName("FILE_ID");
        entity.Property(e => e.FileName)
            .HasColumnType("text")
            .HasColumnName("FILE_NAME");
        entity.Property(e => e.FilePath)
            .HasColumnType("text")
            .HasColumnName("FILE_PATH");
        entity.Property(e => e.LastUpdatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .IsFixedLength()
            .HasColumnName("STATUS");
        entity.Property(e => e.SuccessCount).HasColumnName("SUCCESS_COUNT");
        entity.Property(e => e.TotalCount).HasColumnName("TOTAL_COUNT");
        
    }
}
