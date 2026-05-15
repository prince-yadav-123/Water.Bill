using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConnectionTypeMstConfiguration : IEntityTypeConfiguration<ConnectionTypeMst>
{
    public void Configure(EntityTypeBuilder<ConnectionTypeMst> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");
        
        entity.ToTable("connection_type_mst");
        
        entity.Property(e => e.AutoId).HasColumnName("AUTO_ID");
        entity.Property(e => e.ConnectionMainId)
            .HasMaxLength(50)
            .HasColumnName("CONNECTION_MAIN_ID");
        entity.Property(e => e.ConnectionName)
            .HasMaxLength(100)
            .HasColumnName("CONNECTION_NAME");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
