using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
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

    public NewConnectionApplicationService(ApplicationDbContext db, IWorkflowService workflowService)
    {
        _db = db;
        _workflowService = workflowService;
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
                ConnectionType = NormalizeRequired(form.ConnectionType),
                FlatType = NormalizeRequired(form.FlatType),
                PurposeOfConnection = Normalize(form.PurposeOfConnection),
                PreviousConnectionYesNo = string.IsNullOrWhiteSpace(form.PreviousConnectionYesNo) ? "N" : form.PreviousConnectionYesNo.Trim().ToUpperInvariant(),
                OtherConnection = Normalize(form.OtherConnection),
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
                CanContinue = ContinuableStatuses.Contains(x.ApplicationStatus)
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
                CanContinue = ContinuableStatuses.Contains(x.ApplicationStatus)
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
            entity.ConnectionType = NormalizeRequired(form.ConnectionType);
            entity.FlatType = NormalizeRequired(form.FlatType);
            entity.PurposeOfConnection = Normalize(form.PurposeOfConnection);
            entity.PreviousConnectionYesNo = string.IsNullOrWhiteSpace(form.PreviousConnectionYesNo) ? "N" : form.PreviousConnectionYesNo.Trim().ToUpperInvariant();
            entity.OtherConnection = Normalize(form.OtherConnection);
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
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
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
            await transaction.CommitAsync(ct);
            return await EnrichWorkflowProgressAsync(
                (await GetDetailsQuery().FirstAsync(x => x.Id == entity.Id, ct))!,
                ct);
        });
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

        var taskByStage = tasks
            .GroupBy(x => x.StageId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(t => t.AssignedOn).First());

        details.WorkflowStages = stages.Select(stage =>
        {
            taskByStage.TryGetValue(stage.Id, out var task);
            var state = string.Equals(task?.Status, "Skipped", StringComparison.OrdinalIgnoreCase)
                ? "Upcoming"
                : stage.Id == instance.CurrentStageId && task?.Status == WorkflowService.TaskStatusPending
                ? "Current"
                : task?.Status ?? "Upcoming";

            return new NewConnectionWorkflowStageDto
            {
                StageOrder = stage.StageOrder,
                StageName = stage.StageName,
                State = state,
                Remarks = task?.Remarks,
                AssignedOn = task?.AssignedOn,
                ActionOn = task?.ActionOn
            };
        }).ToList();

        return details;
    }

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
}
