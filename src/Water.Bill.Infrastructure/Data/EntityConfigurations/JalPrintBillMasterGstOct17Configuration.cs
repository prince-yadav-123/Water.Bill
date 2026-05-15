using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class JalPrintBillMasterGstOct17Configuration : IEntityTypeConfiguration<JalPrintBillMasterGstOct17>
{
    public void Configure(EntityTypeBuilder<JalPrintBillMasterGstOct17> entity)
    {
        entity
            .HasNoKey()
            .ToTable("jal_print_bill_master_gst_oct17");
        
        entity.Property(e => e.BillNo)
            .HasMaxLength(20)
            .HasColumnName("BILL_NO");
        entity.Property(e => e.Cgst).HasColumnName("CGST");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Sgst).HasColumnName("SGST");
        entity.Property(e => e.Status)
            .HasMaxLength(1)
            .HasColumnName("STATUS");
        
    }
}
