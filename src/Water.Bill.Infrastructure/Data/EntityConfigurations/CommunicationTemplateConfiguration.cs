using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> entity)
    {
        entity.ToTable("CommunicationTemplates");
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.PurposeId, "IX_CommunicationTemplates_PurposeId");
        entity.HasIndex(e => new { e.PurposeKey, e.Channel, e.Language, e.IsDefault, e.IsActive, e.IsDeleted }, "IX_CommunicationTemplates_DefaultLookup");
        entity.Property(e => e.PurposeKey).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Channel).HasMaxLength(20).IsRequired();
        entity.Property(e => e.TemplateName).HasMaxLength(150).IsRequired();
        entity.Property(e => e.Subject).HasMaxLength(300);
        entity.Property(e => e.Body).HasColumnType("text").IsRequired();
        entity.Property(e => e.ExternalTemplateId).HasMaxLength(150);
        entity.Property(e => e.Language).HasMaxLength(10);
        entity.Property(e => e.IsDefault).HasDefaultValueSql("'1'");
        entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
        entity.Property(e => e.IsDeleted).HasDefaultValueSql("'0'");
        entity.Property(e => e.CreatedAt).HasMaxLength(6).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        entity.HasOne(e => e.Purpose)
            .WithMany(e => e.Templates)
            .HasForeignKey(e => e.PurposeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CommunicationTemplates_CommunicationPurposes_PurposeId");
    }
}
