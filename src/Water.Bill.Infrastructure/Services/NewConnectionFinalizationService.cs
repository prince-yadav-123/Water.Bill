using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionFinalizationService : INewConnectionFinalizationService
{
    public const string StatusApproved = "Approved";
    public const string StatusFinalConsumerCreated = "FinalConsumerCreated";
    public const string ActionFinalConsumerCreated = "FinalConsumerCreated";

    private readonly ApplicationDbContext _db;

    public NewConnectionFinalizationService(ApplicationDbContext db) => _db = db;

    public async Task<string> CreateFinalConsumerAsync(
        long applicationId,
        int? actionByUserId,
        string? actionByName,
        string? actionByRole,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct = default)
    {
        var application = await _db.NewConnectionApplications
            .FirstOrDefaultAsync(x => x.Id == applicationId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("New connection application not found.");

        if (!string.Equals(application.ApplicationStatus, StatusApproved, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Final consumer can be created only after final approval.");

        if (!string.IsNullOrWhiteSpace(application.FinalConsumerNo))
        {
            var existingConsumerExists = await _db.ConsumerDetailsMasters
                .AnyAsync(x => x.ConsNo == application.FinalConsumerNo, ct);

            if (existingConsumerExists)
                return application.FinalConsumerNo;

            throw new InvalidOperationException("Application already has a final consumer number, but consumer record was not found.");
        }

        var consumerNo = await GenerateConsumerNoAsync(application, ct);
        var now = DateTime.Now;
        var actorUserId = actionByUserId?.ToString();

        _db.ConsumerDetailsMasters.Add(new ConsumerDetailsMaster
        {
            ConsNo = consumerNo,
            ConsNm1 = Trim(application.ApplicantName, 150),
            ConsNm2 = Trim(application.FatherName, 30),
            ConTp = Trim(application.ConnectionCategory, 1),
            ConsCtg = Trim(application.ConnectionType, 10),
            FlatType = Trim(application.FlatType, 6),
            FlatNo = Trim(application.FlatNo, 20),
            BlkNo = Trim(application.Block, 15),
            Sector = Trim(application.Sector, 25),
            PlotSize = ToInt(application.PlotSize),
            PipeSize = ToInt(application.PipeSize),
            RegNo = Trim(application.RegNo, 8),
            ConnDt = application.ConnectionDate ?? now,
            EstiNo = Trim(application.EstimationNo, 10),
            EstiAmt = ToInt(application.EstimationAmount),
            Secu = ToInt(application.SecurityAmount),
            EstiDt = application.EstimationDate,
            Esti1Amt = 0,
            DevType = application.DevType,
            OldNewEn = "Y",
            MobNo = Trim(application.MobileNumber, 12),
            EmailId = Trim(application.EmailId, 50),
            ConsAddress = Trim(application.Address, 150),
            OtherCon = Trim(application.OtherConnection, 150),
            IssueOfficer = Trim(application.IssueOfficer, 50),
            PurposeCon = Trim(application.PurposeOfConnection, 100),
            Status = 1,
            Userid = Trim(actorUserId, 10),
            EntryDate = now,
            CessAmt = ToDouble(application.CessAmount),
            MaonthyCharges = ToDouble(application.MonthlyCharges),
            KhasraNo = Trim(application.KhasraNo, 20),
            VillgaeName = Trim(application.VillageName, 100),
            VillgaeId = application.VillageId,
            Rid = ToLong(application.Rid),
            Narration = Trim($"Created from New Connection Application {application.ApplicationNo}", 250)
        });

        _db.ConsumerDetailsTrans.Add(new ConsumerDetailsTran
        {
            ConsNo = consumerNo,
            CalDate = ToLegacyDate(application.ConnectionDate),
            AllotDate = ToLegacyDate(application.AllotmentDate),
            PosDate = ToLegacyDate(application.PossessionDate),
            CompDate = ToLegacyDate(application.ComplianceDate),
            SsiDate = ToLegacyDate(application.SsiDate),
            AffidavitYn = Trim(application.AffidavitYn, 2),
            Status = 1,
            Userid = Trim(actorUserId, 10),
            EntryDate = now,
            DevType = application.DevType
        });

        application.FinalConsumerNo = consumerNo;
        application.ApplicationStatus = StatusFinalConsumerCreated;
        application.UpdatedBy = actionByUserId;
        application.UpdatedOn = now;

        _db.NewConnectionApprovalHistories.Add(new NewConnectionApprovalHistory
        {
            ApplicationId = application.Id,
            ApplicationNo = application.ApplicationNo,
            FromStatus = StatusApproved,
            ToStatus = StatusFinalConsumerCreated,
            Action = ActionFinalConsumerCreated,
            Remarks = $"Consumer number generated: {consumerNo}",
            ActionBy = actionByUserId,
            ActionByName = Trim(actionByName, 100),
            ActionByRole = Trim(actionByRole, 100),
            ActionOn = now,
            IpAddress = Trim(ipAddress, 50),
            UserAgent = Trim(userAgent, 250),
            IsActive = true,
            IsDeleted = false
        });

        await _db.SaveChangesAsync(ct);
        return consumerNo;
    }

    private async Task<string> GenerateConsumerNoAsync(NewConnectionApplication application, CancellationToken ct)
    {
        var villageConsumerNo = await TryGenerateVillageConsumerNoAsync(application, ct);
        if (!string.IsNullOrWhiteSpace(villageConsumerNo))
            return villageConsumerNo;

        var devType = application.DevType.GetValueOrDefault(1);
        if (devType is < 1 or > 9)
            devType = 1;

        var existing = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => !application.DevType.HasValue || x.DevType == application.DevType)
            .Select(x => x.ConsNo)
            .ToListAsync(ct);

        // Old SQL used dbo.FIND_SECTOR for normal-sector numbering. Its body is not present here,
        // so this fallback preserves the common numeric DevType-prefixed format and guarantees uniqueness.
        var prefix = devType.ToString();
        var maxNumeric = existing
            .Where(x => IsAllDigits(x) && x.StartsWith(prefix, StringComparison.Ordinal))
            .Select(long.Parse)
            .DefaultIfEmpty(devType * 10000000L)
            .Max();

        for (var next = maxNumeric + 1; next < devType * 10000000L + 9999999L; next++)
        {
            var candidate = next.ToString();
            if (candidate.Length <= 10 && !existing.Contains(candidate)
                && !await _db.ConsumerDetailsMasters.AnyAsync(x => x.ConsNo == candidate, ct))
                return candidate;
        }

        throw new InvalidOperationException("Unable to generate a unique consumer number.");
    }

    private async Task<string?> TryGenerateVillageConsumerNoAsync(NewConnectionApplication application, CancellationToken ct)
    {
        if (!application.VillageId.HasValue)
            return null;

        var village = await _db.VillageDetails
            .AsNoTracking()
            .Where(x => x.VillageId == application.VillageId
                && (!application.DevType.HasValue || x.DevType == application.DevType)
                && (x.Status == null || x.Status == 1))
            .OrderBy(x => x.VillageNo)
            .FirstOrDefaultAsync(ct);

        var prefix = Trim(village?.VillageStr, 3);
        if (string.IsNullOrWhiteSpace(prefix))
            return null;

        var existing = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.VillgaeId == application.VillageId
                && (!application.DevType.HasValue || x.DevType == application.DevType)
                && x.ConsNo.StartsWith(prefix))
            .Select(x => x.ConsNo)
            .ToListAsync(ct);

        var maxSuffix = existing
            .Select(x => ParseSuffix(x, prefix.Length))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty(10000)
            .Max();

        for (var next = maxSuffix + 1; next < 9999999; next++)
        {
            var candidate = $"{prefix}{next}";
            if (candidate.Length <= 10 && !existing.Contains(candidate)
                && !await _db.ConsumerDetailsMasters.AnyAsync(x => x.ConsNo == candidate, ct))
                return candidate;
        }

        throw new InvalidOperationException("Unable to generate a unique village consumer number.");
    }

    private static int? ParseSuffix(string value, int startIndex)
    {
        if (value.Length <= startIndex)
            return null;

        var suffix = value[startIndex..];
        return IsAllDigits(suffix) && int.TryParse(suffix, out var parsed) ? parsed : null;
    }

    private static bool IsAllDigits(string value) => value.All(char.IsDigit);

    private static int? ToInt(decimal? value) => value.HasValue ? Convert.ToInt32(Math.Round(value.Value, 0)) : null;

    private static int ToInt(decimal value) => Convert.ToInt32(Math.Round(value, 0));

    private static double? ToDouble(decimal? value) => value.HasValue ? Convert.ToDouble(value.Value) : null;

    private static long? ToLong(string? value) => long.TryParse(value, out var parsed) ? parsed : null;

    private static string? ToLegacyDate(DateTime? value) => value?.ToString("dd-MM-yyyy");

    private static string? ToLegacyDate(DateOnly? value) => value?.ToString("dd-MM-yyyy");

    private static string? Trim(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
