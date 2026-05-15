using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterUserRoleConfiguration : IEntityTypeConfiguration<MasterUserRole>
{
    public void Configure(EntityTypeBuilder<MasterUserRole> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_user_role");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Entrydate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRYDATE");
        entity.Property(e => e.RoleDis)
            .HasMaxLength(100)
            .HasColumnName("ROLE_DIS");
        entity.Property(e => e.RoleId).HasColumnName("ROLE_ID");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.UserRole)
            .HasMaxLength(5)
            .HasColumnName("USER_ROLE");
        entity.Property(e => e.Userid)
            .HasMaxLength(20)
            .HasColumnName("USERID");
        
    }
}
