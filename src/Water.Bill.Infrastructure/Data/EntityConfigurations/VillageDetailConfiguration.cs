using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class VillageDetailConfiguration : IEntityTypeConfiguration<VillageDetail>
{
    public void Configure(EntityTypeBuilder<VillageDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("village_detail");
        
        entity.HasIndex(e => e.VillageNo, "IX_Village_Detail_Village_no_AUTO");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.VillageId).HasColumnName("Village_id");
        entity.Property(e => e.VillageName)
            .HasMaxLength(150)
            .HasColumnName("Village_Name");
        entity.Property(e => e.VillageNo)
            .ValueGeneratedOnAdd()
            .HasColumnName("Village_no");
        entity.Property(e => e.VillageStr)
            .HasMaxLength(3)
            .HasColumnName("Village_str");
        
    }
}
