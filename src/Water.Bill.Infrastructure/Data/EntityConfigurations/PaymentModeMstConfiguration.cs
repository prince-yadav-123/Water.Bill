using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentModeMstConfiguration : IEntityTypeConfiguration<PaymentModeMst>
{
    public void Configure(EntityTypeBuilder<PaymentModeMst> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");

        entity.ToTable("payment_mode_mst");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.IsActive)
            .HasMaxLength(1)
            .IsFixedLength()
            .HasColumnName("IS_ACTIVE");
        entity.Property(e => e.LastUpdateOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATE_ON");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.PaymentModeName)
            .HasMaxLength(100)
            .HasColumnName("PAYMENT_MODE_NAME");
        
    }
}
