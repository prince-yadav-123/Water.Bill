using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Audit;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class OperatorAuditController : Controller
{
    private readonly ApplicationDbContext _db;

    public OperatorAuditController(ApplicationDbContext db) => _db = db;

    [HttpGet("/OperatorAudit")]
    [RequirePermission("Operator Activity Logs.view")]
    public async Task<IActionResult> Index(OperatorAuditIndexViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Operator Activity Logs";
        ViewData["ActiveMenu"] = "Operator Activity Logs";
        var query = _db.Auditlogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search.Trim();
            query = query.Where(x => (x.Username != null && x.Username.Contains(term))
                || (x.Module != null && x.Module.Contains(term))
                || (x.EntityId != null && x.EntityId.Contains(term))
                || (x.Details != null && x.Details.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(model.Module))
            query = query.Where(x => x.Module == model.Module);
        if (model.UserId.HasValue)
            query = query.Where(x => x.UserId == model.UserId);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.Timestamp >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.Timestamp < model.ToDate.Value.Date.AddDays(1));
        model.Rows = await query.OrderByDescending(x => x.Timestamp).Take(500).Select(x => new OperatorAuditRowViewModel
        {
            Timestamp = x.Timestamp,
            UserId = x.UserId,
            Username = x.Username,
            Action = x.Action,
            Module = x.Module,
            EntityId = x.EntityId,
            IpAddress = x.IpAddress,
            Details = x.Details,
            Success = x.Success ?? true
        }).ToListAsync(ct);
        return View(model);
    }
}
