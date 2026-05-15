using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<A1jun30jun2023> A1jun30jun2023s { get; set; }

    public virtual DbSet<ApplicationStatus> ApplicationStatuses { get; set; }

    public virtual DbSet<Approle> Approles { get; set; }

    public virtual DbSet<Appuser> Appusers { get; set; }

    public virtual DbSet<AprilOfflinePayment> AprilOfflinePayments { get; set; }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<AuthenticationMst> AuthenticationMsts { get; set; }

    public virtual DbSet<BankCounterpayDetail> BankCounterpayDetails { get; set; }

    public virtual DbSet<BankCounterpayDetailTransLog> BankCounterpayDetailTransLogs { get; set; }

    public virtual DbSet<BankCounterpayPaymentStatus> BankCounterpayPaymentStatuses { get; set; }

    public virtual DbSet<BankMaster> BankMasters { get; set; }

    public virtual DbSet<BillMaster> BillMasters { get; set; }

    public virtual DbSet<BlockDetail> BlockDetails { get; set; }

    public virtual DbSet<CalcualtionMaster> CalcualtionMasters { get; set; }

    public virtual DbSet<CcavenueTransactiponStatus> CcavenueTransactiponStatuses { get; set; }

    public virtual DbSet<Challan> Challans { get; set; }

    public virtual DbSet<ChallanJan2025> ChallanJan2025s { get; set; }

    public virtual DbSet<ChallanLog> ChallanLogs { get; set; }

    public virtual DbSet<ChallanMisc> ChallanMiscs { get; set; }

    public virtual DbSet<Challanfeb25> Challanfeb25s { get; set; }

    public virtual DbSet<ConnectionTypeMst> ConnectionTypeMsts { get; set; }

    public virtual DbSet<ConsCessUpdation> ConsCessUpdations { get; set; }

    public virtual DbSet<ConsumerApplyNdc> ConsumerApplyNdcs { get; set; }

    public virtual DbSet<ConsumerChallanPdf> ConsumerChallanPdfs { get; set; }

    public virtual DbSet<ConsumerDetailsMaster> ConsumerDetailsMasters { get; set; }

    public virtual DbSet<ConsumerDetailsTran> ConsumerDetailsTrans { get; set; }

    public virtual DbSet<ConsumerEconsumerMail> ConsumerEconsumerMails { get; set; }

    public virtual DbSet<ConsumerEmail> ConsumerEmails { get; set; }

    public virtual DbSet<ConsumerEmailRecord> ConsumerEmailRecords { get; set; }

    public virtual DbSet<ConsumerEmailRecord20222023> ConsumerEmailRecord20222023s { get; set; }

    public virtual DbSet<ConsumerLogin> ConsumerLogins { get; set; }

    public virtual DbSet<ConsumerNodue> ConsumerNodues { get; set; }

    public virtual DbSet<ConsumerRidMobEmailInfo> ConsumerRidMobEmailInfos { get; set; }

    public virtual DbSet<ConsumerTransfer> ConsumerTransfers { get; set; }

    public virtual DbSet<ConsumerXmlDetail> ConsumerXmlDetails { get; set; }

    public virtual DbSet<Dec2024> Dec2024s { get; set; }

    public virtual DbSet<Demo5Aabadicsv> Demo5Aabadicsvs { get; set; }

    public virtual DbSet<DemoCommercialcsv> DemoCommercialcsvs { get; set; }

    public virtual DbSet<DemoGroupHousingcsv> DemoGroupHousingcsvs { get; set; }

    public virtual DbSet<DemoHousingcsv> DemoHousingcsvs { get; set; }

    public virtual DbSet<DemoIndustrycsv> DemoIndustrycsvs { get; set; }

    public virtual DbSet<DemoInstitutional> DemoInstitutionals { get; set; }

    public virtual DbSet<DemoResidentialcsv> DemoResidentialcsvs { get; set; }

    public virtual DbSet<DocumentMaster> DocumentMasters { get; set; }

    public virtual DbSet<ErpPaymentResponse> ErpPaymentResponses { get; set; }

    public virtual DbSet<JalBankMaster> JalBankMasters { get; set; }

    public virtual DbSet<JalIntrestRateMaster> JalIntrestRateMasters { get; set; }

    public virtual DbSet<JalMasterConsNo> JalMasterConsNos { get; set; }

    public virtual DbSet<JalNew> JalNews { get; set; }

    public virtual DbSet<JalPrintBillMaster> JalPrintBillMasters { get; set; }

    public virtual DbSet<JalPrintBillMasterAman341> JalPrintBillMasterAman341s { get; set; }

    public virtual DbSet<JalPrintBillMasterGstOct17> JalPrintBillMasterGstOct17s { get; set; }

    public virtual DbSet<JalPrintBillMasterLog> JalPrintBillMasterLogs { get; set; }

    public virtual DbSet<JalRateMaster> JalRateMasters { get; set; }

    public virtual DbSet<JalRateTran> JalRateTrans { get; set; }

    public virtual DbSet<JalnoidaBankpayMaster> JalnoidaBankpayMasters { get; set; }

    public virtual DbSet<JalnoidaBankpayTran> JalnoidaBankpayTrans { get; set; }

    public virtual DbSet<Jan2025> Jan2025s { get; set; }

    public virtual DbSet<June2024> June2024s { get; set; }

    public virtual DbSet<Loginattempt> Loginattempts { get; set; }

    public virtual DbSet<MasterApplicationDetail> MasterApplicationDetails { get; set; }

    public virtual DbSet<MasterApplicationDetailHistory> MasterApplicationDetailHistories { get; set; }

    public virtual DbSet<MasterConnectionTypeDetail> MasterConnectionTypeDetails { get; set; }

    public virtual DbSet<MasterConnectionTypeDetailsTran> MasterConnectionTypeDetailsTrans { get; set; }

    public virtual DbSet<MasterDeptDetail> MasterDeptDetails { get; set; }

    public virtual DbSet<MasterDocumentUpload> MasterDocumentUploads { get; set; }

    public virtual DbSet<MasterNocAmt> MasterNocAmts { get; set; }

    public virtual DbSet<MasterPhoneDetail> MasterPhoneDetails { get; set; }

    public virtual DbSet<MasterSheetAug23> MasterSheetAug23s { get; set; }

    public virtual DbSet<MasterSheetJuly23> MasterSheetJuly23s { get; set; }

    public virtual DbSet<MasterSheetOct23> MasterSheetOct23s { get; set; }

    public virtual DbSet<MasterSheetSep23> MasterSheetSep23s { get; set; }

    public virtual DbSet<MasterUserRole> MasterUserRoles { get; set; }

    public virtual DbSet<MasterVersionInfo> MasterVersionInfos { get; set; }

    public virtual DbSet<Menuitem> Menuitems { get; set; }

    public virtual DbSet<Menumaster> Menumasters { get; set; }

    public virtual DbSet<Menusubmaster> Menusubmasters { get; set; }

    public virtual DbSet<MerchantMst> MerchantMsts { get; set; }

    public virtual DbSet<MessageLog> MessageLogs { get; set; }

    public virtual DbSet<NameChangeDetail> NameChangeDetails { get; set; }

    public virtual DbSet<NchDocument> NchDocuments { get; set; }

    public virtual DbSet<NdcDocument> NdcDocuments { get; set; }

    public virtual DbSet<NewConsumerApplication> NewConsumerApplications { get; set; }

    public virtual DbSet<NoDueAmountMst> NoDueAmountMsts { get; set; }

    public virtual DbSet<Oct2024> Oct2024s { get; set; }

    public virtual DbSet<PaymentCollection> PaymentCollections { get; set; }

    public virtual DbSet<PaymentGatewayRegistration> PaymentGatewayRegistrations { get; set; }

    public virtual DbSet<PaymentGatewayRegistrationApp> PaymentGatewayRegistrationApps { get; set; }

    public virtual DbSet<PaymentModeMst> PaymentModeMsts { get; set; }

    public virtual DbSet<PaymentTypeMst> PaymentTypeMsts { get; set; }

    public virtual DbSet<PaymentUploadFile> PaymentUploadFiles { get; set; }

    public virtual DbSet<PimsAllotmentInfo2> PimsAllotmentInfo2s { get; set; }

    public virtual DbSet<PimsAllotmentInfo3> PimsAllotmentInfo3s { get; set; }

    public virtual DbSet<PimsAllotmentInfo4> PimsAllotmentInfo4s { get; set; }

    public virtual DbSet<PimsAllotmentInfo5> PimsAllotmentInfo5s { get; set; }

    public virtual DbSet<PimsAllotmentInfo6> PimsAllotmentInfo6s { get; set; }

    public virtual DbSet<PipeSizeMaster> PipeSizeMasters { get; set; }

    public virtual DbSet<ProvisionalDueCertificate> ProvisionalDueCertificates { get; set; }

    public virtual DbSet<ReceiptHeadLink> ReceiptHeadLinks { get; set; }

    public virtual DbSet<ReceiptHeadMaster> ReceiptHeadMasters { get; set; }

    public virtual DbSet<Rolepermission> Rolepermissions { get; set; }

    public virtual DbSet<SectorDetail> SectorDetails { get; set; }

    public virtual DbSet<Sep2024> Sep2024s { get; set; }

    public virtual DbSet<Securitysetting> Securitysettings { get; set; }

    public virtual DbSet<TbLoginInfo> TbLoginInfos { get; set; }

    public virtual DbSet<TransApplicationDocument> TransApplicationDocuments { get; set; }

    public virtual DbSet<UserAttendance> UserAttendances { get; set; }

    public virtual DbSet<UserMaster> UserMasters { get; set; }

    public virtual DbSet<Usermaster1> Usermasters1 { get; set; }

    public virtual DbSet<Usersession> Usersessions { get; set; }

    public virtual DbSet<VillageDetail> VillageDetails { get; set; }

    public virtual DbSet<ZeroConsumerjune> ZeroConsumerjunes { get; set; }

    public virtual DbSet<_01nov30nov20231> _01nov30nov20231s { get; set; }

    public virtual DbSet<_26oct30oct20231> _26oct30oct20231s { get; set; }

    public virtual DbSet<_31jan2025> _31jan2025s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
