using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class UserMasterConfiguration : IEntityTypeConfiguration<UserMaster>
{
    public void Configure(EntityTypeBuilder<UserMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("user_master");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.SlNo).HasColumnName("SL_NO");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.UserDepart)
            .HasMaxLength(50)
            .HasColumnName("USER_depart");
        entity.Property(e => e.UserEmail)
            .HasMaxLength(30)
            .HasColumnName("USER_EMAIL");
        entity.Property(e => e.UserFname)
            .HasMaxLength(30)
            .HasColumnName("USER_FNAME");
        entity.Property(e => e.UserMob)
            .HasMaxLength(12)
            .HasColumnName("USER_MOB");
        entity.Property(e => e.UserPass)
            .HasMaxLength(15)
            .HasColumnName("USER_PASS");
        entity.Property(e => e.UserRole)
            .HasMaxLength(4)
            .HasColumnName("USER_ROLE");
        entity.Property(e => e.Userid)
            .HasMaxLength(15)
            .HasColumnName("USERID");
        
    }
}
