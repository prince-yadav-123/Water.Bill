using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class TbLoginInfoConfiguration : IEntityTypeConfiguration<TbLoginInfo>
{
    public void Configure(EntityTypeBuilder<TbLoginInfo> entity)
    {
        entity
            .HasNoKey()
            .ToTable("tb_login_info");
        
        entity.HasIndex(e => e.LoginId, "IX_TB_LOGIN_INFO_LOGIN_ID_AUTO");
        
        entity.Property(e => e.Division)
            .HasMaxLength(10)
            .HasColumnName("DIVISION");
        entity.Property(e => e.EntryDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.IfFor)
            .HasMaxLength(10)
            .HasColumnName("IF_FOR");
        entity.Property(e => e.IsAdmin).HasColumnName("IS_ADMIN");
        entity.Property(e => e.Level).HasColumnName("LEVEL");
        entity.Property(e => e.LoginId)
            .ValueGeneratedOnAdd()
            .HasColumnName("LOGIN_ID");
        entity.Property(e => e.Password)
            .HasMaxLength(15)
            .HasColumnName("PASSWORD");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        entity.Property(e => e.UserId)
            .HasMaxLength(50)
            .HasColumnName("USER_ID");
        entity.Property(e => e.UserRole)
            .HasMaxLength(1)
            .HasColumnName("USER_ROLE");
        
    }
}
