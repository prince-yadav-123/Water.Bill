using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerRidMobEmailInfoConfiguration : IEntityTypeConfiguration<ConsumerRidMobEmailInfo>
{
    public void Configure(EntityTypeBuilder<ConsumerRidMobEmailInfo> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_rid_mob_email_info");
        
        entity.Property(e => e.BlkNo).HasColumnName("BLK_NO");
        entity.Property(e => e.ConsNm1).HasColumnName("CONS_NM1");
        entity.Property(e => e.ConsNo).HasColumnName("CONS_NO");
        entity.Property(e => e.EmailId).HasColumnName("EMAIL_ID");
        entity.Property(e => e.FlatNo).HasColumnName("FLAT_NO");
        entity.Property(e => e.MobNo).HasColumnName("MOB_NO");
        entity.Property(e => e.Rid).HasColumnName("RID");
        entity.Property(e => e.Sector).HasColumnName("SECTOR");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
