using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterPhoneDetailConfiguration : IEntityTypeConfiguration<MasterPhoneDetail>
{
    public void Configure(EntityTypeBuilder<MasterPhoneDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_phone_details");
        
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EntryDate)
            .HasMaxLength(255)
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.TelId).HasColumnName("TEL_ID");
        entity.Property(e => e.TelNo)
            .HasMaxLength(255)
            .HasColumnName("TEL_NO");
        
    }
}
