using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Adjustments;
using Water.Bill.API.Models.Billing;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class BulkBillGenerationController : Controller
{
    private readonly ApplicationDbContext _db;

    public BulkBillGenerationController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/BulkBillGeneration")]
    [RequirePermission("Bulk Bill Generation.view")]
    public async Task<IActionResult> Index(BulkBillGenerationViewModel request, string? preview, CancellationToken ct)
    {
        ViewData["Title"] = "Bulk Bill Generation";
        ViewData["ActiveMenu"] = "Bulk Bill Generation";

        NormalizeRequest(request);
        request.DivisionOptions = BuildDivisionOptions(request.DevType);

        if (!string.IsNullOrWhiteSpace(preview))
        {
            request.HasPreview = true;
            request.PreviewRows = await BuildPreviewAsync(request, ct);
        }

        return View(request);
    }

    [HttpPost("/BulkBillGeneration/Generate")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Bulk Bill Generation.add")]
    public async Task<IActionResult> Generate(BulkBillGenerationViewModel request, CancellationToken ct)
    {
        ViewData["Title"] = "Bulk Bill Generation";
        ViewData["ActiveMenu"] = "Bulk Bill Generation";

        NormalizeRequest(request);
        request.DivisionOptions = BuildDivisionOptions(request.DevType);
        request.HasPreview = true;
        request.PreviewRows = await BuildPreviewAsync(request, ct);

        if (request.PreviewRows.Count == 0)
        {
            TempData["ErrorMessage"] = "No eligible consumers found for bill generation.";
            return View("Index", request);
        }

        var eligibleRows = request.PreviewRows.Where(x => !x.AlreadyGenerated && string.IsNullOrWhiteSpace(x.Warning)).ToList();
        if (eligibleRows.Count == 0)
        {
            TempData["ErrorMessage"] = "All selected consumers are already billed for this period or missing rate details.";
            return View("Index", request);
        }

        var consumers = await LoadConsumersAsync(request, ct);
        var consumerLookup = consumers.ToDictionary(x => x.ConsNo, StringComparer.OrdinalIgnoreCase);
        var generatedBillNos = new List<string>();

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            foreach (var row in eligibleRows)
            {
                if (!consumerLookup.TryGetValue(row.ConsumerNo, out var consumer))
                    continue;

                var billNo = await GenerateBillNoAsync(request.BillDate, consumer.DevType, ct);
                var challanContent = BuildChallanContent(billNo, consumer, row, request);
                var beforeDateText = request.BillDueDate.ToString("dd-MMM-yyyy");
                var afterDateText = $"{request.BillDueDate.AddDays(1):dd-MMM-yyyy} onwards";
                var afterDueAmount = row.AfterDueAmount;
                var totalAfterRebateBeforeLastPaid = Math.Max(0, row.MinimumTotalAmount + row.CessAmount + row.ArrearAmount + row.ArrearInterest + row.GstAmount);
                var billAfterSepAmount = Math.Max(0, totalAfterRebateBeforeLastPaid);
                var divType = AppConstants.Divisions.Find(consumer.DevType)?.Code ?? consumer.DevType?.ToString();
                var userId = CurrentUsername();
                if (userId.Length > 10)
                    userId = userId[..10];
                string blankText = " ";
                string? nullText = null;
                DateTime? nullDate = null;
                double? nullAmount = null;
                int? nullInt = null;

                await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO [jal_print_bill_master]
([BILL_NO], [CONS_NO], [BILL_DATE], [BILL_DUE_DATE], [BILL_DATE_FROM], [BILL_DATE_TO],
 [MIN_RATE], [MIN_TOTAL_AMT], [BILL_REBATE_PER], [BILL_REBATE_AMT], [CESS_AMT],
 [AREAR], [AREAR_TEXT], [AREAR_INT], [AREAR_INT_TEXT], [LAST_BILL_EXTRA],
 [TOTAL_BILL_AMT], [BEFORE_DATE], [AFTER_DATE], [AFTER_DATE_AMT], [Div_type],
 [STATUS], [ENTRY_DATE], [Due_date], [due_amt], [paid_date], [paid_amt], [diff],
 [PAID_STATUS], [new_record], [update_record], [bill_after_sep_amt], [adv_amt],
 [PRINT_STATUS], [OLD_RATE], [BILL_TYPE], [LAST_PAID_AMT], [BILL_COUNT],
 [SCHEME_ID], [BILL_PERCENTAGE], [USERID], [DEV_TYPE], [PAYMENT_TYPE], [CHALLAN_NO],
 [BANK_CODE], [Challan_Content], [Rid], [Part_Amt])
VALUES
({billNo}, {consumer.ConsNo}, {request.BillDate}, {request.BillDueDate}, {request.BillDateFrom}, {request.BillDateTo},
 {row.MonthlyRate}, {row.MinimumTotalAmount}, {row.AppliedRebatePercent}, {row.RebateAmount}, {row.CessAmount},
 {row.ArrearAmount}, {blankText}, {row.ArrearInterest}, {blankText}, {row.GstAmount},
 {row.TotalBillAmount}, {beforeDateText}, {afterDateText}, {afterDueAmount}, {divType},
 {"1"}, {DateTime.Now}, {request.BillDueDate}, {row.TotalBillAmount}, {nullDate}, {0}, {0},
 {"N"}, {"Y"}, {nullText}, {billAfterSepAmount}, {0},
 {1}, {row.OldRateFlag}, {request.BillType}, {0}, {1},
 {request.SchemeId}, {request.BillPercentage}, {userId}, {consumer.DevType}, {nullInt}, {nullText},
 {nullText}, {challanContent}, {consumer.Rid?.ToString()}, {nullAmount});", ct);

                var adjustments = await _db.ConsumerAccountAdjustments
                    .Where(x => !x.IsDeleted
                        && x.IsActive
                        && x.ConsumerNo == row.ConsumerNo
                        && x.Status == ConsumerAdjustmentStatuses.Pending
                        && x.EffectiveDate <= request.BillDateTo)
                    .ToListAsync(ct);

                foreach (var adjustment in adjustments)
                {
                    adjustment.Status = ConsumerAdjustmentStatuses.Applied;
                    adjustment.IsActive = false;
                    adjustment.AppliedBillNo = billNo;
                    adjustment.AppliedOn = DateTime.Now;
                    adjustment.UpdatedAt = DateTime.Now;
                    adjustment.UpdatedByName = userId;
                    adjustment.Histories.Add(new ConsumerAccountAdjustmentHistory
                    {
                        FromStatus = ConsumerAdjustmentStatuses.Pending,
                        ToStatus = ConsumerAdjustmentStatuses.Applied,
                        Action = "AppliedToBill",
                        Remarks = $"Applied during bulk bill generation. Bill No: {billNo}",
                        ActionByName = userId,
                        ActionAt = DateTime.Now
                    });
                }

                generatedBillNos.Add(billNo);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        request.Result = new BulkBillGenerationResultViewModel
        {
            RequestedCount = request.PreviewRows.Count,
            GeneratedCount = generatedBillNos.Count,
            SkippedCount = request.PreviewRows.Count - generatedBillNos.Count,
            GeneratedBillNos = generatedBillNos
        };

        TempData["SuccessMessage"] = $"{generatedBillNos.Count} bill(s) generated successfully.";
        request.PreviewRows = await BuildPreviewAsync(request, ct);
        return View("Index", request);
    }

    private async Task<IReadOnlyList<BulkBillPreviewRowViewModel>> BuildPreviewAsync(BulkBillGenerationViewModel request, CancellationToken ct)
    {
        var consumers = await LoadConsumersAsync(request, ct);
        if (consumers.Count == 0)
            return [];

        var consumerNos = consumers.Select(x => x.ConsNo).ToList();
        var existingBills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo != null
                && consumerNos.Contains(x.ConsNo)
                && x.BillDateFrom == request.BillDateFrom
                && x.BillDateTo == request.BillDateTo
                && x.Status == "1")
            .Select(x => x.ConsNo!)
            .ToListAsync(ct);
        var existingSet = existingBills.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unpaidBills = request.IncludeArrears
            ? await _db.JalPrintBillMasters
                .AsNoTracking()
                .Where(x => x.ConsNo != null
                    && consumerNos.Contains(x.ConsNo)
                    && x.Status == "1"
                    && x.PaidDate == null
                    && x.BillDateTo < request.BillDateFrom)
                .ToListAsync(ct)
            : [];

        var unpaidLookup = unpaidBills
            .GroupBy(x => x.ConsNo!)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(b => Math.Max(0, (b.TotalBillAmt ?? b.DueAmt ?? 0) - (b.PaidAmt ?? 0))),
                StringComparer.OrdinalIgnoreCase);

        var pendingAdjustments = await _db.ConsumerAccountAdjustments
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.IsActive
                && consumerNos.Contains(x.ConsumerNo)
                && x.Status == ConsumerAdjustmentStatuses.Pending
                && x.EffectiveDate <= request.BillDateTo)
            .ToListAsync(ct);

        var adjustmentLookup = pendingAdjustments
            .GroupBy(x => x.ConsumerNo)
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    Debit = x.Where(a => !ConsumerAdjustmentTypes.CreditTypes.Contains(a.AdjustmentType)).Sum(a => (double)a.Amount),
                    Credit = x.Where(a => ConsumerAdjustmentTypes.CreditTypes.Contains(a.AdjustmentType)).Sum(a => (double)a.Amount)
                },
                StringComparer.OrdinalIgnoreCase);

        var rateEffectiveDate = request.UseLegacyRateDate ? request.LegacyRateDate : request.BillDateFrom;
        var rates = await _db.JalRateTrans
            .AsNoTracking()
            .Include(x => x.IdNavigation)
            .Where(x => (x.Status == null || x.Status == "1")
                && (x.EffFrom == null || x.EffFrom <= rateEffectiveDate)
                && (x.EffTo == null || x.EffTo >= rateEffectiveDate))
            .ToListAsync(ct);

        return consumers.Select(consumer =>
        {
            var rate = FindRate(consumer, rates);
            var monthlyRate = ResolveMonthlyRate(consumer, rate);
            var months = CountBillMonths(request.BillDateFrom, request.BillDateTo);
            var minimumTotal = monthlyRate * months;
            var cess = request.IncludeCess ? minimumTotal * ((rate?.CessRate ?? 0) / 100d) : 0;
            var arrear = unpaidLookup.TryGetValue(consumer.ConsNo, out var due) ? due : 0;
            var adjustments = adjustmentLookup.TryGetValue(consumer.ConsNo, out var adj)
                ? adj
                : new { Debit = 0d, Credit = 0d };
            var arrearInterest = CalculateArrearInterest(arrear, request.BillDateFrom, request.BillDateTo, request.ArrearInterestPercent);
            var gst = request.IncludeGst && request.GstPercent > 0 ? minimumTotal * (request.GstPercent / 100d) : 0;
            var rebatePercent = ResolveRebatePercent(request, minimumTotal, adjustments.Credit);
            var rebate = rebatePercent > 0 ? minimumTotal * (rebatePercent / 100d) : 0;
            var total = Math.Max(0, minimumTotal + cess + arrear + arrearInterest + gst + adjustments.Debit - rebate - adjustments.Credit);
            var afterDueAmount = Math.Round(total + (total * (request.AfterDueSurchargePercent / 100d)), 2);

            return new BulkBillPreviewRowViewModel
            {
                ConsumerNo = consumer.ConsNo,
                ConsumerName = consumer.ConsNm1,
                PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
                DevType = consumer.DevType,
                ConnectionType = consumer.ConTp,
                Category = consumer.ConsCtg,
                PipeSize = consumer.PipeSize,
                PlotSize = consumer.PlotSize,
                MonthlyRate = monthlyRate,
                MinimumTotalAmount = minimumTotal,
                CessAmount = cess,
                ArrearAmount = arrear,
                ArrearInterest = arrearInterest,
                DebitAdjustmentAmount = adjustments.Debit,
                CreditAdjustmentAmount = adjustments.Credit,
                RebateAmount = rebate,
                AppliedRebatePercent = rebatePercent,
                GstAmount = gst,
                TotalBillAmount = total,
                AfterDueAmount = afterDueAmount,
                BillPercentage = request.BillPercentage,
                SchemeId = request.SchemeId,
                OldRateFlag = request.UseLegacyRateDate ? 1 : 0,
                AlreadyGenerated = existingSet.Contains(consumer.ConsNo),
                RateSource = rate is null
                    ? "Consumer monthly charge/rate"
                    : request.UseLegacyRateDate
                        ? $"Rate Master ({request.LegacyRateDate:dd-MMM-yyyy})"
                        : "Rate Master",
                Warning = monthlyRate <= 0 ? "Rate not configured" : null
            };
        }).ToList();
    }

    private async Task<List<ConsumerDetailsMaster>> LoadConsumersAsync(BulkBillGenerationViewModel request, CancellationToken ct)
    {
        var query = _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.Status == 1
                && x.PipeSize.GetValueOrDefault() > 0
                && x.PlotSize.GetValueOrDefault() > 0
                && (x.OldNewUp == null || x.OldNewUp == "Y")
                && x.ConTp != null
                && x.ConsCtg != null
                && x.FlatType != null
                && x.ConsCtg != "S"
                && x.ConsCtg != "M"
                && x.ConsCtg != "D"
                && x.ConsCtg != "CC");

        if (!string.IsNullOrWhiteSpace(request.ConsumerNo))
            query = query.Where(x => x.ConsNo.StartsWith(request.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(request.Sector))
            query = query.Where(x => x.Sector != null && x.Sector.StartsWith(request.Sector));
        if (!string.IsNullOrWhiteSpace(request.Block))
            query = query.Where(x => x.BlkNo != null && x.BlkNo.StartsWith(request.Block));
        if (request.DevType.HasValue && request.DevType.Value != AppConstants.Divisions.AllDivision.DevType)
            query = query.Where(x => x.DevType == request.DevType.Value);

        return await query
            .OrderBy(x => x.DevType)
            .ThenBy(x => x.Sector)
            .ThenBy(x => x.BlkNo)
            .ThenBy(x => x.FlatNo)
            .Take(Math.Clamp(request.MaxConsumers, 1, 1000))
            .ToListAsync(ct);
    }

    private async Task<string> GenerateBillNoAsync(DateTime billDate, int? devType, CancellationToken ct)
    {
        var prefix = $"WB{billDate:yyyyMM}{(devType is > 0 ? devType.Value : 0)}";
        var existing = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.BillNo != null && x.BillNo.StartsWith(prefix))
            .Select(x => x.BillNo!)
            .ToListAsync(ct);

        var max = existing
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        string candidate;
        do
        {
            max++;
            candidate = $"{prefix}{max:D4}";
        }
        while (await _db.JalPrintBillMasters.AnyAsync(x => x.BillNo == candidate, ct));

        return candidate;
    }

    private static JalRateTran? FindRate(ConsumerDetailsMaster consumer, IReadOnlyList<JalRateTran> rates)
    {
        var plotSize = consumer.PlotSize ?? 0;
        var pipeSize = consumer.PipeSize ?? 0;
        var connectionType = consumer.ConTp?.Trim();

        return rates
            .Where(x => (x.DevType == null || x.DevType == consumer.DevType)
                && (x.PipeSize == null || x.PipeSize == pipeSize)
                && (x.AreaStart == null || x.AreaStart <= plotSize)
                && (x.AreaEnd == null || x.AreaEnd >= plotSize)
                && (string.IsNullOrWhiteSpace(connectionType)
                    || string.Equals(x.IdNavigation?.IdT, connectionType, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.IdNavigation?.PropertyType, connectionType, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.DevType == consumer.DevType)
            .ThenByDescending(x => x.PipeSize == pipeSize)
            .ThenBy(x => x.AreaEnd ?? int.MaxValue)
            .FirstOrDefault();
    }

    private static double ResolveMonthlyRate(ConsumerDetailsMaster consumer, JalRateTran? rate)
    {
        if (rate is not null)
        {
            var isTemporary = string.Equals(consumer.ConsCtg, "T", StringComparison.OrdinalIgnoreCase);
            var configuredRate = isTemporary ? rate.Temporary : rate.Regular;
            if (configuredRate.GetValueOrDefault() > 0)
                return configuredRate.GetValueOrDefault();
            if (rate.MainRate.GetValueOrDefault() > 0)
                return rate.MainRate.GetValueOrDefault();
        }

        if (consumer.MaonthyCharges.GetValueOrDefault() > 0)
            return consumer.MaonthyCharges.GetValueOrDefault();
        if (consumer.MonthlyRate.GetValueOrDefault() > 0)
            return consumer.MonthlyRate.GetValueOrDefault();

        return 0;
    }

    private static int CountBillMonths(DateTime from, DateTime to)
    {
        if (to < from)
            return 0;

        return ((to.Year - from.Year) * 12) + to.Month - from.Month + 1;
    }

    private static double CalculateArrearInterest(double arrear, DateTime from, DateTime to, double annualPercent)
    {
        if (arrear <= 0 || annualPercent <= 0)
            return 0;

        var months = CountBillMonths(from, to);
        return Math.Round(arrear * (annualPercent / 100d) / 12d * months, 2);
    }

    private static int ResolveRebatePercent(BulkBillGenerationViewModel request, double baseAmount, double creditAmount)
    {
        var percent = request.RebateMode switch
        {
            "EarlyAnnual" => request.BillDate.Month is >= 4 and <= 5 ? request.RebatePercent : 0,
            "HalfYear" => request.BillDate.Month is >= 5 and <= 9 ? request.RebatePercent : 0,
            _ => request.RebatePercent
        };

        if (!request.ApplyCreditRebateGuard)
            return percent;

        return baseAmount > 0 && Math.Abs(creditAmount) > baseAmount * request.CreditRebateThresholdMonths
            ? percent
            : 0;
    }

    private static string BuildChallanContent(string billNo, ConsumerDetailsMaster consumer, BulkBillPreviewRowViewModel row, BulkBillGenerationViewModel request)
        => $"Bill {billNo} generated for consumer {consumer.ConsNo} ({consumer.ConsNm1}) for period {request.BillDateFrom:dd-MMM-yyyy} to {request.BillDateTo:dd-MMM-yyyy}. Total payable: INR {row.TotalBillAmount:N2}.";

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static List<SelectListItem> BuildDivisionOptions(int? selected)
        => AppConstants.Divisions.Options
            .Where(x => x.DevType != AppConstants.Divisions.AllDivision.DevType)
            .Select(x => new SelectListItem(x.DisplayText, x.DevType.ToString(), selected == x.DevType))
            .ToList();

    private static void NormalizeRequest(BulkBillGenerationViewModel request)
    {
        request.Sector = Normalize(request.Sector);
        request.Block = Normalize(request.Block);
        request.ConsumerNo = Normalize(request.ConsumerNo)?.ToUpperInvariant();
        request.BillType = Normalize(request.BillType) ?? "B";
        request.RebateMode = Normalize(request.RebateMode) switch
        {
            "EarlyAnnual" => "EarlyAnnual",
            "HalfYear" => "HalfYear",
            _ => "Manual"
        };
        request.SchemeId = Normalize(request.SchemeId);
        request.MaxConsumers = Math.Clamp(request.MaxConsumers <= 0 ? 100 : request.MaxConsumers, 1, 1000);
        request.RebatePercent = Math.Clamp(request.RebatePercent, 0, 100);
        request.BillPercentage = request.BillPercentage.HasValue ? Math.Clamp(request.BillPercentage.Value, 0, 100) : null;
        request.GstPercent = Math.Clamp(request.GstPercent, 0, 100);
        request.ArrearInterestPercent = Math.Clamp(request.ArrearInterestPercent, 0, 100);
        request.AfterDueSurchargePercent = Math.Clamp(request.AfterDueSurchargePercent, 0, 100);
        request.CreditRebateThresholdMonths = Math.Clamp(request.CreditRebateThresholdMonths <= 0 ? 2 : request.CreditRebateThresholdMonths, 1, 12);

        if (request.BillDateTo < request.BillDateFrom)
            request.BillDateTo = request.BillDateFrom;
        if (request.BillDueDate < request.BillDate)
            request.BillDueDate = request.BillDate.AddDays(15);
        if (request.LegacyRateDate == default)
            request.LegacyRateDate = new DateTime(2013, 7, 1);
    }

    private string CurrentUsername()
        => User.FindFirstValue(AppConstants.Claims.Username)
           ?? User.Identity?.Name
           ?? "System";

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
