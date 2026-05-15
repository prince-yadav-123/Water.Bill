using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class ConsumerDetailsMasterConfiguration : IEntityTypeConfiguration<ConsumerDetailsMaster>
{
    public void Configure(EntityTypeBuilder<ConsumerDetailsMaster> entity)
    {
        entity.HasKey(e => e.ConsNo).HasName("PRIMARY");
        
        entity.ToTable("consumer_details_master");
        
        entity.Property(e => e.ConsNo)
            .HasMaxLength(10)
            .HasColumnName("CONS_NO");
        entity.Property(e => e.BlkNo)
            .HasMaxLength(15)
            .HasColumnName("BLK_NO");
        entity.Property(e => e.CalDate)
            .HasColumnType("datetime")
            .HasColumnName("CAL_DATE");
        entity.Property(e => e.CessAmt).HasColumnName("CESS_AMT");
        entity.Property(e => e.ConTp)
            .HasMaxLength(1)
            .HasColumnName("CON_TP");
        entity.Property(e => e.ConnDt)
            .HasColumnType("datetime")
            .HasColumnName("CONN_DT");
        entity.Property(e => e.ConsAddress)
            .HasMaxLength(150)
            .HasColumnName("CONS_ADDRESS");
        entity.Property(e => e.ConsCtg)
            .HasMaxLength(10)
            .HasColumnName("CONS_CTG");
        entity.Property(e => e.ConsNm1)
            .HasMaxLength(150)
            .HasColumnName("CONS_NM1");
        entity.Property(e => e.ConsNm2)
            .HasMaxLength(30)
            .HasColumnName("CONS_NM2");
        entity.Property(e => e.DeleteDate)
            .HasColumnType("datetime")
            .HasColumnName("DELETE_DATE");
        entity.Property(e => e.DevType).HasColumnName("DEV_TYPE");
        entity.Property(e => e.EmailId)
            .HasMaxLength(50)
            .HasColumnName("EMAIL_ID");
        entity.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .HasColumnName("ENTRY_DATE");
        entity.Property(e => e.Esti1Amt).HasColumnName("ESTI1_AMT");
        entity.Property(e => e.EstiAmt).HasColumnName("ESTI_AMT");
        entity.Property(e => e.EstiDt)
            .HasColumnType("datetime")
            .HasColumnName("ESTI_DT");
        entity.Property(e => e.EstiNo)
            .HasMaxLength(10)
            .HasColumnName("ESTI_NO");
        entity.Property(e => e.FlatNo)
            .HasMaxLength(20)
            .HasColumnName("FLAT_NO");
        entity.Property(e => e.FlatType)
            .HasMaxLength(6)
            .HasColumnName("FLAT_TYPE");
        entity.Property(e => e.IssueOfficer)
            .HasMaxLength(50)
            .HasColumnName("ISSUE_OFFICER");
        entity.Property(e => e.KhasraNo)
            .HasMaxLength(20)
            .HasColumnName("KHASRA_NO");
        entity.Property(e => e.KiloLitter)
            .HasMaxLength(50)
            .HasColumnName("KILO_LITTER");
        entity.Property(e => e.LedgerDate)
            .HasColumnType("datetime")
            .HasColumnName("LEDGER_DATE");
        entity.Property(e => e.MaonthyCharges).HasColumnName("MAONTHY_CHARGES");
        entity.Property(e => e.MobNo)
            .HasMaxLength(12)
            .HasColumnName("MOB_NO");
        entity.Property(e => e.ModifyDate)
            .HasColumnType("datetime")
            .HasColumnName("MODIFY_DATE");
        entity.Property(e => e.MonthlyRate).HasColumnName("MONTHLY_RATE");
        entity.Property(e => e.Narration).HasMaxLength(250);
        entity.Property(e => e.NewStatus).HasColumnName("NEW_STATUS");
        entity.Property(e => e.NoduesDt)
            .HasColumnType("datetime")
            .HasColumnName("NODUES_DT");
        entity.Property(e => e.OldNewEn)
            .HasMaxLength(2)
            .HasColumnName("OLD_NEW_EN");
        entity.Property(e => e.OldNewUp)
            .HasMaxLength(2)
            .HasColumnName("OLD_NEW_UP");
        entity.Property(e => e.OtherCon)
            .HasMaxLength(150)
            .HasColumnName("OTHER_CON");
        entity.Property(e => e.PipeSize).HasColumnName("PIPE_SIZE");
        entity.Property(e => e.PlotMapId)
            .HasMaxLength(25)
            .HasColumnName("PlotMapID");
        entity.Property(e => e.PlotSize).HasColumnName("PLOT_SIZE");
        entity.Property(e => e.PurposeCon)
            .HasMaxLength(100)
            .HasColumnName("PURPOSE_CON");
        entity.Property(e => e.RegNo)
            .HasMaxLength(8)
            .HasColumnName("REG_NO");
        entity.Property(e => e.Sector)
            .HasMaxLength(25)
            .HasColumnName("SECTOR");
        entity.Property(e => e.Secu).HasColumnName("SECU");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.TypeChangeDate)
            .HasColumnType("datetime")
            .HasColumnName("TYPE_CHANGE_DATE");
        entity.Property(e => e.Userid)
            .HasMaxLength(10)
            .HasColumnName("USERID");
        entity.Property(e => e.VillgaeId).HasColumnName("VILLGAE_ID");
        entity.Property(e => e.VillgaeName)
            .HasMaxLength(100)
            .HasColumnName("VILLGAE_NAME");
        
    }
}
