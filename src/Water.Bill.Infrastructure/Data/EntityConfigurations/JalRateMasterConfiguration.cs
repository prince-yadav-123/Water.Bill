using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalRateMasterConfiguration : IEntityTypeConfiguration<JalRateMaster>
{
    public void Configure(EntityTypeBuilder<JalRateMaster> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("jal_rate_master");
        
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.IdT)
            .HasMaxLength(50)
            .HasColumnName("ID_T");
        entity.Property(e => e.PropertyType)
            .HasMaxLength(50)
            .HasColumnName("PROPERTY_TYPE");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
