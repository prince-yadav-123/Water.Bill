using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ApplicationWorkflowInstanceConfiguration : IEntityTypeConfiguration<ApplicationWorkflowInstance>
{
    public void Configure(EntityTypeBuilder<ApplicationWorkflowInstance> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ApplicationWorkflowInstances");
        entity.HasIndex(e => new { e.ApplicationType, e.ApplicationId }, "IX_WorkflowInstances_Application");
        entity.HasIndex(e => e.ApplicationNo, "IX_WorkflowInstances_ApplicationNo");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30);
        entity.Property(e => e.ApplicationType).HasMaxLength(50);
        entity.Property(e => e.CurrentStatus).HasMaxLength(30);
        entity.Property(e => e.StartedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.CompletedOn).HasColumnType("datetime");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.HasOne(e => e.Workflow).WithMany().HasForeignKey(e => e.WorkflowId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(e => e.CurrentStage).WithMany().HasForeignKey(e => e.CurrentStageId).OnDelete(DeleteBehavior.SetNull);
    }
}
