using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionApplicationService : INewConnectionApplicationService
{
    public const string StatusDraft = "Draft";
    public const string StatusSubmitted = "Submitted";
    public const string ActionSubmitted = "Submitted";

    private readonly ApplicationDbContext _db;

    public NewConnectionApplicationService(ApplicationDbContext db) => _db = db;

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

            await UpdateApplicationStatusInternalAsync(
                entity,
                StatusSubmitted,
                ActionSubmitted,
                request.IsPublicApplication ? "Application submitted by public user" : "Application submitted by consumer",
                request.ActionBy,
                request.ActionByName,
                request.ActionByRole,
                request.IpAddress,
                request.UserAgent,
                now,
                ct);

            await transaction.CommitAsync(ct);

            return (await GetDetailsQuery().FirstAsync(x => x.Id == entity.Id, ct))!;
        });
    }

    public async Task<NewConnectionApplicationDetailsDto?> TrackAsync(string applicationNo, string mobileNumber, CancellationToken ct = default)
    {
        var appNo = NormalizeRequired(applicationNo).ToUpperInvariant();
        var mobile = NormalizeMobile(mobileNumber);

        return await GetDetailsQuery()
            .FirstOrDefaultAsync(x => x.ApplicationNo == appNo && x.MobileNumber == mobile, ct);
    }

    public async Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetConsumerApplicationsAsync(string consumerNo, Guid? consumerUserId, CancellationToken ct = default)
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
                ApplicantName = x.ApplicantName,
                MobileNumber = x.MobileNumber,
                Sector = x.Sector,
                Block = x.Block,
                FlatNo = x.FlatNo,
                SubmittedOn = x.SubmittedOn,
                IsPublicApplication = x.IsPublicApplication
            })
            .ToListAsync(ct);
    }

    public async Task<NewConnectionApplicationDetailsDto?> GetConsumerApplicationDetailsAsync(long id, string consumerNo, Guid? consumerUserId, CancellationToken ct = default)
    {
        var normalizedConsumerNo = NormalizeRequired(consumerNo).ToUpperInvariant();

        var allowed = await _db.NewConnectionApplications
            .AsNoTracking()
            .AnyAsync(x => x.Id == id
                && !x.IsDeleted
                && (x.SubmittedByConsumerNo == normalizedConsumerNo
                    || (consumerUserId.HasValue && x.SubmittedByConsumerUserId == consumerUserId)), ct);

        return allowed
            ? await GetDetailsQuery().FirstOrDefaultAsync(x => x.Id == id, ct)
            : null;
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
        Guid? actionBy,
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
                ConnectionCategory = x.ConnectionCategory,
                ConnectionType = x.ConnectionType,
                FlatType = x.FlatType,
                PurposeOfConnection = x.PurposeOfConnection,
                PreviousConnectionYesNo = x.PreviousConnectionYesNo,
                OtherConnection = x.OtherConnection,
                SubmittedByConsumerNo = x.SubmittedByConsumerNo,
                SubmittedOn = x.SubmittedOn,
                IsPublicApplication = x.IsPublicApplication,
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
