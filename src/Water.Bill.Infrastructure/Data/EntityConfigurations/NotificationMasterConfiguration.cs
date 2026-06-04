using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NotificationMasterConfiguration : IEntityTypeConfiguration<NotificationMaster>
{
    public void Configure(EntityTypeBuilder<NotificationMaster> builder)
    {
        builder.ToTable("notification_masters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasColumnType("text").IsRequired();
        builder.Property(x => x.NotificationType).HasMaxLength(50).HasDefaultValue("General");
        builder.Property(x => x.TargetAudience).HasMaxLength(20).HasDefaultValue("Consumer");
        builder.Property(x => x.Channels).HasMaxLength(100).HasDefaultValue("InApp");
        builder.Property(x => x.Priority).HasMaxLength(20).HasDefaultValue("Normal");
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Draft");
        builder.Property(x => x.ValidFrom).IsRequired(false);
        builder.Property(x => x.ValidTo).IsRequired(false);
        builder.Property(x => x.CreatedByName).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.SentAt).IsRequired(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TargetAudience);
        builder.HasIndex(x => new { x.CreatedAt, x.IsDeleted });
    }
}
