using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Audit;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Extensions;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ErrorLogsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public ErrorLogsController(ApplicationDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    [HttpGet("/ErrorLogs")]
    [RequirePermission("Error Logs.view")]
    public async Task<IActionResult> Index(ErrorLogIndexViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Error Logs";
        ViewData["ActiveMenu"] = "Error Logs";

        var query = _db.ErrorLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search.Trim();
            var statusCodeTerm = int.TryParse(term, out var parsedStatusCode) ? parsedStatusCode : (int?)null;
            query = query.Where(x =>
                x.Message.Contains(term) ||
                x.ExceptionType.Contains(term) ||
                (x.RequestPath != null && x.RequestPath.Contains(term)) ||
                (x.TraceId != null && x.TraceId.Contains(term)) ||
                (x.Username != null && x.Username.Contains(term)) ||
                (x.IpAddress != null && x.IpAddress.Contains(term)) ||
                (statusCodeTerm.HasValue && x.StatusCode == statusCodeTerm.Value));
        }

        if (!string.IsNullOrWhiteSpace(model.ExceptionType))
            query = query.Where(x => x.ExceptionType == model.ExceptionType);

        if (!string.IsNullOrWhiteSpace(model.PortalType))
            query = query.Where(x => x.PortalType == model.PortalType);

        if (model.FromDate.HasValue)
            query = query.Where(x => x.CreatedAt >= model.FromDate.Value.Date);

        if (model.ToDate.HasValue)
            query = query.Where(x => x.CreatedAt < model.ToDate.Value.Date.AddDays(1));

        model.ExceptionTypes = await _db.ErrorLogs
            .AsNoTracking()
            .OrderBy(x => x.ExceptionType)
            .Select(x => x.ExceptionType)
            .Distinct()
            .ToListAsync(ct);

        var page = PagingConstants.ValidatePage(model.Page);
        var pageSize = PagingConstants.Validate(model.PageSize == 0 ? PagingConstants.DefaultPageSize : model.PageSize);

        var rowQuery = query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ErrorLogRowViewModel
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                PortalType = x.PortalType,
                ExceptionType = x.ExceptionType,
                Message = x.Message,
                RequestPath = x.RequestPath,
                StatusCode = x.StatusCode,
                Username = x.Username,
                IpAddress = x.IpAddress,
                IsHandled = x.IsHandled
            });

        var paged = await rowQuery.ToPagedResultAsync(page, pageSize, ct);
        model.Rows = paged.Items.ToList();
        model.Page = page;
        model.PageSize = pageSize;
        ViewBag.Pagination = PaginationViewModel.Create(paged);
        return View(model);
    }

    [HttpGet("/ErrorLogs/Details/{id:long}")]
    [RequirePermission("Error Logs.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Error Log Details";
        ViewData["ActiveMenu"] = "Error Logs";

        var model = await _db.ErrorLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ErrorLogDetailsViewModel
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                ExceptionType = x.ExceptionType,
                Message = x.Message,
                StackTrace = x.StackTrace,
                RequestPath = x.RequestPath,
                HttpMethod = x.HttpMethod,
                QueryString = x.QueryString,
                StatusCode = x.StatusCode,
                IpAddress = x.IpAddress,
                Username = x.Username,
                UserId = x.UserId,
                PortalType = x.PortalType,
                UserAgent = x.UserAgent,
                ControllerName = x.ControllerName,
                ActionName = x.ActionName,
                TraceId = x.TraceId,
                IsHandled = x.IsHandled
            })
            .FirstOrDefaultAsync(ct);

        return model is null ? NotFound() : View(model);
    }

    [HttpPost("/ErrorLogs/ClearOld")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Error Logs.delete")]
    public async Task<IActionResult> ClearOld(CancellationToken ct)
    {
        var cutoff = AppTime.IndiaNow.AddDays(-30);
        var oldLogs = await _db.ErrorLogs.Where(x => x.CreatedAt < cutoff).ToListAsync(ct);
        if (oldLogs.Count == 0)
        {
            TempData["InfoMessage"] = "No error logs older than 30 days were found.";
            return RedirectToAction(nameof(Index));
        }

        _db.ErrorLogs.RemoveRange(oldLogs);
        await _db.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(AuditAction.Delete, AppConstants.Modules.ErrorLogs, null, $"Cleared {oldLogs.Count} error logs older than 30 days.", ct: ct);
        TempData["SuccessMessage"] = $"Cleared {oldLogs.Count} old error logs.";
        return RedirectToAction(nameof(Index));
    }
}
