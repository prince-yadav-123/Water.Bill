using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConnectionApprovalHistoryConfiguration : IEntityTypeConfiguration<NewConnectionApprovalHistory>
{
    public void Configure(EntityTypeBuilder<NewConnectionApprovalHistory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("NewConnectionApprovalHistory");

        entity.HasIndex(e => e.ApplicationId, "IX_NewConnectionApprovalHistory_ApplicationId");
        entity.HasIndex(e => e.ApplicationNo, "IX_NewConnectionApprovalHistory_ApplicationNo");
        entity.HasIndex(e => e.ActionOn, "IX_NewConnectionApprovalHistory_ActionOn");

        entity.Property(e => e.Id).HasColumnName("Id");
        entity.Property(e => e.ApplicationId).HasColumnName("ApplicationId");
        entity.Property(e => e.ApplicationNo).HasMaxLength(30).HasColumnName("ApplicationNo");
        entity.Property(e => e.FromStatus).HasMaxLength(30).HasColumnName("FromStatus");
        entity.Property(e => e.ToStatus).HasMaxLength(30).HasColumnName("ToStatus");
        entity.Property(e => e.Action).HasMaxLength(50).HasColumnName("Action");
        entity.Property(e => e.Remarks).HasMaxLength(500).HasColumnName("Remarks");
        entity.Property(e => e.ActionBy).HasColumnName("ActionBy");
        entity.Property(e => e.ActionByName).HasMaxLength(150).HasColumnName("ActionByName");
        entity.Property(e => e.ActionByRole).HasMaxLength(50).HasColumnName("ActionByRole");
        entity.Property(e => e.ActionOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("ActionOn");
        entity.Property(e => e.IpAddress).HasMaxLength(50).HasColumnName("IpAddress");
        entity.Property(e => e.UserAgent).HasMaxLength(500).HasColumnName("UserAgent");
        entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("IsActive");
        entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");

        entity.HasOne(e => e.Application)
            .WithMany(e => e.ApprovalHistory)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_NewConnectionApprovalHistory_Applications");
    }
}
