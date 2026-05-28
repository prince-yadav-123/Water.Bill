using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class WorkflowStageConfiguration : IEntityTypeConfiguration<WorkflowStage>
{
    public void Configure(EntityTypeBuilder<WorkflowStage> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("WorkflowStages");
        entity.HasIndex(e => new { e.WorkflowId, e.StageOrder }, "IX_WorkflowStages_Workflow_Order");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.StageName).HasMaxLength(100);
        entity.Property(e => e.ApprovalType).HasMaxLength(30).HasDefaultValue("DepartmentRole");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

        entity.HasOne(e => e.Workflow).WithMany(e => e.Stages).HasForeignKey(e => e.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
    }
}
