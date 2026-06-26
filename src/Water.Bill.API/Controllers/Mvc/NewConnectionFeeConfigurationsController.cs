using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Models;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Extensions;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class NewConnectionFeeConfigurationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExcelExportService _excelExportService;

    public NewConnectionFeeConfigurationsController(ApplicationDbContext db, IExcelExportService excelExportService)
    {
        _db = db;
        _excelExportService = excelExportService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 0, CancellationToken ct = default)
    {
        ViewData["Title"] = "New Connection Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);

        var paged = await _db.NewConnectionFeeConfigurations
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToPagedResultAsync(page, pageSize, ct);

        ViewBag.Pagination = PaginationViewModel.Create(paged);
        return View(paged.Items);
    }

    [HttpGet("/NewConnectionFeeConfigurations/ExportExcel")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct = default)
    {
        var rawRows = await _db.NewConnectionFeeConfigurations
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => new
            {
                x.ConnectionCategory,
                x.ConnectionType,
                x.PipeSize,
                x.PlotSizeFrom,
                x.PlotSizeTo,
                x.TotalAmount,
                x.EffectiveFrom,
                x.IsActive
            })
            .ToListAsync(ct);

        var rows = rawRows.Select(x => new NewConnectionFeeConfigurationExportRow
        {
            ConnectionCategory = x.ConnectionCategory ?? "Any",
            ConnectionType = x.ConnectionType ?? "Any",
            PipeSize = x.PipeSize.HasValue ? x.PipeSize.Value.ToString("0.##") : "Any",
            PlotRange = $"{(x.PlotSizeFrom.HasValue ? x.PlotSizeFrom.Value.ToString("0.##") : "0")} - {(x.PlotSizeTo.HasValue ? x.PlotSizeTo.Value.ToString("0.##") : "Any")}",
            TotalAmount = x.TotalAmount,
            EffectiveFrom = x.EffectiveFrom,
            Status = x.IsActive ? "Active" : "Inactive"
        }).ToList();

        var bytes = _excelExportService.Export(new ExcelExportRequest<NewConnectionFeeConfigurationExportRow>
        {
            SheetName = "New Connection Fee Configuration",
            Columns =
            [
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Category", ValueFactory = row => row.ConnectionCategory, Width = 18 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Type", ValueFactory = row => row.ConnectionType, Width = 18 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Pipe", ValueFactory = row => row.PipeSize, Width = 12 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Plot Range", ValueFactory = row => row.PlotRange, Width = 18 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Total", ValueFactory = row => row.TotalAmount, NumberFormat = "#,##0.00", Width = 14 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Effective", ValueFactory = row => row.EffectiveFrom, NumberFormat = "dd-mmm-yyyy", Width = 16 },
                new ExcelColumnDefinition<NewConnectionFeeConfigurationExportRow> { Header = "Status", ValueFactory = row => row.Status, Width = 14 }
            ],
            Rows = rows
        });

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"New-Connection-Fee-Configuration-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        return View("Form", new NewConnectionFeeConfiguration { IsActive = true, EffectiveFrom = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewConnectionFeeConfiguration model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        Normalize(model);
        if (!ModelState.IsValid) return View("Form", model);
        model.CreatedOn = DateTime.Now;
        model.TotalAmount = CalculateTotal(model);
        _db.NewConnectionFeeConfigurations.Add(model);
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        var model = await _db.NewConnectionFeeConfigurations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewConnectionFeeConfiguration model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        Normalize(model);
        if (!ModelState.IsValid) return View("Form", model);
        var entity = await _db.NewConnectionFeeConfigurations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();
        entity.ConnectionCategory = model.ConnectionCategory;
        entity.ConnectionType = model.ConnectionType;
        entity.PipeSize = model.PipeSize;
        entity.PlotSizeFrom = model.PlotSizeFrom;
        entity.PlotSizeTo = model.PlotSizeTo;
        entity.ApplicationFee = model.ApplicationFee;
        entity.ProcessingFee = model.ProcessingFee;
        entity.SecurityAmount = model.SecurityAmount;
        entity.MeterInstallationFee = model.MeterInstallationFee;
        entity.OtherCharges = model.OtherCharges;
        entity.TotalAmount = CalculateTotal(model);
        entity.EffectiveFrom = model.EffectiveFrom;
        entity.EffectiveTo = model.EffectiveTo;
        entity.IsActive = model.IsActive;
        entity.UpdatedOn = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
    {
        var entity = await _db.NewConnectionFeeConfigurations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound();
        entity.IsActive = !entity.IsActive;
        entity.UpdatedOn = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    private void Normalize(NewConnectionFeeConfiguration model)
    {
        model.ConnectionCategory = NormalizeOptional(model.ConnectionCategory);
        model.ConnectionType = NormalizeOptional(model.ConnectionType);
        if (model.EffectiveFrom == default)
            ModelState.AddModelError(nameof(model.EffectiveFrom), "Effective from is required.");
    }

    private static decimal CalculateTotal(NewConnectionFeeConfiguration model)
        => model.ApplicationFee + model.ProcessingFee + model.SecurityAmount + model.MeterInstallationFee + model.OtherCharges;

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private sealed class NewConnectionFeeConfigurationExportRow
    {
        public string ConnectionCategory { get; set; } = string.Empty;
        public string ConnectionType { get; set; } = string.Empty;
        public string PipeSize { get; set; } = string.Empty;
        public string PlotRange { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
