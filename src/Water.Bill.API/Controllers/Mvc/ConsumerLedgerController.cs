using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Adjustments;
using Water.Bill.API.Models.Ledger;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Extensions;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConsumerLedgerController : Controller
{
    private const string ModuleName = AppConstants.Modules.ConsumerLedger;
    private readonly ApplicationDbContext _db;

    public ConsumerLedgerController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/ConsumerLedger")]
    [RequirePermission("Consumer Ledger.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        DateTime? fromDate,
        DateTime? toDate,
        string? entryType,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);

        var model = new ConsumerLedgerIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            FromDate = fromDate,
            ToDate = toDate,
            EntryType = Normalize(entryType),
            EntryTypeOptions = ConsumerLedgerEntryTypes.Options(entryType).ToList()
        };

        model.HasSearched = !string.IsNullOrWhiteSpace(model.Search) || !string.IsNullOrWhiteSpace(model.ConsumerNo);

        var selectedConsumer = await ResolveConsumerAsync(model, ct);
        if (selectedConsumer is null)
        {
            if (model.HasSearched)
            {
                var consumerPaged = await SearchConsumersAsync(model.Search ?? model.ConsumerNo, page, pageSize, ct);
                model.Consumers = consumerPaged.Items;
                ViewBag.Pagination = PaginationViewModel.Create(consumerPaged);
            }
            else
            {
                model.Consumers = [];
            }
            return View(model);
        }

        model.ConsumerNo = selectedConsumer.ConsNo;
        model.Consumer = ToConsumerModel(selectedConsumer);
        await PopulateLedgerAsync(model, page, pageSize, ct);
        return View(model);
    }

    [HttpGet("/ConsumerLedger/Print")]
    [RequirePermission("Consumer Ledger.print")]
    public async Task<IActionResult> Print(
        string consumerNo,
        DateTime? fromDate,
        DateTime? toDate,
        string? entryType,
        CancellationToken ct)
    {
        ViewData["Title"] = "Print Consumer Ledger";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        if (consumer is null)
            return NotFound();

        var model = new ConsumerLedgerIndexViewModel
        {
            ConsumerNo = consumer.ConsNo,
            Consumer = ToConsumerModel(consumer),
            FromDate = fromDate,
            ToDate = toDate,
            EntryType = Normalize(entryType),
            EntryTypeOptions = ConsumerLedgerEntryTypes.Options(entryType).ToList()
        };

        await PopulateLedgerAsync(model, 1, int.MaxValue, ct);
        return View(model);
    }

    private async Task<ConsumerDetailsMaster?> ResolveConsumerAsync(ConsumerLedgerIndexViewModel model, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
        {
            var exact = await _db.ConsumerDetailsMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
            if (exact is not null)
                return exact;
        }

        if (string.IsNullOrWhiteSpace(model.Search))
            return null;

        var search = model.Search;
        var matches = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == search
                || (x.MobNo != null && x.MobNo == search)
                || (x.ConsNm1 != null && x.ConsNm1.Contains(search)))
            .OrderBy(x => x.ConsNo)
            .Take(2)
            .ToListAsync(ct);

        return matches.Count == 1 ? matches[0] : null;
    }

    private async Task<PagedResult<ConsumerLedgerConsumerSearchRowViewModel>> SearchConsumersAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        search = Normalize(search);
        if (string.IsNullOrWhiteSpace(search))
            return PagedResult<ConsumerLedgerConsumerSearchRowViewModel>.Empty(page, pageSize);

        return await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.ConsNo.Contains(search)
                || (x.ConsNm1 != null && x.ConsNm1.Contains(search))
                || (x.MobNo != null && x.MobNo.Contains(search))
                || (x.Sector != null && x.Sector.Contains(search))
                || (x.BlkNo != null && x.BlkNo.Contains(search))
                || (x.FlatNo != null && x.FlatNo.Contains(search)))
            .OrderBy(x => x.ConsNo)
            .Take(50)
            .Select(x => new ConsumerLedgerConsumerSearchRowViewModel
            {
                ConsumerNo = x.ConsNo,
                ConsumerName = x.ConsNm1,
                MobileNo = x.MobNo,
                PropertyNo = x.Sector + "/" + x.BlkNo + "-" + x.FlatNo,
                ConnectionType = x.ConTp,
                DevType = x.DevType
            })
            .ToPagedResultAsync(page, pageSize, ct);
    }

    private async Task PopulateLedgerAsync(ConsumerLedgerIndexViewModel model, int page, int pageSize, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.ConsumerNo))
            return;

        var toExclusive = model.ToDate?.Date.AddDays(1);
        var allRows = new List<ConsumerLedgerRowViewModel>();

        if (ShouldInclude(model.EntryType, ConsumerLedgerEntryTypes.Bill))
            allRows.AddRange(await BuildBillRowsAsync(model.ConsumerNo, toExclusive, ct));

        if (ShouldInclude(model.EntryType, ConsumerLedgerEntryTypes.Challan))
            allRows.AddRange(await BuildChallanRowsAsync(model.ConsumerNo, toExclusive, ct));

        if (ShouldInclude(model.EntryType, ConsumerLedgerEntryTypes.Payment))
            allRows.AddRange(await BuildPaymentRowsAsync(model.ConsumerNo, toExclusive, allRows, ct));

        if (ShouldInclude(model.EntryType, ConsumerLedgerEntryTypes.Adjustment))
            allRows.AddRange(await BuildAdjustmentRowsAsync(model.ConsumerNo, toExclusive, ct));

        var ordered = allRows
            .OrderBy(x => x.Date)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.ReferenceNo)
            .ToList();

        var fromDate = model.FromDate?.Date;
        model.OpeningBalance = fromDate.HasValue
            ? ordered.Where(x => x.Date.Date < fromDate.Value && x.AffectsBalance).Sum(x => x.Debit - x.Credit)
            : 0;

        var displayRows = ordered
            .Where(x => !fromDate.HasValue || x.Date.Date >= fromDate.Value)
            .Where(x => !toExclusive.HasValue || x.Date < toExclusive.Value)
            .ToList();

        var balance = model.OpeningBalance;
        foreach (var row in displayRows)
        {
            if (row.AffectsBalance)
                balance += row.Debit - row.Credit;
            row.Balance = balance;
        }

        var orderedRows = displayRows
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.ReferenceNo)
            .ToList();

        var paged = new PagedResult<ConsumerLedgerRowViewModel>
        {
            Items = orderedRows.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = orderedRows.Count,
            Page = page,
            PageSize = pageSize
        };

        model.Rows = paged.Items;
        ViewBag.Pagination = PaginationViewModel.Create(paged);

        model.TotalDebit = displayRows.Where(x => x.AffectsBalance).Sum(x => x.Debit);
        model.TotalCredit = displayRows.Where(x => x.AffectsBalance).Sum(x => x.Credit);
        model.ClosingBalance = balance;
    }

    private async Task<IReadOnlyList<ConsumerLedgerRowViewModel>> BuildBillRowsAsync(string consumerNo, DateTime? toExclusive, CancellationToken ct)
    {
        var query = _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.BillNo != null);

        if (toExclusive.HasValue)
            query = query.Where(x => (x.BillDate ?? x.EntryDate ?? x.BillDateTo) < toExclusive.Value);

        var bills = await query
            .OrderBy(x => x.BillDate)
            .ThenBy(x => x.BillNo)
            .Take(500)
            .ToListAsync(ct);

        return bills.Select(x =>
        {
            var amount = ToDecimal(x.TotalBillAmt ?? x.DueAmt ?? x.MinTotalAmt ?? 0);
            return new ConsumerLedgerRowViewModel
            {
                Date = x.BillDate ?? x.EntryDate ?? x.BillDateTo ?? DateTime.Today,
                EntryType = ConsumerLedgerEntryTypes.Bill,
                ReferenceNo = x.BillNo ?? "-",
                LinkedReferenceNo = x.ChallanNo,
                Description = BuildBillDescription(x),
                Status = IsBillPaid(x, amount) ? "Paid" : "Pending",
                Debit = amount,
                Credit = 0,
                AffectsBalance = amount > 0,
                SortOrder = 10
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<ConsumerLedgerRowViewModel>> BuildChallanRowsAsync(string consumerNo, DateTime? toExclusive, CancellationToken ct)
    {
        var query = _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo);

        if (toExclusive.HasValue)
            query = query.Where(x => x.EntryDate < toExclusive.Value);

        var challans = await query
            .OrderBy(x => x.EntryDate)
            .ThenBy(x => x.Id)
            .Take(500)
            .ToListAsync(ct);

        return challans.Select(x =>
        {
            var status = ResolveChallanStatus(x);
            var isBillLinked = string.Equals(x.RevBilFr, "BILL", StringComparison.OrdinalIgnoreCase);
            var amount = ToDecimal(x.PaidAmt ?? x.BillAmt ?? 0);
            var affectsBalance = !isBillLinked && status != "Cancelled" && amount > 0;
            return new ConsumerLedgerRowViewModel
            {
                Date = x.EntryDate ?? DateTime.Today,
                EntryType = ConsumerLedgerEntryTypes.Challan,
                ReferenceNo = x.RecpNo ?? x.ReceiptId ?? x.Id.ToString(),
                LinkedReferenceNo = isBillLinked ? x.BillId : null,
                Description = isBillLinked
                    ? $"Bill due challan generated for bill {x.BillId ?? "-"}."
                    : $"{PurposeDisplay(x.RevBilFr)} challan generated.",
                Status = status,
                Debit = affectsBalance ? amount : 0,
                Credit = 0,
                AffectsBalance = affectsBalance,
                SortOrder = 20
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<ConsumerLedgerRowViewModel>> BuildPaymentRowsAsync(
        string consumerNo,
        DateTime? toExclusive,
        IReadOnlyList<ConsumerLedgerRowViewModel> existingRows,
        CancellationToken ct)
    {
        var query = _db.ChallanPaymentHistories
            .AsNoTracking()
            .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted);

        if (toExclusive.HasValue)
            query = query.Where(x => x.PaymentDate < toExclusive.Value);

        var paymentRows = await query
            .OrderBy(x => x.PaymentDate)
            .ThenBy(x => x.Id)
            .Select(x => new ConsumerLedgerRowViewModel
            {
                Date = x.PaymentDate,
                EntryType = ConsumerLedgerEntryTypes.Payment,
                ReferenceNo = x.TransactionReferenceNo ?? x.ChallanNo ?? x.Id.ToString(),
                LinkedReferenceNo = x.SourceBillNo ?? x.ChallanNo,
                Description = "Payment received" + (x.PaymentMode != null ? $" by {x.PaymentMode}" : string.Empty),
                Status = "Paid",
                Debit = 0,
                Credit = (decimal)x.Amount,
                AffectsBalance = x.Amount > 0,
                SortOrder = 30
            })
            .ToListAsync(ct);

        var paymentKeys = paymentRows
            .Where(x => !string.IsNullOrWhiteSpace(x.LinkedReferenceNo))
            .Select(x => x.LinkedReferenceNo!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var syntheticRows = await BuildSyntheticOldPaymentRowsAsync(consumerNo, toExclusive, paymentKeys, ct);
        paymentRows.AddRange(syntheticRows);

        return paymentRows;
    }

    private async Task<IReadOnlyList<ConsumerLedgerRowViewModel>> BuildSyntheticOldPaymentRowsAsync(
        string consumerNo,
        DateTime? toExclusive,
        ISet<string> existingPaymentReferences,
        CancellationToken ct)
    {
        var rows = new List<ConsumerLedgerRowViewModel>();

        var billQuery = _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.BillNo != null && x.PaidDate != null);
        if (toExclusive.HasValue)
            billQuery = billQuery.Where(x => x.PaidDate < toExclusive.Value);

        var paidBills = await billQuery.Take(500).ToListAsync(ct);
        foreach (var bill in paidBills)
        {
            if (bill.BillNo is null
                || existingPaymentReferences.Contains(bill.BillNo)
                || (bill.ChallanNo != null && existingPaymentReferences.Contains(bill.ChallanNo)))
                continue;

            var amount = ToDecimal(bill.PaidAmt ?? bill.TotalBillAmt ?? bill.DueAmt ?? bill.MinTotalAmt ?? 0);
            if (amount <= 0)
                continue;

            rows.Add(new ConsumerLedgerRowViewModel
            {
                Date = bill.PaidDate!.Value,
                EntryType = ConsumerLedgerEntryTypes.Payment,
                ReferenceNo = bill.ChallanNo ?? bill.BillNo,
                LinkedReferenceNo = bill.BillNo,
                Description = "Payment received against generated bill.",
                Status = "Paid",
                Debit = 0,
                Credit = amount,
                AffectsBalance = true,
                SortOrder = 31
            });
        }

        var challanQuery = _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.PayDate != null);
        if (toExclusive.HasValue)
            challanQuery = challanQuery.Where(x => x.PayDate < toExclusive.Value);

        var paidChallans = await challanQuery.Take(500).ToListAsync(ct);
        foreach (var challan in paidChallans)
        {
            var challanNo = challan.RecpNo ?? challan.ReceiptId ?? challan.Id.ToString();
            if (existingPaymentReferences.Contains(challanNo) || (challan.BillId != null && existingPaymentReferences.Contains(challan.BillId)))
                continue;

            var amount = ToDecimal(challan.PaidAmt ?? challan.BillAmt ?? 0);
            if (amount <= 0)
                continue;

            rows.Add(new ConsumerLedgerRowViewModel
            {
                Date = challan.PayDate!.Value,
                EntryType = ConsumerLedgerEntryTypes.Payment,
                ReferenceNo = challanNo,
                LinkedReferenceNo = challan.RevBilFr == "BILL" ? challan.BillId : challanNo,
                Description = "Payment received against challan.",
                Status = "Paid",
                Debit = 0,
                Credit = amount,
                AffectsBalance = true,
                SortOrder = 32
            });
        }

        return rows;
    }

    private async Task<IReadOnlyList<ConsumerLedgerRowViewModel>> BuildAdjustmentRowsAsync(string consumerNo, DateTime? toExclusive, CancellationToken ct)
    {
        var query = _db.ConsumerAccountAdjustments
            .AsNoTracking()
            .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted);

        if (toExclusive.HasValue)
            query = query.Where(x => x.EffectiveDate < toExclusive.Value);

        var adjustments = await query
            .OrderBy(x => x.EffectiveDate)
            .ThenBy(x => x.Id)
            .Take(500)
            .ToListAsync(ct);

        return adjustments.Select(x =>
        {
            var signed = ConsumerAdjustmentTypes.SignedAmount(x.AdjustmentType, x.Amount);
            var affectsBalance = x.Status == ConsumerAdjustmentStatuses.Pending;
            return new ConsumerLedgerRowViewModel
            {
                Date = x.EffectiveDate,
                EntryType = ConsumerLedgerEntryTypes.Adjustment,
                ReferenceNo = x.AdjustmentNo,
                LinkedReferenceNo = x.AppliedBillNo ?? x.SourceBillNo ?? x.SourceChallanNo,
                Description = affectsBalance
                    ? $"{ConsumerAdjustmentTypes.Display(x.AdjustmentType)} pending for next bill."
                    : $"{ConsumerAdjustmentTypes.Display(x.AdjustmentType)} - {x.Status}.",
                Status = x.Status,
                Debit = affectsBalance && signed > 0 ? signed : 0,
                Credit = affectsBalance && signed < 0 ? Math.Abs(signed) : 0,
                AffectsBalance = affectsBalance,
                SortOrder = 40
            };
        }).ToList();
    }

    private static bool ShouldInclude(string? selected, string entryType)
        => string.IsNullOrWhiteSpace(selected) || string.Equals(selected, entryType, StringComparison.OrdinalIgnoreCase);

    private static ConsumerLedgerConsumerViewModel ToConsumerModel(ConsumerDetailsMaster consumer)
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
            DevType = consumer.DevType,
            Status = consumer.Status
        };

    private static string BuildBillDescription(JalPrintBillMaster bill)
    {
        var period = bill.BillDateFrom.HasValue || bill.BillDateTo.HasValue
            ? $" for {bill.BillDateFrom:dd MMM yyyy} to {bill.BillDateTo:dd MMM yyyy}"
            : string.Empty;
        return $"Water bill generated{period}.";
    }

    private static bool IsBillPaid(JalPrintBillMaster bill, decimal amount)
    {
        if (bill.PaidDate.HasValue)
            return true;
        if (string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase))
            return true;
        return ToDecimal(bill.PaidAmt ?? 0) >= amount && amount > 0;
    }

    private static string ResolveChallanStatus(Challan challan)
    {
        if (challan.Status == "0" || challan.ChallanStatus == 0)
            return "Cancelled";
        return challan.PayDate.HasValue ? "Paid" : "PendingPayment";
    }

    private static string PurposeDisplay(string? code) => code switch
    {
        "NDC" => "NDC / No Dues",
        "NEWCONN" => "New Connection",
        "OTHER" => "Other Service",
        "BILL" => "Bill Due",
        _ => string.IsNullOrWhiteSpace(code) ? "Service" : code
    };

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static decimal ToDecimal(double value)
        => Convert.ToDecimal(value);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
