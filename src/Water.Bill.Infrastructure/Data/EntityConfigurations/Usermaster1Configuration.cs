using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class Usermaster1Configuration : IEntityTypeConfiguration<Usermaster1>
{
    public void Configure(EntityTypeBuilder<Usermaster1> entity)
    {
        entity.HasKey(e => e.UserId).HasName("PRIMARY");
        
        entity.ToTable("usermaster");
        
        entity.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("UserID");
        entity.Property(e => e.UserEmailId)
            .HasMaxLength(30)
            .HasColumnName("UserEmailID");
        entity.Property(e => e.UserName).HasMaxLength(20);
        entity.Property(e => e.UserPassword).HasMaxLength(10);
        entity.Property(e => e.UserRoles).HasMaxLength(10);
        
    }
}
