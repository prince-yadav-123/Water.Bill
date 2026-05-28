using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Models.SupportQueries;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConsumerQueryManagementController : Controller
{
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

    public ConsumerQueryManagementController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        string? status,
        string? priority,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = "Consumer Query Management";
        ViewData["ActiveMenu"] = "Consumer Query Management";

        var query = _db.ConsumerSupportQueries
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.QueryNo.Contains(term)
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

        return View(new ConsumerQueryManagementListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            Priority = priority,
            FromDate = fromDate,
            ToDate = toDate,
            Categories = await BuildCategoriesAsync(ct),
            Queries = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct)
        });
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Consumer Query Details";
        ViewData["ActiveMenu"] = "Consumer Query Management";

        var query = await _db.ConsumerSupportQueries
            .Include(x => x.Category)
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return query is null ? NotFound() : View(query);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(ConsumerQueryAdminActionViewModel model, CancellationToken ct)
    {
        var query = await _db.ConsumerSupportQueries
            .FirstOrDefaultAsync(x => x.Id == model.QueryId && !x.IsDeleted, ct);
        if (query is null)
            return NotFound();

        var newStatus = NormalizeStatus(model.Status);
        if (!AllowedStatuses.Contains(newStatus))
        {
            TempData["ErrorMessage"] = "Selected status is not valid.";
            return RedirectToAction(nameof(Details), new { id = model.QueryId });
        }

        var oldStatus = query.Status;
        var remarks = NormalizeNullable(model.Remarks);
        if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Selected status is already applied.";
            return RedirectToAction(nameof(Details), new { id = model.QueryId });
        }

        if (!IsValidQueryStatusTransition(oldStatus, newStatus))
        {
            TempData["ErrorMessage"] = "Invalid status transition for this query.";
            return RedirectToAction(nameof(Details), new { id = model.QueryId });
        }

        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Remarks / resolution comments are required.";
            return RedirectToAction(nameof(Details), new { id = model.QueryId });
        }

        var now = DateTime.Now;
        query.Status = newStatus;
        query.AdminRemarks = remarks;
        query.UpdatedAt = now;

        if (newStatus == "Resolved")
        {
            query.ResolvedByUserId = ResolveUserId();
            query.ResolvedAt = now;
        }
        if (newStatus == "Closed")
            query.ClosedAt = now;

        _db.ConsumerSupportQueryHistories.Add(new ConsumerSupportQueryHistory
        {
            QueryId = query.Id,
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
        TempData["SuccessMessage"] = $"Query marked as {newStatus}.";
        return RedirectToAction(nameof(Details), new { id = model.QueryId });
    }

    [HttpGet]
    public async Task<IActionResult> Document(long id, CancellationToken ct)
    {
        var document = await _db.ConsumerSupportQueryDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (document is null)
            return NotFound();

        var fullPath = Path.Combine(GetStorageBasePath(), document.FilePath.Replace('/', Path.DirectorySeparatorChar));
        return System.IO.File.Exists(fullPath)
            ? PhysicalFile(fullPath, document.ContentType ?? "application/octet-stream", document.FileName)
            : NotFound();
    }

    private async Task<List<SelectListItem>> BuildCategoriesAsync(CancellationToken ct)
        => await _db.SupportQueryCategories
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.Id.ToString()))
            .ToListAsync(ct);

    private string GetStorageBasePath()
        => _configuration["FileStorage:ConsumerSupportDocumentBasePath"]
            ?? Path.Combine(_configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads", "ConsumerSupportDocuments");

    private int? ResolveUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private static string NormalizeStatus(string? value)
        => string.IsNullOrWhiteSpace(value) ? "InProgress" : value.Trim();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsValidQueryStatusTransition(string currentStatus, string newStatus)
        => AllowedTransitions.TryGetValue(currentStatus, out var allowed)
            && allowed.Contains(newStatus, StringComparer.OrdinalIgnoreCase);
}
