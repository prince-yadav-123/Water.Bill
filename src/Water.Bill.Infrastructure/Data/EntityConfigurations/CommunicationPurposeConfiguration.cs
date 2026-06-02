using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CommunicationPurposeConfiguration : IEntityTypeConfiguration<CommunicationPurpose>
{
    public void Configure(EntityTypeBuilder<CommunicationPurpose> entity)
    {
        entity.ToTable("CommunicationPurposes");
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.PurposeKey, "UX_CommunicationPurposes_PurposeKey").IsUnique();
        entity.Property(e => e.PurposeKey).HasMaxLength(100).IsRequired();
        entity.Property(e => e.DisplayName).HasMaxLength(150).IsRequired();
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.AllowedPlaceholders).HasColumnType("json").IsRequired();
        entity.Property(e => e.IsSystem).HasDefaultValueSql("'1'");
        entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
        entity.Property(e => e.CreatedAt).HasMaxLength(6).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
    }
}

