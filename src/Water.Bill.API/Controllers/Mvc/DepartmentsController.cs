using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExcelExportService _excelExportService;

    public DepartmentsController(ApplicationDbContext db, IExcelExportService excelExportService)
    {
        _db = db;
        _excelExportService = excelExportService;
    }

    [RequirePermission("Department Master.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Department Master";
        ViewData["ActiveMenu"] = "Department Master";
        var rows = await _db.MasterDeptDetails.AsNoTracking().OrderBy(x => x.DeptName).ToListAsync(ct);
        return View(rows);
    }

    [HttpGet("/Departments/ExportExcel")]
    [RequirePermission("Department Master.view")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var rows = await _db.MasterDeptDetails
            .AsNoTracking()
            .OrderBy(x => x.DeptName)
            .Select(x => new DepartmentExportRow
            {
                DeptId = x.DeptId,
                DeptName = x.DeptName,
                Status = IsActiveStatus(x.Status) ? "Active" : "Inactive"
            })
            .ToListAsync(ct);

        var bytes = _excelExportService.Export(new ExcelExportRequest<DepartmentExportRow>
        {
            SheetName = "Department Master",
            Columns =
            [
                new ExcelColumnDefinition<DepartmentExportRow> { Header = "Code", ValueFactory = row => row.DeptId, Width = 12 },
                new ExcelColumnDefinition<DepartmentExportRow> { Header = "Department", ValueFactory = row => row.DeptName, Width = 28 },
                new ExcelColumnDefinition<DepartmentExportRow> { Header = "Status", ValueFactory = row => row.Status, Width = 14 }
            ],
            Rows = rows
        });

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Department-Master-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [RequirePermission("Department Master.add")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Department";
        ViewData["ActiveMenu"] = "Department Master";
        return View("Form", new MasterDeptDetail { Status = "1" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Department Master.add")]
    public async Task<IActionResult> Create(MasterDeptDetail model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Department";
        ViewData["ActiveMenu"] = "Department Master";
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);

        model.DeptName = model.DeptName?.Trim();
        model.Status = IsActiveStatus(model.Status) ? "1" : "0";
        model.DeptId = await _db.MasterDeptDetails.MaxAsync(x => (int?)x.DeptId, ct) is { } maxDeptId ? maxDeptId + 1 : 1;
        _db.MasterDeptDetails.Add(model);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Department created.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("Department Master.edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Department";
        ViewData["ActiveMenu"] = "Department Master";
        var model = await _db.MasterDeptDetails.FirstOrDefaultAsync(x => x.Id == id, ct);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Department Master.edit")]
    public async Task<IActionResult> Edit(int id, MasterDeptDetail model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Department";
        ViewData["ActiveMenu"] = "Department Master";
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);

        var entity = await _db.MasterDeptDetails.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();
        entity.DeptName = model.DeptName?.Trim();
        entity.Status = IsActiveStatus(model.Status) ? "1" : "0";
        entity.DevType = model.DevType;
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Department updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Department Master.delete")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
    {
        var entity = await _db.MasterDeptDetails.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();
        entity.Status = IsActiveStatus(entity.Status) ? "0" : "1";
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    private void Validate(MasterDeptDetail model)
    {
        if (string.IsNullOrWhiteSpace(model.DeptName)) ModelState.AddModelError(nameof(model.DeptName), "Department name is required.");
    }

    private static bool IsActiveStatus(string? status)
        => string.Equals(status, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "true", StringComparison.OrdinalIgnoreCase);

    private sealed class DepartmentExportRow
    {
        public int DeptId { get; set; }
        public string? DeptName { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
