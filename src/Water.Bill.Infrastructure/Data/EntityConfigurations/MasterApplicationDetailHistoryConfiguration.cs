using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterApplicationDetailHistoryConfiguration : IEntityTypeConfiguration<MasterApplicationDetailHistory>
{
    public void Configure(EntityTypeBuilder<MasterApplicationDetailHistory> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_application_detail_history");
        
        entity.Property(e => e.ApplicationId)
            .HasMaxLength(10)
            .HasColumnName("Application_id");
        entity.Property(e => e.CurentStatus)
            .HasMaxLength(1)
            .HasColumnName("curent_status");
        entity.Property(e => e.CurrentHoldingPer)
            .HasMaxLength(50)
            .HasColumnName("current_holding_per");
        entity.Property(e => e.Division)
            .HasMaxLength(50)
            .HasColumnName("division");
        entity.Property(e => e.Flag)
            .HasMaxLength(1)
            .HasColumnName("flag");
        entity.Property(e => e.ForwardDate).HasColumnName("forward_date");
        entity.Property(e => e.Remark)
            .HasMaxLength(200)
            .HasColumnName("remark");
        entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("status");
        
    }
}
