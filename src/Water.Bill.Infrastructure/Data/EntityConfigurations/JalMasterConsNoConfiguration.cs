using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalMasterConsNoConfiguration : IEntityTypeConfiguration<JalMasterConsNo>
{
    public void Configure(EntityTypeBuilder<JalMasterConsNo> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_master_cons_no");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(8)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        
    }
}
