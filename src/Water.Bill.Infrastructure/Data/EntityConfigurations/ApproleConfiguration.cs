using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ApproleConfiguration : IEntityTypeConfiguration<Approle>
{
    public void Configure(EntityTypeBuilder<Approle> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity
            .ToTable("approles")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => e.Name, "UX_AppRoles_Name").IsUnique();
        
        entity.Property(e => e.CreatedAt)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.Name).HasMaxLength(100);
        entity.Property(e => e.Permissions).HasColumnType("json");
        entity.Property(e => e.UpdatedAt).HasMaxLength(6);
        
    }
}
