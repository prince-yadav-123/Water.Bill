using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalnoidaBankpayTranConfiguration : IEntityTypeConfiguration<JalnoidaBankpayTran>
{
    public void Configure(EntityTypeBuilder<JalnoidaBankpayTran> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jalnoida_bankpay_tran");
        
        entity.Property(e => e.AdditionalInfo1).HasMaxLength(200);
        entity.Property(e => e.AdditionalInfo2).HasMaxLength(200);
        entity.Property(e => e.AdditionalInfo3).HasMaxLength(100);
        entity.Property(e => e.AdditionalInfo4).HasMaxLength(100);
        entity.Property(e => e.AdditionalInfo5).HasMaxLength(100);
        entity.Property(e => e.AdditionalInfo6).HasMaxLength(100);
        entity.Property(e => e.AdditionalInfo7).HasMaxLength(100);
        entity.Property(e => e.AuthStatus).HasMaxLength(20);
        entity.Property(e => e.BankId)
            .HasMaxLength(20)
            .HasColumnName("BankID");
        entity.Property(e => e.BankMerchantId)
            .HasMaxLength(20)
            .HasColumnName("BankMerchantID");
        entity.Property(e => e.BankReferenceNo).HasMaxLength(20);
        entity.Property(e => e.CheckSum1).HasMaxLength(50);
        entity.Property(e => e.CurrencyName).HasMaxLength(20);
        entity.Property(e => e.EntryDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.ErrorDescription).HasMaxLength(200);
        entity.Property(e => e.ErrorStatus).HasMaxLength(100);
        entity.Property(e => e.ItemCode).HasMaxLength(20);
        entity.Property(e => e.Jalrefid)
            .HasMaxLength(12)
            .HasColumnName("JALREFID");
        entity.Property(e => e.MerchantId)
            .HasMaxLength(50)
            .HasColumnName("MerchantID");
        entity.Property(e => e.SecurityId)
            .HasMaxLength(20)
            .HasColumnName("SecurityID");
        entity.Property(e => e.SecurityPassword).HasMaxLength(20);
        entity.Property(e => e.SecurityType).HasMaxLength(20);
        entity.Property(e => e.SettlementType).HasMaxLength(20);
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        entity.Property(e => e.Tcntype)
            .HasMaxLength(100)
            .HasColumnName("TCNType");
        entity.Property(e => e.TrxReferenceNo).HasMaxLength(100);
        entity.Property(e => e.TxnAmount).HasMaxLength(20);
        entity.Property(e => e.TxnDate).HasMaxLength(20);
        
    }
}
