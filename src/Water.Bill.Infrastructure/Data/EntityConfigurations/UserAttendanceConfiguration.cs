using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class UserAttendanceConfiguration : IEntityTypeConfiguration<UserAttendance>
{
    public void Configure(EntityTypeBuilder<UserAttendance> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("user_attendance");
        
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.AType)
            .HasMaxLength(2)
            .IsFixedLength()
            .HasColumnName("A_type");
        entity.Property(e => e.Day)
            .HasMaxLength(10)
            .IsFixedLength()
            .HasColumnName("day");
        entity.Property(e => e.Ddate).HasColumnName("ddate");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("entry_date");
        entity.Property(e => e.Hour)
            .HasMaxLength(10)
            .HasColumnName("hour");
        entity.Property(e => e.InTime)
            .HasMaxLength(10)
            .HasColumnName("in_time");
        entity.Property(e => e.OutTime)
            .HasMaxLength(10)
            .HasColumnName("out_time");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.UpdateDate)
            .HasColumnType("datetime")
            .HasColumnName("update_date");
        entity.Property(e => e.UserId)
            .HasMaxLength(10)
            .IsFixedLength()
            .HasColumnName("user_id");
        entity.Property(e => e.UserName)
            .HasMaxLength(50)
            .HasColumnName("user_name");
        
    }
}
