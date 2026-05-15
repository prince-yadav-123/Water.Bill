using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class BlockDetailConfiguration : IEntityTypeConfiguration<BlockDetail>
{
    public void Configure(EntityTypeBuilder<BlockDetail> entity)
    {
        entity.HasKey(e => new { e.SectorId, e.Block })
            .HasName("PRIMARY")
            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
        
        entity.ToTable("block_detail");
        
        entity.Property(e => e.SectorId)
            .HasMaxLength(50)
            .HasColumnName("sector_id");
        entity.Property(e => e.Block)
            .HasMaxLength(50)
            .HasColumnName("block");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status).HasColumnName("status");
        
    }
}
