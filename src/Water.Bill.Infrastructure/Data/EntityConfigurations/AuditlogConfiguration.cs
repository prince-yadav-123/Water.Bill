using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class AuditlogConfiguration : IEntityTypeConfiguration<Auditlog>
{
    public void Configure(EntityTypeBuilder<Auditlog> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity
            .ToTable("auditlogs")
            .UseCollation("utf8mb4_0900_ai_ci");
        
        entity.HasIndex(e => e.Timestamp, "IX_AuditLogs_Timestamp");
        
        entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");
        
        entity.Property(e => e.Details).HasColumnType("text");
        entity.Property(e => e.EntityId).HasMaxLength(100);
        entity.Property(e => e.IpAddress).HasMaxLength(64);
        entity.Property(e => e.Module).HasMaxLength(100);
        entity.Property(e => e.Success)
            .IsRequired()
            .HasDefaultValueSql("'1'");
        entity.Property(e => e.Timestamp)
            .HasMaxLength(6)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        entity.Property(e => e.UserAgent).HasMaxLength(500);
        entity.Property(e => e.Username).HasMaxLength(100);
        
    }
}
