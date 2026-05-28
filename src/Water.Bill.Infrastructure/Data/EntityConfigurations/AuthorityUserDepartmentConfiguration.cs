using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AuthorityUserDepartmentConfiguration : IEntityTypeConfiguration<AuthorityUserDepartment>
{
    public void Configure(EntityTypeBuilder<AuthorityUserDepartment> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.ToTable("AuthorityUserDepartments");
        entity.HasIndex(e => new { e.UserId, e.DepartmentId }, "UX_AuthorityUserDepartments_User_Department").IsUnique();
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
