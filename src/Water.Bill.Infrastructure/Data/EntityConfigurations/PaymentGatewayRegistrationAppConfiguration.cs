using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentGatewayRegistrationAppConfiguration : IEntityTypeConfiguration<PaymentGatewayRegistrationApp>
{
    public void Configure(EntityTypeBuilder<PaymentGatewayRegistrationApp> entity)
    {
        entity
            .HasNoKey()
            .ToTable("payment_gateway_registration_app");
        
        entity.HasIndex(e => e.AutoId, "IX_PAYMENT_GATEWAY_REGISTRATION_APP_AUTO_ID_AUTO");
        
        entity.Property(e => e.ApiUrl)
            .HasColumnType("text")
            .HasColumnName("API_URL");
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.Currency)
            .HasMaxLength(20)
            .HasColumnName("CURRENCY");
        entity.Property(e => e.IsLive).HasColumnName("IS_LIVE");
        entity.Property(e => e.KeyId)
            .HasMaxLength(100)
            .HasColumnName("KEY_ID");
        entity.Property(e => e.KeySecret)
            .HasMaxLength(200)
            .HasColumnName("KEY_SECRET");
        entity.Property(e => e.MerchantId)
            .HasMaxLength(50)
            .HasColumnName("MERCHANT_ID");
        entity.Property(e => e.SecureToken)
            .HasMaxLength(50)
            .HasColumnName("SECURE_TOKEN");
        entity.Property(e => e.TokenExpireOn)
            .HasColumnType("datetime")
            .HasColumnName("TOKEN_EXPIRE_ON");
        entity.Property(e => e.WebUrl)
            .HasMaxLength(200)
            .HasColumnName("WEB_URL");
        
    }
}
