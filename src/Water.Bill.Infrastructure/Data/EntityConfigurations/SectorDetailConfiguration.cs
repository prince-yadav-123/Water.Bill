using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class SectorDetailConfiguration : IEntityTypeConfiguration<SectorDetail>
{
    public void Configure(EntityTypeBuilder<SectorDetail> entity)
    {
        entity.HasKey(e => e.SNo).HasName("PRIMARY");
        
        entity.ToTable("sector_detail");
        
        entity.HasIndex(e => e.SNo, "IX_Sector_Detail_S_no_AUTO");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.OrderBy).HasColumnName("ORDER_BY");
        entity.Property(e => e.SNo)
            .ValueGeneratedOnAdd()
            .HasColumnName("S_no");
        entity.Property(e => e.SectorId)
            .HasMaxLength(50)
            .HasColumnName("Sector_id");
        entity.Property(e => e.SectorNo)
            .HasMaxLength(10)
            .HasColumnName("sector_no");
        entity.Property(e => e.Status).HasColumnName("status");
        
    }
}
