using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class CalcualtionMasterConfiguration : IEntityTypeConfiguration<CalcualtionMaster>
{
    public void Configure(EntityTypeBuilder<CalcualtionMaster> entity)
    {
        entity
            .HasNoKey()
            .ToTable("calcualtion_master");
        
        entity.HasIndex(e => e.Id, "IX_CALCUALTION_MASTER_ID_AUTO");
        
        entity.Property(e => e.BillDate)
            .HasColumnType("datetime")
            .HasColumnName("BILL_DATE");
        entity.Property(e => e.BillNo)
            .HasMaxLength(50)
            .HasColumnName("BILL_NO");
        entity.Property(e => e.CalMeth).HasColumnName("CAL_METH");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EntryDtae)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DTAE");
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("ID");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
