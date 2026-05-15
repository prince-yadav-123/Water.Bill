using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class DemoIndustrycsvConfiguration : IEntityTypeConfiguration<DemoIndustrycsv>
{
    public void Configure(EntityTypeBuilder<DemoIndustrycsv> entity)
    {
        entity
            .HasNoKey()
            .ToTable("demo_industrycsv");
        
        entity.Property(e => e.AlloteeName)
            .HasColumnType("text")
            .HasColumnName("Allotee_Name");
        entity.Property(e => e.BlockName)
            .HasMaxLength(50)
            .HasColumnName("blockName");
        entity.Property(e => e.MobileNumber).HasMaxLength(50);
        entity.Property(e => e.PropertyNo)
            .HasMaxLength(50)
            .HasColumnName("propertyNo");
        entity.Property(e => e.Rid).HasColumnName("RID");
        entity.Property(e => e.SectorName)
            .HasMaxLength(50)
            .HasColumnName("sectorName");
        
    }
}
