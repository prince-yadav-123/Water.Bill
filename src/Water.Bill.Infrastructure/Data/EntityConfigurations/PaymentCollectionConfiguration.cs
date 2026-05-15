using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class PaymentCollectionConfiguration : IEntityTypeConfiguration<PaymentCollection>
{
    public void Configure(EntityTypeBuilder<PaymentCollection> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");
        
        entity.ToTable("payment_collection");
        
        entity.HasIndex(e => e.ChallanNo, "INX_PAYMENT_COLLECTION_CHALLAN_NO");
        
        entity.HasIndex(e => e.ConsNo, "INX_PAYMENT_COLLECTION_CONS_NO");
        
        entity.HasIndex(e => e.PaymentDate, "INX_PAYMENT_COLLECTION_PAYMENT_DATE");
        
        entity.HasIndex(e => e.TransactionNo, "INX_PAYMENT_COLLECTION_TRANSACTION_NO");
        
        entity.Property(e => e.AutoId).HasColumnName("AUTO_ID");
        entity.Property(e => e.AppFlag)
            .HasMaxLength(20)
            .HasColumnName("APP_FLAG");
        entity.Property(e => e.Att1)
            .HasMaxLength(100)
            .HasColumnName("ATT1");
        entity.Property(e => e.AttemptCount).HasColumnName("ATTEMPT_COUNT");
        entity.Property(e => e.BillDate).HasColumnName("BILL_DATE");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(50)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(20)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.ErpRequestTime)
            .HasColumnType("datetime")
            .HasColumnName("ERP_REQUEST_TIME");
        entity.Property(e => e.ErpResponseMassege)
            .HasColumnType("text")
            .HasColumnName("ERP_RESPONSE_MASSEGE");
        entity.Property(e => e.ErpResponseTime)
            .HasColumnType("datetime")
            .HasColumnName("ERP_RESPONSE_TIME");
        entity.Property(e => e.HashCode).HasColumnName("HASH_CODE");
        entity.Property(e => e.IsErpSend).HasColumnName("IS_ERP_SEND");
        entity.Property(e => e.IsRetry).HasColumnName("IS_RETRY");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.Message)
            .HasColumnType("text")
            .HasColumnName("MESSAGE");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(15)
            .HasColumnName("MOBILE_NO");
        entity.Property(e => e.OrderId)
            .HasMaxLength(200)
            .HasColumnName("ORDER_ID");
        entity.Property(e => e.PaymentDate)
            .HasColumnType("datetime")
            .HasColumnName("PAYMENT_DATE");
        entity.Property(e => e.PaymentId)
            .HasColumnType("text")
            .HasColumnName("PAYMENT_ID");
        entity.Property(e => e.PaymentType)
            .HasMaxLength(20)
            .HasColumnName("PAYMENT_TYPE");
        entity.Property(e => e.Rid)
            .HasMaxLength(20)
            .HasColumnName("RID");
        entity.Property(e => e.Securetoken).HasColumnName("SECURETOKEN");
        entity.Property(e => e.Signature)
            .HasColumnType("text")
            .HasColumnName("SIGNATURE");
        entity.Property(e => e.SuccessStatus)
            .HasMaxLength(1)
            .IsFixedLength()
            .HasColumnName("SUCCESS_STATUS");
        entity.Property(e => e.TotalBillAmt)
            .HasPrecision(18, 2)
            .HasColumnName("TOTAL_BILL_AMT");
        entity.Property(e => e.TrackingId)
            .HasMaxLength(100)
            .HasColumnName("TRACKING_ID");
        entity.Property(e => e.TransDate)
            .HasColumnType("datetime")
            .HasColumnName("TRANS_DATE");
        entity.Property(e => e.TransactionNo)
            .HasMaxLength(100)
            .HasColumnName("TRANSACTION_NO");
        
    }
}
