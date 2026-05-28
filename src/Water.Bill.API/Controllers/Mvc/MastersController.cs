using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
[Route("Masters")]
public class MastersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;

    public MastersController(ApplicationDbContext db, IPermissionService permissionService)
    {
        _db = db;
        _permissionService = permissionService;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Index(string key, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "view", ct)) return Forbid();

        ViewData["Title"] = definition.Title;
        ViewData["ActiveMenu"] = definition.Module;

        return View("Index", new MasterListViewModel
        {
            Key = definition.Key,
            Title = definition.Title,
            Module = definition.Module,
            Description = definition.Description,
            Columns = definition.Columns.Select(x => new MasterColumnViewModel { Key = x.Key, Label = x.Label }).ToList(),
            Rows = await GetRowsAsync(definition.Key, ct)
        });
    }

    [HttpGet("{key}/Create")]
    public async Task<IActionResult> Create(string key, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "add", ct)) return Forbid();

        ViewData["Title"] = $"Create {definition.Title}";
        ViewData["ActiveMenu"] = definition.Module;
        return View("Form", await BuildFormAsync(definition, rowKey: null, isEdit: false, ct));
    }

    [HttpPost("{key}/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string key, IFormCollection form, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "add", ct)) return Forbid();

        ViewData["Title"] = $"Create {definition.Title}";
        ViewData["ActiveMenu"] = definition.Module;

        TrimFormValues(form, definition);
        await ValidateAsync(definition.Key, form, rowKey: null, ct);
        if (!ModelState.IsValid)
            return View("Form", await BuildFormAsync(definition, rowKey: null, isEdit: false, ct, form));

        await CreateRecordAsync(definition.Key, form, ct);
        TempData["SuccessMessage"] = $"{definition.Title} created.";
        return RedirectToAction(nameof(Index), new { key = definition.Key });
    }

    [HttpGet("{key}/Edit/{rowKey}")]
    public async Task<IActionResult> Edit(string key, string rowKey, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "edit", ct)) return Forbid();
        if (!await ExistsAsync(definition.Key, rowKey, ct)) return NotFound();

        ViewData["Title"] = $"Edit {definition.Title}";
        ViewData["ActiveMenu"] = definition.Module;
        return View("Form", await BuildFormAsync(definition, rowKey, isEdit: true, ct));
    }

    [HttpPost("{key}/Edit/{rowKey}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string key, string rowKey, IFormCollection form, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "edit", ct)) return Forbid();

        ViewData["Title"] = $"Edit {definition.Title}";
        ViewData["ActiveMenu"] = definition.Module;

        TrimFormValues(form, definition);
        await ValidateAsync(definition.Key, form, rowKey, ct);
        if (!ModelState.IsValid)
            return View("Form", await BuildFormAsync(definition, rowKey, isEdit: true, ct, form));

        var updated = await UpdateRecordAsync(definition.Key, rowKey, form, ct);
        if (!updated) return NotFound();

        TempData["SuccessMessage"] = $"{definition.Title} updated.";
        return RedirectToAction(nameof(Index), new { key = definition.Key });
    }

    [HttpGet("{key}/Details/{rowKey}")]
    public async Task<IActionResult> Details(string key, string rowKey, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "view", ct)) return Forbid();
        if (!await ExistsAsync(definition.Key, rowKey, ct)) return NotFound();

        ViewData["Title"] = $"View {definition.Title}";
        ViewData["ActiveMenu"] = definition.Module;
        return View("Details", await BuildFormAsync(definition, rowKey, isEdit: true, ct));
    }

    [HttpPost("{key}/Delete/{rowKey}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string key, string rowKey, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "delete", ct)) return Forbid();

        var changed = await DeactivateAsync(definition.Key, rowKey, ct);
        TempData[changed ? "SuccessMessage" : "ErrorMessage"] = changed
            ? $"{definition.Title} deactivated."
            : $"{definition.Title} was not found.";

        return RedirectToAction(nameof(Index), new { key = definition.Key });
    }

    [HttpPost("{key}/ToggleStatus/{rowKey}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string key, string rowKey, IFormCollection form, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return NotFound();
        if (!await HasPermissionAsync(definition.Module, "delete", ct)) return Forbid();

        var activating = Get(form, "Status") != "0";
        var changed = await UpdateStatusAsync(definition.Key, rowKey, form, ct);
        TempData[changed ? "SuccessMessage" : "ErrorMessage"] = changed
            ? $"{definition.Title} {(activating ? "activated" : "deactivated")}."
            : $"{definition.Title} was not found.";

        return RedirectToAction(nameof(Index), new { key = definition.Key });
    }

    private async Task<bool> HasPermissionAsync(string module, string action, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue("RoleId"), out var roleId))
            return false;

        return await _permissionService.HasPermissionAsync(roleId, module, action, ct);
    }

    private async Task<IReadOnlyList<MasterRowViewModel>> GetRowsAsync(string key, CancellationToken ct)
    {
        switch (key)
        {
            case "sectors":
            {
                var rows = await _db.SectorDetails.AsNoTracking()
                .OrderBy(x => x.OrderBy ?? int.MaxValue).ThenBy(x => x.SectorNo)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.SNo.ToString()),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["SectorId"] = x.SectorId,
                        ["SectorNo"] = x.SectorNo,
                        ["OrderBy"] = x.OrderBy.ToString(),
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "blocks":
            {
                var rows = await _db.BlockDetails.AsNoTracking()
                .OrderBy(x => x.SectorId).ThenBy(x => x.Block)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey($"{x.SectorId}||{x.Block}"),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["SectorId"] = x.SectorId,
                        ["Block"] = x.Block,
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "pipe-sizes":
            {
                var rows = await _db.PipeSizeMasters.AsNoTracking()
                .OrderBy(x => x.PipeSize)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.PipeSizeId.ToString()),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["PipeSize"] = x.PipeSize.ToString(),
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "connection-categories":
            {
                var rows = await _db.MasterConnectionTypeDetails.AsNoTracking()
                .OrderBy(x => x.ConName)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.ConId),
                    IsActive = x.Status == null || x.Status == "1",
                    Values = new()
                    {
                        ["ConId"] = x.ConId,
                        ["ConName"] = x.ConName,
                        ["ConMainId"] = x.ConMainId,
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "connection-sub-types":
            {
                var rows = await _db.MasterConnectionTypeDetailsTrans.AsNoTracking()
                .OrderBy(x => x.ConId).ThenBy(x => x.SubConName)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.SubConId.ToString()),
                    IsActive = x.Status == null || x.Status == "1",
                    Values = new()
                    {
                        ["ConId"] = x.ConId,
                        ["SubConName"] = x.SubConName,
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "connection-types":
            {
                var rows = await _db.ConnectionTypeMsts.AsNoTracking()
                .OrderBy(x => x.ConnectionName)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.AutoId.ToString()),
                    IsActive = x.Status == null || x.Status == true,
                    Values = new()
                    {
                        ["ConnectionName"] = x.ConnectionName,
                        ["ConnectionMainId"] = x.ConnectionMainId,
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "villages":
            {
                var rows = await _db.VillageDetails.AsNoTracking()
                .OrderBy(x => x.VillageName)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.VillageNo.ToString()),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["VillageId"] = x.VillageId.ToString(),
                        ["VillageName"] = x.VillageName,
                        ["VillageStr"] = x.VillageStr,
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "document-types":
            {
                var rows = await _db.MasterDocumentUploads.AsNoTracking()
                .OrderBy(x => x.DocumentId)
                .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.DocumentId.ToString()),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["DocumentId"] = x.DocumentId.ToString(),
                        ["DocumentName"] = x.DocumentName,
                        ["DocFor"] = x.DocFor,
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "payment-modes":
            {
                var rows = await _db.PaymentModeMsts.AsNoTracking()
                    .OrderBy(x => x.PaymentModeName)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.AutoId.ToString()),
                    IsActive = IsActive(x.IsActive),
                    Values = new()
                    {
                        ["AutoId"] = x.AutoId.ToString(),
                        ["PaymentModeName"] = x.PaymentModeName,
                        ["Status"] = FormatStatus(x.IsActive)
                    }
                }).ToList();
            }
            case "payment-types":
            {
                var rows = await _db.PaymentTypeMsts.AsNoTracking()
                    .OrderBy(x => x.PaymentTypeName)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.AutoId.ToString()),
                    IsActive = x.IsActive != false,
                    Values = new()
                    {
                        ["AutoId"] = x.AutoId.ToString(),
                        ["PaymentTypeName"] = x.PaymentTypeName,
                        ["Status"] = FormatStatus(x.IsActive)
                    }
                }).ToList();
            }
            case "banks":
            {
                var rows = await _db.JalBankMasters.AsNoTracking()
                    .OrderBy(x => x.BankName)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.Id.ToString()),
                    IsActive = x.Status == null || x.Status == 1,
                    Values = new()
                    {
                        ["BankId"] = x.BankId,
                        ["BankName"] = x.BankName,
                        ["AccountNo"] = x.AccountNo,
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "ndc-amounts":
            {
                var rows = await _db.MasterNocAmts.AsNoTracking()
                    .OrderBy(x => x.ConName)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.ConId),
                    IsActive = IsActive(x.Status),
                    Values = new()
                    {
                        ["ConId"] = x.ConId,
                        ["ConName"] = x.ConName,
                        ["ConMainId"] = x.ConMainId,
                        ["NocAmt"] = x.NocAmt.ToString(),
                        ["Amount"] = x.Amount,
                        ["Sgst"] = x.Sgst,
                        ["Cgst"] = x.Cgst,
                        ["ExpiryTime"] = x.ExpiryTime,
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "application-statuses":
            {
                var rows = await _db.ApplicationStatuses.AsNoTracking()
                    .OrderBy(x => x.StatusName)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.AutoId.ToString()),
                    IsActive = x.IsActive != false,
                    Values = new()
                    {
                        ["AutoId"] = x.AutoId.ToString(),
                        ["StatusName"] = x.StatusName,
                        ["Status"] = FormatStatus(x.IsActive)
                    }
                }).ToList();
            }
            case "rate-categories":
            {
                var rows = await _db.JalRateMasters.AsNoTracking()
                    .OrderBy(x => x.PropertyType)
                    .ThenBy(x => x.IdT)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.Id.ToString()),
                    IsActive = IsActive(x.Status),
                    Values = new()
                    {
                        ["Id"] = x.Id.ToString(),
                        ["PropertyType"] = x.PropertyType,
                        ["IdT"] = x.IdT,
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            case "rates":
            {
                var rows = await _db.JalRateTrans.AsNoTracking()
                    .Include(x => x.IdNavigation)
                    .OrderBy(x => x.IdNavigation!.PropertyType)
                    .ThenBy(x => x.PipeSize)
                    .ThenBy(x => x.AreaStart)
                    .ThenByDescending(x => x.EffFrom)
                    .ToListAsync(ct);
                return rows.Select(x => new MasterRowViewModel
                {
                    Key = EncodeKey(x.Sid.ToString()),
                    IsActive = IsActive(x.Status),
                    Values = new()
                    {
                        ["Category"] = x.IdNavigation?.PropertyType ?? x.Id.ToString(),
                        ["AreaStart"] = x.AreaStart.ToString(),
                        ["AreaEnd"] = x.AreaEnd.ToString(),
                        ["PipeSize"] = x.PipeSize.ToString(),
                        ["Regular"] = x.Regular.ToString(),
                        ["Temporary"] = x.Temporary.ToString(),
                        ["CessRate"] = x.CessRate.ToString(),
                        ["EffFrom"] = ToDisplayDate(x.EffFrom),
                        ["EffTo"] = ToDisplayDate(x.EffTo),
                        ["DevType"] = FormatDivision(x.DevType),
                        ["Status"] = FormatStatus(x.Status)
                    }
                }).ToList();
            }
            default:
                return [];
        }
    }

    private async Task<MasterFormViewModel> BuildFormAsync(
        MasterDefinition definition,
        string? rowKey,
        bool isEdit,
        CancellationToken ct,
        IFormCollection? posted = null)
    {
        var values = posted is null && rowKey is not null
            ? await GetRecordValuesAsync(definition.Key, rowKey, ct)
            : posted?.Keys.ToDictionary(x => x, x => posted[x].ToString(), StringComparer.OrdinalIgnoreCase) ?? [];

        var fields = new List<MasterFieldViewModel>();
        foreach (var field in definition.Fields)
        {
            fields.Add(new MasterFieldViewModel
            {
                Name = field.Name,
                Label = field.Label,
                Value = values.GetValueOrDefault(field.Name),
                InputType = field.InputType,
                IsRequired = field.IsRequired,
                IsReadOnly = isEdit && field.ReadOnlyOnEdit,
                Options = await GetFieldOptionsAsync(definition.Key, field.Name, ct)
            });
        }

        return new MasterFormViewModel
        {
            Key = definition.Key,
            RowKey = rowKey,
            Title = definition.Title,
            Module = definition.Module,
            IsEdit = isEdit,
            Fields = fields
        };
    }

    private async Task<Dictionary<string, string?>> GetRecordValuesAsync(string key, string rowKey, CancellationToken ct)
    {
        var decoded = DecodeKey(rowKey);
        switch (key)
        {
            case "sectors":
                var sector = await _db.SectorDetails.AsNoTracking().FirstOrDefaultAsync(x => x.SNo == int.Parse(decoded), ct);
                return sector is null ? [] : new()
                {
                    ["SectorId"] = sector.SectorId,
                    ["SectorNo"] = sector.SectorNo,
                    ["OrderBy"] = sector.OrderBy.ToString(),
                    ["DevType"] = sector.DevType.ToString(),
                    ["Status"] = (sector.Status ?? 1).ToString()
                };
            case "blocks":
                var blockKey = decoded.Split("||");
                var block = await _db.BlockDetails.AsNoTracking().FirstOrDefaultAsync(x => x.SectorId == blockKey[0] && x.Block == blockKey[1], ct);
                return block is null ? [] : new()
                {
                    ["SectorId"] = block.SectorId,
                    ["Block"] = block.Block,
                    ["DevType"] = block.DevType.ToString(),
                    ["Status"] = (block.Status ?? 1).ToString()
                };
            case "pipe-sizes":
                var pipe = await _db.PipeSizeMasters.AsNoTracking().FirstOrDefaultAsync(x => x.PipeSizeId == int.Parse(decoded), ct);
                return pipe is null ? [] : new()
                {
                    ["PipeSize"] = pipe.PipeSize.ToString(),
                    ["DevType"] = pipe.DevType.ToString(),
                    ["Status"] = (pipe.Status ?? 1).ToString()
                };
            case "connection-categories":
                var category = await _db.MasterConnectionTypeDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                return category is null ? [] : new()
                {
                    ["ConId"] = category.ConId,
                    ["ConName"] = category.ConName,
                    ["ConMainId"] = category.ConMainId,
                    ["DevType"] = category.DevType.ToString(),
                    ["Status"] = category.Status ?? "1"
                };
            case "connection-sub-types":
                var subtype = await _db.MasterConnectionTypeDetailsTrans.AsNoTracking().FirstOrDefaultAsync(x => x.SubConId == int.Parse(decoded), ct);
                return subtype is null ? [] : new()
                {
                    ["ConId"] = subtype.ConId,
                    ["SubConName"] = subtype.SubConName,
                    ["DevType"] = subtype.DevType.ToString(),
                    ["Status"] = subtype.Status ?? "1"
                };
            case "connection-types":
                var type = await _db.ConnectionTypeMsts.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                return type is null ? [] : new()
                {
                    ["ConnectionName"] = type.ConnectionName,
                    ["ConnectionMainId"] = type.ConnectionMainId,
                    ["Status"] = (type.Status ?? true) ? "1" : "0"
                };
            case "villages":
                var village = await _db.VillageDetails.AsNoTracking().FirstOrDefaultAsync(x => x.VillageNo == int.Parse(decoded), ct);
                return village is null ? [] : new()
                {
                    ["VillageId"] = village.VillageId.ToString(),
                    ["VillageName"] = village.VillageName,
                    ["VillageStr"] = village.VillageStr,
                    ["DevType"] = village.DevType.ToString(),
                    ["Status"] = (village.Status ?? 1).ToString()
                };
            case "document-types":
                var doc = await _db.MasterDocumentUploads.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentId == int.Parse(decoded), ct);
                return doc is null ? [] : new()
                {
                    ["DocumentId"] = doc.DocumentId.ToString(),
                    ["DocumentName"] = doc.DocumentName,
                    ["DocFor"] = doc.DocFor,
                    ["Status"] = (doc.Status ?? 1).ToString()
                };
            case "payment-modes":
                var paymentMode = await _db.PaymentModeMsts.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                return paymentMode is null ? [] : new()
                {
                    ["AutoId"] = paymentMode.AutoId.ToString(),
                    ["PaymentModeName"] = paymentMode.PaymentModeName,
                    ["Status"] = IsActive(paymentMode.IsActive) ? "1" : "0"
                };
            case "payment-types":
                var paymentType = await _db.PaymentTypeMsts.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                return paymentType is null ? [] : new()
                {
                    ["AutoId"] = paymentType.AutoId.ToString(),
                    ["PaymentTypeName"] = paymentType.PaymentTypeName,
                    ["Status"] = (paymentType.IsActive ?? true) ? "1" : "0"
                };
            case "banks":
                var bank = await _db.JalBankMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                return bank is null ? [] : new()
                {
                    ["BankId"] = bank.BankId,
                    ["BankName"] = bank.BankName,
                    ["AccountNo"] = bank.AccountNo,
                    ["Status"] = (bank.Status ?? 1).ToString()
                };
            case "ndc-amounts":
                var ndc = await _db.MasterNocAmts.AsNoTracking().FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                return ndc is null ? [] : new()
                {
                    ["ConId"] = ndc.ConId,
                    ["ConName"] = ndc.ConName,
                    ["ConMainId"] = ndc.ConMainId,
                    ["NocAmt"] = ndc.NocAmt.ToString(),
                    ["Amount"] = ndc.Amount,
                    ["Sgst"] = ndc.Sgst,
                    ["Cgst"] = ndc.Cgst,
                    ["ExpiryTime"] = ndc.ExpiryTime,
                    ["Status"] = IsActive(ndc.Status) ? "1" : "0"
                };
            case "application-statuses":
                var applicationStatus = await _db.ApplicationStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                return applicationStatus is null ? [] : new()
                {
                    ["AutoId"] = applicationStatus.AutoId.ToString(),
                    ["StatusName"] = applicationStatus.StatusName,
                    ["Status"] = (applicationStatus.IsActive ?? true) ? "1" : "0"
                };
            case "rate-categories":
                var rateCategory = await _db.JalRateMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                return rateCategory is null ? [] : new()
                {
                    ["Id"] = rateCategory.Id.ToString(),
                    ["PropertyType"] = rateCategory.PropertyType,
                    ["IdT"] = rateCategory.IdT,
                    ["DevType"] = rateCategory.DevType.ToString(),
                    ["Status"] = IsActive(rateCategory.Status) ? "1" : "0"
                };
            case "rates":
                var rate = await _db.JalRateTrans.AsNoTracking().FirstOrDefaultAsync(x => x.Sid == int.Parse(decoded), ct);
                return rate is null ? [] : new()
                {
                    ["Id"] = rate.Id.ToString(),
                    ["AreaStart"] = rate.AreaStart.ToString(),
                    ["AreaEnd"] = rate.AreaEnd.ToString(),
                    ["Regular"] = rate.Regular.ToString(),
                    ["Temporary"] = rate.Temporary.ToString(),
                    ["PipeSize"] = rate.PipeSize.ToString(),
                    ["CessRate"] = rate.CessRate.ToString(),
                    ["EffFrom"] = ToInputDate(rate.EffFrom),
                    ["EffTo"] = ToInputDate(rate.EffTo),
                    ["DevType"] = rate.DevType.ToString(),
                    ["Status"] = IsActive(rate.Status) ? "1" : "0"
                };
            default:
                return [];
        }
    }

    private async Task<IReadOnlyList<MasterOptionViewModel>> GetFieldOptionsAsync(string key, string fieldName, CancellationToken ct)
    {
        if (fieldName == "DevType")
        {
            return AppConstants.Divisions.Options
                .Select(x => new MasterOptionViewModel { Value = x.DevType.ToString(), Text = x.DisplayText })
                .ToList();
        }

        if (key == "blocks" && fieldName == "SectorId")
        {
            return await _db.SectorDetails.AsNoTracking()
                .Where(x => x.Status == null || x.Status == 1)
                .OrderBy(x => x.OrderBy ?? int.MaxValue)
                .ThenBy(x => x.SectorNo)
                .Select(x => new MasterOptionViewModel { Value = x.SectorId, Text = x.SectorNo ?? x.SectorId })
                .ToListAsync(ct);
        }

        if (key == "connection-sub-types" && fieldName == "ConId")
        {
            return await _db.MasterConnectionTypeDetails.AsNoTracking()
                .Where(x => x.Status == null || x.Status == "1")
                .OrderBy(x => x.ConName)
                .Select(x => new MasterOptionViewModel { Value = x.ConId, Text = x.ConName ?? x.ConId })
                .ToListAsync(ct);
        }

        if (key == "rates" && fieldName == "Id")
        {
            return await _db.JalRateMasters.AsNoTracking()
                .Where(x => x.Status == null || x.Status == "1")
                .OrderBy(x => x.PropertyType)
                .Select(x => new MasterOptionViewModel
                {
                    Value = x.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(x.IdT) ? x.PropertyType ?? x.Id.ToString() : $"{x.PropertyType} ({x.IdT})"
                })
                .ToListAsync(ct);
        }

        if (key == "rates" && fieldName == "PipeSize")
        {
            return await _db.PipeSizeMasters.AsNoTracking()
                .Where(x => x.Status == null || x.Status == 1)
                .OrderBy(x => x.PipeSize)
                .Select(x => new MasterOptionViewModel { Value = x.PipeSize.ToString(), Text = x.PipeSize.ToString() })
                .ToListAsync(ct);
        }

        if (fieldName == "Status")
        {
            return
            [
                new() { Value = "1", Text = "Active" },
                new() { Value = "0", Text = "Inactive" }
            ];
        }

        return [];
    }

    private async Task ValidateAsync(string key, IFormCollection form, string? rowKey, CancellationToken ct)
    {
        var definition = GetDefinition(key);
        if (definition is null) return;

        foreach (var field in definition.Fields.Where(x => x.IsRequired))
        {
            if (string.IsNullOrWhiteSpace(form[field.Name]))
                ModelState.AddModelError(field.Name, $"{field.Label} is required.");
        }

        if (!ModelState.IsValid) return;

        var current = rowKey is null ? null : DecodeKey(rowKey);
        switch (key)
        {
            case "sectors":
                if (await _db.SectorDetails.AnyAsync(x =>
                    x.SectorId == Get(form, "SectorId") &&
                    (current == null || x.SNo != int.Parse(current)), ct))
                    ModelState.AddModelError("SectorId", "Duplicate sector code is not allowed.");
                break;
            case "blocks":
                var currentBlock = current?.Split("||");
                if (await _db.BlockDetails.AnyAsync(x =>
                    x.SectorId == Get(form, "SectorId") &&
                    x.Block == Get(form, "Block") &&
                    (currentBlock == null || x.SectorId != currentBlock[0] || x.Block != currentBlock[1]), ct))
                    ModelState.AddModelError("Block", "Duplicate block under the selected sector is not allowed.");
                break;
            case "pipe-sizes":
                var pipeSize = ToInt(form, "PipeSize");
                if (await _db.PipeSizeMasters.AnyAsync(x =>
                    x.PipeSize == pipeSize &&
                    (current == null || x.PipeSizeId != int.Parse(current)), ct))
                    ModelState.AddModelError("PipeSize", "Duplicate pipe size is not allowed.");
                break;
            case "connection-categories":
                if (await _db.MasterConnectionTypeDetails.AnyAsync(x =>
                    x.ConId == Get(form, "ConId") &&
                    (current == null || x.ConId != current), ct))
                    ModelState.AddModelError("ConId", "Duplicate connection category code is not allowed.");
                break;
            case "connection-sub-types":
                var subConId = current is null ? 0 : int.Parse(current);
                if (await _db.MasterConnectionTypeDetailsTrans.AnyAsync(x =>
                    x.ConId == Get(form, "ConId") &&
                    x.SubConName == Get(form, "SubConName") &&
                    x.SubConId != subConId, ct))
                    ModelState.AddModelError("SubConName", "Duplicate sub-type under the selected category is not allowed.");
                break;
            case "connection-types":
                var typeId = current is null ? 0 : int.Parse(current);
                if (await _db.ConnectionTypeMsts.AnyAsync(x =>
                    x.ConnectionName == Get(form, "ConnectionName") &&
                    x.AutoId != typeId, ct))
                    ModelState.AddModelError("ConnectionName", "Duplicate connection type is not allowed.");
                break;
            case "villages":
                var villageNo = current is null ? 0 : int.Parse(current);
                if (await _db.VillageDetails.AnyAsync(x =>
                    x.VillageName == Get(form, "VillageName") &&
                    x.VillageNo != villageNo, ct))
                    ModelState.AddModelError("VillageName", "Duplicate village is not allowed.");
                break;
            case "document-types":
                var documentId = current is null ? 0 : int.Parse(current);
                if (await _db.MasterDocumentUploads.AnyAsync(x =>
                    x.DocumentName == Get(form, "DocumentName") &&
                    x.DocumentId != documentId, ct))
                    ModelState.AddModelError("DocumentName", "Duplicate document type is not allowed.");
                break;
            case "payment-modes":
                var paymentModeId = current is null ? 0 : int.Parse(current);
                if (await _db.PaymentModeMsts.AnyAsync(x =>
                    x.PaymentModeName == Get(form, "PaymentModeName") &&
                    x.AutoId != paymentModeId, ct))
                    ModelState.AddModelError("PaymentModeName", "Duplicate payment mode is not allowed.");
                break;
            case "payment-types":
                var paymentTypeId = current is null ? 0 : int.Parse(current);
                if (await _db.PaymentTypeMsts.AnyAsync(x =>
                    x.PaymentTypeName == Get(form, "PaymentTypeName") &&
                    x.AutoId != paymentTypeId, ct))
                    ModelState.AddModelError("PaymentTypeName", "Duplicate payment type is not allowed.");
                break;
            case "banks":
                var bankId = current is null ? 0 : int.Parse(current);
                if (await _db.JalBankMasters.AnyAsync(x =>
                    x.BankId == Get(form, "BankId") &&
                    x.Id != bankId, ct))
                    ModelState.AddModelError("BankId", "Duplicate bank code is not allowed.");
                break;
            case "ndc-amounts":
                if (await _db.MasterNocAmts.AnyAsync(x =>
                    x.ConId == Get(form, "ConId") &&
                    (current == null || x.ConId != current), ct))
                    ModelState.AddModelError("ConId", "Duplicate NDC amount code is not allowed.");
                break;
            case "application-statuses":
                var applicationStatusId = current is null ? 0 : int.Parse(current);
                if (await _db.ApplicationStatuses.AnyAsync(x =>
                    x.StatusName == Get(form, "StatusName") &&
                    x.AutoId != applicationStatusId, ct))
                    ModelState.AddModelError("StatusName", "Duplicate application status is not allowed.");
                break;
            case "rate-categories":
                var rateCategoryId = current is null ? 0 : int.Parse(current);
                if (await _db.JalRateMasters.AnyAsync(x =>
                    x.IdT == Get(form, "IdT") &&
                    x.Id != rateCategoryId, ct))
                    ModelState.AddModelError("IdT", "Duplicate rate category code is not allowed.");
                if (await _db.JalRateMasters.AnyAsync(x =>
                    x.PropertyType == Get(form, "PropertyType") &&
                    x.Id != rateCategoryId, ct))
                    ModelState.AddModelError("PropertyType", "Duplicate rate category name is not allowed.");
                break;
            case "rates":
                var sid = current is null ? 0 : int.Parse(current);
                var selectedCategoryId = ToNullableInt(form, "Id");
                var selectedPipeSize = ToNullableInt(form, "PipeSize");
                var selectedDevType = ToNullableInt(form, "DevType");
                var selectedEffFrom = ToNullableDate(form, "EffFrom");
                if (selectedCategoryId is null || !await _db.JalRateMasters.AnyAsync(x => x.Id == selectedCategoryId, ct))
                    ModelState.AddModelError("Id", "Select a valid rate category.");
                if (!int.TryParse(Get(form, "AreaStart"), out var areaStart) || areaStart < 0)
                    ModelState.AddModelError("AreaStart", "Area start must be a valid number.");
                if (!int.TryParse(Get(form, "AreaEnd"), out var areaEnd) || areaEnd < 0)
                    ModelState.AddModelError("AreaEnd", "Area end must be a valid number.");
                if (areaStart > areaEnd)
                    ModelState.AddModelError("AreaEnd", "Area end must be greater than or equal to area start.");
                if (selectedEffFrom is null)
                    ModelState.AddModelError("EffFrom", "Effective from date is required.");
                if (selectedEffFrom is DateTime effFrom && ToNullableDate(form, "EffTo") is DateTime effTo && effFrom > effTo)
                    ModelState.AddModelError("EffTo", "Effective to date must be after effective from date.");
                if (ModelState.IsValid && await _db.JalRateTrans.AnyAsync(x =>
                    x.Id == selectedCategoryId &&
                    x.PipeSize == selectedPipeSize &&
                    x.AreaStart == areaStart &&
                    x.AreaEnd == areaEnd &&
                    x.EffFrom == selectedEffFrom &&
                    x.DevType == selectedDevType &&
                    x.Sid != sid, ct))
                    ModelState.AddModelError("AreaStart", "Duplicate rate slab for category, pipe size, area range, effective date, and division is not allowed.");
                break;
        }
    }

    private async Task CreateRecordAsync(string key, IFormCollection form, CancellationToken ct)
    {
        switch (key)
        {
            case "sectors":
                _db.SectorDetails.Add(new SectorDetail { SectorId = Get(form, "SectorId"), SectorNo = GetOptional(form, "SectorNo"), OrderBy = ToNullableInt(form, "OrderBy"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusInt(form) });
                break;
            case "blocks":
                _db.BlockDetails.Add(new BlockDetail { SectorId = Get(form, "SectorId"), Block = Get(form, "Block"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusInt(form) });
                break;
            case "pipe-sizes":
                _db.PipeSizeMasters.Add(new PipeSizeMaster { PipeSize = ToInt(form, "PipeSize"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusInt(form) });
                break;
            case "connection-categories":
                _db.MasterConnectionTypeDetails.Add(new MasterConnectionTypeDetail { ConId = Get(form, "ConId"), ConName = GetOptional(form, "ConName"), ConMainId = GetOptional(form, "ConMainId"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusString(form) });
                break;
            case "connection-sub-types":
                _db.MasterConnectionTypeDetailsTrans.Add(new MasterConnectionTypeDetailsTran { ConId = Get(form, "ConId"), SubConName = GetOptional(form, "SubConName"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusString(form) });
                break;
            case "connection-types":
                _db.ConnectionTypeMsts.Add(new ConnectionTypeMst { ConnectionName = GetOptional(form, "ConnectionName"), ConnectionMainId = GetOptional(form, "ConnectionMainId"), Status = ToStatusBool(form), CreatedOn = DateTime.Now });
                break;
            case "villages":
                _db.VillageDetails.Add(new VillageDetail { VillageId = ToNullableInt(form, "VillageId"), VillageName = Get(form, "VillageName"), VillageStr = GetOptional(form, "VillageStr"), DevType = ToNullableInt(form, "DevType"), Status = ToStatusInt(form) });
                break;
            case "document-types":
                _db.MasterDocumentUploads.Add(new MasterDocumentUpload { DocumentId = ToInt(form, "DocumentId"), DocumentName = GetOptional(form, "DocumentName"), DocFor = GetOptional(form, "DocFor"), Status = ToStatusInt(form) });
                break;
            case "payment-modes":
                _db.PaymentModeMsts.Add(new PaymentModeMst { PaymentModeName = Get(form, "PaymentModeName"), IsActive = ToStatusString(form), CreatedOn = DateTime.Now });
                break;
            case "payment-types":
                _db.PaymentTypeMsts.Add(new PaymentTypeMst { PaymentTypeName = Get(form, "PaymentTypeName"), IsActive = ToStatusBool(form), CreatedOn = DateTime.Now });
                break;
            case "banks":
                _db.JalBankMasters.Add(new JalBankMaster { BankId = Get(form, "BankId"), BankName = Get(form, "BankName"), AccountNo = GetOptional(form, "AccountNo"), Status = ToStatusInt(form), EntryDate = DateTime.Now });
                break;
            case "ndc-amounts":
                _db.MasterNocAmts.Add(new MasterNocAmt { ConId = Get(form, "ConId"), ConName = Get(form, "ConName"), ConMainId = GetOptional(form, "ConMainId"), NocAmt = ToNullableInt(form, "NocAmt"), Amount = GetOptional(form, "Amount"), Sgst = GetOptional(form, "Sgst"), Cgst = GetOptional(form, "Cgst"), ExpiryTime = GetOptional(form, "ExpiryTime"), Status = ToStatusString(form) });
                break;
            case "application-statuses":
                _db.ApplicationStatuses.Add(new ApplicationStatus { StatusName = Get(form, "StatusName"), IsActive = ToStatusBool(form) });
                break;
            case "rate-categories":
                _db.JalRateMasters.Add(new JalRateMaster
                {
                    Id = (await _db.JalRateMasters.MaxAsync(x => (int?)x.Id, ct) ?? 0) + 1,
                    PropertyType = Get(form, "PropertyType"),
                    IdT = Get(form, "IdT"),
                    DevType = ToNullableInt(form, "DevType"),
                    Status = ToStatusString(form)
                });
                break;
            case "rates":
                _db.JalRateTrans.Add(new JalRateTran
                {
                    Id = ToNullableInt(form, "Id"),
                    AreaStart = ToNullableInt(form, "AreaStart"),
                    AreaEnd = ToNullableInt(form, "AreaEnd"),
                    Regular = ToNullableDouble(form, "Regular"),
                    Temporary = ToNullableDouble(form, "Temporary"),
                    MainRate = 0,
                    EstRateReg = 0,
                    EstRateTemp = 0,
                    PipeSize = ToNullableInt(form, "PipeSize"),
                    CessRate = ToNullableDouble(form, "CessRate"),
                    EffFrom = ToNullableDate(form, "EffFrom"),
                    EffTo = ToNullableDate(form, "EffTo"),
                    DevType = ToNullableInt(form, "DevType"),
                    Status = ToStatusString(form)
                });
                break;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<bool> UpdateRecordAsync(string key, string rowKey, IFormCollection form, CancellationToken ct)
    {
        var decoded = DecodeKey(rowKey);
        switch (key)
        {
            case "sectors":
                var sector = await _db.SectorDetails.FirstOrDefaultAsync(x => x.SNo == int.Parse(decoded), ct);
                if (sector is null) return false;
                sector.SectorId = Get(form, "SectorId");
                sector.SectorNo = GetOptional(form, "SectorNo");
                sector.OrderBy = ToNullableInt(form, "OrderBy");
                sector.DevType = ToNullableInt(form, "DevType");
                sector.Status = ToStatusInt(form);
                break;
            case "blocks":
                var blockKey = decoded.Split("||");
                var block = await _db.BlockDetails.FirstOrDefaultAsync(x => x.SectorId == blockKey[0] && x.Block == blockKey[1], ct);
                if (block is null) return false;
                block.DevType = ToNullableInt(form, "DevType");
                block.Status = ToStatusInt(form);
                break;
            case "pipe-sizes":
                var pipe = await _db.PipeSizeMasters.FirstOrDefaultAsync(x => x.PipeSizeId == int.Parse(decoded), ct);
                if (pipe is null) return false;
                pipe.PipeSize = ToInt(form, "PipeSize");
                pipe.DevType = ToNullableInt(form, "DevType");
                pipe.Status = ToStatusInt(form);
                break;
            case "connection-categories":
                var category = await _db.MasterConnectionTypeDetails.FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                if (category is null) return false;
                category.ConName = GetOptional(form, "ConName");
                category.ConMainId = GetOptional(form, "ConMainId");
                category.DevType = ToNullableInt(form, "DevType");
                category.Status = ToStatusString(form);
                break;
            case "connection-sub-types":
                var subtype = await _db.MasterConnectionTypeDetailsTrans.FirstOrDefaultAsync(x => x.SubConId == int.Parse(decoded), ct);
                if (subtype is null) return false;
                subtype.ConId = Get(form, "ConId");
                subtype.SubConName = GetOptional(form, "SubConName");
                subtype.DevType = ToNullableInt(form, "DevType");
                subtype.Status = ToStatusString(form);
                break;
            case "connection-types":
                var type = await _db.ConnectionTypeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (type is null) return false;
                type.ConnectionName = GetOptional(form, "ConnectionName");
                type.ConnectionMainId = GetOptional(form, "ConnectionMainId");
                type.Status = ToStatusBool(form);
                type.LastUpdatedOn = DateTime.Now;
                break;
            case "villages":
                var village = await _db.VillageDetails.FirstOrDefaultAsync(x => x.VillageNo == int.Parse(decoded), ct);
                if (village is null) return false;
                village.VillageId = ToNullableInt(form, "VillageId");
                village.VillageName = Get(form, "VillageName");
                village.VillageStr = GetOptional(form, "VillageStr");
                village.DevType = ToNullableInt(form, "DevType");
                village.Status = ToStatusInt(form);
                break;
            case "document-types":
                var doc = await _db.MasterDocumentUploads.FirstOrDefaultAsync(x => x.DocumentId == int.Parse(decoded), ct);
                if (doc is null) return false;
                doc.DocumentName = GetOptional(form, "DocumentName");
                doc.DocFor = GetOptional(form, "DocFor");
                doc.Status = ToStatusInt(form);
                break;
            case "payment-modes":
                var paymentMode = await _db.PaymentModeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (paymentMode is null) return false;
                paymentMode.PaymentModeName = Get(form, "PaymentModeName");
                paymentMode.IsActive = ToStatusString(form);
                paymentMode.LastUpdateOn = DateTime.Now;
                break;
            case "payment-types":
                var paymentType = await _db.PaymentTypeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (paymentType is null) return false;
                paymentType.PaymentTypeName = Get(form, "PaymentTypeName");
                paymentType.IsActive = ToStatusBool(form);
                paymentType.LastUpdateOn = DateTime.Now;
                break;
            case "banks":
                var bank = await _db.JalBankMasters.FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                if (bank is null) return false;
                bank.BankId = Get(form, "BankId");
                bank.BankName = Get(form, "BankName");
                bank.AccountNo = GetOptional(form, "AccountNo");
                bank.Status = ToStatusInt(form);
                bank.ModifyDate = DateTime.Now;
                break;
            case "ndc-amounts":
                var ndc = await _db.MasterNocAmts.FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                if (ndc is null) return false;
                ndc.ConName = Get(form, "ConName");
                ndc.ConMainId = GetOptional(form, "ConMainId");
                ndc.NocAmt = ToNullableInt(form, "NocAmt");
                ndc.Amount = GetOptional(form, "Amount");
                ndc.Sgst = GetOptional(form, "Sgst");
                ndc.Cgst = GetOptional(form, "Cgst");
                ndc.ExpiryTime = GetOptional(form, "ExpiryTime");
                ndc.Status = ToStatusString(form);
                break;
            case "application-statuses":
                var applicationStatus = await _db.ApplicationStatuses.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (applicationStatus is null) return false;
                applicationStatus.StatusName = Get(form, "StatusName");
                applicationStatus.IsActive = ToStatusBool(form);
                break;
            case "rate-categories":
                var rateCategory = await _db.JalRateMasters.FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                if (rateCategory is null) return false;
                rateCategory.PropertyType = Get(form, "PropertyType");
                rateCategory.IdT = Get(form, "IdT");
                rateCategory.DevType = ToNullableInt(form, "DevType");
                rateCategory.Status = ToStatusString(form);
                break;
            case "rates":
                var rate = await _db.JalRateTrans.FirstOrDefaultAsync(x => x.Sid == int.Parse(decoded), ct);
                if (rate is null) return false;
                rate.Id = ToNullableInt(form, "Id");
                rate.AreaStart = ToNullableInt(form, "AreaStart");
                rate.AreaEnd = ToNullableInt(form, "AreaEnd");
                rate.Regular = ToNullableDouble(form, "Regular");
                rate.Temporary = ToNullableDouble(form, "Temporary");
                rate.PipeSize = ToNullableInt(form, "PipeSize");
                rate.CessRate = ToNullableDouble(form, "CessRate");
                rate.EffFrom = ToNullableDate(form, "EffFrom");
                rate.EffTo = ToNullableDate(form, "EffTo");
                rate.DevType = ToNullableInt(form, "DevType");
                rate.Status = ToStatusString(form);
                break;
            default:
                return false;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<bool> DeactivateAsync(string key, string rowKey, CancellationToken ct)
    {
        var values = new FormCollection(new Dictionary<string, StringValues> { ["Status"] = "0" });
        return await UpdateStatusAsync(key, rowKey, values, ct);
    }

    private async Task<bool> UpdateStatusAsync(string key, string rowKey, IFormCollection form, CancellationToken ct)
    {
        var decoded = DecodeKey(rowKey);
        switch (key)
        {
            case "sectors":
                var sector = await _db.SectorDetails.FirstOrDefaultAsync(x => x.SNo == int.Parse(decoded), ct);
                if (sector is null) return false;
                sector.Status = ToStatusInt(form);
                break;
            case "blocks":
                var blockKey = decoded.Split("||");
                var block = await _db.BlockDetails.FirstOrDefaultAsync(x => x.SectorId == blockKey[0] && x.Block == blockKey[1], ct);
                if (block is null) return false;
                block.Status = ToStatusInt(form);
                break;
            case "pipe-sizes":
                var pipe = await _db.PipeSizeMasters.FirstOrDefaultAsync(x => x.PipeSizeId == int.Parse(decoded), ct);
                if (pipe is null) return false;
                pipe.Status = ToStatusInt(form);
                break;
            case "connection-categories":
                var category = await _db.MasterConnectionTypeDetails.FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                if (category is null) return false;
                category.Status = ToStatusString(form);
                break;
            case "connection-sub-types":
                var subtype = await _db.MasterConnectionTypeDetailsTrans.FirstOrDefaultAsync(x => x.SubConId == int.Parse(decoded), ct);
                if (subtype is null) return false;
                subtype.Status = ToStatusString(form);
                break;
            case "connection-types":
                var type = await _db.ConnectionTypeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (type is null) return false;
                type.Status = false;
                type.LastUpdatedOn = DateTime.Now;
                break;
            case "villages":
                var village = await _db.VillageDetails.FirstOrDefaultAsync(x => x.VillageNo == int.Parse(decoded), ct);
                if (village is null) return false;
                village.Status = ToStatusInt(form);
                break;
            case "document-types":
                var doc = await _db.MasterDocumentUploads.FirstOrDefaultAsync(x => x.DocumentId == int.Parse(decoded), ct);
                if (doc is null) return false;
                doc.Status = ToStatusInt(form);
                break;
            case "payment-modes":
                var paymentMode = await _db.PaymentModeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (paymentMode is null) return false;
                paymentMode.IsActive = ToStatusString(form);
                paymentMode.LastUpdateOn = DateTime.Now;
                break;
            case "payment-types":
                var paymentType = await _db.PaymentTypeMsts.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (paymentType is null) return false;
                paymentType.IsActive = ToStatusBool(form);
                paymentType.LastUpdateOn = DateTime.Now;
                break;
            case "banks":
                var bank = await _db.JalBankMasters.FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                if (bank is null) return false;
                bank.Status = ToStatusInt(form);
                bank.ModifyDate = DateTime.Now;
                if (bank.Status == 0) bank.DeleteDate = DateTime.Now;
                break;
            case "ndc-amounts":
                var ndc = await _db.MasterNocAmts.FirstOrDefaultAsync(x => x.ConId == decoded, ct);
                if (ndc is null) return false;
                ndc.Status = ToStatusString(form);
                break;
            case "application-statuses":
                var applicationStatus = await _db.ApplicationStatuses.FirstOrDefaultAsync(x => x.AutoId == int.Parse(decoded), ct);
                if (applicationStatus is null) return false;
                applicationStatus.IsActive = ToStatusBool(form);
                break;
            case "rate-categories":
                var rateCategory = await _db.JalRateMasters.FirstOrDefaultAsync(x => x.Id == int.Parse(decoded), ct);
                if (rateCategory is null) return false;
                rateCategory.Status = ToStatusString(form);
                break;
            case "rates":
                var rate = await _db.JalRateTrans.FirstOrDefaultAsync(x => x.Sid == int.Parse(decoded), ct);
                if (rate is null) return false;
                rate.Status = ToStatusString(form);
                break;
            default:
                return false;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<bool> ExistsAsync(string key, string rowKey, CancellationToken ct)
        => (await GetRecordValuesAsync(key, rowKey, ct)).Count > 0;

    private static void TrimFormValues(IFormCollection form, MasterDefinition definition)
    {
        _ = form;
        _ = definition;
    }

    private static string Get(IFormCollection form, string key) => form[key].ToString().Trim();
    private static string? GetOptional(IFormCollection form, string key) => string.IsNullOrWhiteSpace(Get(form, key)) ? null : Get(form, key);
    private static int ToInt(IFormCollection form, string key) => int.TryParse(Get(form, key), out var value) ? value : 0;
    private static int? ToNullableInt(IFormCollection form, string key) => int.TryParse(Get(form, key), out var value) ? value : null;
    private static double? ToNullableDouble(IFormCollection form, string key) => double.TryParse(Get(form, key), out var value) ? value : null;
    private static DateTime? ToNullableDate(IFormCollection form, string key) => DateTime.TryParse(Get(form, key), out var value) ? value : null;
    private static int ToStatusInt(IFormCollection form) => Get(form, "Status") == "0" ? 0 : 1;
    private static string ToStatusString(IFormCollection form) => Get(form, "Status") == "0" ? "0" : "1";
    private static bool ToStatusBool(IFormCollection form) => Get(form, "Status") != "0";
    private static bool IsActive(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return true;
        return status.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || status.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)
            || status.Trim().Equals("T", StringComparison.OrdinalIgnoreCase)
            || status.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatStatus(int? status) => status == 0 ? "Inactive" : "Active";
    private static string FormatStatus(string? status) => IsActive(status) ? "Active" : "Inactive";
    private static string FormatStatus(bool? status) => status == false ? "Inactive" : "Active";
    private static string FormatDivision(int? devType) => AppConstants.Divisions.FormatDisplay(devType);
    private static string? ToDisplayDate(DateTime? value) => value?.ToString("dd-MMM-yyyy");
    private static string? ToInputDate(DateTime? value) => value?.ToString("yyyy-MM-dd");

    private static string EncodeKey(string value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string DecodeKey(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    private static MasterDefinition? GetDefinition(string key)
        => Definitions.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

    private static readonly IReadOnlyList<MasterDefinition> Definitions =
    [
        new("sectors", "Sector Master", "Sector Master", "Manage Noida sector codes used in new connection and consumer records.",
            [new("SectorId", "Sector Code"), new("SectorNo", "Display Sector"), new("OrderBy", "Order"), new("DevType", "Division"), new("Status", "Status")],
            [new("SectorId", "Sector Code", true), new("SectorNo", "Display Sector", true), new("OrderBy", "Order", false, "number"), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("blocks", "Block Master", "Block Master", "Manage blocks linked with sectors.",
            [new("SectorId", "Sector"), new("Block", "Block"), new("DevType", "Division"), new("Status", "Status")],
            [new("SectorId", "Sector", true, "select", true), new("Block", "Block", true, "text", true), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("pipe-sizes", "Pipe Size Master", "Pipe Size Master", "Manage pipe size values used during connection application.",
            [new("PipeSize", "Pipe Size"), new("DevType", "Division"), new("Status", "Status")],
            [new("PipeSize", "Pipe Size", true, "number"), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("connection-categories", "Connection Category Master", "Connection Category Master", "Manage connection category codes such as residential, commercial, institutional, and related values.",
            [new("ConId", "Code"), new("ConName", "Name"), new("ConMainId", "Saved Code"), new("DevType", "Division"), new("Status", "Status")],
            [new("ConId", "Code", true, "text", true), new("ConName", "Name", true), new("ConMainId", "Saved Code", false), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("connection-sub-types", "Connection Sub-Type Master", "Connection Sub-Type Master", "Manage flat/property sub-types linked with connection categories.",
            [new("ConId", "Category"), new("SubConName", "Sub-Type"), new("DevType", "Division"), new("Status", "Status")],
            [new("ConId", "Category", true, "select"), new("SubConName", "Sub-Type", true), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("connection-types", "Connection Type Master", "Connection Type Master", "Manage regular/temporary and other connection type values.",
            [new("ConnectionName", "Name"), new("ConnectionMainId", "Saved Code"), new("Status", "Status")],
            [new("ConnectionName", "Name", true), new("ConnectionMainId", "Saved Code", false), new("Status", "Status", true, "select")]),
        new("villages", "Village Master", "Village Master", "Manage village records used for village connection applications.",
            [new("VillageId", "Village Id"), new("VillageName", "Village Name"), new("VillageStr", "Prefix"), new("DevType", "Division"), new("Status", "Status")],
            [new("VillageId", "Village Id", false, "number"), new("VillageName", "Village Name", true), new("VillageStr", "Prefix", false), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("document-types", "Document Type Master", "Document Type Master", "Manage document types required for applications.",
            [new("DocumentId", "Document Id"), new("DocumentName", "Document Name"), new("DocFor", "Document For"), new("Status", "Status")],
            [new("DocumentId", "Document Id", true, "number", true), new("DocumentName", "Document Name", true), new("DocFor", "Document For", false), new("Status", "Status", true, "select")]),
        new("payment-modes", "Payment Mode Master", "Payment Mode Master", "Manage payment modes used during challan and online payment entry.",
            [new("AutoId", "Id"), new("PaymentModeName", "Payment Mode"), new("Status", "Status")],
            [new("PaymentModeName", "Payment Mode", true), new("Status", "Status", true, "select")]),
        new("payment-types", "Payment Type Master", "Payment Type Master", "Manage payment type values linked with bill and challan payment records.",
            [new("AutoId", "Id"), new("PaymentTypeName", "Payment Type"), new("Status", "Status")],
            [new("PaymentTypeName", "Payment Type", true), new("Status", "Status", true, "select")]),
        new("banks", "Bank Master", "Bank Master", "Manage Jal bank records used by challan and payment flows.",
            [new("BankId", "Bank Code"), new("BankName", "Bank Name"), new("AccountNo", "Account No"), new("Status", "Status")],
            [new("BankId", "Bank Code", true), new("BankName", "Bank Name", true), new("AccountNo", "Account No", false), new("Status", "Status", true, "select")]),
        new("ndc-amounts", "NDC Amount Master", "NDC Amount Master", "Manage No Dues/NDC charge, tax, and expiry values by connection type.",
            [new("ConId", "Code"), new("ConName", "Connection Type"), new("ConMainId", "Saved Code"), new("NocAmt", "NOC Amount"), new("Amount", "Charge"), new("Sgst", "SGST"), new("Cgst", "CGST"), new("ExpiryTime", "Expiry Time"), new("Status", "Status")],
            [new("ConId", "Code", true, "text", true), new("ConName", "Connection Type", true), new("ConMainId", "Saved Code", false), new("NocAmt", "NOC Amount", false, "number"), new("Amount", "Charge", false), new("Sgst", "SGST", false), new("Cgst", "CGST", false), new("ExpiryTime", "Expiry Time", false), new("Status", "Status", true, "select")]),
        new("application-statuses", "Application Status Master", "Application Status Master", "Manage reusable status labels for application tracking and approvals.",
            [new("AutoId", "Id"), new("StatusName", "Status Name"), new("Status", "Status")],
            [new("StatusName", "Status Name", true), new("Status", "Status", true, "select")]),
        new("rate-categories", "Rate Category Master", "Rate Category Master", "Manage Jal rate categories used to map connection/property types before applying rate slabs.",
            [new("Id", "Id"), new("PropertyType", "Category Name"), new("IdT", "Connection Code"), new("DevType", "Division"), new("Status", "Status")],
            [new("PropertyType", "Category Name", true), new("IdT", "Connection Code", true), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")]),
        new("rates", "Rate Master", "Rate Master", "Manage rate slabs by category, area range, pipe size, regular/temporary rate, cess rate, and effective dates.",
            [new("Category", "Rate Category"), new("AreaStart", "Area Start"), new("AreaEnd", "Area End"), new("PipeSize", "Pipe Size"), new("Regular", "Regular Rate"), new("Temporary", "Temporary Rate"), new("CessRate", "Cess Rate"), new("EffFrom", "Effective From"), new("EffTo", "Effective To"), new("DevType", "Division"), new("Status", "Status")],
            [new("Id", "Rate Category", true, "select"), new("AreaStart", "Area Start", true, "number"), new("AreaEnd", "Area End", true, "number"), new("PipeSize", "Pipe Size", true, "select"), new("Regular", "Regular Rate", true, "number"), new("Temporary", "Temporary Rate", true, "number"), new("CessRate", "Cess Rate", false, "number"), new("EffFrom", "Effective From", true, "date"), new("EffTo", "Effective To", false, "date"), new("DevType", "Division", false, "select"), new("Status", "Status", true, "select")])
    ];

    private sealed record MasterDefinition(
        string Key,
        string Title,
        string Module,
        string Description,
        IReadOnlyList<MasterColumnDefinition> Columns,
        IReadOnlyList<MasterFieldDefinition> Fields);

    private sealed record MasterColumnDefinition(string Key, string Label);

    private sealed record MasterFieldDefinition(
        string Name,
        string Label,
        bool IsRequired,
        string InputType = "text",
        bool ReadOnlyOnEdit = false);
}
