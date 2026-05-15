using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsCessUpdationConfiguration : IEntityTypeConfiguration<ConsCessUpdation>
{
    public void Configure(EntityTypeBuilder<ConsCessUpdation> entity)
    {
        entity
            .HasNoKey()
            .ToTable("cons_cess_updation");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(255)
            .HasColumnName("cons_no");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        
    }
}
