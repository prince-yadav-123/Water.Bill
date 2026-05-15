using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PimsAllotmentInfo5Configuration : IEntityTypeConfiguration<PimsAllotmentInfo5>
{
    public void Configure(EntityTypeBuilder<PimsAllotmentInfo5> entity)
    {
        entity
            .HasNoKey()
            .ToTable("pims_allotment_info5");
        
        entity.Property(e => e.Allotmentdate)
            .HasColumnType("datetime")
            .HasColumnName("allotmentdate");
        entity.Property(e => e.BlockName)
            .HasMaxLength(255)
            .HasColumnName("blockName");
        entity.Property(e => e.CurrentAlloteeName)
            .HasMaxLength(255)
            .HasColumnName("Current Allotee Name");
        entity.Property(e => e.Email).HasMaxLength(255);
        entity.Property(e => e.PropertyName)
            .HasMaxLength(255)
            .HasColumnName("Property Name");
        entity.Property(e => e.PropertyNo).HasColumnName("propertyNo");
        entity.Property(e => e.PropertyTypeId).HasColumnName("PropertyTypeID");
        entity.Property(e => e.PropertyTypeName).HasMaxLength(255);
        entity.Property(e => e.Rid).HasColumnName("RID");
        entity.Property(e => e.SectorName).HasColumnName("sectorName");
        entity.Property(e => e.TotalArea).HasColumnName("totalArea");
        
    }
}
