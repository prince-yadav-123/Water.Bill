using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalBankMasterConfiguration : IEntityTypeConfiguration<JalBankMaster>
{
    public void Configure(EntityTypeBuilder<JalBankMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_bank_master");
        
        entity.Property(e => e.AccountNo)
            .HasMaxLength(16)
            .HasColumnName("ACCOUNT_NO");
        entity.Property(e => e.BankId)
            .HasMaxLength(15)
            .HasColumnName("BANK_ID");
        entity.Property(e => e.BankName)
            .HasMaxLength(100)
            .HasColumnName("BANK_NAME");
        entity.Property(e => e.DeleteDate)
            .HasColumnType("datetime")
            .HasColumnName("DELETE_DATE");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.ModifyDate)
            .HasColumnType("datetime")
            .HasColumnName("MODIFY_DATE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
