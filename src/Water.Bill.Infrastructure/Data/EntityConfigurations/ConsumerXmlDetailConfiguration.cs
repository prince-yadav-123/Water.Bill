using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerXmlDetailConfiguration : IEntityTypeConfiguration<ConsumerXmlDetail>
{
    public void Configure(EntityTypeBuilder<ConsumerXmlDetail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_xml_details");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(15)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.XmlData).HasColumnName("XML_DATA");
        
    }
}
