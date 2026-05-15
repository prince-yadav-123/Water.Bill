using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerEmailConfiguration : IEntityTypeConfiguration<ConsumerEmail>
{
    public void Configure(EntityTypeBuilder<ConsumerEmail> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_email");
        
        entity.HasIndex(e => e.Id, "IX_CONSUMER_EMAIL_ID_AUTO");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(10)
            .IsFixedLength()
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Emailid)
            .HasMaxLength(100)
            .IsFixedLength()
            .HasColumnName("EMAILID");
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("ID");
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
