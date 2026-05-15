using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ApplicationStatusConfiguration : IEntityTypeConfiguration<ApplicationStatus>
{
    public void Configure(EntityTypeBuilder<ApplicationStatus> entity)
    {
        entity
            .HasNoKey()
            .ToTable("application_status");
        
        entity.HasIndex(e => e.AutoId, "IX_APPLICATION_STATUS_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.IsActive)
            .HasDefaultValueSql("'1'")
            .HasColumnName("IS_ACTIVE");
        entity.Property(e => e.StatusName)
            .HasMaxLength(100)
            .HasColumnName("STATUS_NAME");
        
    }
}
