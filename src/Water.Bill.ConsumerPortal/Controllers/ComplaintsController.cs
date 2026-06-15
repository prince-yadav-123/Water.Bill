using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.ConsumerPortal.Filters;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
[RequirePermission("Consumer Complaints.view")]
public class ComplaintsController : Controller
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Open", "InProgress", "Resolved", "Closed", "Rejected"
    };

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public ComplaintsController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("/Consumer/Complaints")]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? status, CancellationToken ct)
    {
        ViewData["Title"] = "Complaints & Requests";
        ViewData["ActiveMenu"] = "Complaints & Requests";

        var consumerNo = ResolveConsumerNo();
        var query = _db.ConsumerComplaints
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
            query = query.Where(x => x.ComplaintNo.Contains(term)
                || x.Subject.Contains(term)
                || x.CategoryName.Contains(term)
                || (x.RelatedBillNo != null && x.RelatedBillNo.Contains(term))
                || (x.RelatedApplicationNo != null && x.RelatedApplicationNo.Contains(term)));
        }

        return View(new ConsumerComplaintListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            Categories = await BuildCategoriesAsync(ct),
            Complaints = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct)
        });
    }

    [HttpGet("/Consumer/Complaints/Create")]
    [RequirePermission("Consumer Complaints.add")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Raise Complaint";
        ViewData["ActiveMenu"] = "Complaints & Requests";

        return View(new ConsumerComplaintFormViewModel
        {
            ConsumerNo = ResolveConsumerNo(),
            MobileNo = User.FindFirstValue("MobileNo"),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Categories = await BuildCategoriesAsync(ct)
        });
    }

    [HttpPost("/Consumer/Complaints/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Complaints.add")]
    public async Task<IActionResult> Create(ConsumerComplaintFormViewModel model, List<IFormFile>? documents, CancellationToken ct)
    {
        ViewData["Title"] = "Raise Complaint";
        ViewData["ActiveMenu"] = "Complaints & Requests";
        model.Categories = await BuildCategoriesAsync(ct);

        model.ConsumerNo = ResolveConsumerNo();
        model.MobileNo = NormalizeNullable(model.MobileNo);
        model.Email = NormalizeNullable(model.Email);
        model.Priority = NormalizeNullable(model.Priority) ?? "Normal";

        if (!ModelState.IsValid)
            return View(model);

        ValidateDocumentFiles(documents);
        if (!ModelState.IsValid)
            return View(model);

        var category = await _db.ComplaintCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == model.CategoryId && x.IsActive && !x.IsDeleted, ct);
        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected complaint category is not available.");
            return View(model);
        }

        var now = DateTime.Now;
        var complaint = new ConsumerComplaint
        {
            ComplaintNo = GenerateComplaintNo(),
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
            LocationDetails = NormalizeNullable(model.LocationDetails),
            RelatedBillNo = NormalizeNullable(model.RelatedBillNo),
            RelatedApplicationNo = NormalizeNullable(model.RelatedApplicationNo),
            Status = "Open",
            CreatedAt = now,
            IsActive = true,
            IsDeleted = false
        };

        complaint.Histories.Add(new ConsumerComplaintHistory
        {
            ToStatus = "Open",
            Action = "Created",
            Remarks = "Complaint raised by consumer.",
            ActionByUserId = complaint.ConsumerUserId,
            ActionByName = complaint.ConsumerName,
            ActionByRole = AppConstants.Roles.Consumer,
            ActionAt = now
        });

        _db.ConsumerComplaints.Add(complaint);
        await _db.SaveChangesAsync(ct);

        await SaveDocumentsAsync(complaint, documents, ct);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = $"Complaint {complaint.ComplaintNo} submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = complaint.Id });
    }

    [HttpGet("/Consumer/Complaints/Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Complaint Details";
        ViewData["ActiveMenu"] = "Complaints & Requests";

        var complaint = await GetOwnedComplaint(id)
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
        return complaint is null ? NotFound() : View(complaint);
    }

    [HttpGet("/Consumer/Complaints/Document/{id:long}")]
    public async Task<IActionResult> Document(long id, CancellationToken ct)
    {
        var document = await _db.ConsumerComplaintDocuments
            .Include(x => x.Complaint)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.Complaint.ConsumerNo == ResolveConsumerNo(), ct);
        return document is null ? NotFound() : ServeDocument(document);
    }

    private IQueryable<ConsumerComplaint> GetOwnedComplaint(long id)
        => _db.ConsumerComplaints.Where(x => x.Id == id && !x.IsDeleted && x.ConsumerNo == ResolveConsumerNo());

    private async Task<List<SelectListItem>> BuildCategoriesAsync(CancellationToken ct)
        => await _db.ComplaintCategories
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.Id.ToString()))
            .ToListAsync(ct);

    private async Task SaveDocumentsAsync(ConsumerComplaint complaint, IEnumerable<IFormFile>? files, CancellationToken ct)
    {
        if (files is null)
            return;

        foreach (var file in files.Where(x => x.Length > 0))
        {
            var saved = await SaveDocumentFileAsync(complaint.ComplaintNo, file, ct);
            complaint.Documents.Add(new ConsumerComplaintDocument
            {
                DocumentType = "Complaint Document",
                FileName = saved.FileName,
                FilePath = saved.RelativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedByConsumerUserId = complaint.ConsumerUserId,
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

    private async Task<(string FileName, string RelativePath)> SaveDocumentFileAsync(string complaintNo, IFormFile file, CancellationToken ct)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var basePath = GetStorageBasePath();
        var relativeDirectory = Path.Combine("complaints", complaintNo);
        var directory = Path.Combine(basePath, relativeDirectory);
        Directory.CreateDirectory(directory);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, safeFileName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return (Path.GetFileName(file.FileName), Path.Combine(relativeDirectory, safeFileName).Replace('\\', '/'));
    }

    private IActionResult ServeDocument(ConsumerComplaintDocument document)
    {
        var fullPath = Path.Combine(GetStorageBasePath(), document.FilePath.Replace('/', Path.DirectorySeparatorChar));
        return System.IO.File.Exists(fullPath)
            ? PhysicalFile(fullPath, document.ContentType ?? "application/octet-stream", document.FileName)
            : NotFound();
    }

    private string GetStorageBasePath()
        => _configuration["FileStorage:ConsumerComplaintDocumentBasePath"]
            ?? Path.Combine(_configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads", "ConsumerComplaintDocuments");

    private string ResolveConsumerNo()
        => Normalize(User.FindFirstValue("ConsumerNo") ?? string.Empty);

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private static string GenerateComplaintNo()
        => $"CMP{DateTime.Now:yyyyMMddHHmmssfff}";

    private static string Normalize(string value)
        => value.Trim().ToUpperInvariant();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
