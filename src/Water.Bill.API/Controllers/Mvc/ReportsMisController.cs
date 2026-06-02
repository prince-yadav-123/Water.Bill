using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Reports;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ReportsMisController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsMisController(ApplicationDbContext db) => _db = db;

    [HttpGet("/ReportsMis")]
    [RequirePermission("Reports / MIS.view")]
    public async Task<IActionResult> Index(MisReportIndexViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Reports / MIS";
        ViewData["ActiveMenu"] = "Reports / MIS";
        model.ReportType = NormalizeReportType(model.ReportType);
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant();
        model.Status = Normalize(model.Status);
        model.Rows = model.ReportType switch
        {
            "Dues" => await BuildDuesRowsAsync(model, ct),
            "Challan" => await BuildChallanRowsAsync(model, ct),
            "Bill" => await BuildBillRowsAsync(model, ct),
            _ => await BuildCollectionRowsAsync(model, ct)
        };
        model.Summary = new MisReportSummaryViewModel
        {
            TotalCount = model.Rows.Count,
            TotalAmount = model.Rows.Sum(x => x.Amount),
            PaidAmount = model.Rows.Sum(x => x.PaidAmount),
            PendingAmount = model.Rows.Sum(x => Math.Max(0, x.Amount - x.PaidAmount))
        };
        return View(model);
    }

    private async Task<IReadOnlyList<MisReportRowViewModel>> BuildCollectionRowsAsync(MisReportIndexViewModel model, CancellationToken ct)
    {
        var query = _db.JalPrintBillMasters.AsNoTracking().Where(x => x.PaidDate != null || x.PaidAmt > 0 || x.PaidStatus == "Y");
        query = ApplyBillFilters(query, model, paidDate: true);
        var rows = await query.OrderByDescending(x => x.PaidDate ?? x.EntryDate).Take(500).ToListAsync(ct);
        return rows.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? x.DevType?.ToString(),
            Status = "Paid",
            Date = x.PaidDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? x.TotalBillAmt ?? 0
        }).ToList();
    }

    private async Task<IReadOnlyList<MisReportRowViewModel>> BuildDuesRowsAsync(MisReportIndexViewModel model, CancellationToken ct)
    {
        var query = _db.JalPrintBillMasters.AsNoTracking().Where(x => x.Status == "1" && x.PaidDate == null && x.PaidStatus != "Y");
        query = ApplyBillFilters(query, model, paidDate: false);
        var rows = await query.OrderByDescending(x => x.BillDate ?? x.EntryDate).Take(500).ToListAsync(ct);
        return rows.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? x.DevType?.ToString(),
            Status = "Pending",
            Date = x.BillDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? 0
        }).ToList();
    }

    private async Task<IReadOnlyList<MisReportRowViewModel>> BuildBillRowsAsync(MisReportIndexViewModel model, CancellationToken ct)
    {
        var query = ApplyBillFilters(_db.JalPrintBillMasters.AsNoTracking(), model, paidDate: false);
        var rows = await query.OrderByDescending(x => x.BillDate ?? x.EntryDate).Take(500).ToListAsync(ct);
        return rows.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? x.DevType?.ToString(),
            Status = x.Status == "0" ? "Reversed" : x.PaidStatus == "Y" ? "Paid" : "Generated",
            Date = x.BillDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? 0
        }).ToList();
    }

    private async Task<IReadOnlyList<MisReportRowViewModel>> BuildChallanRowsAsync(MisReportIndexViewModel model, CancellationToken ct)
    {
        var query = _db.Challans.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsNo == model.ConsumerNo);
        if (model.FromDate.HasValue)
            query = query.Where(x => (x.EntryDate ?? x.PayDate) >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => (x.EntryDate ?? x.PayDate) < model.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(model.Status))
            query = query.Where(x => x.Status == model.Status);
        var rows = await query.OrderByDescending(x => x.EntryDate ?? x.PayDate).Take(500).ToListAsync(ct);
        return rows.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.RecpNo ?? x.ReceiptId ?? x.ReceiptId1,
            ConsumerNo = x.ConsNo,
            PropertyNo = $"{x.Sec}/{x.Blk}-{x.FlatNo}".Trim('/', '-'),
            Status = x.PayDate.HasValue ? "Paid" : "Pending",
            Date = x.EntryDate ?? x.PayDate,
            Amount = x.PaidAmt ?? x.BillAmt ?? 0,
            PaidAmount = x.PayDate.HasValue ? x.PaidAmt ?? x.BillAmt ?? 0 : 0
        }).ToList();
    }

    private static IQueryable<Water.Bill.Infrastructure.Data.Entities.JalPrintBillMaster> ApplyBillFilters(
        IQueryable<Water.Bill.Infrastructure.Data.Entities.JalPrintBillMaster> query,
        MisReportIndexViewModel model,
        bool paidDate)
    {
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsNo == model.ConsumerNo);
        if (model.DevType.HasValue)
            query = query.Where(x => x.DevType == model.DevType.Value);
        if (model.FromDate.HasValue)
            query = paidDate
                ? query.Where(x => x.PaidDate >= model.FromDate.Value.Date)
                : query.Where(x => (x.BillDate ?? x.EntryDate) >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = paidDate
                ? query.Where(x => x.PaidDate < model.ToDate.Value.Date.AddDays(1))
                : query.Where(x => (x.BillDate ?? x.EntryDate) < model.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(model.Status))
            query = query.Where(x => x.Status == model.Status || x.PaidStatus == model.Status);
        return query;
    }

    private static string NormalizeReportType(string? value) => value?.Trim() switch
    {
        "Dues" => "Dues",
        "Challan" => "Challan",
        "Bill" => "Bill",
        _ => "Collection"
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
