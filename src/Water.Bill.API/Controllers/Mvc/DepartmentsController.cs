using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public DepartmentsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Department Master";
        ViewData["ActiveMenu"] = "Department Master";
        var rows = await _db.MasterDeptDetails.AsNoTracking().OrderBy(x => x.DeptName).ToListAsync(ct);
        return View(rows);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Department";
        ViewData["ActiveMenu"] = "Department Master";
        return View("Form", new MasterDeptDetail { Status = "1" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Department";
        ViewData["ActiveMenu"] = "Department Master";
        var model = await _db.MasterDeptDetails.FirstOrDefaultAsync(x => x.Id == id, ct);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
}
