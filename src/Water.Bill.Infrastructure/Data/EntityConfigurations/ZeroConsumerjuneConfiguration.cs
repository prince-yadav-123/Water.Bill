using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ZeroConsumerjuneConfiguration : IEntityTypeConfiguration<ZeroConsumerjune>
{
    public void Configure(EntityTypeBuilder<ZeroConsumerjune> entity)
    {
        entity
            .HasNoKey()
            .ToTable("zero_consumerjune");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(255)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Id).HasColumnName("ID");
        
    }
}
