using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class SupportQueriesController : Controller
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Open", "InProgress", "Resolved", "Closed", "Rejected"
    };

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public SupportQueriesController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("/Consumer/SupportQueries")]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? status, CancellationToken ct)
    {
        ViewData["Title"] = "Support & Queries";
        ViewData["ActiveMenu"] = "Support & Queries";

        var consumerNo = ResolveConsumerNo();
        var query = _db.ConsumerSupportQueries
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ConsumerNo == consumerNo);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(status) && AllowedStatuses.Contains(status))
            query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.QueryNo.Contains(term)
                || x.Subject.Contains(term)
                || x.CategoryName.Contains(term)
                || (x.RelatedBillNo != null && x.RelatedBillNo.Contains(term))
                || (x.RelatedApplicationNo != null && x.RelatedApplicationNo.Contains(term)));
        }

        return View(new ConsumerSupportQueryListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            Categories = await BuildCategoriesAsync(ct),
            Queries = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct)
        });
    }

    [HttpGet("/Consumer/SupportQueries/Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Raise Query";
        ViewData["ActiveMenu"] = "Support & Queries";

        return View(new ConsumerSupportQueryFormViewModel
        {
            ConsumerNo = ResolveConsumerNo(),
            MobileNo = User.FindFirstValue("MobileNo"),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Categories = await BuildCategoriesAsync(ct)
        });
    }

    [HttpPost("/Consumer/SupportQueries/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsumerSupportQueryFormViewModel model, List<IFormFile>? documents, CancellationToken ct)
    {
        ViewData["Title"] = "Raise Query";
        ViewData["ActiveMenu"] = "Support & Queries";
        model.Categories = await BuildCategoriesAsync(ct);

        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.MobileNo = NormalizeNullable(model.MobileNo);
        model.Email = NormalizeNullable(model.Email);
        model.Priority = NormalizeNullable(model.Priority) ?? "Normal";

        if (!ModelState.IsValid)
            return View(model);

        ValidateDocumentFiles(documents);
        if (!ModelState.IsValid)
            return View(model);

        var category = await _db.SupportQueryCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == model.CategoryId && x.IsActive && !x.IsDeleted, ct);
        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected category is not available.");
            return View(model);
        }

        var now = DateTime.Now;
        var query = new ConsumerSupportQuery
        {
            QueryNo = GenerateQueryNo(),
            ConsumerUserId = ResolveConsumerUserId(),
            ConsumerNo = model.ConsumerNo,
            ConsumerName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
            MobileNo = model.MobileNo,
            Email = model.Email,
            CategoryId = category.Id,
            CategoryName = category.CategoryName,
            Subject = model.Subject.Trim(),
            Description = model.Description.Trim(),
            Priority = model.Priority,
            RelatedBillNo = NormalizeNullable(model.RelatedBillNo),
            RelatedApplicationNo = NormalizeNullable(model.RelatedApplicationNo),
            Status = "Open",
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        };

        query.Histories.Add(new ConsumerSupportQueryHistory
        {
            FromStatus = null,
            ToStatus = "Open",
            Action = "Created",
            Remarks = "Query raised by consumer.",
            ActionByUserId = query.ConsumerUserId,
            ActionByName = query.ConsumerName,
            ActionByRole = AppConstants.Roles.Consumer,
            ActionAt = now
        });

        _db.ConsumerSupportQueries.Add(query);
        await _db.SaveChangesAsync(ct);

        await SaveDocumentsAsync(query, documents, ct);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = $"Query {query.QueryNo} submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = query.Id });
    }

    [HttpGet("/Consumer/SupportQueries/Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Query Details";
        ViewData["ActiveMenu"] = "Support & Queries";

        var query = await GetOwnedQuery(id)
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
        return query is null ? NotFound() : View(query);
    }

    [HttpGet("/Consumer/SupportQueries/Document/{id:long}")]
    public async Task<IActionResult> Document(long id, CancellationToken ct)
    {
        var document = await _db.ConsumerSupportQueryDocuments
            .Include(x => x.Query)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.Query.ConsumerNo == ResolveConsumerNo(), ct);
        if (document is null)
            return NotFound();

        return ServeDocument(document);
    }

    private IQueryable<ConsumerSupportQuery> GetOwnedQuery(long id)
        => _db.ConsumerSupportQueries.Where(x => x.Id == id && !x.IsDeleted && x.ConsumerNo == ResolveConsumerNo());

    private async Task<List<SelectListItem>> BuildCategoriesAsync(CancellationToken ct)
        => await _db.SupportQueryCategories
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.Id.ToString()))
            .ToListAsync(ct);

    private async Task SaveDocumentsAsync(ConsumerSupportQuery query, IEnumerable<IFormFile>? files, CancellationToken ct)
    {
        if (files is null)
            return;

        foreach (var file in files.Where(x => x.Length > 0))
        {
            var saved = await SaveDocumentFileAsync(query.QueryNo, file, ct);
            query.Documents.Add(new ConsumerSupportQueryDocument
            {
                DocumentType = "Support Document",
                FileName = saved.FileName,
                FilePath = saved.RelativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedByConsumerUserId = query.ConsumerUserId,
                UploadedAt = DateTime.Now
            });
        }
    }

    private void ValidateDocumentFiles(IEnumerable<IFormFile>? files)
    {
        if (files is null)
            return;

        var maxBytes = (_configuration.GetValue<int?>("FileStorage:MaxUploadSizeMb") ?? 5) * 1024L * 1024L;
        var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? [".pdf", ".jpg", ".jpeg", ".png"];
        foreach (var file in files.Where(x => x.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (file.Length > maxBytes)
                ModelState.AddModelError(string.Empty, $"File {file.FileName} exceeds allowed upload size.");
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                ModelState.AddModelError(string.Empty, $"File type {extension} is not allowed.");
        }
    }

    private async Task<(string FileName, string RelativePath)> SaveDocumentFileAsync(string queryNo, IFormFile file, CancellationToken ct)
    {
        var maxBytes = (_configuration.GetValue<int?>("FileStorage:MaxUploadSizeMb") ?? 5) * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File {file.FileName} exceeds allowed upload size.");

        var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? [".pdf", ".jpg", ".jpeg", ".png"];
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"File type {extension} is not allowed.");

        var basePath = GetStorageBasePath();
        var relativeDirectory = Path.Combine("support-queries", queryNo);
        var directory = Path.Combine(basePath, relativeDirectory);
        Directory.CreateDirectory(directory);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, safeFileName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return (Path.GetFileName(file.FileName), Path.Combine(relativeDirectory, safeFileName).Replace('\\', '/'));
    }

    private IActionResult ServeDocument(ConsumerSupportQueryDocument document)
    {
        var fullPath = Path.Combine(GetStorageBasePath(), document.FilePath.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, document.ContentType ?? "application/octet-stream", document.FileName);
    }

    private string GetStorageBasePath()
        => _configuration["FileStorage:ConsumerSupportDocumentBasePath"]
            ?? Path.Combine(_configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads", "ConsumerSupportDocuments");

    private string ResolveConsumerNo()
        => Normalize(User.FindFirstValue("ConsumerNo") ?? string.Empty);

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private static string GenerateQueryNo()
        => $"QRY{DateTime.Now:yyyyMMddHHmmssfff}";

    private static string Normalize(string value)
        => value.Trim().ToUpperInvariant();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
