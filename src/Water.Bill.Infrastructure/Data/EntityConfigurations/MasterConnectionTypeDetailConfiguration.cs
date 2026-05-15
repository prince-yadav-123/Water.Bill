using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterConnectionTypeDetailConfiguration : IEntityTypeConfiguration<MasterConnectionTypeDetail>
{
    public void Configure(EntityTypeBuilder<MasterConnectionTypeDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_connection_type_details");
        
        entity.Property(e => e.ConId)
            .HasMaxLength(2)
            .HasColumnName("CON_ID");
        entity.Property(e => e.ConMainId)
            .HasMaxLength(4)
            .HasColumnName("CON_MAIN_ID");
        entity.Property(e => e.ConName)
            .HasMaxLength(50)
            .HasColumnName("CON_NAME");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
