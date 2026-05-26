using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterConnectionTypeDetailsTranConfiguration : IEntityTypeConfiguration<MasterConnectionTypeDetailsTran>
{
    public void Configure(EntityTypeBuilder<MasterConnectionTypeDetailsTran> entity)
    {
        entity.HasKey(e => e.SubConId).HasName("PRIMARY");
        
        entity.ToTable("master_connection_type_details_trans");
        
        entity.Property(e => e.ConId)
            .HasMaxLength(4)
            .HasColumnName("CON_ID");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        entity.Property(e => e.SubConId)
            .ValueGeneratedOnAdd()
            .HasColumnName("SUB_CON_ID");
        entity.Property(e => e.SubConName)
            .HasMaxLength(50)
            .HasColumnName("SUB_CON_NAME");
        
    }
}
