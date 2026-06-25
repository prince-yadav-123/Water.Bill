using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Reports;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ReportsMisController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExcelExportService _excelExportService;

    public ReportsMisController(ApplicationDbContext db, IExcelExportService excelExportService)
    {
        _db = db;
        _excelExportService = excelExportService;
    }

    [HttpGet("/ReportsMis")]
    [RequirePermission("Reports / MIS.view")]
    public async Task<IActionResult> Index(MisReportIndexViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Reports / MIS";
        ViewData["ActiveMenu"] = "Reports / MIS";

        model.ReportType = NormalizeReportType(model.ReportType);
        model.Search = Normalize(model.Search);
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant();
        model.Status = Normalize(model.Status);
        model.PageSize = PagingConstants.Validate(model.PageSize == 0 ? PagingConstants.DefaultPageSize : model.PageSize);
        model.Page = PagingConstants.ValidatePage(model.Page);

        var query = model.ReportType switch
        {
            "Dues" => BuildDuesQuery(model),
            "Challan" => BuildChallanQuery(model),
            "Bill" => BuildBillQuery(model),
            _ => BuildCollectionQuery(model)
        };

        query = ApplyCommonSearch(query, model.Search);
        var summaryQuery = query.Select(x => new { x.Amount, x.PaidAmount });

        model.Summary = await summaryQuery
            .GroupBy(_ => 1)
            .Select(g => new MisReportSummaryViewModel
            {
                TotalCount = g.Count(),
                TotalAmount = g.Sum(x => x.Amount),
                PaidAmount = g.Sum(x => x.PaidAmount),
                PendingAmount = g.Sum(x => x.Amount > x.PaidAmount ? x.Amount - x.PaidAmount : 0)
            })
            .FirstOrDefaultAsync(ct) ?? new MisReportSummaryViewModel();

        var totalCount = model.Summary.TotalCount;

        model.Rows = totalCount == 0
            ? []
            : await query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.ReferenceNo)
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync(ct);

        ViewBag.Pagination = PaginationViewModel.Create(new PagedResult<MisReportRowViewModel>
        {
            Items = model.Rows,
            TotalCount = totalCount,
            Page = model.Page,
            PageSize = model.PageSize
        });

        return View(model);
    }

    [HttpGet("/ReportsMis/ExportExcel")]
    [RequirePermission("Reports / MIS.download")]
    public async Task<IActionResult> ExportExcel(MisReportIndexViewModel model, CancellationToken ct)
    {
        model.ReportType = NormalizeReportType(model.ReportType);
        model.Search = Normalize(model.Search);
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant();
        model.Status = Normalize(model.Status);

        var query = model.ReportType switch
        {
            "Dues" => BuildDuesQuery(model),
            "Challan" => BuildChallanQuery(model),
            "Bill" => BuildBillQuery(model),
            _ => BuildCollectionQuery(model)
        };

        var rows = await ApplyCommonSearch(query, model.Search)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.ReferenceNo)
            .ToListAsync(ct);

        var bytes = _excelExportService.Export(new ExcelExportRequest<MisReportRowViewModel>
        {
            SheetName = $"{model.ReportType} Report",
            Rows = rows,
            Columns =
            [
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Reference", ValueFactory = x => x.ReferenceNo ?? "-", Width = 24 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Consumer No", ValueFactory = x => x.ConsumerNo ?? "-", Width = 18 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Consumer Name", ValueFactory = x => x.ConsumerName ?? "-", Width = 26 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Property No", ValueFactory = x => x.PropertyNo ?? "-", Width = 24 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Division", ValueFactory = x => x.Division ?? "-", Width = 16 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Status", ValueFactory = x => x.Status ?? "-", Width = 16 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Date", ValueFactory = x => x.Date, NumberFormat = "dd mmm yyyy", Width = 16 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Amount", ValueFactory = x => x.Amount, NumberFormat = "#,##0.00", Width = 16 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Paid Amount", ValueFactory = x => x.PaidAmount, NumberFormat = "#,##0.00", Width = 16 },
                new ExcelColumnDefinition<MisReportRowViewModel> { Header = "Pending Amount", ValueFactory = x => Math.Max(0, x.Amount - x.PaidAmount), NumberFormat = "#,##0.00", Width = 18 }
            ]
        });

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reports-mis-{model.ReportType?.ToLowerInvariant()}-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    private IQueryable<MisReportRowViewModel> BuildCollectionQuery(MisReportIndexViewModel model)
    {
        var query = _db.JalPrintBillMasters.AsNoTracking().Where(x => x.PaidDate != null || x.PaidAmt > 0 || x.PaidStatus == "Y");
        query = ApplyBillFilters(query, model, paidDate: true);
        return query.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? (x.DevType.HasValue ? x.DevType.Value.ToString() : null),
            Status = "Paid",
            Date = x.PaidDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? x.TotalBillAmt ?? 0
        });
    }

    private IQueryable<MisReportRowViewModel> BuildDuesQuery(MisReportIndexViewModel model)
    {
        var query = _db.JalPrintBillMasters.AsNoTracking().Where(x => x.Status == "1" && x.PaidDate == null && x.PaidStatus != "Y");
        query = ApplyBillFilters(query, model, paidDate: false);
        return query.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? (x.DevType.HasValue ? x.DevType.Value.ToString() : null),
            Status = "Pending",
            Date = x.BillDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? 0
        });
    }

    private IQueryable<MisReportRowViewModel> BuildBillQuery(MisReportIndexViewModel model)
    {
        var query = ApplyBillFilters(_db.JalPrintBillMasters.AsNoTracking(), model, paidDate: false);
        return query.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            Division = x.DivType ?? (x.DevType.HasValue ? x.DevType.Value.ToString() : null),
            Status = x.Status == "0" ? "Reversed" : x.PaidStatus == "Y" ? "Paid" : "Generated",
            Date = x.BillDate ?? x.EntryDate,
            Amount = x.TotalBillAmt ?? x.DueAmt ?? 0,
            PaidAmount = x.PaidAmt ?? 0
        });
    }

    private IQueryable<MisReportRowViewModel> BuildChallanQuery(MisReportIndexViewModel model)
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
        return query.Select(x => new MisReportRowViewModel
        {
            ReferenceNo = x.RecpNo ?? x.ReceiptId ?? x.ReceiptId1,
            ConsumerNo = x.ConsNo,
            PropertyNo = (((x.Sec ?? string.Empty) + "/" + (x.Blk ?? string.Empty)) + "-" + (x.FlatNo ?? string.Empty)).Trim('/', '-'),
            Status = x.PayDate.HasValue ? "Paid" : "Pending",
            Date = x.EntryDate ?? x.PayDate,
            Amount = x.PaidAmt ?? x.BillAmt ?? 0,
            PaidAmount = x.PayDate.HasValue ? x.PaidAmt ?? x.BillAmt ?? 0 : 0
        });
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

    private static IQueryable<MisReportRowViewModel> ApplyCommonSearch(IQueryable<MisReportRowViewModel> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var pattern = $"%{EscapeLikePattern(search)}%";
        return query.Where(x =>
            (x.ReferenceNo != null && EF.Functions.Like(x.ReferenceNo, pattern)) ||
            (x.ConsumerNo != null && EF.Functions.Like(x.ConsumerNo, pattern)) ||
            (x.ConsumerName != null && EF.Functions.Like(x.ConsumerName, pattern)) ||
            (x.PropertyNo != null && EF.Functions.Like(x.PropertyNo, pattern)) ||
            (x.Division != null && EF.Functions.Like(x.Division, pattern)) ||
            (x.Status != null && EF.Functions.Like(x.Status, pattern)));
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);

    private static string NormalizeReportType(string? value) => value?.Trim() switch
    {
        "Dues" => "Dues",
        "Challan" => "Challan",
        "Bill" => "Bill",
        _ => "Collection"
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
