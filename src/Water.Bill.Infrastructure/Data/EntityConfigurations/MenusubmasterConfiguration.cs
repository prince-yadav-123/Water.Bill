using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MenusubmasterConfiguration : IEntityTypeConfiguration<Menusubmaster>
{
    public void Configure(EntityTypeBuilder<Menusubmaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("menusubmaster");
        
        entity.Property(e => e.Entrydate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRYDATE");
        entity.Property(e => e.Haschild)
            .HasMaxLength(1)
            .HasColumnName("HASCHILD");
        entity.Property(e => e.Linkurl)
            .HasMaxLength(100)
            .HasColumnName("LINKURL");
        entity.Property(e => e.Menuid).HasColumnName("MENUID");
        entity.Property(e => e.Menuname)
            .HasMaxLength(35)
            .HasColumnName("MENUNAME");
        entity.Property(e => e.Role)
            .HasMaxLength(50)
            .HasColumnName("ROLE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.Submenuid).HasColumnName("SUBMENUID");
        entity.Property(e => e.Userid)
            .HasMaxLength(20)
            .HasColumnName("USERID");
        
    }
}
