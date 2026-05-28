using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class WorkflowStageNotificationConfiguration : IEntityTypeConfiguration<WorkflowStageNotification>
{
    public void Configure(EntityTypeBuilder<WorkflowStageNotification> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("WorkflowStageNotifications");
        entity.HasIndex(e => new { e.WorkflowStageId, e.EventType }, "IX_WorkflowStageNotifications_Stage_Event");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.EventType).HasMaxLength(50);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.HasOne(e => e.WorkflowStage).WithMany().HasForeignKey(e => e.WorkflowStageId).OnDelete(DeleteBehavior.Cascade);
    }
}
