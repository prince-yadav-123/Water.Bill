using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MerchantMstConfiguration : IEntityTypeConfiguration<MerchantMst>
{
    public void Configure(EntityTypeBuilder<MerchantMst> entity)
    {
        entity
            .HasNoKey()
            .ToTable("merchant_mst");
        
        entity.HasIndex(e => e.MerchantId, "IX_MERCHANT_MST_MERCHANT_ID_AUTO");
        
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.EndDate)
            .HasColumnType("datetime")
            .HasColumnName("END_DATE");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.MerchantId)
            .ValueGeneratedOnAdd()
            .HasColumnName("MERCHANT_ID");
        entity.Property(e => e.MerchantKey)
            .HasMaxLength(20)
            .HasColumnName("MERCHANT_KEY");
        entity.Property(e => e.MerchantSecreateKey)
            .HasMaxLength(50)
            .HasColumnName("MERCHANT_SECREATE_KEY");
        entity.Property(e => e.SecureToken)
            .HasMaxLength(100)
            .HasColumnName("SECURE_TOKEN");
        entity.Property(e => e.StartDate)
            .HasColumnType("datetime")
            .HasColumnName("START_DATE");
        
    }
}
