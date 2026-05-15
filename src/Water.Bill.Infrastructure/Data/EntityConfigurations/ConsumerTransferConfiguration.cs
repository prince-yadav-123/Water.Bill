using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerTransferConfiguration : IEntityTypeConfiguration<ConsumerTransfer>
{
    public void Configure(EntityTypeBuilder<ConsumerTransfer> entity)
    {
        entity.HasKey(e => e.TransfId).HasName("PRIMARY");
        
        entity.ToTable("consumer_transfer");
        
        entity.HasIndex(e => e.ConsNo, "FK_CONSUMER_TRANSFER_CONSUMER_DETAILS_MASTER");
        
        entity.Property(e => e.TransfId).HasColumnName("TRANSF_ID");
        entity.Property(e => e.BankNm)
            .HasMaxLength(50)
            .HasColumnName("BANK_NM");
        entity.Property(e => e.ChallanDate)
            .HasColumnType("datetime")
            .HasColumnName("CHALLAN_DATE");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(50)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.ConsFnm)
            .HasMaxLength(100)
            .HasColumnName("CONS_FNM");
        entity.Property(e => e.ConsNm)
            .HasMaxLength(100)
            .HasColumnName("CONS_NM");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(10)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DeleteDate)
            .HasColumnType("datetime")
            .HasColumnName("DELETE_DATE");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.ModifyDate)
            .HasColumnType("datetime")
            .HasColumnName("MODIFY_DATE");
        entity.Property(e => e.Secu).HasColumnName("SECU");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.TransAmt).HasColumnName("TRANS_AMT");
        entity.Property(e => e.TransDate)
            .HasColumnType("datetime")
            .HasColumnName("TRANS_DATE");
        entity.Property(e => e.Userid)
            .HasMaxLength(10)
            .HasColumnName("USERID");
        
        entity.HasOne(d => d.ConsNoNavigation).WithMany(p => p.ConsumerTransfers)
            .HasForeignKey(d => d.ConsNo)
            .HasConstraintName("FK_CONSUMER_TRANSFER_CONSUMER_DETAILS_MASTER");
        
    }
}
