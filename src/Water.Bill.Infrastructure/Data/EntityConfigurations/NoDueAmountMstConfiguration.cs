using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NoDueAmountMstConfiguration : IEntityTypeConfiguration<NoDueAmountMst>
{
    public void Configure(EntityTypeBuilder<NoDueAmountMst> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");
        
        entity.ToTable("no_due_amount_mst");
        
        entity.Property(e => e.AutoId).HasColumnName("AUTO_ID");
        entity.Property(e => e.Amount)
            .HasMaxLength(50)
            .HasColumnName("AMOUNT");
        entity.Property(e => e.ConTp)
            .HasMaxLength(50)
            .HasColumnName("CON_TP");
        entity.Property(e => e.CreatedBy)
            .HasMaxLength(50)
            .HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
        
    }
}
