using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.MeterReadings;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class MeterReadingManagementController : Controller
{
    private const string ModuleName = AppConstants.Modules.MeterReadingManagement;
    private readonly ApplicationDbContext _db;

    public MeterReadingManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/MeterReadingManagement")]
    [RequirePermission("Meter Reading Management.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        string? meterStatus,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;

        var model = new MeterReadingIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            MeterStatus = Normalize(meterStatus),
            FromDate = fromDate,
            ToDate = toDate,
            StatusOptions = MeterReadingStatuses.Options(meterStatus)
        };

        model.HasConsumerSearch = HasAnySearch(model.Search, model.ConsumerNo, model.ConsumerName, model.MobileNo, model.Sector, model.Block, model.PlotNo);
        model.Consumers = model.HasConsumerSearch ? await SearchConsumersAsync(model, ct) : [];
        model.Readings = await SearchReadingsAsync(model, ct);

        return View(model);
    }

    [HttpGet("/MeterReadingManagement/Create")]
    [RequirePermission("Meter Reading Management.add")]
    public async Task<IActionResult> Create(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Record Meter Reading";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction(nameof(Index));

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
        if (consumer is null)
            return NotFound();

        var lastReading = await GetLastReadingAsync(consumerNo, null, ct);
        var model = new MeterReadingCreateViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = ToConsumerSummary(consumer),
            PreviousReading = lastReading?.CurrentReading,
            PeriodFrom = lastReading?.ReadingDate.AddDays(1),
            PeriodTo = DateTime.Today,
            MeterStatus = MeterReadingStatuses.Normal,
            StatusOptions = MeterReadingStatuses.Options(MeterReadingStatuses.Normal)
        };

        return View(model);
    }

    [HttpPost("/MeterReadingManagement/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Meter Reading Management.add")]
    public async Task<IActionResult> Create(MeterReadingCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Record Meter Reading";
        ViewData["ActiveMenu"] = ModuleName;

        NormalizeCreateModel(model);
        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
        if (consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number was not found.");

        model.Consumer = consumer is null ? null : ToConsumerSummary(consumer);
        model.StatusOptions = MeterReadingStatuses.Options(model.MeterStatus);

        var previous = await GetLastReadingAsync(model.ConsumerNo, null, ct);
        model.PreviousReading ??= previous?.CurrentReading;
        ValidateReading(model, previous);

        if (!ModelState.IsValid)
            return View(model);

        var now = DateTime.Now;
        var consumption = CalculateConsumption(model);
        var reading = new ConsumerMeterReading
        {
            ReadingNo = await GenerateReadingNoAsync(ct),
            ConsumerNo = model.ConsumerNo,
            ReadingDate = model.ReadingDate.Date,
            PeriodFrom = model.PeriodFrom?.Date,
            PeriodTo = model.PeriodTo?.Date,
            PreviousReading = model.PreviousReading,
            CurrentReading = model.CurrentReading,
            Consumption = consumption,
            MeterStatus = model.MeterStatus,
            MeterNo = model.MeterNo,
            Remarks = model.Remarks,
            Source = "Admin",
            RecordedByUserId = CurrentUserId(),
            RecordedByName = CurrentUsername(),
            RecordedAt = now,
            IsActive = true,
            IsDeleted = false
        };

        _db.ConsumerMeterReadings.Add(reading);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = $"Meter reading {reading.ReadingNo} saved successfully.";
        return RedirectToAction(nameof(Details), new { id = reading.Id });
    }

    [HttpGet("/MeterReadingManagement/Details/{id:long}")]
    [RequirePermission("Meter Reading Management.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Meter Reading Details";
        ViewData["ActiveMenu"] = ModuleName;

        var reading = await _db.ConsumerMeterReadings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (reading is null)
            return NotFound();

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == reading.ConsumerNo, ct);

        var history = await _db.ConsumerMeterReadings
            .AsNoTracking()
            .Where(x => x.ConsumerNo == reading.ConsumerNo && !x.IsDeleted && x.Id != reading.Id)
            .OrderByDescending(x => x.ReadingDate)
            .ThenByDescending(x => x.Id)
            .Take(10)
            .Select(x => new MeterReadingListRowViewModel
            {
                Id = x.Id,
                ReadingNo = x.ReadingNo,
                ConsumerNo = x.ConsumerNo,
                ReadingDate = x.ReadingDate,
                PreviousReading = x.PreviousReading,
                CurrentReading = x.CurrentReading,
                Consumption = x.Consumption,
                MeterStatus = x.MeterStatus,
                MeterNo = x.MeterNo
            })
            .ToListAsync(ct);

        return View(new MeterReadingDetailsViewModel
        {
            Id = reading.Id,
            ReadingNo = reading.ReadingNo,
            ConsumerNo = reading.ConsumerNo,
            ConsumerName = consumer?.ConsNm1,
            MobileNo = consumer?.MobNo,
            PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
            Consumer = consumer is null ? null : ToConsumerSummary(consumer),
            ReadingDate = reading.ReadingDate,
            PeriodFrom = reading.PeriodFrom,
            PeriodTo = reading.PeriodTo,
            PreviousReading = reading.PreviousReading,
            CurrentReading = reading.CurrentReading,
            Consumption = reading.Consumption,
            MeterStatus = reading.MeterStatus,
            MeterNo = reading.MeterNo,
            Remarks = reading.Remarks,
            Source = reading.Source,
            RecordedByName = reading.RecordedByName,
            RecordedAt = reading.RecordedAt,
            History = history
        });
    }

    private async Task<IReadOnlyList<MeterReadingConsumerSearchRowViewModel>> SearchConsumersAsync(MeterReadingIndexViewModel model, CancellationToken ct)
    {
        var query = _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.Status == 1);

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x => x.ConsNo.Contains(model.Search)
                || (x.ConsNm1 != null && x.ConsNm1.Contains(model.Search))
                || (x.MobNo != null && x.MobNo.Contains(model.Search))
                || (x.Sector != null && x.Sector.Contains(model.Search))
                || (x.BlkNo != null && x.BlkNo.Contains(model.Search))
                || (x.FlatNo != null && x.FlatNo.Contains(model.Search)));
        }

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.ConsNm1 != null && x.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.MobNo != null && x.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            query = query.Where(x => x.Sector != null && x.Sector.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            query = query.Where(x => x.BlkNo != null && x.BlkNo.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            query = query.Where(x => x.FlatNo != null && x.FlatNo.StartsWith(model.PlotNo));

        var consumers = await query
            .OrderBy(x => x.ConsNo)
            .Take(50)
            .ToListAsync(ct);
        var consumerNos = consumers.Select(x => x.ConsNo).ToList();
        var readingRows = await _db.ConsumerMeterReadings
            .AsNoTracking()
            .Where(x => consumerNos.Contains(x.ConsumerNo) && !x.IsDeleted)
            .OrderByDescending(x => x.ReadingDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
        var lastReadings = readingRows
            .GroupBy(x => x.ConsumerNo)
            .ToDictionary(x => x.Key, x => x.First());

        return consumers.Select(x =>
        {
            lastReadings.TryGetValue(x.ConsNo, out var last);
            return new MeterReadingConsumerSearchRowViewModel
            {
                ConsumerNo = x.ConsNo,
                ConsumerName = x.ConsNm1,
                MobileNo = x.MobNo,
                PropertyNo = BuildPropertyNo(x.Sector, x.BlkNo, x.FlatNo),
                ConnectionType = x.ConTp,
                DevType = x.DevType,
                LastReading = last?.CurrentReading,
                LastReadingDate = last?.ReadingDate
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<MeterReadingListRowViewModel>> SearchReadingsAsync(MeterReadingIndexViewModel model, CancellationToken ct)
    {
        var query =
            from reading in _db.ConsumerMeterReadings.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on reading.ConsumerNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where !reading.IsDeleted
            select new { reading, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x => x.reading.ReadingNo.Contains(model.Search)
                || x.reading.ConsumerNo.Contains(model.Search)
                || (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search))
                || (x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.Search)));
        }

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.reading.ConsumerNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            query = query.Where(x => x.consumer != null && x.consumer.Sector != null && x.consumer.Sector.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            query = query.Where(x => x.consumer != null && x.consumer.BlkNo != null && x.consumer.BlkNo.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            query = query.Where(x => x.consumer != null && x.consumer.FlatNo != null && x.consumer.FlatNo.StartsWith(model.PlotNo));
        if (!string.IsNullOrWhiteSpace(model.MeterStatus))
            query = query.Where(x => x.reading.MeterStatus == model.MeterStatus);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.reading.ReadingDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.reading.ReadingDate < model.ToDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(x => x.reading.ReadingDate)
            .ThenByDescending(x => x.reading.Id)
            .Take(200)
            .Select(x => new MeterReadingListRowViewModel
            {
                Id = x.reading.Id,
                ReadingNo = x.reading.ReadingNo,
                ConsumerNo = x.reading.ConsumerNo,
                ConsumerName = x.consumer != null ? x.consumer.ConsNm1 : null,
                MobileNo = x.consumer != null ? x.consumer.MobNo : null,
                PropertyNo = x.consumer != null ? x.consumer.Sector + "/" + x.consumer.BlkNo + "-" + x.consumer.FlatNo : null,
                ReadingDate = x.reading.ReadingDate,
                PreviousReading = x.reading.PreviousReading,
                CurrentReading = x.reading.CurrentReading,
                Consumption = x.reading.Consumption,
                MeterStatus = x.reading.MeterStatus,
                MeterNo = x.reading.MeterNo
            })
            .ToListAsync(ct);
    }

    private async Task<ConsumerMeterReading?> GetLastReadingAsync(string consumerNo, long? excludeId, CancellationToken ct)
    {
        var query = _db.ConsumerMeterReadings
            .AsNoTracking()
            .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query
            .OrderByDescending(x => x.ReadingDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private void ValidateReading(MeterReadingCreateViewModel model, ConsumerMeterReading? previous)
    {
        var validStatuses = MeterReadingStatuses.Options().Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!validStatuses.Contains(model.MeterStatus))
            ModelState.AddModelError(nameof(model.MeterStatus), "Invalid meter status.");

        if (model.PeriodFrom.HasValue && model.PeriodTo.HasValue && model.PeriodFrom.Value.Date > model.PeriodTo.Value.Date)
            ModelState.AddModelError(nameof(model.PeriodTo), "Period to date cannot be before period from date.");

        if (previous is not null && model.ReadingDate.Date < previous.ReadingDate.Date)
            ModelState.AddModelError(nameof(model.ReadingDate), $"Reading date cannot be before last reading date {previous.ReadingDate:dd MMM yyyy}.");

        if (model.MeterStatus == MeterReadingStatuses.Normal && model.PreviousReading.HasValue && model.CurrentReading < model.PreviousReading.Value)
            ModelState.AddModelError(nameof(model.CurrentReading), "Current reading cannot be less than previous reading for normal status.");

        if (model.MeterStatus != MeterReadingStatuses.Normal && string.IsNullOrWhiteSpace(model.Remarks))
            ModelState.AddModelError(nameof(model.Remarks), "Remarks are required for non-normal meter status.");
    }

    private static decimal CalculateConsumption(MeterReadingCreateViewModel model)
    {
        if (model.MeterStatus != MeterReadingStatuses.Normal && model.MeterStatus != MeterReadingStatuses.Average)
            return 0;

        return model.PreviousReading.HasValue
            ? Math.Max(0, model.CurrentReading - model.PreviousReading.Value)
            : model.CurrentReading;
    }

    private async Task<string> GenerateReadingNoAsync(CancellationToken ct)
    {
        var prefix = $"MR{DateTime.Today:yyyyMM}";
        var existing = await _db.ConsumerMeterReadings.AsNoTracking()
            .Where(x => x.ReadingNo.StartsWith(prefix))
            .Select(x => x.ReadingNo)
            .ToListAsync(ct);
        var max = existing
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        string candidate;
        do
        {
            max++;
            candidate = $"{prefix}{max:D5}";
        }
        while (await _db.ConsumerMeterReadings.AnyAsync(x => x.ReadingNo == candidate, ct));

        return candidate;
    }

    private static MeterReadingConsumerSummaryViewModel ToConsumerSummary(ConsumerDetailsMaster consumer)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            Address = !string.IsNullOrWhiteSpace(consumer.ConsAddress)
                ? consumer.ConsAddress
                : BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            PipeSize = consumer.PipeSize,
            DevType = consumer.DevType
        };

    private static void NormalizeCreateModel(MeterReadingCreateViewModel model)
    {
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant() ?? string.Empty;
        model.MeterStatus = Normalize(model.MeterStatus) ?? MeterReadingStatuses.Normal;
        model.MeterNo = Normalize(model.MeterNo);
        model.Remarks = Normalize(model.Remarks);
        model.Consumption = CalculateConsumption(model);
    }

    private string CurrentUsername()
        => User.FindFirstValue(AppConstants.Claims.Username)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.Identity?.Name
            ?? "Admin";

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId), out var value) ? value : null;

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasAnySearch(params string?[] values)
        => values.Any(x => !string.IsNullOrWhiteSpace(x));
}
