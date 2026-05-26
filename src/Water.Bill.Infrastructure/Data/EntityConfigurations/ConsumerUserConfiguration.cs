using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerUserConfiguration : IEntityTypeConfiguration<ConsumerUser>
{
    public void Configure(EntityTypeBuilder<ConsumerUser> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        
entity.ToTable("ConsumerUsers");

        entity.HasIndex(e => e.ConsumerNo, "IX_ConsumerUsers_ConsumerNo");
        entity.HasIndex(e => e.Username, "UX_ConsumerUsers_Username").IsUnique();
        entity.HasIndex(e => e.Email, "UX_ConsumerUsers_Email").IsUnique();

        entity.Property(e => e.Id).HasColumnName("Id");
        entity.Property(e => e.ConsumerNo).HasMaxLength(10);
        entity.Property(e => e.Username).HasMaxLength(100);
        entity.Property(e => e.Email).HasMaxLength(150);
        entity.Property(e => e.PasswordHash).HasMaxLength(512);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
        entity.Property(e => e.LockoutUntil).HasColumnType("datetime");
        entity.Property(e => e.IsDeleted).HasDefaultValue(false);

        entity.HasOne(e => e.Consumer)
            .WithMany()
            .HasForeignKey(e => e.ConsumerNo)
            .HasPrincipalKey(e => e.ConsNo)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ConsumerUsers_ConsumerDetailsMaster_ConsumerNo");
    }
}
