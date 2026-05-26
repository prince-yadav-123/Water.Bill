using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class NewConnectionController : Controller
{
    private static readonly string[] RequiredDocumentTypes =
    [
        "ID Proof",
        "Address Proof",
        "Property Document"
    ];

    private readonly IConfiguration _configuration;
    private readonly INewConnectionApplicationService _service;
    private readonly INewConnectionLookupService _lookupService;

    public NewConnectionController(
        IConfiguration configuration,
        INewConnectionApplicationService service,
        INewConnectionLookupService lookupService)
    {
        _configuration = configuration;
        _service = service;
        _lookupService = lookupService;
    }

    [HttpGet("/Consumer/NewConnection/Apply")]
    public async Task<IActionResult> Apply(CancellationToken ct)
    {
        ViewData["Title"] = "New Connection";
        ViewData["ActiveMenu"] = "New Connection";
        await LoadLookupDataAsync(ct);
        return View(new NewConnectionApplicationFormDto
        {
            MobileNumber = User.FindFirstValue("MobileNo"),
            EmailId = User.FindFirstValue(ClaimTypes.Email)
        });
    }

    [HttpPost("/Consumer/NewConnection/Apply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        ViewData["Title"] = "New Connection";
        ViewData["ActiveMenu"] = "New Connection";
        await LoadLookupDataAsync(ct);

        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View(model);

        ValidateRequiredDocuments(Request.Form.Files, await GetDocumentTypeNamesAsync(ct));
        if (!ModelState.IsValid)
            return View(model);

        var consumerNo = ResolveConsumerNo();
        var consumerUserId = ResolveConsumerUserId();
        var applicationNo = GenerateApplicationNo();

        try
        {
            var savedDocuments = await SaveDocumentsAsync(Request.Form.Files, applicationNo, ct);
            var result = await _service.SubmitAsync(new NewConnectionSubmitRequest
            {
                Form = model,
                Documents = savedDocuments,
                ApplicationNo = applicationNo,
                IsPublicApplication = false,
                SubmittedByConsumerNo = consumerNo,
                SubmittedByConsumerUserId = consumerUserId,
                ActionBy = consumerUserId,
                ActionByName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
                ActionByRole = AppConstants.Roles.Consumer,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            }, ct);

            TempData["SuccessMessage"] = $"Application submitted successfully. Your application number is {result.ApplicationNo}.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("/Consumer/NewConnection/Lookups/Blocks")]
    public async Task<IActionResult> Blocks(string sectorId, CancellationToken ct)
        => Json(await _lookupService.GetBlocksBySectorAsync(sectorId, ResolveDevType(), ct));

    [HttpGet("/Consumer/NewConnection/Lookups/ConnectionSubTypes")]
    public async Task<IActionResult> ConnectionSubTypes(string connectionCategoryId, CancellationToken ct)
        => Json(await _lookupService.GetConnectionSubTypesAsync(connectionCategoryId, ResolveDevType(), ct));

    [HttpGet("/Consumer/NewConnection/MyApplications")]
    public async Task<IActionResult> MyApplications(CancellationToken ct)
    {
        ViewData["Title"] = "My Applications";
        ViewData["ActiveMenu"] = "My Applications";
        var applications = await _service.GetConsumerApplicationsAsync(ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        return View(applications);
    }

    [HttpGet("/Consumer/NewConnection/Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Application Details";
        ViewData["ActiveMenu"] = "My Applications";
        var details = await _service.GetConsumerApplicationDetailsAsync(id, ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        if (details is null)
            return NotFound();

        return View(details);
    }

    private string ResolveConsumerNo()
        => (User.FindFirstValue("ConsumerNo") ?? string.Empty).Trim().ToUpperInvariant();

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task<IReadOnlyList<NewConnectionDocumentInputDto>> SaveDocumentsAsync(IFormFileCollection files, string applicationNo, CancellationToken ct)
    {
        var result = new List<NewConnectionDocumentInputDto>();
        var storageRoot = _configuration["FileStorage:DocumentBasePath"];
        if (string.IsNullOrWhiteSpace(storageRoot))
            throw new InvalidOperationException("Document storage path is not configured.");

        var maxUploadSizeMb = int.TryParse(_configuration["FileStorage:MaxUploadSizeMb"], out var configuredMaxUploadSizeMb)
            ? configuredMaxUploadSizeMb
            : 5;
        var maxBytes = maxUploadSizeMb * 1024L * 1024L;
        var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        if (allowedExtensions.Length == 0)
            allowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        var uploadRoot = Path.Combine(storageRoot, applicationNo);
        Directory.CreateDirectory(uploadRoot);

        foreach (var file in files.Where(x => x.Length > 0))
        {
            if (file.Length > maxBytes)
                throw new InvalidOperationException($"Each document must be {maxUploadSizeMb} MB or smaller.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF, JPG, JPEG, and PNG documents are allowed.");

            var safeOriginalName = MakeSafeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var safeName = $"{safeOriginalName}-{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadRoot, safeName);
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream, ct);

            result.Add(new NewConnectionDocumentInputDto
            {
                DocumentType = ResolveDocumentType(file.Name),
                FileName = Path.GetFileName(file.FileName),
                FilePath = $"{applicationNo}/{safeName}",
                ContentType = file.ContentType,
                FileSize = file.Length
            });
        }

        return result;
    }

    private static string ResolveDocumentType(string input)
    {
        var normalized = input.Replace("Documents_", string.Empty).Replace("_", " ").Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Other" : normalized;
    }

    private void ValidateRequiredDocuments(IFormFileCollection files, IReadOnlyCollection<string> configuredDocumentTypes)
    {
        var uploadedTypes = files
            .Where(x => x.Length > 0)
            .Select(x => ResolveDocumentType(x.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requiredTypes = RequiredDocumentTypes
            .Where(requiredType => configuredDocumentTypes.Contains(requiredType, StringComparer.OrdinalIgnoreCase));

        foreach (var requiredType in requiredTypes)
        {
            if (!uploadedTypes.Contains(requiredType))
                ModelState.AddModelError(string.Empty, $"{requiredType} is required.");
        }
    }

    private async Task LoadLookupDataAsync(CancellationToken ct)
    {
        var lookups = await _lookupService.GetLookupDataAsync(ResolveDevType(), ct);
        ViewData["LookupData"] = lookups;
        ViewData["DocumentTypes"] = lookups.DocumentTypes.Select(x => x.Text).ToArray();
    }

    private async Task<IReadOnlyList<string>> GetDocumentTypeNamesAsync(CancellationToken ct)
    {
        var documentTypes = await _lookupService.GetDocumentTypesAsync(ct);
        return documentTypes.Select(x => x.Text).ToArray();
    }

    private int? ResolveDevType()
        => int.TryParse(_configuration["NewConnection:DefaultDevType"], out var devType) ? devType : null;

    private void NormalizeDeclarationFromRequest(NewConnectionApplicationFormDto model)
    {
        if (!Request.HasFormContentType)
            return;

        if (!Request.Form.TryGetValue(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), out var values))
            return;

        var accepted = values.Any(value =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase)
            || value == "1");

        if (!accepted)
            return;

        model.DeclarationAccepted = true;
        ModelState.Remove(nameof(NewConnectionApplicationFormDto.DeclarationAccepted));
    }

    private void ValidateDeclaration(NewConnectionApplicationFormDto model)
    {
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), "Please accept the declaration.");
    }

    private static string GenerateApplicationNo() => $"NC{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

    private static string MakeSafeFileName(string fileName)
    {
        var safe = new string(fileName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch).ToArray()).Trim('-', ' ');
        return string.IsNullOrWhiteSpace(safe) ? "document" : safe;
    }
}
