using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Complaints;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Security;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ComplaintManagementController : Controller
{
    private const string ModuleName = AppConstants.Modules.ComplaintManagement;
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Open", "InProgress", "Resolved", "Closed", "Rejected"
    };
    private static readonly string[] Priorities = ["Low", "Normal", "High", "Urgent"];
    private static readonly IReadOnlyDictionary<string, string[]> AllowedTransitions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Open"] = ["InProgress", "Resolved", "Rejected"],
        ["InProgress"] = ["Resolved", "Rejected"],
        ["Resolved"] = ["Closed"],
        ["Rejected"] = ["Closed"],
        ["Closed"] = []
    };

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public ComplaintManagementController(ApplicationDbContext db, IConfiguration configuration, IMemoryCache cache)
    {
        _db = db;
        _configuration = configuration;
        _cache = cache;
    }

    [HttpGet("/ComplaintManagement")]
    [RequirePermission("Complaint Management.view")]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? status, string? priority, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;

        var query = _db.ConsumerComplaints.Include(x => x.Category).AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ComplaintNo.Contains(term)
                || x.ConsumerNo.Contains(term)
                || x.ConsumerName.Contains(term)
                || (x.MobileNo != null && x.MobileNo.Contains(term))
                || x.Subject.Contains(term));
        }
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(status) && AllowedStatuses.Contains(status))
            query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(priority) && Priorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
            query = query.Where(x => x.Priority == priority);
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAt.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAt.Date <= toDate.Value.Date);

        return View(new ComplaintManagementListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            Priority = priority,
            FromDate = fromDate,
            ToDate = toDate,
            Categories = await BuildCategoriesAsync(ct),
            Complaints = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct)
        });
    }

    [HttpGet("/ComplaintManagement/Details/{id:long}")]
    [RequirePermission("Complaint Management.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Complaint Details";
        ViewData["ActiveMenu"] = ModuleName;

        var complaint = await _db.ConsumerComplaints
            .Include(x => x.Category)
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return complaint is null ? NotFound() : View(complaint);
    }

    [HttpPost("/ComplaintManagement/UpdateStatus")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Complaint Management.edit")]
    public async Task<IActionResult> UpdateStatus(ComplaintAdminActionViewModel model, CancellationToken ct)
    {
        var complaint = await _db.ConsumerComplaints.FirstOrDefaultAsync(x => x.Id == model.ComplaintId && !x.IsDeleted, ct);
        if (complaint is null)
            return NotFound();

        var newStatus = NormalizeStatus(model.Status);
        if (!AllowedStatuses.Contains(newStatus))
        {
            TempData["ErrorMessage"] = "Selected status is not valid.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        var oldStatus = complaint.Status;
        var remarks = NormalizeNullable(model.Remarks);
        if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Selected status is already applied.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }
        if (!IsValidStatusTransition(oldStatus, newStatus))
        {
            TempData["ErrorMessage"] = "Invalid status transition for this complaint.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Remarks / resolution comments are required.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        var now = DateTime.Now;
        complaint.Status = newStatus;
        complaint.AdminRemarks = remarks;
        complaint.UpdatedAt = now;
        if (newStatus == "Resolved")
        {
            complaint.ResolvedByUserId = ResolveUserId();
            complaint.ResolvedAt = now;
        }
        if (newStatus == "Closed")
            complaint.ClosedAt = now;

        _db.ConsumerComplaintHistories.Add(new ConsumerComplaintHistory
        {
            ComplaintId = complaint.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            Action = newStatus,
            Remarks = remarks,
            ActionByUserId = ResolveUserId(),
            ActionByName = User.FindFirstValue("FullName") ?? User.Identity?.Name,
            ActionByRole = User.FindFirstValue(ClaimTypes.Role),
            ActionAt = now
        });

        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Complaint marked as {newStatus}.";
        return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
    }

    [HttpGet("/ComplaintManagement/Document/{id:long}")]
    [RequirePermission("Complaint Management.view")]
    public async Task<IActionResult> Document(long id, CancellationToken ct)
    {
        var document = await _db.ConsumerComplaintDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (document is null)
            return NotFound();

        if (!FileUploadSecurityHelper.TryResolveSafeStoredFilePath(GetStorageBasePath(), document.FilePath, out var fullPath))
            return NotFound();

        return System.IO.File.Exists(fullPath)
            ? PhysicalFile(fullPath, FileUploadSecurityHelper.ResolveSafeContentType(document.FilePath), document.FileName)
            : NotFound();
    }

    private async Task<List<SelectListItem>> BuildCategoriesAsync(CancellationToken ct)
        => await _cache.GetOrCreateAsync("lookup:complaint-categories:active", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.ComplaintCategories.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.CategoryName)
                .Select(x => new SelectListItem(x.CategoryName, x.Id.ToString()))
                .ToListAsync(ct);
        }) ?? [];

    private string GetStorageBasePath()
        => _configuration["FileStorage:ConsumerComplaintDocumentBasePath"]
            ?? Path.Combine(_configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads", "ConsumerComplaintDocuments");

    private int? ResolveUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private static string NormalizeStatus(string? value)
        => string.IsNullOrWhiteSpace(value) ? "InProgress" : value.Trim();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        => AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus, StringComparer.OrdinalIgnoreCase);
}
