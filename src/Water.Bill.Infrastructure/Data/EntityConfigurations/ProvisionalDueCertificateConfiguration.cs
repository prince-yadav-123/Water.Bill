using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ProvisionalDueCertificateConfiguration : IEntityTypeConfiguration<ProvisionalDueCertificate>
{
    public void Configure(EntityTypeBuilder<ProvisionalDueCertificate> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");
        
        entity.ToTable("provisional_due_certificate");
        
        entity.Property(e => e.AutoId).HasColumnName("AUTO_ID");
        entity.Property(e => e.Block)
            .HasMaxLength(50)
            .HasColumnName("BLOCK");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(50)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.CreateOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATE_ON");
        entity.Property(e => e.PlotNo)
            .HasMaxLength(50)
            .HasColumnName("PLOT_NO");
        entity.Property(e => e.Sector)
            .HasMaxLength(50)
            .HasColumnName("SECTOR");
        
    }
}
