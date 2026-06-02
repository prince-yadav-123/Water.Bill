using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NoticeTemplateConfiguration : IEntityTypeConfiguration<NoticeTemplate>
{
    public void Configure(EntityTypeBuilder<NoticeTemplate> entity)
    {
        entity.ToTable("NoticeTemplates");

        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.NoticeType, e.IsDeleted }, "IX_NoticeTemplates_Type");
        entity.HasIndex(e => new { e.TemplateName, e.IsDeleted }, "IX_NoticeTemplates_Name");

        entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
        entity.Property(e => e.NoticeType).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Body).HasColumnType("text").IsRequired();
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
