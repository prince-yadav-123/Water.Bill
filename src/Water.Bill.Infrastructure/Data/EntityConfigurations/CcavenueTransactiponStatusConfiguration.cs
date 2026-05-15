using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CcavenueTransactiponStatusConfiguration : IEntityTypeConfiguration<CcavenueTransactiponStatus>
{
    public void Configure(EntityTypeBuilder<CcavenueTransactiponStatus> entity)
    {
        entity
            .HasNoKey()
            .ToTable("ccavenue_transactipon_status");
        
        entity.HasIndex(e => e.AutoId, "IX_CCAVENUE_TRANSACTIPON_STATUS_AUTO_ID_AUTO");
        
        entity.Property(e => e.Amount)
            .HasMaxLength(100)
            .HasColumnName("AMOUNT");
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.BankRefNo)
            .HasMaxLength(100)
            .HasColumnName("BANK_REF_NO");
        entity.Property(e => e.BillingAddress)
            .HasMaxLength(100)
            .HasColumnName("BILLING_ADDRESS");
        entity.Property(e => e.BillingCity)
            .HasMaxLength(100)
            .HasColumnName("BILLING_CITY");
        entity.Property(e => e.BillingCountry)
            .HasMaxLength(100)
            .HasColumnName("BILLING_COUNTRY");
        entity.Property(e => e.BillingName)
            .HasMaxLength(100)
            .HasColumnName("BILLING_NAME");
        entity.Property(e => e.BillingNotes)
            .HasMaxLength(100)
            .HasColumnName("BILLING_NOTES");
        entity.Property(e => e.BillingState)
            .HasMaxLength(100)
            .HasColumnName("BILLING_STATE");
        entity.Property(e => e.BillingTel)
            .HasMaxLength(100)
            .HasColumnName("BILLING_TEL");
        entity.Property(e => e.BillingZip)
            .HasMaxLength(100)
            .HasColumnName("BILLING_ZIP");
        entity.Property(e => e.BinCountry)
            .HasMaxLength(100)
            .HasColumnName("BIN_COUNTRY");
        entity.Property(e => e.CardName)
            .HasMaxLength(100)
            .HasColumnName("CARD_NAME");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.Currency)
            .HasMaxLength(100)
            .HasColumnName("CURRENCY");
        entity.Property(e => e.DeliveryAddress)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_ADDRESS");
        entity.Property(e => e.DeliveryCity)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_CITY");
        entity.Property(e => e.DeliveryCountry)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_COUNTRY");
        entity.Property(e => e.DeliveryName)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_NAME");
        entity.Property(e => e.DeliveryState)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_STATE");
        entity.Property(e => e.DeliveryTel)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_TEL");
        entity.Property(e => e.DeliveryZip)
            .HasMaxLength(100)
            .HasColumnName("DELIVERY_ZIP");
        entity.Property(e => e.DiscountValue)
            .HasMaxLength(100)
            .HasColumnName("DISCOUNT_VALUE");
        entity.Property(e => e.EciValue)
            .HasMaxLength(100)
            .HasColumnName("ECI_VALUE");
        entity.Property(e => e.EncResp).HasColumnName("ENC_RESP");
        entity.Property(e => e.ErrorMessage).HasColumnName("ERROR_MESSAGE");
        entity.Property(e => e.FailureMessage)
            .HasMaxLength(100)
            .HasColumnName("FAILURE_MESSAGE");
        entity.Property(e => e.MerAmount)
            .HasMaxLength(100)
            .HasColumnName("MER_AMOUNT");
        entity.Property(e => e.MerchantParam1)
            .HasMaxLength(100)
            .HasColumnName("MERCHANT_PARAM1");
        entity.Property(e => e.MerchantParam2)
            .HasMaxLength(100)
            .HasColumnName("MERCHANT_PARAM2");
        entity.Property(e => e.MerchantParam3)
            .HasMaxLength(100)
            .HasColumnName("MERCHANT_PARAM3");
        entity.Property(e => e.MerchantParam4)
            .HasMaxLength(100)
            .HasColumnName("MERCHANT_PARAM4");
        entity.Property(e => e.MerchantParam5)
            .HasMaxLength(100)
            .HasColumnName("MERCHANT_PARAM5");
        entity.Property(e => e.OfferCode)
            .HasMaxLength(100)
            .HasColumnName("OFFER_CODE");
        entity.Property(e => e.OfferType)
            .HasMaxLength(100)
            .HasColumnName("OFFER_TYPE");
        entity.Property(e => e.OrderId)
            .HasMaxLength(100)
            .HasColumnName("ORDER_ID");
        entity.Property(e => e.OrderStatus)
            .HasMaxLength(100)
            .HasColumnName("ORDER_STATUS");
        entity.Property(e => e.PaymentMode)
            .HasMaxLength(100)
            .HasColumnName("PAYMENT_MODE");
        entity.Property(e => e.PaymentStatus)
            .HasMaxLength(50)
            .HasColumnName("PAYMENT_STATUS");
        entity.Property(e => e.ResponseCode)
            .HasMaxLength(100)
            .HasColumnName("RESPONSE_CODE");
        entity.Property(e => e.Retry)
            .HasMaxLength(100)
            .HasColumnName("RETRY");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.StatusCode)
            .HasMaxLength(100)
            .HasColumnName("STATUS_CODE");
        entity.Property(e => e.StatusMessage)
            .HasColumnType("text")
            .HasColumnName("STATUS_MESSAGE");
        entity.Property(e => e.TrackingId)
            .HasMaxLength(100)
            .HasColumnName("TRACKING_ID");
        entity.Property(e => e.TransDate)
            .HasColumnType("datetime")
            .HasColumnName("TRANS_DATE");
        entity.Property(e => e.Vault)
            .HasMaxLength(100)
            .HasColumnName("VAULT");
        
    }
}
