using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ApplicationWorkflowHistoryConfiguration : IEntityTypeConfiguration<ApplicationWorkflowHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationWorkflowHistory> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ApplicationWorkflowHistory");
        entity.HasIndex(e => e.WorkflowInstanceId, "IX_WorkflowHistory_Instance");
        entity.HasIndex(e => e.ApplicationNo, "IX_WorkflowHistory_ApplicationNo");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30);
        entity.Property(e => e.FromStatus).HasMaxLength(30);
        entity.Property(e => e.ToStatus).HasMaxLength(30);
        entity.Property(e => e.Action).HasMaxLength(50);
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.ActionByName).HasMaxLength(150);
        entity.Property(e => e.ActionByRole).HasMaxLength(50);
        entity.Property(e => e.ActionOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
