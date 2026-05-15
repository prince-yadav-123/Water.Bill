using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentGatewayRegistrationConfiguration : IEntityTypeConfiguration<PaymentGatewayRegistration>
{
    public void Configure(EntityTypeBuilder<PaymentGatewayRegistration> entity)
    {
        entity
            .HasNoKey()
            .ToTable("payment_gateway_registration");
        
        entity.HasIndex(e => e.AutoId, "IX_PAYMENT_GATEWAY_REGISTRATION_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.Currency)
            .HasMaxLength(100)
            .HasColumnName("CURRENCY");
        entity.Property(e => e.IsLive).HasColumnName("IS_LIVE");
        entity.Property(e => e.KeyId)
            .HasColumnType("text")
            .HasColumnName("KEY_ID");
        entity.Property(e => e.KeySecret)
            .HasColumnType("text")
            .HasColumnName("KEY_SECRET");
        entity.Property(e => e.MerchantId).HasColumnName("MERCHANT_ID");
        entity.Property(e => e.SecureToken)
            .HasMaxLength(100)
            .HasColumnName("SECURE_TOKEN");
        entity.Property(e => e.TokenExpireOn)
            .HasColumnType("datetime")
            .HasColumnName("TOKEN_EXPIRE_ON");
        
    }
}
