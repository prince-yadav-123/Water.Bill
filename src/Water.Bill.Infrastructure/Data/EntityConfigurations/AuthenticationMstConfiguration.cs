using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AuthenticationMstConfiguration : IEntityTypeConfiguration<AuthenticationMst>
{
    public void Configure(EntityTypeBuilder<AuthenticationMst> entity)
    {
        entity
            .HasNoKey()
            .ToTable("authentication_mst");
        
        entity.HasIndex(e => e.AutoId, "IX_AUTHENTICATION_MST_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.ExpiryDate)
            .HasColumnType("datetime")
            .HasColumnName("EXPIRY_DATE");
        entity.Property(e => e.MerchantId).HasColumnName("MERCHANT_ID");
        entity.Property(e => e.SecureToken)
            .HasMaxLength(100)
            .HasColumnName("SECURE_TOKEN");
        entity.Property(e => e.StartDate)
            .HasColumnType("datetime")
            .HasColumnName("START_DATE");
        entity.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY");
        entity.Property(e => e.UpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("UPDATED_ON");
        
    }
}
