using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Communication;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
[RequirePermission("Communication Templates.view")]
public class CommunicationTemplatesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ITemplateRenderer _renderer;

    public CommunicationTemplatesController(ApplicationDbContext db, ITemplateRenderer renderer)
    {
        _db = db;
        _renderer = renderer;
    }

    [HttpGet("/CommunicationTemplates")]
    public async Task<IActionResult> Index(string? search, string? purposeKey, string? channel, string? activeStatus, CancellationToken ct)
    {
        ViewData["Title"] = "Communication Templates";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        var query = _db.CommunicationTemplates.Include(x => x.Purpose).AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.TemplateName.Contains(term) || x.PurposeKey.Contains(term) || x.Body.Contains(term) || (x.Subject != null && x.Subject.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(purposeKey))
            query = query.Where(x => x.PurposeKey == purposeKey);
        if (!string.IsNullOrWhiteSpace(channel))
            query = query.Where(x => x.Channel == channel);
        if (activeStatus == "Active")
            query = query.Where(x => x.IsActive);
        if (activeStatus == "Inactive")
            query = query.Where(x => !x.IsActive);

        return View(new CommunicationTemplateListViewModel
        {
            Search = search,
            PurposeKey = purposeKey,
            Channel = channel,
            ActiveStatus = activeStatus,
            Purposes = await BuildPurposeItemsAsync(ct),
            Channels = BuildChannelItems(),
            Templates = await query.OrderBy(x => x.PurposeKey).ThenBy(x => x.Channel).ThenByDescending(x => x.IsDefault).ToListAsync(ct)
        });
    }

    [HttpGet("/CommunicationTemplates/Create")]
    [RequirePermission("Communication Templates.add")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Create Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        var purpose = await _db.CommunicationPurposes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DisplayName).FirstOrDefaultAsync(ct);
        return View("Form", await PrepareFormAsync(new CommunicationTemplateFormViewModel
        {
            PurposeId = purpose?.Id ?? 0,
            PurposeKey = purpose?.PurposeKey ?? string.Empty,
            Channel = CommunicationChannels.Email,
            IsActive = true,
            IsDefault = true
        }, ct));
    }

    [HttpPost("/CommunicationTemplates/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Communication Templates.add")]
    public async Task<IActionResult> Create(CommunicationTemplateFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        await ValidateFormAsync(model, ct);
        if (!ModelState.IsValid)
            return View("Form", await PrepareFormAsync(model, ct));

        var purpose = await _db.CommunicationPurposes.FirstAsync(x => x.Id == model.PurposeId, ct);
        var now = DateTime.Now;
        var entity = new CommunicationTemplate
        {
            PurposeId = purpose.Id,
            PurposeKey = purpose.PurposeKey,
            Channel = model.Channel,
            TemplateName = model.TemplateName.Trim(),
            Subject = NormalizeNullable(model.Subject),
            Body = model.Body.Trim(),
            ExternalTemplateId = NormalizeNullable(model.ExternalTemplateId),
            Language = null,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive,
            CreatedAt = now
        };

        if (entity.IsDefault && entity.IsActive)
            await ClearOtherDefaultsAsync(entity.PurposeKey, entity.Channel, entity.Language, null, ct);

        _db.CommunicationTemplates.Add(entity);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Communication template created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/CommunicationTemplates/Edit/{id:int}")]
    [RequirePermission("Communication Templates.edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        var entity = await _db.CommunicationTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return NotFound();

        return View("Form", await PrepareFormAsync(new CommunicationTemplateFormViewModel
        {
            Id = entity.Id,
            PurposeId = entity.PurposeId,
            PurposeKey = entity.PurposeKey,
            Channel = entity.Channel,
            TemplateName = entity.TemplateName,
            Subject = entity.Subject,
            Body = entity.Body,
            ExternalTemplateId = entity.ExternalTemplateId,
            Language = null,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive
        }, ct));
    }

    [HttpPost("/CommunicationTemplates/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Communication Templates.edit")]
    public async Task<IActionResult> Edit(int id, CommunicationTemplateFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;
        model.Id = id;

        await ValidateFormAsync(model, ct);
        if (!ModelState.IsValid)
            return View("Form", await PrepareFormAsync(model, ct));

        var entity = await _db.CommunicationTemplates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return NotFound();

        var purpose = await _db.CommunicationPurposes.FirstAsync(x => x.Id == model.PurposeId, ct);
        entity.PurposeId = purpose.Id;
        entity.PurposeKey = purpose.PurposeKey;
        entity.Channel = model.Channel;
        entity.TemplateName = model.TemplateName.Trim();
        entity.Subject = NormalizeNullable(model.Subject);
        entity.Body = model.Body.Trim();
        entity.ExternalTemplateId = NormalizeNullable(model.ExternalTemplateId);
        entity.Language = null;
        entity.IsDefault = model.IsDefault;
        entity.IsActive = model.IsActive;
        entity.UpdatedAt = DateTime.Now;

        if (entity.IsDefault && entity.IsActive)
            await ClearOtherDefaultsAsync(entity.PurposeKey, entity.Channel, entity.Language, entity.Id, ct);

        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Communication template updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/CommunicationTemplates/Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        var entity = await _db.CommunicationTemplates.Include(x => x.Purpose).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return entity is null ? NotFound() : View(entity);
    }

    [HttpGet("/CommunicationTemplates/Preview/{id:int}")]
    public async Task<IActionResult> Preview(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Preview Communication Template";
        ViewData["ActiveMenu"] = AppConstants.Modules.CommunicationTemplates;

        var entity = await _db.CommunicationTemplates.Include(x => x.Purpose).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return NotFound();

        var samples = BuildSampleValues(ParsePlaceholders(entity.Purpose?.AllowedPlaceholders));
        return View(new CommunicationTemplatePreviewViewModel
        {
            Id = entity.Id,
            PurposeKey = entity.PurposeKey,
            Channel = entity.Channel,
            TemplateName = entity.TemplateName,
            Subject = entity.Subject,
            Body = entity.Body,
            PreviewSubject = _renderer.Render(entity.Subject ?? entity.TemplateName, samples),
            PreviewBody = _renderer.Render(entity.Body, samples),
            SampleValues = samples
        });
    }

    [HttpPost("/CommunicationTemplates/ToggleStatus/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Communication Templates.edit")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
    {
        var entity = await _db.CommunicationTemplates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null)
            return NotFound();

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = entity.IsActive ? "Template activated." : "Template deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/CommunicationTemplates/AllowedPlaceholders")]
    public async Task<IActionResult> AllowedPlaceholders(int purposeId, CancellationToken ct)
    {
        var purpose = await _db.CommunicationPurposes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == purposeId, ct);
        return Json(purpose is null ? Array.Empty<string>() : ParsePlaceholders(purpose.AllowedPlaceholders));
    }

    private async Task<CommunicationTemplateFormViewModel> PrepareFormAsync(CommunicationTemplateFormViewModel model, CancellationToken ct)
    {
        model.Purposes = await BuildPurposeItemsAsync(ct);
        model.Channels = BuildChannelItems();
        var purpose = await _db.CommunicationPurposes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.PurposeId, ct);
        model.PurposeKey = purpose?.PurposeKey ?? model.PurposeKey;
        model.Language = null;
        model.AllowedPlaceholders = ParsePlaceholders(purpose?.AllowedPlaceholders);
        return model;
    }

    private async Task ValidateFormAsync(CommunicationTemplateFormViewModel model, CancellationToken ct)
    {
        model.TemplateName = model.TemplateName?.Trim() ?? string.Empty;
        model.Body = model.Body?.Trim() ?? string.Empty;
        model.Channel = model.Channel?.Trim() ?? string.Empty;
        model.Language = null;

        var purpose = await _db.CommunicationPurposes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.PurposeId && x.IsActive, ct);
        if (purpose is null)
            ModelState.AddModelError(nameof(model.PurposeId), "Please select a valid purpose.");
        if (!CommunicationChannels.All.Contains(model.Channel))
            ModelState.AddModelError(nameof(model.Channel), "Please select a valid channel.");
        if (string.IsNullOrWhiteSpace(model.TemplateName))
            ModelState.AddModelError(nameof(model.TemplateName), "Template name is required.");
        if (string.IsNullOrWhiteSpace(model.Body))
            ModelState.AddModelError(nameof(model.Body), "Template body is required.");
        if (model.Channel == CommunicationChannels.Email && string.IsNullOrWhiteSpace(model.Subject))
            ModelState.AddModelError(nameof(model.Subject), "Subject is required for email templates.");
        if (model.Channel == CommunicationChannels.InApp && string.IsNullOrWhiteSpace(model.Subject))
            ModelState.AddModelError(nameof(model.Subject), "Subject is required for in-app notifications.");
        if (model.Channel is CommunicationChannels.Sms or CommunicationChannels.WhatsApp)
            model.Subject = null;
        if (model.Channel is CommunicationChannels.Email or CommunicationChannels.InApp)
            model.ExternalTemplateId = null;

        if (purpose is not null)
        {
            var allowed = ParsePlaceholders(purpose.AllowedPlaceholders);
            var unknown = _renderer.ExtractPlaceholders($"{model.Subject} {model.Body}")
                .Where(x => !allowed.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (unknown.Count > 0)
                ModelState.AddModelError(nameof(model.Body), $"Unknown placeholder(s): {string.Join(", ", unknown)}.");
        }
    }

    private async Task ClearOtherDefaultsAsync(string purposeKey, string channel, string? language, int? exceptId, CancellationToken ct)
    {
        var rows = await _db.CommunicationTemplates
            .Where(x => x.PurposeKey == purposeKey
                && x.Channel == channel
                && x.Language == language
                && x.IsDefault
                && x.IsActive
                && !x.IsDeleted
                && (!exceptId.HasValue || x.Id != exceptId.Value))
            .ToListAsync(ct);
        foreach (var row in rows)
            row.IsDefault = false;
    }

    private async Task<List<SelectListItem>> BuildPurposeItemsAsync(CancellationToken ct)
        => await _db.CommunicationPurposes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayName)
            .Select(x => new SelectListItem($"{x.DisplayName} ({x.PurposeKey})", x.Id.ToString()))
            .ToListAsync(ct);

    private static List<SelectListItem> BuildChannelItems()
        => CommunicationChannels.All.Select(x => new SelectListItem(x, x)).ToList();

    private static Dictionary<string, string> BuildSampleValues(IReadOnlyList<string> placeholders)
        => placeholders.ToDictionary(x => x, x => x switch
        {
            "ConsumerName" => "Rajesh Kumar",
            "ApplicationNo" => "NC202606020001",
            "QueryNo" => "QRY202606020001",
            "ChallanNo" => "1-2000000001",
            "Amount" => "500.00",
            "Status" => "Submitted",
            "Date" => DateTime.Now.ToString("dd MMM yyyy"),
            "Otp" => "123456",
            "StageName" => "Initial Review",
            "ConsumerNo" => "C0001001",
            _ => $"Sample {x}"
        });

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IReadOnlyList<string> ParsePlaceholders(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

}
