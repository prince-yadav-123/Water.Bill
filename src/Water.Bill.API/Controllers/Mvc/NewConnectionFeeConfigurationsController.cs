using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class NewConnectionFeeConfigurationsController : Controller
{
    private readonly ApplicationDbContext _db;

    public NewConnectionFeeConfigurationsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "New Connection Fee Configuration";
        ViewData["ActiveMenu"] = "New Connection Fee Configuration";
        var rows = await _db.NewConnectionFeeConfigurations.AsNoTracking().Where(x => !x.IsDeleted).OrderByDescending(x => x.EffectiveFrom).ToListAsync(ct);
        return View(rows);
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
}
