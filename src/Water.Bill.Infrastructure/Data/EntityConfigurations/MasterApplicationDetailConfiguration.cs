using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterApplicationDetailConfiguration : IEntityTypeConfiguration<MasterApplicationDetail>
{
    public void Configure(EntityTypeBuilder<MasterApplicationDetail> entity)
    {
        entity.HasKey(e => e.ApplicationId).HasName("PRIMARY");
        
        entity.ToTable("master_application_detail");
        
        entity.Property(e => e.ApplicationId)
            .HasMaxLength(10)
            .HasColumnName("Application_id");
        entity.Property(e => e.AppType)
            .HasMaxLength(3)
            .HasColumnName("app_type");
        entity.Property(e => e.ApplcationStatusDetail)
            .HasColumnType("text")
            .HasColumnName("applcation_status_detail");
        entity.Property(e => e.ApplicationStatus)
            .HasMaxLength(10)
            .HasColumnName("application_status");
        entity.Property(e => e.Block).HasMaxLength(10);
        entity.Property(e => e.ConAddress)
            .HasMaxLength(150)
            .HasColumnName("con_Address");
        entity.Property(e => e.ConName)
            .HasMaxLength(50)
            .HasColumnName("con_Name");
        entity.Property(e => e.ConPhoneMobile)
            .HasMaxLength(12)
            .HasColumnName("con_Phone_mobile");
        entity.Property(e => e.ConnType)
            .HasMaxLength(50)
            .HasColumnName("conn_type");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(15)
            .HasColumnName("cons_no");
        entity.Property(e => e.CurrentHoldingPer)
            .HasMaxLength(50)
            .HasColumnName("current_holding_per");
        entity.Property(e => e.DivName)
            .HasMaxLength(10)
            .HasColumnName("div_name");
        entity.Property(e => e.EnterDate).HasColumnName("enter_date");
        entity.Property(e => e.PhotoId).HasColumnName("photo_id");
        entity.Property(e => e.PipeSize)
            .HasMaxLength(50)
            .HasColumnName("Pipe_size");
        entity.Property(e => e.PlotArea)
            .HasMaxLength(50)
            .HasColumnName("plot_Area");
        entity.Property(e => e.PlotNo)
            .HasMaxLength(50)
            .HasColumnName("Plot_no");
        entity.Property(e => e.PrevConDetail)
            .HasMaxLength(100)
            .HasColumnName("prev_con_detail");
        entity.Property(e => e.PrevConYesNo)
            .HasMaxLength(1)
            .HasColumnName("prev_con_yes_no");
        entity.Property(e => e.PropertyType)
            .HasMaxLength(20)
            .HasColumnName("property_type");
        entity.Property(e => e.Reg).HasColumnName("reg");
        entity.Property(e => e.SectorVill)
            .HasMaxLength(10)
            .HasColumnName("Sector_vill");
        entity.Property(e => e.SeewarPurpose)
            .HasMaxLength(50)
            .HasColumnName("Seewar_purpose");
        entity.Property(e => e.SingId).HasColumnName("sing_id");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.StatusDate).HasColumnName("status_date");
        
    }
}
