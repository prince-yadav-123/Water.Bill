using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ReceiptHeadMasterConfiguration : IEntityTypeConfiguration<ReceiptHeadMaster>
{
    public void Configure(EntityTypeBuilder<ReceiptHeadMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("receipt_head_master");
        
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.HeadId).HasColumnName("HEAD_ID");
        entity.Property(e => e.HeadName)
            .HasMaxLength(255)
            .HasColumnName("HEAD_NAME");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.Userid)
            .HasMaxLength(255)
            .HasColumnName("USERID");
        
    }
}
