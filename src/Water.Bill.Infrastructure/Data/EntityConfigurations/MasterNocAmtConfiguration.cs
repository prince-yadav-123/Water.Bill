using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class MasterNocAmtConfiguration : IEntityTypeConfiguration<MasterNocAmt>
{
    public void Configure(EntityTypeBuilder<MasterNocAmt> entity)
    {
        entity
            .HasNoKey()
            .ToTable("master_noc_amt");
        
        entity.Property(e => e.Amount)
            .HasMaxLength(6)
            .HasColumnName("AMOUNT");
        entity.Property(e => e.Cgst)
            .HasMaxLength(6)
            .HasColumnName("CGST");
        entity.Property(e => e.ConId)
            .HasMaxLength(2)
            .HasColumnName("CON_ID");
        entity.Property(e => e.ConMainId)
            .HasMaxLength(4)
            .HasColumnName("CON_MAIN_ID");
        entity.Property(e => e.ConName)
            .HasMaxLength(50)
            .HasColumnName("CON_NAME");
        entity.Property(e => e.ExpiryTime)
            .HasMaxLength(50)
            .HasColumnName("EXPIRY_TIME");
        entity.Property(e => e.NocAmt).HasColumnName("NOC_AMT");
        entity.Property(e => e.Sgst)
            .HasMaxLength(6)
            .HasColumnName("SGST");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
