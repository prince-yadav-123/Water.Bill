using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class BankMasterConfiguration : IEntityTypeConfiguration<BankMaster>
{
    public void Configure(EntityTypeBuilder<BankMaster> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("bank_master");
        
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id");
        entity.Property(e => e.AccountNumber).HasColumnName("accountNumber");
        entity.Property(e => e.BankBranchCode)
            .HasMaxLength(255)
            .HasColumnName("Bank_BranchCode");
        entity.Property(e => e.BankId).HasColumnName("bankId");
        entity.Property(e => e.BankId1).HasColumnName("bankId1");
        entity.Property(e => e.BankName)
            .HasMaxLength(255)
            .HasColumnName("bankName");
        entity.Property(e => e.BankType).HasMaxLength(255);
        entity.Property(e => e.BranchId).HasColumnName("branchId");
        entity.Property(e => e.BranchName)
            .HasMaxLength(255)
            .HasColumnName("branchName");
        entity.Property(e => e.BranchType).HasMaxLength(255);
        entity.Property(e => e.CreatedOn).HasColumnType("datetime");
        entity.Property(e => e.CreatedOn1).HasColumnType("datetime");
        entity.Property(e => e.FixedAccountNumber).HasMaxLength(255);
        entity.Property(e => e.Ifsccode)
            .HasMaxLength(255)
            .HasColumnName("IFSCcode");
        entity.Property(e => e.IsAppOnline).HasColumnName("IS_APP_ONLINE");
        entity.Property(e => e.IsNgt)
            .HasMaxLength(255)
            .HasColumnName("Is_NGT");
        entity.Property(e => e.IsNgt1)
            .HasMaxLength(255)
            .HasColumnName("Is_NGT1");
        entity.Property(e => e.IsOffline)
            .HasMaxLength(255)
            .HasColumnName("Is_Offline");
        entity.Property(e => e.IsOnline)
            .HasMaxLength(255)
            .HasColumnName("Is_Online");
        entity.Property(e => e.ModifiedBy).HasMaxLength(255);
        entity.Property(e => e.ModifiedOn).HasMaxLength(255);
        entity.Property(e => e.ModifiedOn1).HasColumnType("datetime");
        entity.Property(e => e.OfflineChallanText).HasMaxLength(255);
        entity.Property(e => e.OnlineChallanText).HasMaxLength(255);
        entity.Property(e => e.Prefix).HasMaxLength(255);
        entity.Property(e => e.Status).HasColumnName("status");
        
    }
}
