using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalRateTranConfiguration : IEntityTypeConfiguration<JalRateTran>
{
    public void Configure(EntityTypeBuilder<JalRateTran> entity)
    {
        entity.HasKey(e => e.Sid).HasName("PRIMARY");
        
        entity.ToTable("jal_rate_trans");
        
        entity.HasIndex(e => e.Id, "FK_JAL_RATE_TRANS_JAL_RATE_MASTER");
        
        entity.Property(e => e.Sid).HasColumnName("SID");
        entity.Property(e => e.AreaEnd).HasColumnName("AREA_END");
        entity.Property(e => e.AreaStart).HasColumnName("AREA_START");
        entity.Property(e => e.CessRate).HasColumnName("CESS_RATE");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EffFrom)
            .HasColumnType("datetime")
            .HasColumnName("EFF_FROM");
        entity.Property(e => e.EffTo)
            .HasColumnType("datetime")
            .HasColumnName("EFF_TO");
        entity.Property(e => e.EstRateReg).HasColumnName("EST_RATE_REG");
        entity.Property(e => e.EstRateTemp).HasColumnName("EST_RATE_TEMP");
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.MainRate).HasColumnName("MAIN_RATE");
        entity.Property(e => e.PipeSize).HasColumnName("PIPE_SIZE");
        entity.Property(e => e.Regular).HasColumnName("REGULAR");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        entity.Property(e => e.Temporary).HasColumnName("TEMPORARY");
        
        entity.HasOne(d => d.IdNavigation).WithMany(p => p.JalRateTrans)
            .HasForeignKey(d => d.Id)
            .HasConstraintName("FK_JAL_RATE_TRANS_JAL_RATE_MASTER");
        
    }
}
