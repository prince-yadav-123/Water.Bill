using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NotificationTargetConfiguration : IEntityTypeConfiguration<NotificationTarget>
{
    public void Configure(EntityTypeBuilder<NotificationTarget> builder)
    {
        builder.ToTable("notification_targets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(x => x.TargetType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TargetId).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.TargetName).HasMaxLength(300).IsRequired(false);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.Notification)
               .WithMany(x => x.Targets)
               .HasForeignKey(x => x.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}
