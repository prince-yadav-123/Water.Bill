using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionApplicationService : INewConnectionApplicationService
{
    public const string StatusOtpVerified = "OtpVerified";
    public const string StatusDraft = "Draft";
    public const string StatusPendingPayment = "PendingPayment";
    public const string StatusPaymentFailed = "PaymentFailed";
    public const string StatusFeePending = "FeePending";
    public const string StatusSubmitted = "Submitted";
    public const string StatusUnderReview = "UnderReview";
    public const string StatusCorrectionRequired = "CorrectionRequired";
    public const string StatusApproved = "Approved";
    public const string StatusRejected = "Rejected";
    public const string StatusFinalConsumerCreated = "FinalConsumerCreated";
    public const string ActionFeeCalculated = "FeeCalculated";
    public const string ActionPendingPayment = "PendingPayment";
    public const string ActionSubmitted = "Submitted";
    private static readonly string[] ContinuableStatuses =
    [
        StatusDraft,
        StatusPendingPayment,
        StatusPaymentFailed,
        StatusFeePending
    ];

    private readonly ApplicationDbContext _db;
    private readonly IWorkflowService _workflowService;
    private readonly ICommunicationService _communicationService;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IErrorLogService _errorLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NewConnectionApplicationService(
        ApplicationDbContext db,
        IWorkflowService workflowService,
        ICommunicationService communicationService,
        ITemplateRenderer templateRenderer,
        IErrorLogService errorLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _workflowService = workflowService;
        _communicationService = communicationService;
        _templateRenderer = templateRenderer;
        _errorLogService = errorLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<NewConnectionApplicationDetailsDto> SubmitAsync(NewConnectionSubmitRequest request, CancellationToken ct = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            var now = DateTime.Now;
            var form = request.Form;
            var applicationNo = Normalize(request.ApplicationNo)?.ToUpperInvariant() ?? GenerateApplicationNo(now);
            var entity = new NewConnectionApplication
            {
                ApplicationNo = applicationNo,
                ApplicationStatus = StatusDraft,
                IsPublicApplication = request.IsPublicApplication,
                ApplicantName = NormalizeRequired(form.ApplicantName),
                FatherName = Normalize(form.FatherName),
                MobileNumber = NormalizeMobile(form.MobileNumber),
                EmailId = Normalize(form.EmailId),
                Address = NormalizeRequired(form.Address),
                Sector = NormalizeRequired(form.Sector),
                Block = NormalizeRequired(form.Block),
                FlatNo = NormalizeRequired(form.FlatNo),
                PlotSize = form.PlotSize ?? 0,
                PipeSize = form.PipeSize,
                KhasraNo = Normalize(form.KhasraNo),
                VillageName = Normalize(form.VillageName),
                VillageId = form.VillageId,
                ConnectionCategory = NormalizeRequired(form.ConnectionCategory),
                ConnectionType = Normalize(form.ConnectionType),
                FlatType = NormalizeRequired(form.FlatType),
                PurposeOfConnection = Normalize(form.PurposeOfConnection),
                PreviousConnectionYesNo = string.IsNullOrWhiteSpace(form.PreviousConnectionYesNo) ? "N" : form.PreviousConnectionYesNo.Trim().ToUpperInvariant(),
                OtherConnection = string.Equals(form.PreviousConnectionYesNo?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
                    ? Normalize(form.OtherConnection)
                    : null,
                Rid = Normalize(form.Rid),
                DevType = form.DevType,
                Remarks = Normalize(form.Remarks),
                DeclarationAccepted = form.DeclarationAccepted,
                SubmittedByConsumerNo = Normalize(request.SubmittedByConsumerNo)?.ToUpperInvariant(),
                SubmittedByConsumerUserId = request.SubmittedByConsumerUserId,
                SubmittedOn = now,
                CreatedBy = request.ActionBy,
                CreatedOn = now,
                IsActive = true,
                IsDeleted = false
            };

            _db.NewConnectionApplications.Add(entity);
            await _db.SaveChangesAsync(ct);

            if (request.FeeQuote is not null)
            {
                var fee = request.FeeQuote;
                entity.SecurityAmount = fee.SecurityAmount;
                entity.EstimationAmount = fee.TotalAmount;
                _db.NewConnectionApplicationFees.Add(new NewConnectionApplicationFee
                {
                    ApplicationId = entity.Id,
                    ApplicationNo = entity.ApplicationNo,
                    FeeConfigurationId = fee.ConfigurationId,
                    ApplicationFee = fee.ApplicationFee,
                    ProcessingFee = fee.ProcessingFee,
                    SecurityAmount = fee.SecurityAmount,
                    MeterInstallationFee = fee.MeterInstallationFee,
                    OtherCharges = fee.OtherCharges,
                    TotalAmount = fee.TotalAmount,
                    PaymentStatus = "Pending",
                    CreatedOn = now
                });
            }

            foreach (var document in request.Documents.Where(x => !string.IsNullOrWhiteSpace(x.DocumentType)))
            {
                entity.Documents.Add(new NewConnectionApplicationDocument
                {
                    DocumentType = NormalizeRequired(document.DocumentType),
                    DocumentNo = Normalize(document.DocumentNo),
                    DocumentDate = document.DocumentDate,
                    FileName = Normalize(document.FileName),
                    FilePath = Normalize(document.FilePath),
                    ContentType = Normalize(document.ContentType),
                    FileSize = document.FileSize,
                    UploadedBy = request.ActionBy,
                    UploadedOn = now,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            await _db.SaveChangesAsync(ct);

            var targetStatus = Normalize(request.TargetStatus) ?? StatusSubmitted;
            var action = Normalize(request.StatusAction) ?? (targetStatus == StatusPendingPayment ? ActionPendingPayment : ActionSubmitted);
            var remarks = Normalize(request.StatusRemarks)
                ?? (targetStatus == StatusPendingPayment
                    ? "Application fee calculated and payment is pending."
                    : request.IsPublicApplication ? "Application submitted by public user" : "Application submitted by consumer");

            await UpdateApplicationStatusInternalAsync(
                entity,
                targetStatus,
                action,
                remarks,
                request.ActionBy,
                request.ActionByName,
                request.ActionByRole,
                request.IpAddress,
                request.UserAgent,
                now,
                ct);

            if (request.StartWorkflow)
            {
                await _workflowService.StartWorkflowAsync(
                    WorkflowService.ApplicationTypeNewConnection,
                    entity.Id,
                    entity.ApplicationNo,
                    entity.ApplicationStatus,
                    request.ActionBy,
                    request.ActionByName,
                    request.ActionByRole,
                    ct);
            }

            await transaction.CommitAsync(ct);

            await SendApplicantCommunicationAsync(
                CommunicationPurposes.NewConnectionSubmitted,
                entity,
                null,
                "Application submitted successfully.",
                remarks,
                entity.SubmittedByConsumerNo,
                now,
                ct,
                entity.EstimationAmount,
                request.ActionByName);

            return await EnrichWorkflowProgressAsync(
                (await GetDetailsQuery().FirstAsync(x => x.Id == entity.Id, ct))!,
                ct);
        });
    }

    public Task<NewConnectionApplicationDetailsDto> CompletePublicApplicationAsync(long id, string mobileNumber, NewConnectionSubmitRequest request, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        return CompleteExistingApplicationAsync(
            id,
            x => x.IsPublicApplication && x.MobileNumber == mobile,
            request,
            ct);
    }

    public Task<NewConnectionApplicationDetailsDto> CompleteConsumerApplicationAsync(long id, string consumerNo, int? consumerUserId, NewConnectionSubmitRequest request, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();
        return CompleteExistingApplicationAsync(
            id,
            x => x.SubmittedByConsumerNo == normalizedConsumerNo
                || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId),
            request,
            ct);
    }

    public Task<NewConnectionApplicationDetailsDto> CompletePublicPaymentAsync(long id, string mobileNumber, NewConnectionPaymentRequestDto request, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        return CompletePaymentAsync(
            id,
            x => x.IsPublicApplication && x.MobileNumber == mobile,
            request,
            ct);
    }

    public Task<NewConnectionApplicationDetailsDto> CompleteConsumerPaymentAsync(long id, string consumerNo, int? consumerUserId, NewConnectionPaymentRequestDto request, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();
        return CompletePaymentAsync(
            id,
            x => x.SubmittedByConsumerNo == normalizedConsumerNo
                || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId),
            request,
            ct);
    }

    public Task<NewConnectionApplicationDetailsDto> FinalizeGatewayPaymentAsync(long id, NewConnectionPaymentRequestDto request, CancellationToken ct = default)
        => CompletePaymentAsync(id, _ => true, request, ct);

    public async Task<NewConnectionApplicationDetailsDto?> TrackAsync(string applicationNo, string mobileNumber, CancellationToken ct = default)
    {
        var appNo = NormalizeRequired(applicationNo).ToUpperInvariant();
        var mobile = NormalizeMobile(mobileNumber);

        var details = await GetDetailsQuery()
            .FirstOrDefaultAsync(x => x.ApplicationNo == appNo && x.MobileNumber == mobile, ct);
        return details is null ? null : await EnrichWorkflowProgressAsync(details, ct);
    }

    public async Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetConsumerApplicationsAsync(string consumerNo, int? consumerUserId, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();

        return await _db.NewConnectionApplications
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)))
            .OrderByDescending(x => x.SubmittedOn ?? x.CreatedOn)
            .Select(x => new NewConnectionApplicationSummaryDto
            {
                Id = x.Id,
                ApplicationNo = x.ApplicationNo,
                ApplicationStatus = x.ApplicationStatus,
                FinalConsumerNo = x.FinalConsumerNo,
                ApplicantName = x.ApplicantName,
                MobileNumber = x.MobileNumber,
                Sector = x.Sector,
                Block = x.Block,
                FlatNo = x.FlatNo,
                SubmittedOn = x.SubmittedOn,
                IsPublicApplication = x.IsPublicApplication,
                TotalFee = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => (decimal?)f.TotalAmount)
                    .FirstOrDefault(),
                PaymentStatus = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => f.PaymentStatus)
                    .FirstOrDefault(),
                CanContinue = ContinuableStatuses.Contains(x.ApplicationStatus),
                CanResubmit = x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired,
                SentBackRemarks = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                               || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => h.Remarks)
                        .FirstOrDefault()
                    : null,
                SentBackAt = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => (DateTime?)h.ActionOn)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetPublicApplicationsByMobileAsync(string mobileNumber, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        return await _db.NewConnectionApplications
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsPublicApplication && x.MobileNumber == mobile)
            .OrderByDescending(x => x.SubmittedOn ?? x.CreatedOn)
            .Select(x => new NewConnectionApplicationSummaryDto
            {
                Id = x.Id,
                ApplicationNo = x.ApplicationNo,
                ApplicationStatus = x.ApplicationStatus,
                FinalConsumerNo = x.FinalConsumerNo,
                ApplicantName = x.ApplicantName,
                MobileNumber = x.MobileNumber,
                Sector = x.Sector,
                Block = x.Block,
                FlatNo = x.FlatNo,
                SubmittedOn = x.SubmittedOn,
                IsPublicApplication = x.IsPublicApplication,
                TotalFee = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => (decimal?)f.TotalAmount)
                    .FirstOrDefault(),
                PaymentStatus = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => f.PaymentStatus)
                    .FirstOrDefault(),
                CanContinue = ContinuableStatuses.Contains(x.ApplicationStatus),
                CanResubmit = x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired,
                SentBackRemarks = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                               || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => h.Remarks)
                        .FirstOrDefault()
                    : null,
                SentBackAt = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => (DateTime?)h.ActionOn)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(ct);
    }

    public async Task<NewConnectionApplicationDetailsDto?> GetPublicApplicationDetailsAsync(long id, string mobileNumber, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var allowed = await _db.NewConnectionApplications
            .AsNoTracking()
            .AnyAsync(x => x.Id == id && !x.IsDeleted && x.IsPublicApplication && x.MobileNumber == mobile, ct);

        if (!allowed)
            return null;

        var details = await GetDetailsQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        return details is null ? null : await EnrichWorkflowProgressAsync(details, ct);
    }

    public async Task<NewConnectionApplicationDetailsDto?> GetConsumerApplicationDetailsAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();

        var allowed = await _db.NewConnectionApplications
            .AsNoTracking()
            .AnyAsync(x => x.Id == id
                && !x.IsDeleted
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)), ct);

        if (!allowed)
            return null;

        var details = await GetDetailsQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        return details is null ? null : await EnrichWorkflowProgressAsync(details, ct);
    }

    public async Task<NewConnectionApplicationFormDto?> GetPublicContinuationFormAsync(long id, string mobileNumber, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var application = await _db.NewConnectionApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && x.IsPublicApplication
                && x.MobileNumber == mobile
                && ContinuableStatuses.Contains(x.ApplicationStatus), ct);

        return application is null ? null : MapToForm(application);
    }

    /// <summary>
    /// Loads the application form data for resubmission (SentBackToApplicant status — public applicant).
    /// Does NOT check ContinuableStatuses since resubmit has its own status requirement.
    /// </summary>
    public async Task<NewConnectionApplicationFormDto?> GetPublicResubmitFormAsync(long id, string mobileNumber, CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var application = await _db.NewConnectionApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && x.IsPublicApplication
                && x.MobileNumber == mobile
                && (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                    || x.ApplicationStatus == StatusCorrectionRequired), ct);

        return application is null ? null : MapToForm(application);
    }

    /// <summary>
    /// Loads the application form data for resubmission (SentBackToApplicant status — consumer portal).
    /// Does NOT check ContinuableStatuses since resubmit has its own status requirement.
    /// </summary>
    public async Task<NewConnectionApplicationFormDto?> GetConsumerResubmitFormAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();
        var application = await _db.NewConnectionApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                    || x.ApplicationStatus == StatusCorrectionRequired)
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)), ct);

        return application is null ? null : MapToForm(application);
    }

    public async Task<NewConnectionApplicationFormDto?> GetConsumerContinuationFormAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();
        var application = await _db.NewConnectionApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && ContinuableStatuses.Contains(x.ApplicationStatus)
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)), ct);

        return application is null ? null : MapToForm(application);
    }

    public async Task<NewConnectionFeeQuoteDto?> GetApplicationFeeAsync(long applicationId, CancellationToken ct = default)
    {
        return await _db.NewConnectionApplicationFees
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderByDescending(x => x.Id)
            .Select(x => new NewConnectionFeeQuoteDto
            {
                ConfigurationId = x.FeeConfigurationId,
                ApplicationFee = x.ApplicationFee,
                ProcessingFee = x.ProcessingFee,
                SecurityAmount = x.SecurityAmount,
                MeterInstallationFee = x.MeterInstallationFee,
                OtherCharges = x.OtherCharges,
                TotalAmount = x.TotalAmount,
                EffectiveFrom = x.CreatedOn
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateApplicationStatusAsync(NewConnectionStatusChangeRequest request, CancellationToken ct = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            var application = await _db.NewConnectionApplications
                .FirstOrDefaultAsync(x => x.Id == request.ApplicationId && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Application not found.");

            await UpdateApplicationStatusInternalAsync(
                application,
                request.ToStatus,
                request.Action,
                request.Remarks,
                request.ActionBy,
                request.ActionByName,
                request.ActionByRole,
                request.IpAddress,
                request.UserAgent,
                DateTime.Now,
                ct);

            await transaction.CommitAsync(ct);
        });
    }

    private async Task UpdateApplicationStatusInternalAsync(
        NewConnectionApplication application,
        string toStatus,
        string action,
        string? remarks,
        int? actionBy,
        string? actionByName,
        string? actionByRole,
        string? ipAddress,
        string? userAgent,
        DateTime actionOn,
        CancellationToken ct)
    {
        var fromStatus = application.ApplicationStatus;
        application.ApplicationStatus = NormalizeRequired(toStatus);
        application.UpdatedBy = actionBy;
        application.UpdatedOn = actionOn;

        _db.NewConnectionApprovalHistories.Add(new NewConnectionApprovalHistory
        {
            ApplicationId = application.Id,
            ApplicationNo = application.ApplicationNo,
            FromStatus = fromStatus,
            ToStatus = application.ApplicationStatus,
            Action = NormalizeRequired(action),
            Remarks = Normalize(remarks),
            ActionBy = actionBy,
            ActionByName = Normalize(actionByName),
            ActionByRole = Normalize(actionByRole),
            ActionOn = actionOn,
            IpAddress = Normalize(ipAddress),
            UserAgent = Normalize(userAgent),
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
    }

    private async Task<NewConnectionApplicationDetailsDto> CompleteExistingApplicationAsync(
        long id,
        Func<NewConnectionApplication, bool> ownershipPredicate,
        NewConnectionSubmitRequest request,
        CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            var now = DateTime.Now;
            var entity = await _db.NewConnectionApplications
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Application not found.");

            if (!ownershipPredicate(entity))
                throw new InvalidOperationException("Application not found.");

            if (!ContinuableStatuses.Contains(entity.ApplicationStatus))
                throw new InvalidOperationException("This application cannot be completed from its current status.");

            var form = request.Form;
            var originalStatus = entity.ApplicationStatus;
            entity.ApplicantName = NormalizeRequired(form.ApplicantName);
            entity.FatherName = Normalize(form.FatherName);
            entity.MobileNumber = NormalizeMobile(form.MobileNumber);
            entity.EmailId = Normalize(form.EmailId);
            entity.Address = NormalizeRequired(form.Address);
            entity.Sector = NormalizeRequired(form.Sector);
            entity.Block = NormalizeRequired(form.Block);
            entity.FlatNo = NormalizeRequired(form.FlatNo);
            entity.PlotSize = form.PlotSize ?? 0;
            entity.PipeSize = form.PipeSize;
            entity.KhasraNo = Normalize(form.KhasraNo);
            entity.VillageName = Normalize(form.VillageName);
            entity.VillageId = form.VillageId;
            entity.ConnectionCategory = NormalizeRequired(form.ConnectionCategory);
            entity.ConnectionType = Normalize(form.ConnectionType);
            entity.FlatType = NormalizeRequired(form.FlatType);
            entity.PurposeOfConnection = Normalize(form.PurposeOfConnection);
            entity.PreviousConnectionYesNo = string.IsNullOrWhiteSpace(form.PreviousConnectionYesNo) ? "N" : form.PreviousConnectionYesNo.Trim().ToUpperInvariant();
            entity.OtherConnection = string.Equals(form.PreviousConnectionYesNo?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)
                ? Normalize(form.OtherConnection)
                : null;
            entity.Rid = Normalize(form.Rid);
            entity.DevType = form.DevType;
            entity.Remarks = Normalize(form.Remarks);
            entity.DeclarationAccepted = form.DeclarationAccepted;
            entity.SubmittedByConsumerNo = Normalize(request.SubmittedByConsumerNo)?.ToUpperInvariant() ?? entity.SubmittedByConsumerNo;
            entity.SubmittedByConsumerUserId = request.SubmittedByConsumerUserId ?? entity.SubmittedByConsumerUserId;
            entity.SubmittedOn = now;

            foreach (var document in request.Documents.Where(x => !string.IsNullOrWhiteSpace(x.DocumentType)))
            {
                var documentType = NormalizeRequired(document.DocumentType);
                foreach (var existing in entity.Documents.Where(x => !x.IsDeleted && string.Equals(x.DocumentType, documentType, StringComparison.OrdinalIgnoreCase)))
                {
                    existing.IsActive = false;
                    existing.IsDeleted = true;
                }

                entity.Documents.Add(new NewConnectionApplicationDocument
                {
                    DocumentType = documentType,
                    DocumentNo = Normalize(document.DocumentNo),
                    DocumentDate = document.DocumentDate,
                    FileName = Normalize(document.FileName),
                    FilePath = Normalize(document.FilePath),
                    ContentType = Normalize(document.ContentType),
                    FileSize = document.FileSize,
                    UploadedBy = request.ActionBy,
                    UploadedOn = now,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            var targetStatus = Normalize(request.TargetStatus) ?? StatusSubmitted;
            var paymentSucceeded = string.Equals(targetStatus, StatusSubmitted, StringComparison.OrdinalIgnoreCase);

            var feeRecord = await _db.NewConnectionApplicationFees
                .FirstOrDefaultAsync(x => x.ApplicationId == entity.Id, ct);
            if (feeRecord is null)
            {
                var fee = request.FeeQuote ?? throw new InvalidOperationException("Fee configuration is not available for the selected connection details. Please contact support.");
                entity.SecurityAmount = fee.SecurityAmount;
                entity.EstimationAmount = fee.TotalAmount;
                _db.NewConnectionApplicationFees.Add(new NewConnectionApplicationFee
                {
                    ApplicationId = entity.Id,
                    ApplicationNo = entity.ApplicationNo,
                    FeeConfigurationId = fee.ConfigurationId,
                    ApplicationFee = fee.ApplicationFee,
                    ProcessingFee = fee.ProcessingFee,
                    SecurityAmount = fee.SecurityAmount,
                    MeterInstallationFee = fee.MeterInstallationFee,
                    OtherCharges = fee.OtherCharges,
                    TotalAmount = fee.TotalAmount,
                    PaymentStatus = paymentSucceeded ? "Success" : "Pending",
                    CreatedOn = now,
                    UpdatedOn = now
                });
            }
            else
            {
                entity.SecurityAmount = feeRecord.SecurityAmount;
                entity.EstimationAmount = feeRecord.TotalAmount;
                feeRecord.PaymentStatus = paymentSucceeded ? "Success" : "Pending";
                feeRecord.UpdatedOn = now;
            }

            await _db.SaveChangesAsync(ct);

            var remarks = Normalize(request.StatusRemarks)
                ?? (originalStatus == StatusDraft
                    ? "Application completed and submitted by user."
                    : "Application completed and submitted by public user. Payment treated as successful for current phase.");

            await UpdateApplicationStatusInternalAsync(
                entity,
                targetStatus,
                Normalize(request.StatusAction) ?? ActionSubmitted,
                remarks,
                request.ActionBy,
                request.ActionByName,
                request.ActionByRole,
                request.IpAddress,
                request.UserAgent,
                now,
                ct);

            var existingWorkflowInstances = await _db.ApplicationWorkflowInstances
                .Where(x => x.ApplicationType == WorkflowService.ApplicationTypeNewConnection
                    && x.ApplicationId == entity.Id
                    && !x.IsDeleted)
                .ToListAsync(ct);
            foreach (var instance in existingWorkflowInstances)
                instance.CurrentStatus = entity.ApplicationStatus;

            if (existingWorkflowInstances.Count > 0)
                await _db.SaveChangesAsync(ct);

            if (request.StartWorkflow && string.Equals(targetStatus, StatusSubmitted, StringComparison.OrdinalIgnoreCase))
            {
                await _workflowService.StartWorkflowAsync(
                    WorkflowService.ApplicationTypeNewConnection,
                    entity.Id,
                    entity.ApplicationNo,
                    entity.ApplicationStatus,
                    request.ActionBy,
                    request.ActionByName,
                    request.ActionByRole,
                    ct);
            }

            await transaction.CommitAsync(ct);
            return await EnrichWorkflowProgressAsync(
                (await GetDetailsQuery().FirstAsync(x => x.Id == entity.Id, ct))!,
                ct);
        });
    }

    private async Task<NewConnectionApplicationDetailsDto> CompletePaymentAsync(
        long id,
        Func<NewConnectionApplication, bool> ownershipPredicate,
        NewConnectionPaymentRequestDto request,
        CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is not null)
        {
            return await CompletePaymentCoreAsync(id, ownershipPredicate, request, ct, useLocalTransaction: false);
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
            await CompletePaymentCoreAsync(id, ownershipPredicate, request, ct, useLocalTransaction: true));
    }

    private async Task<NewConnectionApplicationDetailsDto> CompletePaymentCoreAsync(
        long id,
        Func<NewConnectionApplication, bool> ownershipPredicate,
        NewConnectionPaymentRequestDto request,
        CancellationToken ct,
        bool useLocalTransaction)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (useLocalTransaction)
                transaction = await _db.Database.BeginTransactionAsync(ct);

            var now = DateTime.Now;
            var entity = await _db.NewConnectionApplications
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException("Application not found.");

            if (!ownershipPredicate(entity))
                throw new InvalidOperationException("Application not found.");

            if (!ContinuableStatuses.Contains(entity.ApplicationStatus))
                throw new InvalidOperationException("This application cannot be completed from its current status.");

            var feeRecord = await _db.NewConnectionApplicationFees
                .FirstOrDefaultAsync(x => x.ApplicationId == entity.Id, ct);
            if (feeRecord is null)
            {
                var fee = request.FeeQuote ?? throw new InvalidOperationException("Fee configuration is not available for the selected connection details. Please contact support.");
                entity.SecurityAmount = fee.SecurityAmount;
                entity.EstimationAmount = fee.TotalAmount;
                _db.NewConnectionApplicationFees.Add(new NewConnectionApplicationFee
                {
                    ApplicationId = entity.Id,
                    ApplicationNo = entity.ApplicationNo,
                    FeeConfigurationId = fee.ConfigurationId,
                    ApplicationFee = fee.ApplicationFee,
                    ProcessingFee = fee.ProcessingFee,
                    SecurityAmount = fee.SecurityAmount,
                    MeterInstallationFee = fee.MeterInstallationFee,
                    OtherCharges = fee.OtherCharges,
                    TotalAmount = fee.TotalAmount,
                    PaymentStatus = "Success",
                    CreatedOn = now,
                    UpdatedOn = now
                });
            }
            else
            {
                entity.SecurityAmount = feeRecord.SecurityAmount;
                entity.EstimationAmount = feeRecord.TotalAmount;
                feeRecord.PaymentStatus = "Success";
                feeRecord.UpdatedOn = now;
            }

            entity.SubmittedOn ??= now;
            await UpdateApplicationStatusInternalAsync(
                entity,
                StatusSubmitted,
                ActionSubmitted,
                $"Application payment completed through {Normalize(request.PaymentMethod) ?? "selected payment option"} and submitted. Payment treated as successful for current phase.",
                request.ActionBy,
                request.ActionByName,
                request.ActionByRole,
                request.IpAddress,
                request.UserAgent,
                now,
                ct);

            var existingWorkflowInstances = await _db.ApplicationWorkflowInstances
                .Where(x => x.ApplicationType == WorkflowService.ApplicationTypeNewConnection
                    && x.ApplicationId == entity.Id
                    && !x.IsDeleted)
                .ToListAsync(ct);
            foreach (var instance in existingWorkflowInstances)
                instance.CurrentStatus = entity.ApplicationStatus;

            if (request.StartWorkflow)
            {
                await _workflowService.StartWorkflowAsync(
                    WorkflowService.ApplicationTypeNewConnection,
                    entity.Id,
                    entity.ApplicationNo,
                    entity.ApplicationStatus,
                    request.ActionBy,
                    request.ActionByName,
                    request.ActionByRole,
                    ct);
            }

            await _db.SaveChangesAsync(ct);
            if (transaction is not null)
                await transaction.CommitAsync(ct);

            return await EnrichWorkflowProgressAsync(
                (await GetDetailsQuery().FirstAsync(x => x.Id == entity.Id, ct))!,
                ct);
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private async Task<NewConnectionApplicationDetailsDto> EnrichWorkflowProgressAsync(NewConnectionApplicationDetailsDto details, CancellationToken ct)
    {
        var instance = await _db.ApplicationWorkflowInstances
            .AsNoTracking()
            .Where(x => x.ApplicationType == WorkflowService.ApplicationTypeNewConnection
                && x.ApplicationId == details.Id
                && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (instance is null)
            return details;

        var stages = await _db.WorkflowStages
            .AsNoTracking()
            .Where(x => x.WorkflowId == instance.WorkflowId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.StageOrder)
            .ToListAsync(ct);

        var tasks = await _db.ApplicationWorkflowTasks
            .AsNoTracking()
            .Where(x => x.WorkflowInstanceId == instance.Id && !x.IsDeleted)
            .OrderBy(x => x.AssignedOn)
            .ToListAsync(ct);

        var histories = await _db.ApplicationWorkflowHistories
            .AsNoTracking()
            .Where(x => x.WorkflowInstanceId == instance.Id)
            .OrderBy(x => x.ActionOn)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var taskByStage = tasks
            .GroupBy(x => x.StageId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(t => t.AssignedOn).First());

        var assignedUserIds = tasks.Where(x => x.AssignedUserId.HasValue).Select(x => x.AssignedUserId!.Value).Distinct().ToList();
        var assignedRoleIds = tasks.Where(x => x.AssignedRoleId.HasValue).Select(x => x.AssignedRoleId!.Value).Distinct().ToList();

        var userNames = assignedUserIds.Count > 0
            ? await _db.Appusers.AsNoTracking()
                .Where(x => assignedUserIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName ?? x.Username, ct)
            : new Dictionary<int, string>();

        var roleNames = assignedRoleIds.Count > 0
            ? await _db.Approles.AsNoTracking()
                .Where(x => assignedRoleIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct)
            : new Dictionary<int, string>();

        var historyByStage = histories
            .Where(x => x.StageId.HasValue)
            .GroupBy(x => x.StageId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(h => h.ActionOn).ThenByDescending(h => h.Id).First());

        var currentStageOrder = stages.FirstOrDefault(x => x.Id == instance.CurrentStageId)?.StageOrder ?? int.MaxValue;
        var isTerminal = IsTerminalStatus(details.ApplicationStatus);

        details.WorkflowStages = stages.Select(stage =>
        {
            taskByStage.TryGetValue(stage.Id, out var task);
            historyByStage.TryGetValue(stage.Id, out var history);

            var isPendingStage = task is not null && string.Equals(task.Status, WorkflowService.TaskStatusPending, StringComparison.OrdinalIgnoreCase);
            var state = ResolveStageState(
                stage,
                task,
                history,
                currentStageOrder,
                isTerminal);

            var assignedToName = ResolveAssignedToName(task, userNames, roleNames);
            var assignedToRole = ResolveAssignedToRole(task, roleNames);
            var actionType = isPendingStage ? null : ResolveStageActionType(task, history, state);
            var actionByName = isPendingStage ? null : history?.ActionByName;
            var actionByRole = isPendingStage ? null : history?.ActionByRole;
            var remarks = isPendingStage ? null : history?.Remarks ?? task?.Remarks;
            var actionOn = isPendingStage ? null : history?.ActionOn ?? task?.ActionOn;

            return new NewConnectionWorkflowStageDto
            {
                StageOrder = stage.StageOrder,
                StageName = stage.StageName,
                State = state,
                AssignedToName = assignedToName,
                AssignedToRole = assignedToRole,
                ActionType = actionType,
                ActionByName = actionByName,
                ActionByRole = actionByRole,
                Remarks = remarks,
                AssignedOn = task?.AssignedOn,
                ActionOn = actionOn
            };
        }).ToList();

        return details;
    }

    private static bool IsTerminalStatus(string? status)
        => string.Equals(status, StatusApproved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, StatusRejected, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, StatusFinalConsumerCreated, StringComparison.OrdinalIgnoreCase);

    private static string ResolveStageState(
        WorkflowStage stage,
        ApplicationWorkflowTask? task,
        ApplicationWorkflowHistory? history,
        int currentStageOrder,
        bool isTerminal)
    {
        if (isTerminal && stage.StageOrder > currentStageOrder)
            return "Not Required";

        if (stage.StageOrder < currentStageOrder)
            return "Completed";

        if (stage.StageOrder > currentStageOrder)
            return "Upcoming";

        var latestStatus = task?.Status;
        var latestAction = history?.Action;

        if (string.Equals(latestStatus, WorkflowService.TaskStatusRejected, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionRejected, StringComparison.OrdinalIgnoreCase))
            return "Rejected";

        if (string.Equals(latestStatus, WorkflowService.TaskStatusSentBackToApplicant, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestStatus, WorkflowService.TaskStatusCorrectionRequired, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionSendBackToApplicant, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionCorrectionRequired, StringComparison.OrdinalIgnoreCase))
            return "Action Required";

        if (string.Equals(latestAction, WorkflowService.ActionFinalApproval, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionFinalConsumerCreated, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestStatus, WorkflowService.TaskStatusApproved, StringComparison.OrdinalIgnoreCase))
            return stage.IsFinalStage || isTerminal ? "Final Approved" : "Completed";

        if (string.Equals(latestStatus, WorkflowService.TaskStatusApproved, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionAcceptMoveNext, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionMoveNext, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionApproved, StringComparison.OrdinalIgnoreCase))
            return "Completed";

        if (string.Equals(latestStatus, WorkflowService.TaskStatusPending, StringComparison.OrdinalIgnoreCase))
            return "Pending Action";

        if (string.Equals(latestAction, WorkflowService.ActionStageAssigned, StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestAction, WorkflowService.ActionWorkflowStarted, StringComparison.OrdinalIgnoreCase))
            return "Pending Action";

        return stage.IsFinalStage && isTerminal ? "Final Approved" : "Pending Action";
    }

    private static string? ResolveStageActionType(ApplicationWorkflowTask? task, ApplicationWorkflowHistory? history, string state)
    {
        var action = history?.Action;

        return action switch
        {
            WorkflowService.ActionWorkflowStarted => "Started",
            WorkflowService.ActionStageAssigned => "Assigned",
            WorkflowService.ActionAcceptMoveNext or WorkflowService.ActionMoveNext or WorkflowService.ActionForwardToUser => "Forwarded to Next Stage",
            WorkflowService.ActionApproved => "Stage Completed",
            WorkflowService.ActionFinalApproval or WorkflowService.ActionFinalConsumerCreated => "Final Approved",
            WorkflowService.ActionSendBackToApplicant or WorkflowService.ActionCorrectionRequired => "Sent Back for Correction",
            WorkflowService.ActionSendBackToPrevious => "Returned to Previous Stage",
            WorkflowService.ActionRejected => "Rejected",
            _ when string.Equals(task?.Status, WorkflowService.TaskStatusPending, StringComparison.OrdinalIgnoreCase) => state,
            _ => !string.IsNullOrWhiteSpace(action) ? action : state
        };
    }

    private static string? ResolveAssignedToName(
        ApplicationWorkflowTask? task,
        IReadOnlyDictionary<int, string> userNames,
        IReadOnlyDictionary<int, string> roleNames)
    {
        if (task?.AssignedUserId is int userId && userNames.TryGetValue(userId, out var userName))
            return userName;

        if (task?.AssignedRoleId is int roleId && roleNames.TryGetValue(roleId, out var roleName))
            return roleName;

        return null;
    }

    private static string? ResolveAssignedToRole(
        ApplicationWorkflowTask? task,
        IReadOnlyDictionary<int, string> roleNames)
        => task?.AssignedRoleId is int roleId && roleNames.TryGetValue(roleId, out var roleName)
            ? roleName
            : null;

    private IQueryable<NewConnectionApplicationDetailsDto> GetDetailsQuery()
        => _db.NewConnectionApplications
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new NewConnectionApplicationDetailsDto
            {
                Id = x.Id,
                ApplicationNo = x.ApplicationNo,
                ApplicationStatus = x.ApplicationStatus,
                ApplicantName = x.ApplicantName,
                FatherName = x.FatherName,
                MobileNumber = x.MobileNumber,
                EmailId = x.EmailId,
                Address = x.Address,
                Sector = x.Sector,
                Block = x.Block,
                FlatNo = x.FlatNo,
                PlotSize = x.PlotSize,
                PipeSize = x.PipeSize,
                KhasraNo = x.KhasraNo,
                VillageName = x.VillageName,
                VillageId = x.VillageId,
                ConnectionCategory = x.ConnectionCategory,
                ConnectionType = x.ConnectionType,
                FlatType = x.FlatType,
                PurposeOfConnection = x.PurposeOfConnection,
                PreviousConnectionYesNo = x.PreviousConnectionYesNo,
                OtherConnection = x.OtherConnection,
                Rid = x.Rid,
                DevType = x.DevType,
                Remarks = x.Remarks,
                DeclarationAccepted = x.DeclarationAccepted,
                SubmittedByConsumerNo = x.SubmittedByConsumerNo,
                SubmittedOn = x.SubmittedOn,
                IsPublicApplication = x.IsPublicApplication,
                TotalFee = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => (decimal?)f.TotalAmount)
                    .FirstOrDefault(),
                PaymentStatus = _db.NewConnectionApplicationFees
                    .Where(f => f.ApplicationId == x.Id)
                    .Select(f => f.PaymentStatus)
                    .FirstOrDefault(),
                CanContinue = ContinuableStatuses.Contains(x.ApplicationStatus),
                CanResubmit = x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired,
                SentBackRemarks = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                               || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => h.Remarks)
                        .FirstOrDefault()
                    : null,
                SentBackAt = (x.ApplicationStatus == WorkflowService.StatusSentBackToApplicant
                           || x.ApplicationStatus == StatusCorrectionRequired)
                    ? _db.NewConnectionApprovalHistories
                        .Where(h => h.ApplicationId == x.Id
                            && (h.Action == WorkflowService.ActionSendBackToApplicant
                                || h.Action == WorkflowService.ActionCorrectionRequired))
                        .OrderByDescending(h => h.ActionOn)
                        .Select(h => (DateTime?)h.ActionOn)
                        .FirstOrDefault()
                    : null,
                Documents = x.Documents
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.DocumentType)
                    .Select(d => new NewConnectionDocumentDto
                    {
                        Id = d.Id,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        FilePath = d.FilePath,
                        UploadedOn = d.UploadedOn
                    })
                    .ToList(),
                Timeline = x.ApprovalHistory
                    .Where(h => !h.IsDeleted)
                    .OrderBy(h => h.ActionOn)
                    .Select(h => new NewConnectionApprovalHistoryDto
                    {
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        Action = h.Action,
                        Remarks = h.Remarks,
                        ActionByName = h.ActionByName,
                        ActionByRole = h.ActionByRole,
                        ActionOn = h.ActionOn
                    })
                    .ToList()
            });

    private static NewConnectionApplicationFormDto MapToForm(NewConnectionApplication application)
        => new()
        {
            ApplicantName = application.ApplicantName,
            FatherName = application.FatherName,
            MobileNumber = application.MobileNumber,
            EmailId = application.EmailId,
            Address = application.Address,
            Sector = application.Sector,
            Block = application.Block,
            FlatNo = application.FlatNo,
            PlotSize = application.PlotSize,
            PipeSize = application.PipeSize,
            KhasraNo = application.KhasraNo,
            VillageName = application.VillageName,
            VillageId = application.VillageId,
            ConnectionCategory = application.ConnectionCategory,
            ConnectionType = application.ConnectionType,
            FlatType = application.FlatType,
            PurposeOfConnection = application.PurposeOfConnection,
            PreviousConnectionYesNo = application.PreviousConnectionYesNo,
            OtherConnection = application.OtherConnection,
            Rid = application.Rid,
            DevType = application.DevType,
            DeclarationAccepted = application.DeclarationAccepted,
            Remarks = application.Remarks
        };

    private static string GenerateApplicationNo(DateTime now) => $"NC{now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

    // ─────────────────────────────────────────────────────────────────────────
    // Resubmit (Send Back to Applicant correction flow)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<NewConnectionApplicationDetailsDto> ResubmitApplicationAsync(
        long id,
        string consumerNo,
        int? consumerUserId,
        string? applicantRemarks,
        IReadOnlyList<NewConnectionDocumentInputDto> newDocuments,
        CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();

        var application = await _db.NewConnectionApplications
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)), ct)
            ?? throw new InvalidOperationException("Application not found or access denied.");

        // Security: only the original consumer can resubmit
        if (application.SubmittedByConsumerNo != normalizedConsumerNo
            && !(consumerUserId.HasValue && application.SubmittedByConsumerUserId == consumerUserId))
            throw new InvalidOperationException("You do not have permission to resubmit this application.");

        // Only SentBackToApplicant or CorrectionRequired can be resubmitted
        if (application.ApplicationStatus != WorkflowService.StatusSentBackToApplicant
            && application.ApplicationStatus != StatusCorrectionRequired)
            throw new InvalidOperationException("This application cannot be resubmitted in its current status.");

        var now = DateTime.Now;

        // Find the workflow instance and the task that sent it back
        var instance = await _db.ApplicationWorkflowInstances
            .FirstOrDefaultAsync(x => x.ApplicationId == id
                && x.ApplicationType == WorkflowService.ApplicationTypeNewConnection
                && !x.IsDeleted, ct);

        if (instance is null)
            throw new InvalidOperationException("Workflow instance not found for this application.");

        // Find the task that sent it back (so we know which stage to return to)
        var sentBackTask = await _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Where(x => x.WorkflowInstanceId == instance.Id
                && !x.IsDeleted
                && (x.Status == WorkflowService.TaskStatusSentBackToApplicant
                    || x.Status == WorkflowService.TaskStatusCorrectionRequired))
            .OrderByDescending(x => x.ActionOn)
            .FirstOrDefaultAsync(ct);

        // Save new/replacement documents
        foreach (var doc in newDocuments)
        {
            var existing = application.Documents
                .FirstOrDefault(d => string.Equals(d.DocumentType, doc.DocumentType, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.FileName = doc.FileName;
                existing.FilePath = doc.FilePath;
                existing.ContentType = doc.ContentType;
                existing.FileSize = doc.FileSize;
                existing.UploadedOn = now;
            }
            else
            {
                _db.NewConnectionApplicationDocuments.Add(new NewConnectionApplicationDocument
                {
                    ApplicationId = id,
                    DocumentType = doc.DocumentType,
                    FileName = doc.FileName,
                    FilePath = doc.FilePath,
                    ContentType = doc.ContentType,
                    FileSize = doc.FileSize,
                    UploadedOn = now,
                    IsDeleted = false
                });
            }
        }

        // Update application status
        var fromStatus = application.ApplicationStatus;
        application.ApplicationStatus = StatusUnderReview;
        application.UpdatedOn = now;
        application.UpdatedBy = consumerUserId;
        if (!string.IsNullOrWhiteSpace(applicantRemarks))
            application.Remarks = applicantRemarks;

        // Approval history entry
        _db.NewConnectionApprovalHistories.Add(new NewConnectionApprovalHistory
        {
            ApplicationId = id,
            ApplicationNo = application.ApplicationNo,
            FromStatus = fromStatus,
            ToStatus = StatusUnderReview,
            Action = "ResubmittedByApplicant",
            Remarks = string.IsNullOrWhiteSpace(applicantRemarks)
                ? "Application resubmitted by applicant after correction."
                : $"Resubmitted by applicant: {applicantRemarks.Trim()}",
            ActionBy = consumerUserId,
            ActionByName = normalizedConsumerNo,
            ActionByRole = AppConstants.Roles.Consumer,
            ActionOn = now,
            IsActive = true,
            IsDeleted = false
        });

        // Restore workflow — create a new pending task at the stage that sent it back
        if (sentBackTask is not null)
        {
            instance.CurrentStageId = sentBackTask.StageId;
            instance.CurrentStatus = StatusUnderReview;
            instance.IsActive = true;
            instance.CompletedOn = null;

            _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
            {
                WorkflowInstanceId = instance.Id,
                ApplicationId = id,
                ApplicationNo = application.ApplicationNo,
                StageId = sentBackTask.StageId,
                AssignedDepartmentId = null,
                AssignedRoleId = sentBackTask.AssignedRoleId,
                AssignedUserId = sentBackTask.AssignedUserId,
                Status = WorkflowService.TaskStatusPending,
                AssignedOn = now,
                IsActive = true,
                IsDeleted = false
            });

            _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
            {
                WorkflowInstanceId = instance.Id,
                ApplicationId = id,
                ApplicationNo = application.ApplicationNo,
                StageId = sentBackTask.StageId,
                FromStatus = fromStatus,
                ToStatus = StatusUnderReview,
                Action = "ResubmittedByApplicant",
                Remarks = string.IsNullOrWhiteSpace(applicantRemarks)
                    ? "Applicant resubmitted after correction. Returned to review stage."
                    : $"Applicant resubmitted: {applicantRemarks.Trim()}",
                ActionBy = consumerUserId,
                ActionByName = normalizedConsumerNo,
                ActionByRole = AppConstants.Roles.Consumer,
                ActionOn = now
            });

            // In-App notification to the authority stage user(s)
            if (sentBackTask.Stage is not null)
            {
                var values = BuildWorkflowResubmittedTemplateValues(application.ApplicationNo, sentBackTask.Stage?.StageName, applicantRemarks, now);

                if (sentBackTask.AssignedUserId.HasValue)
                {
                    var notification = await BuildTemplatedInAppNotificationAsync(
                        userId: sentBackTask.AssignedUserId.Value,
                        values: values,
                        referenceId: id.ToString(),
                        referenceNo: application.ApplicationNo,
                        redirectUrl: $"/Approvals/Details/{sentBackTask.Id}",
                        createdAt: now,
                        ct: ct);
                    if (notification is not null)
                        _db.InAppNotifications.Add(notification);
                }
                else if (sentBackTask.AssignedRoleId.HasValue)
                {
                    var usersQ = _db.Appusers.AsNoTracking()
                        .Where(u => u.IsActive == true && !u.IsDeleted);
                    if (sentBackTask.AssignedRoleId.HasValue)
                        usersQ = usersQ.Where(u => u.RoleId == sentBackTask.AssignedRoleId.Value);
                    usersQ = ApplyDivisionRecipientFilter(usersQ, application.DevType);
                    var uids = await usersQ.Select(u => u.Id).ToListAsync(ct);
                    foreach (var uid in uids)
                    {
                        var notification = await BuildTemplatedInAppNotificationAsync(
                            userId: uid,
                            values: values,
                            referenceId: id.ToString(),
                            referenceNo: application.ApplicationNo,
                            redirectUrl: $"/Approvals/Details/{sentBackTask.Id}",
                            createdAt: now,
                            ct: ct);
                        if (notification is not null)
                            _db.InAppNotifications.Add(notification);
                    }
                }
            }
        }
        else
        {
            // No sent-back task found — restart from Stage 1
            instance.CurrentStatus = StatusUnderReview;
            instance.IsActive = true;
            instance.CompletedOn = null;
        }

        await _db.SaveChangesAsync(ct);

        await SendApplicantCommunicationAsync(
            CommunicationPurposes.NewConnectionResubmitted,
            application,
            sentBackTask?.Stage?.StageName,
            "Your updated application has been resubmitted successfully and is back under review.",
            applicantRemarks,
            application.FinalConsumerNo,
            now,
            ct);

        return await GetConsumerApplicationDetailsAsync(id, consumerNo, consumerUserId, ct)
            ?? throw new InvalidOperationException("Application not found after resubmission.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Resubmit — Public applicant (mobile OTP verified, no consumer login)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<NewConnectionApplicationDetailsDto> ResubmitPublicApplicationAsync(
        long id,
        string mobileNumber,
        string? applicantRemarks,
        IReadOnlyList<NewConnectionDocumentInputDto> newDocuments,
        CancellationToken ct = default)
    {
        var mobile = NormalizeMobile(mobileNumber);

        // Security: only the original public applicant (matched by mobile) can resubmit
        var application = await _db.NewConnectionApplications
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id
                && !x.IsDeleted
                && x.IsPublicApplication
                && x.MobileNumber == mobile, ct)
            ?? throw new InvalidOperationException("Application not found or access denied.");

        if (application.ApplicationStatus != WorkflowService.StatusSentBackToApplicant
            && application.ApplicationStatus != StatusCorrectionRequired)
            throw new InvalidOperationException("This application cannot be resubmitted in its current status.");

        var now = DateTime.Now;

        var instance = await _db.ApplicationWorkflowInstances
            .FirstOrDefaultAsync(x => x.ApplicationId == id
                && x.ApplicationType == WorkflowService.ApplicationTypeNewConnection
                && !x.IsDeleted, ct);

        if (instance is null)
            throw new InvalidOperationException("Workflow instance not found for this application.");

        var sentBackTask = await _db.ApplicationWorkflowTasks
            .Include(x => x.Stage)
            .Where(x => x.WorkflowInstanceId == instance.Id
                && !x.IsDeleted
                && (x.Status == WorkflowService.TaskStatusSentBackToApplicant
                    || x.Status == WorkflowService.TaskStatusCorrectionRequired))
            .OrderByDescending(x => x.ActionOn)
            .FirstOrDefaultAsync(ct);

        // Save new/replacement documents
        foreach (var doc in newDocuments)
        {
            var existing = application.Documents
                .FirstOrDefault(d => string.Equals(d.DocumentType, doc.DocumentType, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.FileName = doc.FileName;
                existing.FilePath = doc.FilePath;
                existing.ContentType = doc.ContentType;
                existing.FileSize = doc.FileSize;
                existing.UploadedOn = now;
            }
            else
            {
                _db.NewConnectionApplicationDocuments.Add(new NewConnectionApplicationDocument
                {
                    ApplicationId = id,
                    DocumentType = doc.DocumentType,
                    FileName = doc.FileName,
                    FilePath = doc.FilePath,
                    ContentType = doc.ContentType,
                    FileSize = doc.FileSize,
                    UploadedOn = now,
                    IsDeleted = false
                });
            }
        }

        var fromStatus = application.ApplicationStatus;
        application.ApplicationStatus = StatusUnderReview;
        application.UpdatedOn = now;
        if (!string.IsNullOrWhiteSpace(applicantRemarks))
            application.Remarks = applicantRemarks;

        _db.NewConnectionApprovalHistories.Add(new NewConnectionApprovalHistory
        {
            ApplicationId = id,
            ApplicationNo = application.ApplicationNo,
            FromStatus = fromStatus,
            ToStatus = StatusUnderReview,
            Action = "ResubmittedByApplicant",
            Remarks = string.IsNullOrWhiteSpace(applicantRemarks)
                ? "Application resubmitted by public applicant after correction."
                : $"Resubmitted by applicant: {applicantRemarks.Trim()}",
            ActionByName = mobile,
            ActionByRole = "PublicApplicant",
            ActionOn = now,
            IsActive = true,
            IsDeleted = false
        });

        if (sentBackTask is not null)
        {
            instance.CurrentStageId = sentBackTask.StageId;
            instance.CurrentStatus = StatusUnderReview;
            instance.IsActive = true;
            instance.CompletedOn = null;

            _db.ApplicationWorkflowTasks.Add(new ApplicationWorkflowTask
            {
                WorkflowInstanceId = instance.Id,
                ApplicationId = id,
                ApplicationNo = application.ApplicationNo,
                StageId = sentBackTask.StageId,
                AssignedDepartmentId = null,
                AssignedRoleId = sentBackTask.AssignedRoleId,
                AssignedUserId = sentBackTask.AssignedUserId,
                Status = WorkflowService.TaskStatusPending,
                AssignedOn = now,
                IsActive = true,
                IsDeleted = false
            });

            _db.ApplicationWorkflowHistories.Add(new ApplicationWorkflowHistory
            {
                WorkflowInstanceId = instance.Id,
                ApplicationId = id,
                ApplicationNo = application.ApplicationNo,
                StageId = sentBackTask.StageId,
                FromStatus = fromStatus,
                ToStatus = StatusUnderReview,
                Action = "ResubmittedByApplicant",
                Remarks = string.IsNullOrWhiteSpace(applicantRemarks)
                    ? "Public applicant resubmitted after correction."
                    : $"Resubmitted: {applicantRemarks.Trim()}",
                ActionByName = mobile,
                ActionByRole = "PublicApplicant",
                ActionOn = now
            });

            // InApp notification to authority stage user(s)
            if (sentBackTask.Stage is not null)
            {
                var values = BuildWorkflowResubmittedTemplateValues(application.ApplicationNo, sentBackTask.Stage?.StageName, applicantRemarks, now);

                if (sentBackTask.AssignedUserId.HasValue)
                {
                    var notification = await BuildTemplatedInAppNotificationAsync(
                        userId: sentBackTask.AssignedUserId.Value,
                        values: values,
                        referenceId: id.ToString(),
                        referenceNo: application.ApplicationNo,
                        redirectUrl: $"/Approvals/Details/{sentBackTask.Id}",
                        createdAt: now,
                        ct: ct);
                    if (notification is not null)
                        _db.InAppNotifications.Add(notification);
                }
                else if (sentBackTask.AssignedRoleId.HasValue)
                {
                    var usersQ = _db.Appusers.AsNoTracking()
                        .Where(u => u.IsActive == true && !u.IsDeleted);
                    if (sentBackTask.AssignedRoleId.HasValue)
                        usersQ = usersQ.Where(u => u.RoleId == sentBackTask.AssignedRoleId.Value);
                    usersQ = ApplyDivisionRecipientFilter(usersQ, application.DevType);
                    var uids = await usersQ.Select(u => u.Id).ToListAsync(ct);
                    foreach (var uid in uids)
                    {
                        var notification = await BuildTemplatedInAppNotificationAsync(
                            userId: uid,
                            values: values,
                            referenceId: id.ToString(),
                            referenceNo: application.ApplicationNo,
                            redirectUrl: $"/Approvals/Details/{sentBackTask.Id}",
                            createdAt: now,
                            ct: ct);
                        if (notification is not null)
                            _db.InAppNotifications.Add(notification);
                    }
                }
            }
        }
        else
        {
            instance.CurrentStatus = StatusUnderReview;
            instance.IsActive = true;
            instance.CompletedOn = null;
        }

        await _db.SaveChangesAsync(ct);

        await SendApplicantCommunicationAsync(
            CommunicationPurposes.NewConnectionResubmitted,
            application,
            sentBackTask?.Stage?.StageName,
            "Your updated application has been resubmitted successfully and is back under review.",
            applicantRemarks,
            application.FinalConsumerNo,
            now,
            ct);

        return await GetPublicApplicationDetailsAsync(id, mobileNumber, ct)
            ?? throw new InvalidOperationException("Application not found after resubmission.");
    }

    private async Task<InAppNotification?> BuildTemplatedInAppNotificationAsync(
        long userId,
        IReadOnlyDictionary<string, string?> values,
        string referenceId,
        string referenceNo,
        string redirectUrl,
        DateTime createdAt,
        CancellationToken ct)
    {
        var template = await _db.CommunicationTemplates
            .AsNoTracking()
            .Where(x => x.PurposeKey == CommunicationPurposes.WorkflowResubmitted
                && x.Channel == CommunicationChannels.InApp
                && x.IsActive
                && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (template is null)
        {
            await LogNotificationTemplateIssueAsync(
                CommunicationPurposes.WorkflowResubmitted,
                "Active in-app communication template was not found for workflow resubmission notification.",
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        if (string.IsNullOrWhiteSpace(template.Body))
        {
            await LogNotificationTemplateIssueAsync(
                CommunicationPurposes.WorkflowResubmitted,
                "In-app communication template body is empty for workflow resubmission notification.",
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        string title;
        string message;
        try
        {
            title = _templateRenderer.Render(template.Subject ?? template.TemplateName ?? CommunicationPurposes.WorkflowResubmitted, values);
            message = _templateRenderer.Render(template.Body, values);
        }
        catch (Exception ex)
        {
            await LogNotificationTemplateIssueAsync(
                CommunicationPurposes.WorkflowResubmitted,
                $"Failed to render workflow resubmission notification template. {ex.Message}",
                referenceId,
                referenceNo,
                ct);
            return null;
        }

        return new InAppNotification
        {
            UserType = "Internal",
            UserId = userId,
            Title = title,
            Message = message,
            PurposeKey = CommunicationPurposes.WorkflowResubmitted,
            ReferenceType = "NewConnectionApplication",
            ReferenceId = referenceId,
            ReferenceNo = referenceNo,
            RedirectUrl = redirectUrl,
            IsRead = false,
            CreatedAt = createdAt
        };
    }

    private async Task LogNotificationTemplateIssueAsync(
        string purposeKey,
        string message,
        string? referenceId,
        string? referenceNo,
        CancellationToken ct)
    {
        await _errorLogService.TryLogAsync(new ErrorLogWriteModel
        {
            ExceptionType = "NotificationTemplateIssue",
            Message = message,
            RequestPath = "NewConnectionApplicationService/InAppNotification",
            HttpMethod = "INTERNAL",
            QueryString = $"referenceId={referenceId}&referenceNo={referenceNo}",
            StatusCode = 500,
            PortalType = "Admin",
            ControllerName = "NewConnectionApplicationService",
            ActionName = purposeKey,
            TraceId = referenceNo ?? referenceId,
            IsHandled = true
        }, ct);
    }

    private static Dictionary<string, string?> BuildWorkflowResubmittedTemplateValues(
        string applicationNo,
        string? stageName,
        string? remarks,
        DateTime when)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApplicationNo"] = applicationNo,
            ["StageName"] = stageName,
            ["Remarks"] = Normalize(remarks),
            ["Date"] = when.ToString("dd MMM yyyy hh:mm tt")
        };

    private async Task SendApplicantCommunicationAsync(
        string purposeKey,
        NewConnectionApplication application,
        string? stageName,
        string? fallbackMessage,
        string? remarks,
        string? consumerNo,
        DateTime when,
        CancellationToken ct,
        decimal? amount = null,
        string? actionBy = null)
    {
        try
        {
            await _communicationService.SendAsync(
                purposeKey,
                BuildApplicantRecipient(application),
                BuildApplicantNotificationValues(
                    application,
                    stageName,
                    actionBy,
                    remarks ?? fallbackMessage,
                    consumerNo,
                    application.ApplicationStatus,
                    when,
                    amount),
                NotificationChannelOptions.For(
                    CommunicationChannels.InApp,
                    CommunicationChannels.Email,
                    CommunicationChannels.Sms,
                    CommunicationChannels.WhatsApp),
                "NewConnectionApplication",
                application.Id.ToString(),
                application.ApplicationNo,
                BuildApplicantPortalUrl(application),
                ct);
        }
        catch (Exception ex)
        {
            await _errorLogService.TryLogAsync(new ErrorLogWriteModel
            {
                CreatedAt = when,
                ExceptionType = "ApplicantNotificationDispatchException",
                Message = $"Applicant notification dispatch failed for purpose {purposeKey}. {ex.Message}",
                StackTrace = ex.ToString(),
                RequestPath = "NewConnectionApplicationService/SendApplicantCommunication",
                HttpMethod = "INTERNAL",
                QueryString = $"purposeKey={purposeKey}&applicationNo={application.ApplicationNo}",
                StatusCode = 500,
                PortalType = application.IsPublicApplication ? AppConstants.PortalTypes.Public : AppConstants.PortalTypes.Consumer,
                ControllerName = "NewConnectionApplicationService",
                ActionName = purposeKey,
                TraceId = application.ApplicationNo,
                IsHandled = true
            }, ct);
        }
    }

    private Dictionary<string, string?> BuildApplicantNotificationValues(
        NewConnectionApplication application,
        string? stageName,
        string? actionBy,
        string? remarks,
        string? consumerNo,
        string applicationStatus,
        DateTime when,
        decimal? amount = null)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["ApplicantName"] = application.ApplicantName,
            ["ConsumerName"] = application.ApplicantName,
            ["ApplicationNo"] = application.ApplicationNo,
            ["ApplicationNumber"] = application.ApplicationNo,
            ["ApplicationStatus"] = NormalizeApplicantStatusLabel(applicationStatus),
            ["StageName"] = stageName,
            ["ActionBy"] = actionBy,
            ["ActionDate"] = when.ToString("dd MMM yyyy hh:mm tt"),
            ["Date"] = when.ToString("dd MMM yyyy hh:mm tt"),
            ["Remarks"] = Normalize(remarks),
            ["ConsumerNumber"] = consumerNo,
            ["ConsumerNo"] = consumerNo,
            ["Amount"] = amount?.ToString("0.00"),
            ["PortalUrl"] = BuildApplicantPortalUrl(application)
        };

    private static string NormalizeApplicantStatusLabel(string? status)
        => status switch
        {
            StatusSubmitted => "Submitted",
            StatusPendingPayment => "Payment Pending",
            StatusPaymentFailed => "Payment Failed",
            StatusFeePending => "Fee Pending",
            StatusUnderReview => "Under Review",
            StatusCorrectionRequired or WorkflowService.StatusSentBackToApplicant => "Action Required",
            StatusApproved => "Approved",
            StatusRejected => "Rejected",
            StatusFinalConsumerCreated => "Final Consumer Created",
            _ when string.IsNullOrWhiteSpace(status) => "Updated",
            _ => status!
        };

    private static NotificationRecipient BuildApplicantRecipient(NewConnectionApplication application)
        => new()
        {
            Name = application.ApplicantName,
            Email = application.EmailId,
            Mobile = application.MobileNumber,
            UserType = application.SubmittedByConsumerUserId.HasValue ? AppConstants.Roles.Consumer : null,
            UserId = application.SubmittedByConsumerUserId.HasValue ? application.SubmittedByConsumerUserId : null
        };

    private string BuildApplicantPortalUrl(NewConnectionApplication application)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var path = application.IsPublicApplication || !application.SubmittedByConsumerUserId.HasValue
            ? $"/NewConnection/Track?applicationNo={Uri.EscapeDataString(application.ApplicationNo)}&mobileNumber={Uri.EscapeDataString(application.MobileNumber ?? string.Empty)}"
            : $"/Consumer/NewConnection/Details/{application.Id}";

        if (request is null)
            return path;

        return $"{request.Scheme}://{request.Host}{request.PathBase}{path}";
    }

    private static string NormalizeRequired(string? value)
        => Normalize(value) ?? throw new InvalidOperationException("Required value is missing.");

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string NormalizeMobile(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length > 10)
            digits = digits[^10..];

        if (digits.Length != 10)
            throw new InvalidOperationException("Enter a valid 10 digit mobile number.");

        return digits;
    }

    private IQueryable<Appuser> ApplyDivisionRecipientFilter(IQueryable<Appuser> query, int? applicationDevType)
    {
        if (!applicationDevType.HasValue)
            return query;

        return query.Where(x =>
            !x.DivisionDevType.HasValue
            || x.DivisionDevType == applicationDevType.Value
            || x.DivisionDevType == AppConstants.Divisions.AllDivision.DevType);
    }
}
