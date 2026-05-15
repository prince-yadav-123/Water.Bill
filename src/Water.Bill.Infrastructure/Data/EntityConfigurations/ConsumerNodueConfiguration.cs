using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerNodueConfiguration : IEntityTypeConfiguration<ConsumerNodue>
{
    public void Configure(EntityTypeBuilder<ConsumerNodue> entity)
    {
        entity.HasKey(e => e.NodueId).HasName("PRIMARY");
        
        entity.ToTable("consumer_nodue");
        
        entity.HasIndex(e => e.ConsNo, "FK_CONSUMER_NODUE_CONSUMER_DETAILS_MASTER");
        
        entity.Property(e => e.NodueId).HasColumnName("NODUE_ID");
        entity.Property(e => e.ChallanBank)
            .HasMaxLength(150)
            .HasColumnName("CHALLAN_BANK");
        entity.Property(e => e.ChallanDt)
            .HasColumnType("datetime")
            .HasColumnName("CHALLAN_DT");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(50)
            .HasColumnName("CHALLAN_NO");
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
        entity.Property(e => e.NdcUpto)
            .HasColumnType("datetime")
            .HasColumnName("NDC_upto");
        entity.Property(e => e.NodueAmt).HasColumnName("NODUE_AMT");
        entity.Property(e => e.NodueDt)
            .HasColumnType("datetime")
            .HasColumnName("NODUE_DT");
        entity.Property(e => e.Secu).HasColumnName("SECU");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.Userid)
            .HasMaxLength(10)
            .HasColumnName("USERID");
        
        entity.HasOne(d => d.ConsNoNavigation).WithMany(p => p.ConsumerNodues)
            .HasForeignKey(d => d.ConsNo)
            .HasConstraintName("FK_CONSUMER_NODUE_CONSUMER_DETAILS_MASTER");
        
    }
}
