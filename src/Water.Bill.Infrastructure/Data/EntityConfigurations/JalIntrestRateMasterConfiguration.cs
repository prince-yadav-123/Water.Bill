using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalIntrestRateMasterConfiguration : IEntityTypeConfiguration<JalIntrestRateMaster>
{
    public void Configure(EntityTypeBuilder<JalIntrestRateMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_intrest_rate_master");
        
        entity.HasIndex(e => e.IntId, "IX_JAL_INTREST_RATE_MASTER_INT_ID_AUTO");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.IntDateFrom)
            .HasColumnType("datetime")
            .HasColumnName("INT_DATE_FROM");
        entity.Property(e => e.IntDateTo)
            .HasColumnType("datetime")
            .HasColumnName("INT_DATE_TO");
        entity.Property(e => e.IntId)
            .ValueGeneratedOnAdd()
            .HasColumnName("INT_ID");
        entity.Property(e => e.Intrest).HasColumnName("INTREST");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
