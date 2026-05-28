using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ApplicationWorkflowTaskConfiguration : IEntityTypeConfiguration<ApplicationWorkflowTask>
{
    public void Configure(EntityTypeBuilder<ApplicationWorkflowTask> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("ApplicationWorkflowTasks");
        entity.HasIndex(e => e.WorkflowInstanceId, "IX_WorkflowTasks_Instance");
        entity.HasIndex(e => new { e.Status, e.AssignedRoleId, e.AssignedUserId, e.AssignedDepartmentId }, "IX_WorkflowTasks_Assignment");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30);
        entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("Pending");
        entity.Property(e => e.AssignedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.ActionOn).HasColumnType("datetime");
        entity.Property(e => e.Remarks).HasMaxLength(500);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.HasOne(e => e.WorkflowInstance).WithMany().HasForeignKey(e => e.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Restrict);
    }
}
