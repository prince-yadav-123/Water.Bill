using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerEconsumerMailConfiguration : IEntityTypeConfiguration<ConsumerEconsumerMail>
{
    public void Configure(EntityTypeBuilder<ConsumerEconsumerMail> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        
        entity.ToTable("consumer_econsumer_mail");
        
        entity.Property(e => e.Id).HasColumnName("ID");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(10)
            .IsFixedLength()
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Emailid)
            .HasMaxLength(100)
            .IsFixedLength()
            .HasColumnName("EMAILID");
        entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
        entity.Property(e => e.Name)
            .HasMaxLength(150)
            .IsFixedLength()
            .HasColumnName("NAME");
        entity.Property(e => e.Property)
            .HasMaxLength(50)
            .IsFixedLength()
            .HasColumnName("PROPERTY");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        
    }
}
