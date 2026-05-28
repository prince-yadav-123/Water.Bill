using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class WorkflowMasterConfiguration : IEntityTypeConfiguration<WorkflowMaster>
{
    public void Configure(EntityTypeBuilder<WorkflowMaster> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("WorkflowMasters");
        entity.HasIndex(e => new { e.ApplicationType, e.IsActive }, "IX_WorkflowMasters_ApplicationType_Active");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.WorkflowName).HasMaxLength(100);
        entity.Property(e => e.ApplicationType).HasMaxLength(50);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
    }
}
