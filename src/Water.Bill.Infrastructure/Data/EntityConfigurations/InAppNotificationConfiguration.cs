using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> entity)
    {
        entity.ToTable("InAppNotifications");
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => new { e.UserType, e.UserId, e.IsRead, e.IsDeleted }, "IX_InAppNotifications_User_Read");
        entity.Property(e => e.UserType).HasMaxLength(20).IsRequired();
        entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
        entity.Property(e => e.Message).HasColumnType("text").IsRequired();
        entity.Property(e => e.PurposeKey).HasMaxLength(100).IsRequired();
        entity.Property(e => e.ReferenceType).HasMaxLength(100);
        entity.Property(e => e.ReferenceId).HasMaxLength(100);
        entity.Property(e => e.ReferenceNo).HasMaxLength(100);
        entity.Property(e => e.IsRead).HasDefaultValueSql("'0'");
        entity.Property(e => e.ReadAt).HasMaxLength(6);
        entity.Property(e => e.CreatedAt).HasMaxLength(6).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
    }
}

