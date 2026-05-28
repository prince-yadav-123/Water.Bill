using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("NotificationLogs");
        entity.HasIndex(e => e.ApplicationNo, "IX_NotificationLogs_ApplicationNo");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30);
        entity.Property(e => e.Channel).HasMaxLength(30);
        entity.Property(e => e.Recipient).HasMaxLength(150);
        entity.Property(e => e.Message).HasMaxLength(1000);
        entity.Property(e => e.Status).HasMaxLength(30);
        entity.Property(e => e.SentOn).HasColumnType("datetime");
        entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
