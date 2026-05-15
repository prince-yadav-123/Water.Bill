using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerEmailRecord20222023Configuration : IEntityTypeConfiguration<ConsumerEmailRecord20222023>
{
    public void Configure(EntityTypeBuilder<ConsumerEmailRecord20222023> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_email_record_2022_2023");
        
        entity.HasIndex(e => e.UId, "IX_CONSUMER_EMAIL_RECORD_2022_2023_U_ID_AUTO");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(8)
            .IsFixedLength()
            .HasColumnName("CONS_NO");
        entity.Property(e => e.Emailid)
            .HasMaxLength(50)
            .IsFixedLength()
            .HasColumnName("EMAILID");
        entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsFixedLength()
            .HasColumnName("NAME");
        entity.Property(e => e.Property)
            .HasMaxLength(30)
            .IsFixedLength()
            .HasColumnName("PROPERTY");
        entity.Property(e => e.Status).HasColumnName("STATUS");
        entity.Property(e => e.UId)
            .ValueGeneratedOnAdd()
            .HasColumnName("U_ID");
        
    }
}
