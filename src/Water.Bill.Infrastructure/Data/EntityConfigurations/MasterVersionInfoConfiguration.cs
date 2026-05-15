using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterVersionInfoConfiguration : IEntityTypeConfiguration<MasterVersionInfo>
{
    public void Configure(EntityTypeBuilder<MasterVersionInfo> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_version_info");
        
        entity.HasIndex(e => e.Id, "IX_MASTER_VERSION_INFO_ID_AUTO");
        
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("ID");
        entity.Property(e => e.Link)
            .HasMaxLength(200)
            .HasColumnName("LINK");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.Text)
            .HasMaxLength(100)
            .HasColumnName("TEXT");
        entity.Property(e => e.VersionInfo)
            .HasMaxLength(15)
            .HasColumnName("VERSION_INFO");
        
    }
}
