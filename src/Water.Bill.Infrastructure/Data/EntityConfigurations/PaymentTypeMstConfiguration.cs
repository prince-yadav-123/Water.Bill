using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentTypeMstConfiguration : IEntityTypeConfiguration<PaymentTypeMst>
{
    public void Configure(EntityTypeBuilder<PaymentTypeMst> entity)
    {
        entity
            .HasNoKey()
            .ToTable("payment_type_mst");
        
        entity.HasIndex(e => e.AutoId, "IX_PAYMENT_TYPE_MST_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
        entity.Property(e => e.LastUpdateOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATE_ON");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.PaymentTypeName)
            .HasMaxLength(50)
            .HasColumnName("PAYMENT_TYPE_NAME");
        
    }
}
