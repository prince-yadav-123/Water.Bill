using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data.EntityConfigurations;

public class NewConnectionApplicationConfiguration : IEntityTypeConfiguration<NewConnectionApplication>
{
    public void Configure(EntityTypeBuilder<NewConnectionApplication> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("new_connection_applications");

        entity.HasIndex(e => e.ApplicationNo, "UX_NewConnectionApplications_ApplicationNo").IsUnique();
        entity.HasIndex(e => e.MobileNumber, "IX_NewConnectionApplications_MobileNumber");
        entity.HasIndex(e => e.ApplicationStatus, "IX_NewConnectionApplications_ApplicationStatus");
        entity.HasIndex(e => e.SubmittedByConsumerNo, "IX_NewConnectionApplications_SubmittedByConsumerNo");
        entity.HasIndex(e => e.SubmittedByConsumerUserId, "IX_NewConnectionApplications_SubmittedByConsumerUserId");

        entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        entity.Property(e => e.ApplicationNo).HasMaxLength(30).HasColumnName("ApplicationNo");
        entity.Property(e => e.ApplicationStatus).HasMaxLength(30).HasDefaultValue("Draft").HasColumnName("ApplicationStatus");
        entity.Property(e => e.FinalConsumerNo).HasMaxLength(10).HasColumnName("FinalConsumerNo");
        entity.Property(e => e.IsPublicApplication).HasColumnName("IsPublicApplication");
        entity.Property(e => e.ApplicantName).HasMaxLength(100).HasColumnName("ApplicantName");
        entity.Property(e => e.FatherName).HasMaxLength(100).HasColumnName("FatherName");
        entity.Property(e => e.MobileNumber).HasMaxLength(12).HasColumnName("MobileNumber");
        entity.Property(e => e.EmailId).HasMaxLength(50).HasColumnName("EmailId");
        entity.Property(e => e.Address).HasMaxLength(150).HasColumnName("Address");
        entity.Property(e => e.Sector).HasMaxLength(10).HasColumnName("Sector");
        entity.Property(e => e.Block).HasMaxLength(10).HasColumnName("Block");
        entity.Property(e => e.FlatNo).HasMaxLength(15).HasColumnName("FlatNo");
        entity.Property(e => e.PlotSize).HasPrecision(8, 2).HasColumnName("PlotSize");
        entity.Property(e => e.PipeSize).HasPrecision(8, 2).HasColumnName("PipeSize");
        entity.Property(e => e.KhasraNo).HasMaxLength(20).HasColumnName("KhasraNo");
        entity.Property(e => e.VillageName).HasMaxLength(100).HasColumnName("VillageName");
        entity.Property(e => e.VillageId).HasColumnName("VillageId");
        entity.Property(e => e.ConnectionCategory).HasMaxLength(4).HasColumnName("ConnectionCategory");
        entity.Property(e => e.ConnectionType).HasMaxLength(10).HasColumnName("ConnectionType");
        entity.Property(e => e.FlatType).HasMaxLength(50).HasColumnName("FlatType");
        entity.Property(e => e.PurposeOfConnection).HasMaxLength(50).HasColumnName("PurposeOfConnection");
        entity.Property(e => e.PreviousConnectionYesNo).HasMaxLength(1).HasColumnName("PreviousConnectionYesNo");
        entity.Property(e => e.OtherConnection).HasMaxLength(150).HasColumnName("OtherConnection");
        entity.Property(e => e.Rid).HasMaxLength(15).HasColumnName("Rid");
        entity.Property(e => e.DevType).HasColumnName("DevType");
        entity.Property(e => e.RegNo).HasMaxLength(10).HasColumnName("RegNo");
        entity.Property(e => e.ConnectionDate).HasColumnType("datetime").HasColumnName("ConnectionDate");
        entity.Property(e => e.EstimationNo).HasMaxLength(10).HasColumnName("EstimationNo");
        entity.Property(e => e.EstimationAmount).HasPrecision(12, 2).HasColumnName("EstimationAmount");
        entity.Property(e => e.SecurityAmount).HasPrecision(12, 2).HasColumnName("SecurityAmount");
        entity.Property(e => e.EstimationDate).HasColumnType("datetime").HasColumnName("EstimationDate");
        entity.Property(e => e.CessAmount).HasPrecision(12, 2).HasColumnName("CessAmount");
        entity.Property(e => e.MonthlyCharges).HasPrecision(12, 2).HasColumnName("MonthlyCharges");
        entity.Property(e => e.IssueOfficer).HasMaxLength(50).HasColumnName("IssueOfficer");
        entity.Property(e => e.AllotmentDate).HasColumnName("AllotmentDate");
        entity.Property(e => e.PossessionDate).HasColumnName("PossessionDate");
        entity.Property(e => e.ComplianceDate).HasColumnName("ComplianceDate");
        entity.Property(e => e.SsiDate).HasColumnName("SsiDate");
        entity.Property(e => e.AffidavitYn).HasMaxLength(2).HasColumnName("AffidavitYn");
        entity.Property(e => e.SubmittedByConsumerNo).HasMaxLength(10).HasColumnName("SubmittedByConsumerNo");
        entity.Property(e => e.SubmittedByConsumerUserId).HasColumnName("SubmittedByConsumerUserId");
        entity.Property(e => e.SubmittedOn).HasColumnType("datetime").HasColumnName("SubmittedOn");
        entity.Property(e => e.CreatedBy).HasColumnName("CreatedBy");
        entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("CreatedOn");
        entity.Property(e => e.UpdatedBy).HasColumnName("UpdatedBy");
        entity.Property(e => e.UpdatedOn).HasColumnType("datetime").HasColumnName("UpdatedOn");
        entity.Property(e => e.ApprovedBy).HasColumnName("ApprovedBy");
        entity.Property(e => e.ApprovedOn).HasColumnType("datetime").HasColumnName("ApprovedOn");
        entity.Property(e => e.RejectedBy).HasColumnName("RejectedBy");
        entity.Property(e => e.RejectedOn).HasColumnType("datetime").HasColumnName("RejectedOn");
        entity.Property(e => e.RejectionReason).HasMaxLength(500).HasColumnName("RejectionReason");
        entity.Property(e => e.Remarks).HasMaxLength(500).HasColumnName("Remarks");
        entity.Property(e => e.DeclarationAccepted).HasColumnName("DeclarationAccepted");
        entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("IsActive");
        entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");
    }
}
