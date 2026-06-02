using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Adjustments;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConsumerAccountAdjustmentsController : Controller
{
    private const string ModuleName = "Consumer Account Adjustments";
    private readonly ApplicationDbContext _db;

    public ConsumerAccountAdjustmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/ConsumerAccountAdjustments")]
    [RequirePermission("Consumer Account Adjustments.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? status,
        string? adjustmentType,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;

        var model = new ConsumerAccountAdjustmentIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            Status = Normalize(status),
            AdjustmentType = Normalize(adjustmentType),
            FromDate = fromDate,
            ToDate = toDate,
            TypeOptions = ConsumerAdjustmentTypes.Options(adjustmentType)
        };

        model.Rows = await SearchAdjustmentsAsync(model, ct);
        return View(model);
    }

    [HttpGet("/ConsumerAccountAdjustments/Create")]
    [RequirePermission("Consumer Account Adjustments.add")]
    public async Task<IActionResult> Create(string? consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "New Adjustment";
        ViewData["ActiveMenu"] = ModuleName;

        var model = new ConsumerAccountAdjustmentCreateViewModel
        {
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty,
            TypeOptions = ConsumerAdjustmentTypes.Options()
        };

        await PopulateConsumerSummaryAsync(model, ct);
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo) && string.IsNullOrWhiteSpace(model.ConsumerName))
            ViewData["ConsumerLookupError"] = "Consumer number not found.";
        return View(model);
    }

    [HttpPost("/ConsumerAccountAdjustments/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Account Adjustments.add")]
    public async Task<IActionResult> Create(ConsumerAccountAdjustmentCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New Adjustment";
        ViewData["ActiveMenu"] = ModuleName;

        NormalizeCreateModel(model);
        await PopulateConsumerSummaryAsync(model, ct);
        ValidateCreateModel(model);

        if (!ModelState.IsValid)
        {
            model.TypeOptions = ConsumerAdjustmentTypes.Options(model.AdjustmentType);
            return View(model);
        }

        var userId = CurrentUserId();
        var userName = CurrentUserName();
        var adjustment = new ConsumerAccountAdjustment
        {
            AdjustmentNo = await GenerateAdjustmentNoAsync(ct),
            ConsumerNo = model.ConsumerNo,
            AdjustmentType = model.AdjustmentType,
            Amount = Math.Abs(model.Amount),
            EffectiveDate = model.EffectiveDate.Date,
            SourceBillNo = model.SourceBillNo,
            SourceChallanNo = model.SourceChallanNo,
            Remarks = model.Remarks,
            Status = ConsumerAdjustmentStatuses.Pending,
            CreatedByUserId = userId,
            CreatedByName = userName,
            CreatedAt = DateTime.Now,
            IsActive = true,
            IsDeleted = false
        };

        adjustment.Histories.Add(new ConsumerAccountAdjustmentHistory
        {
            FromStatus = null,
            ToStatus = ConsumerAdjustmentStatuses.Pending,
            Action = "Created",
            Remarks = model.Remarks,
            ActionByUserId = userId,
            ActionByName = userName,
            ActionAt = DateTime.Now
        });

        _db.ConsumerAccountAdjustments.Add(adjustment);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Adjustment created. It will be considered during future bill generation.";
        return RedirectToAction(nameof(Details), new { id = adjustment.Id });
    }

    [HttpGet("/ConsumerAccountAdjustments/Details/{id:long}")]
    [RequirePermission("Consumer Account Adjustments.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Adjustment Details";
        ViewData["ActiveMenu"] = ModuleName;

        var adjustment = await _db.ConsumerAccountAdjustments
            .AsNoTracking()
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (adjustment is null)
            return NotFound();

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == adjustment.ConsumerNo, ct);

        return View(ToDetailsModel(adjustment, consumer));
    }

    [HttpPost("/ConsumerAccountAdjustments/Cancel/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Account Adjustments.delete")]
    public async Task<IActionResult> Cancel(long id, string remarks, CancellationToken ct)
    {
        var adjustment = await _db.ConsumerAccountAdjustments
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (adjustment is null)
            return NotFound();
        if (adjustment.Status != ConsumerAdjustmentStatuses.Pending)
        {
            TempData["ErrorMessage"] = "Only pending adjustments can be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Cancellation remarks are required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        AddHistory(adjustment, ConsumerAdjustmentStatuses.Cancelled, "Cancelled", remarks);
        adjustment.Status = ConsumerAdjustmentStatuses.Cancelled;
        adjustment.IsActive = false;
        adjustment.UpdatedAt = DateTime.Now;
        adjustment.UpdatedByUserId = CurrentUserId();
        adjustment.UpdatedByName = CurrentUserName();

        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Adjustment cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/ConsumerAccountAdjustments/Reverse/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Account Adjustments.add")]
    public async Task<IActionResult> Reverse(long id, string remarks, CancellationToken ct)
    {
        var adjustment = await _db.ConsumerAccountAdjustments
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (adjustment is null)
            return NotFound();
        if (adjustment.Status != ConsumerAdjustmentStatuses.Applied)
        {
            TempData["ErrorMessage"] = "Only applied adjustments can be reversed.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Reversal remarks are required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var reverseType = ConsumerAdjustmentTypes.CreditTypes.Contains(adjustment.AdjustmentType)
            ? ConsumerAdjustmentTypes.OtherDebit
            : ConsumerAdjustmentTypes.OtherCredit;
        var userId = CurrentUserId();
        var userName = CurrentUserName();

        AddHistory(adjustment, ConsumerAdjustmentStatuses.Reversed, "Reversed", remarks);
        adjustment.Status = ConsumerAdjustmentStatuses.Reversed;
        adjustment.IsActive = false;
        adjustment.UpdatedAt = DateTime.Now;
        adjustment.UpdatedByUserId = userId;
        adjustment.UpdatedByName = userName;

        var reversal = new ConsumerAccountAdjustment
        {
            AdjustmentNo = await GenerateAdjustmentNoAsync(ct),
            ConsumerNo = adjustment.ConsumerNo,
            AdjustmentType = reverseType,
            Amount = adjustment.Amount,
            EffectiveDate = DateTime.Today,
            SourceBillNo = adjustment.AppliedBillNo,
            Remarks = $"Reversal of {adjustment.AdjustmentNo}. {remarks}",
            Status = ConsumerAdjustmentStatuses.Pending,
            ReversalOfAdjustmentId = adjustment.Id,
            CreatedByUserId = userId,
            CreatedByName = userName,
            CreatedAt = DateTime.Now,
            IsActive = true,
            IsDeleted = false
        };
        reversal.Histories.Add(new ConsumerAccountAdjustmentHistory
        {
            ToStatus = ConsumerAdjustmentStatuses.Pending,
            Action = "Created",
            Remarks = reversal.Remarks,
            ActionByUserId = userId,
            ActionByName = userName,
            ActionAt = DateTime.Now
        });

        _db.ConsumerAccountAdjustments.Add(reversal);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Applied adjustment reversed. A matching pending reversal entry has been created.";
        return RedirectToAction(nameof(Details), new { id = reversal.Id });
    }

    private async Task<IReadOnlyList<ConsumerAccountAdjustmentRowViewModel>> SearchAdjustmentsAsync(ConsumerAccountAdjustmentIndexViewModel model, CancellationToken ct)
    {
        var query = _db.ConsumerAccountAdjustments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var search = model.Search;
            query = query.Where(x => x.AdjustmentNo.Contains(search)
                || x.ConsumerNo.Contains(search)
                || (x.Remarks != null && x.Remarks.Contains(search))
                || (x.SourceBillNo != null && x.SourceBillNo.Contains(search))
                || (x.SourceChallanNo != null && x.SourceChallanNo.Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsumerNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.Status))
            query = query.Where(x => x.Status == model.Status);
        if (!string.IsNullOrWhiteSpace(model.AdjustmentType))
            query = query.Where(x => x.AdjustmentType == model.AdjustmentType);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.EffectiveDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.EffectiveDate <= model.ToDate.Value.Date);

        var rows = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(300)
            .ToListAsync(ct);

        var consumerNos = rows.Select(x => x.ConsumerNo).Distinct().ToList();
        var consumers = await _db.ConsumerDetailsMasters.AsNoTracking()
            .Where(x => consumerNos.Contains(x.ConsNo))
            .ToDictionaryAsync(x => x.ConsNo, ct);

        return rows.Select(x =>
        {
            consumers.TryGetValue(x.ConsumerNo, out var consumer);
            return new ConsumerAccountAdjustmentRowViewModel
            {
                Id = x.Id,
                AdjustmentNo = x.AdjustmentNo,
                ConsumerNo = x.ConsumerNo,
                ConsumerName = consumer?.ConsNm1,
                MobileNo = consumer?.MobNo,
                PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
                AdjustmentType = x.AdjustmentType,
                Amount = x.Amount,
                SignedAmount = ConsumerAdjustmentTypes.SignedAmount(x.AdjustmentType, x.Amount),
                EffectiveDate = x.EffectiveDate,
                Status = x.Status,
                AppliedBillNo = x.AppliedBillNo,
                AppliedOn = x.AppliedOn,
                CreatedAt = x.CreatedAt
            };
        }).ToList();
    }

    private async Task PopulateConsumerSummaryAsync(ConsumerAccountAdjustmentCreateViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.ConsumerNo))
            return;

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
        if (consumer is null)
            return;

        model.ConsumerName = consumer.ConsNm1;
        model.MobileNo = consumer.MobNo;
        model.PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo);
    }

    private void ValidateCreateModel(ConsumerAccountAdjustmentCreateViewModel model)
    {
        var validTypes = ConsumerAdjustmentTypes.Options().Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!validTypes.Contains(model.AdjustmentType))
            ModelState.AddModelError(nameof(model.AdjustmentType), "Invalid adjustment type.");
        if (string.IsNullOrWhiteSpace(model.ConsumerName))
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number not found.");
    }

    private async Task<string> GenerateAdjustmentNoAsync(CancellationToken ct)
    {
        var prefix = $"ADJ{DateTime.Today:yyyyMM}";
        var existing = await _db.ConsumerAccountAdjustments.AsNoTracking()
            .Where(x => x.AdjustmentNo.StartsWith(prefix))
            .Select(x => x.AdjustmentNo)
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
        while (await _db.ConsumerAccountAdjustments.AnyAsync(x => x.AdjustmentNo == candidate, ct));

        return candidate;
    }

    private ConsumerAccountAdjustmentDetailsViewModel ToDetailsModel(ConsumerAccountAdjustment adjustment, ConsumerDetailsMaster? consumer)
        => new()
        {
            Id = adjustment.Id,
            AdjustmentNo = adjustment.AdjustmentNo,
            ConsumerNo = adjustment.ConsumerNo,
            ConsumerName = consumer?.ConsNm1,
            MobileNo = consumer?.MobNo,
            PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
            AdjustmentType = adjustment.AdjustmentType,
            Amount = adjustment.Amount,
            SignedAmount = ConsumerAdjustmentTypes.SignedAmount(adjustment.AdjustmentType, adjustment.Amount),
            EffectiveDate = adjustment.EffectiveDate,
            SourceBillNo = adjustment.SourceBillNo,
            SourceChallanNo = adjustment.SourceChallanNo,
            Remarks = adjustment.Remarks,
            Status = adjustment.Status,
            AppliedBillNo = adjustment.AppliedBillNo,
            AppliedOn = adjustment.AppliedOn,
            CreatedAt = adjustment.CreatedAt,
            CreatedByName = adjustment.CreatedByName,
            Histories = adjustment.Histories
                .OrderByDescending(x => x.ActionAt)
                .Select(x => new ConsumerAccountAdjustmentHistoryViewModel
                {
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Action = x.Action,
                    Remarks = x.Remarks,
                    ActionByName = x.ActionByName,
                    ActionAt = x.ActionAt
                }).ToList()
        };

    private void AddHistory(ConsumerAccountAdjustment adjustment, string toStatus, string action, string? remarks)
    {
        adjustment.Histories.Add(new ConsumerAccountAdjustmentHistory
        {
            FromStatus = adjustment.Status,
            ToStatus = toStatus,
            Action = action,
            Remarks = remarks,
            ActionByUserId = CurrentUserId(),
            ActionByName = CurrentUserName(),
            ActionAt = DateTime.Now
        });
    }

    private static void NormalizeCreateModel(ConsumerAccountAdjustmentCreateViewModel model)
    {
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant() ?? string.Empty;
        model.AdjustmentType = Normalize(model.AdjustmentType) ?? string.Empty;
        model.SourceBillNo = Normalize(model.SourceBillNo);
        model.SourceChallanNo = Normalize(model.SourceChallanNo);
        model.Remarks = Normalize(model.Remarks);
    }

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId), out var value) ? value : null;

    private string CurrentUserName()
        => User.FindFirstValue(AppConstants.Claims.Username)
            ?? User.Identity?.Name
            ?? "System";

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
