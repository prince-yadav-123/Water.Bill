using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> entity)
    {
        entity.ToTable("ErrorLogs");
        entity.HasKey(e => e.Id).HasName("PK_ErrorLogs");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasIndex(e => e.CreatedAt, "IX_ErrorLogs_CreatedAt");
        entity.HasIndex(e => e.ExceptionType, "IX_ErrorLogs_ExceptionType");
        entity.HasIndex(e => e.StatusCode, "IX_ErrorLogs_StatusCode");
        entity.HasIndex(e => e.PortalType, "IX_ErrorLogs_PortalType");
        entity.HasIndex(e => new { e.CreatedAt, e.PortalType, e.StatusCode }, "IX_ErrorLogs_CreatedAt_Portal_Status");

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
        entity.Property(e => e.ExceptionType).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        entity.Property(e => e.StackTrace).HasColumnType("nvarchar(max)");
        entity.Property(e => e.RequestPath).HasMaxLength(500);
        entity.Property(e => e.HttpMethod).HasMaxLength(10);
        entity.Property(e => e.QueryString).HasMaxLength(2000);
        entity.Property(e => e.IpAddress).HasMaxLength(64);
        entity.Property(e => e.Username).HasMaxLength(150);
        entity.Property(e => e.UserId).HasMaxLength(100);
        entity.Property(e => e.PortalType).HasMaxLength(20).HasDefaultValue(AppConstants.PortalTypes.Unknown).IsRequired();
        entity.Property(e => e.UserAgent).HasMaxLength(1000);
        entity.Property(e => e.ControllerName).HasMaxLength(150);
        entity.Property(e => e.ActionName).HasMaxLength(150);
        entity.Property(e => e.TraceId).HasMaxLength(100);
        entity.Property(e => e.IsHandled).HasDefaultValue(false);
    }
}
