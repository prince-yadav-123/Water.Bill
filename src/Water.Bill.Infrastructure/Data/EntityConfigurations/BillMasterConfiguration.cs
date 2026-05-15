using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class BillMasterConfiguration : IEntityTypeConfiguration<BillMaster>
{
    public void Configure(EntityTypeBuilder<BillMaster> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("bill_master");
        
        entity.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("ID");
        entity.Property(e => e.ArrearInt).HasColumnName("ARREAR_INT");
        entity.Property(e => e.BalAmt).HasColumnName("BAL_AMT");
        entity.Property(e => e.CessAmt).HasColumnName("CESS_AMT");
        entity.Property(e => e.CessInt).HasColumnName("CESS_INT");
        entity.Property(e => e.ConCtg)
            .HasMaxLength(10)
            .HasColumnName("CON_CTG");
        entity.Property(e => e.ConType)
            .HasMaxLength(10)
            .HasColumnName("CON_TYPE");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DeleteDate)
            .HasColumnType("datetime")
            .HasColumnName("DELETE_DATE");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.DiffAmt).HasColumnName("DIFF_AMT");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.FYear)
            .HasMaxLength(50)
            .HasColumnName("F_YEAR");
        entity.Property(e => e.MinRate).HasColumnName("MIN_RATE");
        entity.Property(e => e.ModifyDate)
            .HasColumnType("datetime")
            .HasColumnName("MODIFY_DATE");
        entity.Property(e => e.OldRate).HasColumnName("OLD_RATE");
        entity.Property(e => e.PaidAmt).HasColumnName("PAID_AMT");
        entity.Property(e => e.PaidDate)
            .HasColumnType("datetime")
            .HasColumnName("PAID_DATE");
        entity.Property(e => e.PaidMonth).HasColumnName("PAID_MONTH");
        entity.Property(e => e.PipeSize).HasColumnName("PIPE_SIZE");
        entity.Property(e => e.PlotSize).HasColumnName("PLOT_SIZE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.TotalRate).HasColumnName("TOTAL_RATE");
        
    }
}
