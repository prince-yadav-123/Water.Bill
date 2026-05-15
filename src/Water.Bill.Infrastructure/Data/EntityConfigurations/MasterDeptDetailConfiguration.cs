using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterDeptDetailConfiguration : IEntityTypeConfiguration<MasterDeptDetail>
{
    public void Configure(EntityTypeBuilder<MasterDeptDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_dept_details");
        
        entity.Property(e => e.DeptId).HasColumnName("DEPT_ID");
        entity.Property(e => e.DeptName)
            .HasMaxLength(50)
            .HasColumnName("DEPT_NAME");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
