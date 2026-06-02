using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Disconnections;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class DisconnectionManagementController : Controller
{
    private const string ModuleName = AppConstants.Modules.DisconnectionManagement;
    private readonly ApplicationDbContext _db;

    public DisconnectionManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/DisconnectionManagement")]
    [RequirePermission("Disconnection / Reconnection Management.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;

        var model = new DisconnectionManagementIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            Status = Normalize(status),
            FromDate = fromDate,
            ToDate = toDate,
            StatusOptions = DisconnectionCaseStatuses.Options(status)
        };

        model.HasConsumerSearch = HasAnySearch(model.ConsumerNo, model.ConsumerName, model.MobileNo, model.Sector, model.Block, model.PlotNo);
        model.Consumers = model.HasConsumerSearch ? await SearchConsumersAsync(model, ct) : [];
        model.Cases = await SearchCasesAsync(model, ct);
        return View(model);
    }

    [HttpGet("/DisconnectionManagement/Create")]
    [RequirePermission("Disconnection / Reconnection Management.add")]
    public async Task<IActionResult> Create(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Generate Disconnection Notice";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction(nameof(Index));

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
        if (consumer is null)
            return NotFound();

        var model = new DisconnectionCreateViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = ToConsumerSummary(consumer),
            OutstandingAmount = await CalculateOutstandingAsync(consumerNo, ct),
            ReasonOptions = DisconnectionReasons.Options(DisconnectionReasons.NonPayment)
        };

        return View(model);
    }

    [HttpPost("/DisconnectionManagement/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Disconnection / Reconnection Management.add")]
    public async Task<IActionResult> Create(DisconnectionCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Generate Disconnection Notice";
        ViewData["ActiveMenu"] = ModuleName;

        NormalizeCreateModel(model);
        model.ReasonOptions = DisconnectionReasons.Options(model.Reason);

        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
        if (consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number was not found.");
        model.Consumer = consumer is null ? null : ToConsumerSummary(consumer);

        ValidateCreateModel(model);

        var hasOpenCase = await _db.ConsumerDisconnectionCases.AnyAsync(x =>
            x.ConsumerNo == model.ConsumerNo &&
            !x.IsDeleted &&
            x.Status != DisconnectionCaseStatuses.Reconnected &&
            x.Status != DisconnectionCaseStatuses.Cancelled, ct);
        if (hasOpenCase)
            ModelState.AddModelError(nameof(model.ConsumerNo), "An active disconnection/reconnection case already exists for this consumer.");

        if (!ModelState.IsValid || consumer is null)
            return View(model);

        var now = DateTime.Now;
        var userId = CurrentUserId();
        var userName = CurrentUsername();
        var caseRow = new ConsumerDisconnectionCase
        {
            CaseNo = await GenerateCaseNoAsync(ct),
            ConsumerNo = model.ConsumerNo,
            CaseType = "Disconnection",
            Reason = model.Reason,
            Status = DisconnectionCaseStatuses.NoticeGenerated,
            NoticeDate = model.NoticeDate.Date,
            DueDate = model.DueDate?.Date,
            OutstandingAmount = model.OutstandingAmount,
            DisconnectionFee = model.DisconnectionFee,
            ReconnectionFee = model.ReconnectionFee,
            FieldOfficerName = model.FieldOfficerName,
            Remarks = model.Remarks,
            PreviousConsumerCategory = consumer.ConsCtg,
            PreviousStatus = consumer.Status,
            PreviousNewStatus = consumer.NewStatus,
            CreatedByUserId = userId,
            CreatedByName = userName,
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        };
        caseRow.Histories.Add(new ConsumerDisconnectionCaseHistory
        {
            ToStatus = DisconnectionCaseStatuses.NoticeGenerated,
            Action = "NoticeGenerated",
            Remarks = model.Remarks,
            ActionByUserId = userId,
            ActionByName = userName,
            ActionAt = now
        });

        _db.ConsumerDisconnectionCases.Add(caseRow);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = $"Disconnection notice {caseRow.CaseNo} generated successfully.";
        return RedirectToAction(nameof(Details), new { id = caseRow.Id });
    }

    [HttpGet("/DisconnectionManagement/Details/{id:long}")]
    [RequirePermission("Disconnection / Reconnection Management.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Disconnection Case Details";
        ViewData["ActiveMenu"] = ModuleName;

        var model = await BuildDetailsAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet("/DisconnectionManagement/Print/{id:long}")]
    [RequirePermission("Disconnection / Reconnection Management.print")]
    public async Task<IActionResult> Print(long id, CancellationToken ct)
    {
        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();

        ViewData["Title"] = "Print Disconnection Notice";
        return View(model);
    }

    [HttpPost("/DisconnectionManagement/Disconnect/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Disconnection / Reconnection Management.edit")]
    public async Task<IActionResult> Disconnect(long id, string? remarks, CancellationToken ct)
    {
        remarks = Normalize(remarks);
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Remarks are required to mark disconnection.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var caseRow = await _db.ConsumerDisconnectionCases.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (caseRow is null)
            return NotFound();
        if (caseRow.Status != DisconnectionCaseStatuses.NoticeGenerated)
        {
            TempData["ErrorMessage"] = "Only notice generated cases can be marked disconnected.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == caseRow.ConsumerNo, ct);
        if (consumer is null)
            return NotFound();

        caseRow.PreviousConsumerCategory ??= consumer.ConsCtg;
        caseRow.PreviousStatus ??= consumer.Status;
        caseRow.PreviousNewStatus ??= consumer.NewStatus;
        caseRow.DisconnectedOn = DateTime.Now;
        ChangeStatus(caseRow, DisconnectionCaseStatuses.Disconnected, "Disconnected", remarks);

        consumer.ConsCtg = "D";
        consumer.NewStatus = 0;
        consumer.ModifyDate = DateTime.Now;
        consumer.Userid = LegacyUserId();

        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Consumer marked as disconnected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/DisconnectionManagement/RequestReconnect/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Disconnection / Reconnection Management.edit")]
    public async Task<IActionResult> RequestReconnect(long id, string? remarks, CancellationToken ct)
    {
        remarks = Normalize(remarks);
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Remarks are required for reconnection request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var caseRow = await _db.ConsumerDisconnectionCases.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (caseRow is null)
            return NotFound();
        if (caseRow.Status != DisconnectionCaseStatuses.Disconnected)
        {
            TempData["ErrorMessage"] = "Only disconnected cases can be moved to reconnection request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        caseRow.ReconnectionRequestedOn = DateTime.Now;
        ChangeStatus(caseRow, DisconnectionCaseStatuses.ReconnectionRequested, "ReconnectionRequested", remarks);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Reconnection request recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/DisconnectionManagement/Reconnect/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Disconnection / Reconnection Management.edit")]
    public async Task<IActionResult> Reconnect(long id, string? remarks, CancellationToken ct)
    {
        remarks = Normalize(remarks);
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Remarks are required to mark reconnection.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var caseRow = await _db.ConsumerDisconnectionCases.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (caseRow is null)
            return NotFound();
        if (caseRow.Status != DisconnectionCaseStatuses.ReconnectionRequested && caseRow.Status != DisconnectionCaseStatuses.Disconnected)
        {
            TempData["ErrorMessage"] = "Only disconnected/reconnection requested cases can be reconnected.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == caseRow.ConsumerNo, ct);
        if (consumer is null)
            return NotFound();

        caseRow.ReconnectedOn = DateTime.Now;
        caseRow.IsActive = false;
        ChangeStatus(caseRow, DisconnectionCaseStatuses.Reconnected, "Reconnected", remarks);

        consumer.ConsCtg = string.IsNullOrWhiteSpace(caseRow.PreviousConsumerCategory) || caseRow.PreviousConsumerCategory == "D"
            ? "R"
            : caseRow.PreviousConsumerCategory;
        consumer.Status = caseRow.PreviousStatus ?? consumer.Status ?? 1;
        consumer.NewStatus = caseRow.PreviousNewStatus ?? 1;
        consumer.ModifyDate = DateTime.Now;
        consumer.Userid = LegacyUserId();

        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Consumer marked as reconnected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/DisconnectionManagement/Cancel/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Disconnection / Reconnection Management.delete")]
    public async Task<IActionResult> Cancel(long id, string? remarks, CancellationToken ct)
    {
        remarks = Normalize(remarks);
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Cancellation remarks are required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var caseRow = await _db.ConsumerDisconnectionCases.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (caseRow is null)
            return NotFound();
        if (caseRow.Status is DisconnectionCaseStatuses.Reconnected or DisconnectionCaseStatuses.Cancelled)
        {
            TempData["ErrorMessage"] = "Completed/cancelled cases cannot be cancelled again.";
            return RedirectToAction(nameof(Details), new { id });
        }

        caseRow.IsActive = false;
        ChangeStatus(caseRow, DisconnectionCaseStatuses.Cancelled, "Cancelled", remarks);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Case cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IReadOnlyList<DisconnectionConsumerSearchRowViewModel>> SearchConsumersAsync(DisconnectionManagementIndexViewModel model, CancellationToken ct)
    {
        var query = _db.ConsumerDetailsMasters.AsNoTracking().Where(x => x.Status == 1);
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

        var consumers = await query.OrderBy(x => x.ConsNo).Take(50).ToListAsync(ct);
        var consumerNos = consumers.Select(x => x.ConsNo).ToList();
        var activeCases = await _db.ConsumerDisconnectionCases.AsNoTracking()
            .Where(x => consumerNos.Contains(x.ConsumerNo) && !x.IsDeleted && x.IsActive)
            .ToDictionaryAsync(x => x.ConsumerNo, ct);

        var rows = new List<DisconnectionConsumerSearchRowViewModel>();
        foreach (var consumer in consumers)
        {
            activeCases.TryGetValue(consumer.ConsNo, out var activeCase);
            rows.Add(new DisconnectionConsumerSearchRowViewModel
            {
                ConsumerNo = consumer.ConsNo,
                ConsumerName = consumer.ConsNm1,
                MobileNo = consumer.MobNo,
                PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
                ConnectionType = consumer.ConTp,
                DevType = consumer.DevType,
                OutstandingAmount = await CalculateOutstandingAsync(consumer.ConsNo, ct),
                ActiveCaseNo = activeCase?.CaseNo
            });
        }
        return rows;
    }

    private async Task<IReadOnlyList<DisconnectionCaseRowViewModel>> SearchCasesAsync(DisconnectionManagementIndexViewModel model, CancellationToken ct)
    {
        var query =
            from row in _db.ConsumerDisconnectionCases.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on row.ConsumerNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where !row.IsDeleted
            select new { row, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
            query = query.Where(x => x.row.CaseNo.Contains(model.Search) || x.row.ConsumerNo.Contains(model.Search) || (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)));
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.row.ConsumerNo.StartsWith(model.ConsumerNo));
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
        if (!string.IsNullOrWhiteSpace(model.Status))
            query = query.Where(x => x.row.Status == model.Status);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.row.NoticeDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.row.NoticeDate < model.ToDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(x => x.row.NoticeDate)
            .ThenByDescending(x => x.row.Id)
            .Take(200)
            .Select(x => new DisconnectionCaseRowViewModel
            {
                Id = x.row.Id,
                CaseNo = x.row.CaseNo,
                ConsumerNo = x.row.ConsumerNo,
                ConsumerName = x.consumer != null ? x.consumer.ConsNm1 : null,
                MobileNo = x.consumer != null ? x.consumer.MobNo : null,
                PropertyNo = x.consumer != null ? x.consumer.Sector + "/" + x.consumer.BlkNo + "-" + x.consumer.FlatNo : null,
                Reason = x.row.Reason,
                Status = x.row.Status,
                NoticeDate = x.row.NoticeDate,
                DueDate = x.row.DueDate,
                OutstandingAmount = x.row.OutstandingAmount,
                DisconnectionFee = x.row.DisconnectionFee,
                ReconnectionFee = x.row.ReconnectionFee
            })
            .ToListAsync(ct);
    }

    private async Task<DisconnectionDetailsViewModel?> BuildDetailsAsync(long id, CancellationToken ct)
    {
        var row = await _db.ConsumerDisconnectionCases.AsNoTracking()
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (row is null)
            return null;

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == row.ConsumerNo, ct);
        return new DisconnectionDetailsViewModel
        {
            Id = row.Id,
            CaseNo = row.CaseNo,
            ConsumerNo = row.ConsumerNo,
            ConsumerName = consumer?.ConsNm1,
            MobileNo = consumer?.MobNo,
            PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
            Consumer = consumer is null ? null : ToConsumerSummary(consumer),
            Reason = row.Reason,
            Status = row.Status,
            NoticeDate = row.NoticeDate,
            DueDate = row.DueDate,
            OutstandingAmount = row.OutstandingAmount,
            DisconnectionFee = row.DisconnectionFee,
            ReconnectionFee = row.ReconnectionFee,
            FieldOfficerName = row.FieldOfficerName,
            Remarks = row.Remarks,
            ChallanNo = row.ChallanNo,
            DisconnectedOn = row.DisconnectedOn,
            ReconnectionRequestedOn = row.ReconnectionRequestedOn,
            ReconnectedOn = row.ReconnectedOn,
            CreatedByName = row.CreatedByName,
            CreatedAt = row.CreatedAt,
            CanDisconnect = row.Status == DisconnectionCaseStatuses.NoticeGenerated,
            CanRequestReconnect = row.Status == DisconnectionCaseStatuses.Disconnected,
            CanReconnect = row.Status == DisconnectionCaseStatuses.Disconnected || row.Status == DisconnectionCaseStatuses.ReconnectionRequested,
            CanCancel = row.Status != DisconnectionCaseStatuses.Reconnected && row.Status != DisconnectionCaseStatuses.Cancelled,
            Histories = row.Histories.OrderByDescending(x => x.ActionAt).Select(x => new DisconnectionHistoryViewModel
            {
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                Action = x.Action,
                Remarks = x.Remarks,
                ActionByName = x.ActionByName,
                ActionAt = x.ActionAt
            }).ToList()
        };
    }

    private async Task<decimal> CalculateOutstandingAsync(string consumerNo, CancellationToken ct)
    {
        var bills = await _db.JalPrintBillMasters.AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .Select(x => new { x.TotalBillAmt, x.DueAmt, x.MinTotalAmt, x.PaidAmt, x.PaidDate, x.PaidStatus })
            .Take(300)
            .ToListAsync(ct);

        return bills.Sum(x =>
        {
            var total = ToDecimal(x.TotalBillAmt ?? x.DueAmt ?? x.MinTotalAmt ?? 0);
            var paid = x.PaidDate.HasValue || x.PaidStatus == "Y"
                ? (x.PaidAmt.HasValue ? ToDecimal(x.PaidAmt.Value) : total)
                : ToDecimal(x.PaidAmt ?? 0);
            return Math.Max(0, total - paid);
        });
    }

    private void ChangeStatus(ConsumerDisconnectionCase row, string toStatus, string action, string? remarks)
    {
        var fromStatus = row.Status;
        row.Status = toStatus;
        row.UpdatedAt = DateTime.Now;
        row.UpdatedByUserId = CurrentUserId();
        row.UpdatedByName = CurrentUsername();
        row.Histories.Add(new ConsumerDisconnectionCaseHistory
        {
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Action = action,
            Remarks = remarks,
            ActionByUserId = CurrentUserId(),
            ActionByName = CurrentUsername(),
            ActionAt = DateTime.Now
        });
    }

    private void ValidateCreateModel(DisconnectionCreateViewModel model)
    {
        if (!DisconnectionReasons.Options().Any(x => x.Value == model.Reason))
            ModelState.AddModelError(nameof(model.Reason), "Invalid disconnection reason.");
        if (model.DueDate.HasValue && model.DueDate.Value.Date < model.NoticeDate.Date)
            ModelState.AddModelError(nameof(model.DueDate), "Due date cannot be before notice date.");
        if (model.Reason == DisconnectionReasons.NonPayment && model.OutstandingAmount <= 0)
            ModelState.AddModelError(nameof(model.OutstandingAmount), "Outstanding amount is required for non-payment disconnection.");
    }

    private async Task<string> GenerateCaseNoAsync(CancellationToken ct)
    {
        var prefix = $"DR{DateTime.Today:yyyyMM}";
        var existing = await _db.ConsumerDisconnectionCases.AsNoTracking()
            .Where(x => x.CaseNo.StartsWith(prefix))
            .Select(x => x.CaseNo)
            .ToListAsync(ct);
        var max = existing.Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0).DefaultIfEmpty(0).Max();
        string candidate;
        do
        {
            max++;
            candidate = $"{prefix}{max:D5}";
        }
        while (await _db.ConsumerDisconnectionCases.AnyAsync(x => x.CaseNo == candidate, ct));
        return candidate;
    }

    private static DisconnectionConsumerSummaryViewModel ToConsumerSummary(ConsumerDetailsMaster consumer)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            Address = !string.IsNullOrWhiteSpace(consumer.ConsAddress) ? consumer.ConsAddress : BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            DevType = consumer.DevType,
            Status = consumer.Status
        };

    private static void NormalizeCreateModel(DisconnectionCreateViewModel model)
    {
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant() ?? string.Empty;
        model.Reason = Normalize(model.Reason) ?? DisconnectionReasons.NonPayment;
        model.FieldOfficerName = Normalize(model.FieldOfficerName);
        model.Remarks = Normalize(model.Remarks);
    }

    private string CurrentUsername()
        => User.FindFirstValue(AppConstants.Claims.Username) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Admin";

    private string LegacyUserId()
    {
        var value = CurrentUsername();
        return value.Length > 10 ? value[..10] : value;
    }

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId), out var value) ? value : null;

    private static decimal ToDecimal(double value) => Convert.ToDecimal(value);

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasAnySearch(params string?[] values)
        => values.Any(x => !string.IsNullOrWhiteSpace(x));
}
