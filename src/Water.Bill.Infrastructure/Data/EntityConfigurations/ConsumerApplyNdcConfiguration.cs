using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerApplyNdcConfiguration : IEntityTypeConfiguration<ConsumerApplyNdc>
{
    public void Configure(EntityTypeBuilder<ConsumerApplyNdc> entity)
    {
        entity.HasKey(e => e.AutoId).HasName("PRIMARY");

        entity.ToTable("consumer_apply_ndc");
        
        entity.HasIndex(e => e.AutoId, "IX_CONSUMER_APPLY_NDC_AUTO_ID_AUTO");
        
        entity.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .HasColumnName("AMOUNT");
        entity.Property(e => e.ApplicationNo)
            .HasMaxLength(50)
            .HasColumnName("APPLICATION_NO");
        entity.Property(e => e.Attachment1)
            .HasColumnType("text")
            .HasColumnName("ATTACHMENT1");
        entity.Property(e => e.Attachment2)
            .HasColumnType("text")
            .HasColumnName("ATTACHMENT2");
        entity.Property(e => e.Attachment3)
            .HasColumnType("text")
            .HasColumnName("ATTACHMENT3");
        entity.Property(e => e.Attribute1)
            .HasMaxLength(100)
            .HasColumnName("ATTRIBUTE1");
        entity.Property(e => e.Attribute2)
            .HasMaxLength(100)
            .HasColumnName("ATTRIBUTE2");
        entity.Property(e => e.Attribute3)
            .HasMaxLength(100)
            .HasColumnName("ATTRIBUTE3");
        entity.Property(e => e.Attribute4)
            .HasMaxLength(100)
            .HasColumnName("ATTRIBUTE4");
        entity.Property(e => e.Attribute5)
            .HasMaxLength(100)
            .HasColumnName("ATTRIBUTE5");
        entity.Property(e => e.AutoId)
            .ValueGeneratedOnAdd()
            .HasColumnName("AUTO_ID");
        entity.Property(e => e.BillFromDate).HasColumnName("BILL_FROM_DATE");
        entity.Property(e => e.BillNo)
            .HasMaxLength(50)
            .HasColumnName("BILL_NO");
        entity.Property(e => e.BillToDate).HasColumnName("BILL_TO_DATE");
        entity.Property(e => e.Block)
            .HasMaxLength(10)
            .HasColumnName("BLOCK");
        entity.Property(e => e.CertificateUrl).HasColumnName("CERTIFICATE_URL");
        entity.Property(e => e.ChallanFile)
            .HasColumnType("text")
            .HasColumnName("CHALLAN_FILE");
        entity.Property(e => e.ChallanNo)
            .HasMaxLength(50)
            .HasColumnName("CHALLAN_NO");
        entity.Property(e => e.CompletedDate)
            .HasColumnType("datetime")
            .HasColumnName("COMPLETED_DATE");
        entity.Property(e => e.ConsName)
            .HasMaxLength(100)
            .HasColumnName("CONS_NAME");
        entity.Property(e => e.ConsTp)
            .HasMaxLength(1)
            .HasColumnName("CONS_TP");
        entity.Property(e => e.ConsumerApplicationUrl)
            .HasColumnType("text")
            .HasColumnName("CONSUMER_APPLICATION_URL");
        entity.Property(e => e.ConsumerNo)
            .HasMaxLength(50)
            .HasColumnName("CONSUMER_NO");
        entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
        entity.Property(e => e.CreatedOn)
            .HasColumnType("datetime")
            .HasColumnName("CREATED_ON");
        entity.Property(e => e.CurrentStatus)
            .HasMaxLength(10)
            .HasColumnName("CURRENT_STATUS");
        entity.Property(e => e.DivisionType).HasColumnName("DIVISION_TYPE");
        entity.Property(e => e.DivisionTypeName)
            .HasColumnType("text")
            .HasColumnName("DIVISION_TYPE_NAME");
        entity.Property(e => e.Email)
            .HasColumnType("text")
            .HasColumnName("EMAIL");
        entity.Property(e => e.FinalStatus)
            .HasMaxLength(10)
            .HasColumnName("FINAL_STATUS");
        entity.Property(e => e.LastUpdatedBy).HasColumnName("LAST_UPDATED_BY");
        entity.Property(e => e.LastUpdatedOn)
            .HasColumnType("datetime")
            .HasColumnName("LAST_UPDATED_ON");
        entity.Property(e => e.Level).HasColumnName("LEVEL");
        entity.Property(e => e.Level1Action1)
            .HasMaxLength(30)
            .HasColumnName("LEVEL1_ACTION1");
        entity.Property(e => e.Level1Action2)
            .HasMaxLength(100)
            .HasColumnName("LEVEL1_ACTION2");
        entity.Property(e => e.Level1ActionDate1)
            .HasColumnType("datetime")
            .HasColumnName("LEVEL1_ACTION_DATE1");
        entity.Property(e => e.Level1ActionDate2)
            .HasColumnType("datetime")
            .HasColumnName("LEVEL1_ACTION_DATE2");
        entity.Property(e => e.Level1Remark1)
            .HasColumnType("text")
            .HasColumnName("LEVEL1_REMARK1");
        entity.Property(e => e.Level1Remark2)
            .HasColumnType("text")
            .HasColumnName("LEVEL1_REMARK2");
        entity.Property(e => e.Level2Action1)
            .HasMaxLength(100)
            .HasColumnName("LEVEL2_ACTION1");
        entity.Property(e => e.Level2Action2)
            .HasMaxLength(100)
            .HasColumnName("LEVEL2_ACTION2");
        entity.Property(e => e.Level2ActionDate)
            .HasColumnType("datetime")
            .HasColumnName("LEVEL2_ACTION_DATE");
        entity.Property(e => e.Level2ActionDate2)
            .HasColumnType("datetime")
            .HasColumnName("LEVEL2_ACTION_DATE2");
        entity.Property(e => e.Level2Remark)
            .HasColumnType("text")
            .HasColumnName("LEVEL2_REMARK");
        entity.Property(e => e.Level2Remark2)
            .HasColumnType("text")
            .HasColumnName("LEVEL2_REMARK2");
        entity.Property(e => e.MobileNo)
            .HasMaxLength(15)
            .HasColumnName("MOBILE_NO");
        entity.Property(e => e.OrderId)
            .HasMaxLength(50)
            .HasColumnName("ORDER_ID");
        entity.Property(e => e.PaymentSuccessDate)
            .HasColumnType("datetime")
            .HasColumnName("PAYMENT_SUCCESS_DATE");
        entity.Property(e => e.PipeSize)
            .HasMaxLength(10)
            .HasColumnName("PIPE_SIZE");
        entity.Property(e => e.PlotArea)
            .HasMaxLength(10)
            .HasColumnName("PLOT_AREA");
        entity.Property(e => e.PlotNo)
            .HasMaxLength(10)
            .HasColumnName("PLOT_NO");
        entity.Property(e => e.Rid)
            .HasMaxLength(50)
            .HasColumnName("RID");
        entity.Property(e => e.Sector)
            .HasMaxLength(10)
            .HasColumnName("SECTOR");
        entity.Property(e => e.Status)
            .HasMaxLength(20)
            .HasColumnName("STATUS");
        entity.Property(e => e.SuccessStatus)
            .HasMaxLength(1)
            .IsFixedLength()
            .HasColumnName("SUCCESS_STATUS");
        entity.Property(e => e.TrackingId)
            .HasMaxLength(50)
            .HasColumnName("TRACKING_ID");
        entity.Property(e => e.Type)
            .HasMaxLength(10)
            .HasColumnName("TYPE");
        
    }
}
