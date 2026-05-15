using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerDetailsTranConfiguration : IEntityTypeConfiguration<ConsumerDetailsTran>
{
    public void Configure(EntityTypeBuilder<ConsumerDetailsTran> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("consumer_details_trans");
        
        entity.HasIndex(e => e.ConsNo, "FK_CONSUMER_DETAILS_TRANS_CONSUMER_DETAILS_MASTER");
        
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.AffidavitYn)
            .HasMaxLength(2)
            .HasColumnName("AFFIDAVIT_YN");
        entity.Property(e => e.AllotDate)
            .HasMaxLength(12)
            .HasColumnName("ALLOT_DATE");
        entity.Property(e => e.CalDate)
            .HasMaxLength(12)
            .HasColumnName("CAL_DATE");
        entity.Property(e => e.CompDate)
            .HasMaxLength(12)
            .HasColumnName("COMP_DATE");
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
        entity.Property(e => e.PosDate)
            .HasMaxLength(12)
            .HasColumnName("POS_DATE");
        entity.Property(e => e.SsiDate)
            .HasMaxLength(12)
            .HasColumnName("SSI_DATE");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.Userid)
            .HasMaxLength(10)
            .HasColumnName("USERID");
        
        entity.HasOne(d => d.ConsNoNavigation).WithMany(p => p.ConsumerDetailsTrans)
            .HasForeignKey(d => d.ConsNo)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CONSUMER_DETAILS_TRANS_CONSUMER_DETAILS_MASTER");
        
    }
}
