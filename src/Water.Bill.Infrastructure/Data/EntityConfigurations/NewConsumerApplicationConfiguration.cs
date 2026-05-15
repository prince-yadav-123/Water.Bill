using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConsumerApplicationConfiguration : IEntityTypeConfiguration<NewConsumerApplication>
{
    public void Configure(EntityTypeBuilder<NewConsumerApplication> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");
        
        entity.ToTable("new_consumer_application");
        
        entity.Property(e => e.AutoId).HasColumnName("AUTO_ID");
        entity.Property(e => e.Address)
            .HasMaxLength(200)
            .HasColumnName("ADDRESS");
        entity.Property(e => e.ApplicantName)
            .HasMaxLength(100)
            .HasColumnName("APPLICANT_NAME");
        entity.Property(e => e.Block)
            .HasMaxLength(10)
            .HasColumnName("BLOCK");
        entity.Property(e => e.Category)
            .HasMaxLength(50)
            .HasColumnName("CATEGORY");
        entity.Property(e => e.CategoryId)
            .HasMaxLength(50)
            .HasColumnName("CATEGORY_ID");
        entity.Property(e => e.ConnectionType)
            .HasMaxLength(50)
            .HasColumnName("CONNECTION_TYPE");
        entity.Property(e => e.ConnectionTypeId).HasColumnName("CONNECTION_TYPE_ID");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.EmailId)
            .HasMaxLength(50)
            .HasColumnName("EMAIL_ID");
        entity.Property(e => e.FatherName)
            .HasMaxLength(100)
            .HasColumnName("FATHER_NAME");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.MobileNumber)
            .HasMaxLength(10)
            .HasColumnName("MOBILE_NUMBER");
        entity.Property(e => e.PipeSize).HasColumnName("PIPE_SIZE");
        entity.Property(e => e.PlotNo).HasColumnName("PLOT_NO");
        entity.Property(e => e.PurposeOfConnection)
            .HasMaxLength(100)
            .HasColumnName("PURPOSE_OF_CONNECTION");
        entity.Property(e => e.Sector)
            .HasMaxLength(10)
            .HasColumnName("SECTOR");
        entity.Property(e => e.Size).HasColumnName("SIZE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.SubCategory)
            .HasMaxLength(50)
            .HasColumnName("SUB_CATEGORY");
        entity.Property(e => e.SubCategoryId).HasColumnName("SUB_CATEGORY_ID");
        
    }
}
