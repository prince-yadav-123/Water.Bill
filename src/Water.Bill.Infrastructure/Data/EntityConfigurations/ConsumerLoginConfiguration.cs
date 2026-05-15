using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerLoginConfiguration : IEntityTypeConfiguration<ConsumerLogin>
{
    public void Configure(EntityTypeBuilder<ConsumerLogin> entity)
    {
        entity
            .HasNoKey()
            .ToTable("consumer_login");
        
        entity.HasIndex(e => e.AutoId, "IX_CONSUMER_LOGIN_AUTO_ID_AUTO");
        
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.ConsNo)
            .HasMaxLength(20)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.ConsumerName)
            .HasMaxLength(100)
            .HasColumnName("CONSUMER_NAME");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.FirebaseToken)
            .HasMaxLength(100)
            .HasColumnName("FIREBASE_TOKEN");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(15)
            .HasColumnName("MOBILE_NO");
        entity.Property(e => e.OtpDatetime)
            .HasColumnType("datetime")
            .HasColumnName("OTP_DATETIME");
        entity.Property(e => e.OtpExpiredTime)
            .HasColumnType("datetime")
            .HasColumnName("OTP_EXPIRED_TIME");
        entity.Property(e => e.OtpNo)
            .HasMaxLength(5)
            .HasColumnName("OTP_NO");
        entity.Property(e => e.OtpVerified).HasColumnName("OTP_VERIFIED");
        entity.Property(e => e.Photo)
            .HasMaxLength(100)
            .HasColumnName("PHOTO");
        
    }
}
