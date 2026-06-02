using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> entity)
    {
        entity.ToTable("CommunicationLogs");
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => new { e.PurposeKey, e.Channel, e.CreatedAt }, "IX_CommunicationLogs_Purpose_Channel_CreatedAt");
        entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "IX_CommunicationLogs_Reference");
        entity.Property(e => e.PurposeKey).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Channel).HasMaxLength(20).IsRequired();
        entity.Property(e => e.RecipientName).HasMaxLength(150);
        entity.Property(e => e.RecipientEmail).HasMaxLength(150);
        entity.Property(e => e.RecipientMobile).HasMaxLength(20);
        entity.Property(e => e.Subject).HasMaxLength(300);
        entity.Property(e => e.MessageBody).HasColumnType("text").IsRequired();
        entity.Property(e => e.ExternalTemplateId).HasMaxLength(150);
        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValueSql("'Pending'");
        entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        entity.Property(e => e.SentAt).HasMaxLength(6);
        entity.Property(e => e.CreatedAt).HasMaxLength(6).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.ReferenceType).HasMaxLength(100);
        entity.Property(e => e.ReferenceId).HasMaxLength(100);
        entity.Property(e => e.ReferenceNo).HasMaxLength(100);
    }
}

