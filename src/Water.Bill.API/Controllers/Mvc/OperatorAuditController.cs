using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Audit;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Extensions;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class OperatorAuditController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public OperatorAuditController(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("/OperatorAudit")]
    [HttpGet("/UserActivityLogs")]
    [RequirePermission("User Activity Logs.view")]
    public Task<IActionResult> Index(ActivityLogIndexViewModel model, CancellationToken ct)
        => BuildIndexAsync(ActivityLogAudience.Authority, model, ct);

    [HttpGet("/ConsumerActivityLogs")]
    [RequirePermission("Consumer Activity Logs.view")]
    public Task<IActionResult> ConsumerIndex(ActivityLogIndexViewModel model, CancellationToken ct)
        => BuildIndexAsync(ActivityLogAudience.Consumer, model, ct);

    [HttpGet("/UserActivityLogs/Details/{id:int}")]
    [RequirePermission("User Activity Logs.view")]
    public Task<IActionResult> Details(int id, CancellationToken ct)
        => BuildDetailsAsync(ActivityLogAudience.Authority, id, ct);

    [HttpGet("/ConsumerActivityLogs/Details/{id:int}")]
    [RequirePermission("Consumer Activity Logs.view")]
    public Task<IActionResult> ConsumerDetails(int id, CancellationToken ct)
        => BuildDetailsAsync(ActivityLogAudience.Consumer, id, ct);

    private async Task<IActionResult> BuildIndexAsync(ActivityLogAudience audience, ActivityLogIndexViewModel model, CancellationToken ct)
    {
        var config = BuildScreenConfig(audience);
        ViewData["Title"] = config.Title;
        ViewData["ActiveMenu"] = config.ActiveMenu;

        var query = BuildAudienceQuery(audience)
            .Select(x => new ActivityLogListProjection
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                UserId = x.UserId,
                Username = x.Username,
                Action = x.Action,
                Module = x.Module,
                EntityId = x.EntityId,
                IpAddress = x.IpAddress,
                Details = x.Details,
                Success = x.Success
            });
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search.Trim();
            var hasActionSearch = int.TryParse(term, out var actionSearch);
            query = query.Where(x =>
                (x.Username != null && x.Username.Contains(term)) ||
                (x.Module != null && x.Module.Contains(term)) ||
                (x.EntityId != null && x.EntityId.Contains(term)) ||
                (x.Details != null && x.Details.Contains(term)) ||
                (x.IpAddress != null && x.IpAddress.Contains(term)) ||
                (hasActionSearch && x.Action == actionSearch));
        }

        if (model.Action.HasValue)
            query = query.Where(x => x.Action == model.Action.Value);

        if (!string.IsNullOrWhiteSpace(model.Module))
            query = query.Where(x => x.Module == model.Module);

        if (model.Success.HasValue)
            query = query.Where(x => (x.Success ?? true) == model.Success.Value);

        if (model.FromDate.HasValue)
            query = query.Where(x => x.Timestamp >= model.FromDate.Value.Date);

        if (model.ToDate.HasValue)
            query = query.Where(x => x.Timestamp < model.ToDate.Value.Date.AddDays(1));

        var page = PagingConstants.ValidatePage(model.Page);
        var pageSize = PagingConstants.Validate(model.PageSize == 0 ? PagingConstants.DefaultPageSize : model.PageSize);

        var paged = await query
            .OrderByDescending(x => x.Timestamp)
            .ToPagedResultAsync(page, pageSize, ct);

        model.Rows = paged.Items.Select(x => new ActivityLogRowViewModel
        {
            Id = x.Id,
            Timestamp = x.Timestamp,
            UserId = x.UserId,
            Username = x.Username,
            Action = x.Action,
            ActionLabel = AuditLogDisplayHelper.GetActionLabel(x.Action, x.Module, x.Details),
            Module = x.Module,
            ModuleLabel = AuditLogDisplayHelper.GetModuleLabel(x.Module),
            EntityLabel = AuditLogDisplayHelper.GetEntityLabel(x.Module),
            EntityId = x.EntityId,
            IpAddress = x.IpAddress,
            Details = x.Details,
            Success = x.Success ?? true,
            PortalType = ResolvePortalType(audience)
        }).ToList();
        model.Page = page;
        model.PageSize = pageSize;
        model.Audience = audience;
        model.Title = config.Title;
        model.Description = config.Description;
        model.ActiveMenu = config.ActiveMenu;
        model.DetailsRouteName = config.DetailsRouteName;
        model.LegacyRouteName = config.LegacyRouteName;
        model.ActionOptions = await BuildActionOptionsAsync(audience, ct);
        model.ModuleOptions = await BuildModuleOptionsAsync(audience, ct);
        ViewBag.Pagination = PaginationViewModel.Create(paged);

        return View("Index", model);
    }

    private async Task<IActionResult> BuildDetailsAsync(ActivityLogAudience audience, int id, CancellationToken ct)
    {
        var config = BuildScreenConfig(audience);
        ViewData["Title"] = config.Title;
        ViewData["ActiveMenu"] = config.ActiveMenu;

        var entity = await BuildAudienceQuery(audience)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            return NotFound();

        var model = new ActivityLogDetailsViewModel
        {
            Id = entity.Id,
            Audience = audience,
            Title = config.Title,
            ActiveMenu = config.ActiveMenu,
            Description = config.Description,
            BackRouteName = config.LegacyRouteName,
            Timestamp = entity.Timestamp,
            UserId = entity.UserId,
            Username = entity.Username,
            PortalType = ResolvePortalType(audience),
            Action = entity.Action,
            ActionLabel = AuditLogDisplayHelper.GetActionLabel(entity.Action, entity.Module, entity.Details),
            Module = entity.Module,
            ModuleLabel = AuditLogDisplayHelper.GetModuleLabel(entity.Module),
            EntityLabel = AuditLogDisplayHelper.GetEntityLabel(entity.Module),
            EntityId = entity.EntityId,
            Success = entity.Success ?? true,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Details = entity.Details
        };

        return View("Details", model);
    }

    private IQueryable<Auditlog> BuildAudienceQuery(ActivityLogAudience audience)
    {
        var authorityUsernames = _db.Appusers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => x.Username);

        var consumerUsernames = _db.ConsumerUsers
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .Select(x => x.Username);

        var authorityModules = AuditLogDisplayHelper.AuthorityModules;
        var consumerModules = AuditLogDisplayHelper.ConsumerModules;

        var query = _db.Auditlogs.AsNoTracking().AsQueryable();
        if (audience == ActivityLogAudience.Consumer)
        {
            return query.Where(x =>
                (x.Module != null && consumerModules.Contains(x.Module)) ||
                (
                    (x.Module == null || !authorityModules.Contains(x.Module))
                    && x.Username != null
                    && consumerUsernames.Contains(x.Username)
                ) ||
                (
                    x.Module == AuditLogDisplayHelper.AuthorizationModule
                    && x.Details != null
                    && x.Details.Contains("/Consumer/")
                ));
        }

        return query.Where(x =>
            (
                x.Module == AuditLogDisplayHelper.AuthorizationModule
                && (x.Details == null || !x.Details.Contains("/Consumer/"))
            ) ||
            (x.Module != null && authorityModules.Contains(x.Module)) ||
            (
                (x.Module == null || !consumerModules.Contains(x.Module))
                && x.Username != null
                && authorityUsernames.Contains(x.Username)
            ));
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildActionOptionsAsync(ActivityLogAudience audience, CancellationToken ct)
        => await _cache.GetOrCreateAsync($"lookup:activity-log-actions:{audience}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var actions = await BuildAudienceQuery(audience)
                .Select(x => x.Action)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(ct);

            return actions
                .Select(x => new SelectListItem(AuditLogDisplayHelper.GetActionLabel(x), x.ToString()))
                .ToList();
        }) ?? [];

    private async Task<IReadOnlyList<SelectListItem>> BuildModuleOptionsAsync(ActivityLogAudience audience, CancellationToken ct)
        => await _cache.GetOrCreateAsync($"lookup:activity-log-modules:{audience}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var modules = await BuildAudienceQuery(audience)
                .Where(x => x.Module != null && x.Module != string.Empty)
                .Select(x => x.Module!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(ct);

            return modules
                .Select(x => new SelectListItem(AuditLogDisplayHelper.GetModuleLabel(x), x))
                .ToList();
        }) ?? [];

    private static string ResolvePortalType(ActivityLogAudience audience)
        => audience == ActivityLogAudience.Consumer ? AppConstants.PortalTypes.Consumer : AppConstants.PortalTypes.Admin;

    private static (string Title, string Description, string ActiveMenu, string DetailsRouteName, string LegacyRouteName) BuildScreenConfig(ActivityLogAudience audience)
    {
        return audience == ActivityLogAudience.Consumer
            ? (
                AppConstants.Modules.ConsumerActivityLogs,
                "Review Consumer Portal actions separately from authority-side operations.",
                AppConstants.Modules.ConsumerActivityLogs,
                nameof(ConsumerDetails),
                nameof(ConsumerIndex))
            : (
                AppConstants.Modules.UserActivityLogs,
                "Review admin and authority user activity captured in the audit trail.",
                AppConstants.Modules.UserActivityLogs,
                nameof(Details),
                nameof(Index));
    }

    private sealed class ActivityLogListProjection
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public int Action { get; set; }
        public string? Module { get; set; }
        public string? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public bool? Success { get; set; }
    }
}
