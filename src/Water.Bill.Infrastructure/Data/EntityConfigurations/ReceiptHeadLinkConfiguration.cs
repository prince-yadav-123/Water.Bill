using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ReceiptHeadLinkConfiguration : IEntityTypeConfiguration<ReceiptHeadLink>
{
    public void Configure(EntityTypeBuilder<ReceiptHeadLink> entity)
    {
        entity
            .HasNoKey()
            .ToTable("receipt_head_link");
        
        entity.Property(e => e.DepId).HasColumnName("DEP_ID");
        entity.Property(e => e.HeadId).HasColumnName("HEAD_ID");
        entity.Property(e => e.ReceiptSubheadId).HasColumnName("RECEIPT_SUBHEAD_ID");
        
    }
}
