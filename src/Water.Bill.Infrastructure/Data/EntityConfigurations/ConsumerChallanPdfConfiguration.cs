using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerChallanPdfConfiguration : IEntityTypeConfiguration<ConsumerChallanPdf>
{
    public void Configure(EntityTypeBuilder<ConsumerChallanPdf> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_challan_pdf");
        
        entity.HasIndex(e => e.AutoId, "IX_CONSUMER_CHALLAN_PDF_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.BillPeriod)
            .HasMaxLength(50)
            .HasColumnName("BILL_PERIOD");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(20)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.ConsumerName)
            .HasMaxLength(30)
            .HasColumnName("CONSUMER_NAME");
        entity.Property(e => e.ConsumerNo)
            .HasMaxLength(100)
            .HasColumnName("CONSUMER_NO");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.FileName)
            .HasMaxLength(50)
            .HasColumnName("FILE_NAME");
        entity.Property(e => e.FileUrl)
            .HasMaxLength(200)
            .HasColumnName("FILE_URL");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(15)
            .HasColumnName("MOBILE_NO");
        
    }
}
