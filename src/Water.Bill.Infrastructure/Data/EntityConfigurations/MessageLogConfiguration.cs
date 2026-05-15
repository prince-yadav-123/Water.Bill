using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MessageLogConfiguration : IEntityTypeConfiguration<MessageLog>
{
    public void Configure(EntityTypeBuilder<MessageLog> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("message_log");
        
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Errorcode)
            .HasMaxLength(4)
            .HasColumnName("ERRORCODE");
        entity.Property(e => e.Errormessage)
            .HasMaxLength(5)
            .HasColumnName("ERRORMESSAGE");
        entity.Property(e => e.Flag)
            .HasMaxLength(1)
            .HasColumnName("FLAG");
        entity.Property(e => e.Jobid)
            .HasMaxLength(12)
            .HasColumnName("JOBID");
        entity.Property(e => e.Message)
            .HasMaxLength(200)
            .HasColumnName("MESSAGE");
        entity.Property(e => e.MobNo)
            .HasMaxLength(12)
            .HasColumnName("MOB_NO");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
