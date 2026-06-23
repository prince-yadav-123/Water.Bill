using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CommunicationChannelSettingConfiguration : IEntityTypeConfiguration<CommunicationChannelSetting>
{
    public void Configure(EntityTypeBuilder<CommunicationChannelSetting> entity)
    {
        entity.ToTable("CommunicationChannelSettings");
        entity.HasKey(e => e.Id).HasName("PK_CommunicationChannelSettings");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.ChannelName, e.IsDeleted }, "UX_CommunicationChannelSettings_ChannelName_IsDeleted")
            .IsUnique();

        entity.Property(e => e.ChannelName).HasMaxLength(50).IsRequired();
        entity.Property(e => e.IsEnabled).HasDefaultValue(true);
        entity.Property(e => e.ConfigurationJson).HasColumnType("nvarchar(max)").HasDefaultValue("{}").IsRequired();
        entity.Property(e => e.CreatedByName).HasMaxLength(200);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
        entity.Property(e => e.UpdatedByName).HasMaxLength(200);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
