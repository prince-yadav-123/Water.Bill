using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Notices;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class NoticeManagementController : Controller
{
    private const string ModuleName = AppConstants.Modules.NoticeManagement;
    private readonly ApplicationDbContext _db;

    public NoticeManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/NoticeManagement")]
    [RequirePermission("Notice Management.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? noticeType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;

        var model = new NoticeManagementIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            NoticeType = Normalize(noticeType),
            Status = Normalize(status),
            FromDate = fromDate,
            ToDate = toDate,
            TypeOptions = NoticeTypes.Options(noticeType),
            StatusOptions = NoticeStatuses.Options(status)
        };

        model.HasConsumerSearch = HasAnySearch(model.ConsumerNo, model.ConsumerName, model.MobileNo);
        model.Consumers = model.HasConsumerSearch ? await SearchConsumersAsync(model, ct) : [];
        model.Notices = await SearchNoticesAsync(model, ct);
        return View(model);
    }

    [HttpGet("/NoticeManagement/Create")]
    [RequirePermission("Notice Management.add")]
    public async Task<IActionResult> Create(string consumerNo, string? noticeType, int? templateId, CancellationToken ct)
    {
        ViewData["Title"] = "Create Notice";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction(nameof(Index));

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
        if (consumer is null)
            return NotFound();

        var selectedType = Normalize(noticeType) ?? NoticeTypes.GeneralNotice;
        var template = templateId.HasValue
            ? await _db.NoticeTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == templateId.Value && !x.IsDeleted, ct)
            : await _db.NoticeTemplates.AsNoTracking()
                .Where(x => x.NoticeType == selectedType && x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefaultAsync(ct);

        if (template is not null)
            selectedType = template.NoticeType;

        var amountDue = await CalculateOutstandingAsync(consumerNo, ct);
        var model = new NoticeCreateViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = ToConsumerSummary(consumer),
            TemplateId = template?.Id,
            NoticeType = selectedType,
            Subject = template?.Subject ?? NoticeTypes.Display(selectedType),
            Body = RenderTemplate(template?.Body ?? DefaultBody(selectedType), consumer, amountDue, null, null, null),
            AmountDue = amountDue,
            DueDate = selectedType is NoticeTypes.DueNotice or NoticeTypes.DisconnectionNotice or NoticeTypes.DemandNotice
                ? DateTime.Today.AddDays(15)
                : null
        };

        await PrepareCreateOptionsAsync(model, ct);
        return View(model);
    }

    [HttpPost("/NoticeManagement/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Notice Management.add")]
    public async Task<IActionResult> Create(NoticeCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Notice";
        ViewData["ActiveMenu"] = ModuleName;

        NormalizeCreateModel(model);
        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
        if (consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number was not found.");
        model.Consumer = consumer is null ? null : ToConsumerSummary(consumer);
        await PrepareCreateOptionsAsync(model, ct);
        ValidateCreateModel(model);

        if (!ModelState.IsValid || consumer is null)
            return View(model);

        var notice = new ConsumerNotice
        {
            NoticeNo = await GenerateNoticeNoAsync(ct),
            ConsumerNo = model.ConsumerNo,
            TemplateId = model.TemplateId,
            NoticeType = model.NoticeType,
            Subject = model.Subject,
            Body = RenderTemplate(model.Body, consumer, model.AmountDue, model.DueDate, model.RelatedBillNo, model.RelatedChallanNo),
            NoticeDate = model.NoticeDate.Date,
            DueDate = model.DueDate?.Date,
            Status = NoticeStatuses.Draft,
            RelatedBillNo = model.RelatedBillNo,
            RelatedChallanNo = model.RelatedChallanNo,
            RelatedDisconnectionCaseId = model.RelatedDisconnectionCaseId,
            AmountDue = model.AmountDue,
            Remarks = model.Remarks,
            CreatedByUserId = CurrentUserId(),
            CreatedByName = CurrentUsername(),
            CreatedAt = DateTime.Now,
            IsActive = true,
            IsDeleted = false
        };

        notice.Histories.Add(new ConsumerNoticeHistory
        {
            ToStatus = NoticeStatuses.Draft,
            Action = "Created",
            Remarks = model.Remarks,
            ActionByUserId = CurrentUserId(),
            ActionByName = CurrentUsername(),
            ActionAt = DateTime.Now
        });

        _db.ConsumerNotices.Add(notice);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = $"Notice {notice.NoticeNo} created successfully.";
        return RedirectToAction(nameof(Details), new { id = notice.Id });
    }

    [HttpGet("/NoticeManagement/Details/{id:long}")]
    [RequirePermission("Notice Management.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Notice Details";
        ViewData["ActiveMenu"] = ModuleName;

        var model = await BuildDetailsAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet("/NoticeManagement/Print/{id:long}")]
    [RequirePermission("Notice Management.print")]
    public async Task<IActionResult> Print(long id, CancellationToken ct)
    {
        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();
        ViewData["Title"] = "Print Notice";
        return View(model);
    }

    [HttpPost("/NoticeManagement/Issue/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Notice Management.edit")]
    public async Task<IActionResult> Issue(long id, string? remarks, CancellationToken ct)
    {
        var notice = await _db.ConsumerNotices.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (notice is null)
            return NotFound();
        if (notice.Status != NoticeStatuses.Draft)
        {
            TempData["ErrorMessage"] = "Only draft notices can be issued.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ChangeStatus(notice, NoticeStatuses.Issued, "Issued", Normalize(remarks));
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Notice issued successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/NoticeManagement/Cancel/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Notice Management.delete")]
    public async Task<IActionResult> Cancel(long id, string? remarks, CancellationToken ct)
    {
        remarks = Normalize(remarks);
        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["ErrorMessage"] = "Cancellation remarks are required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var notice = await _db.ConsumerNotices.Include(x => x.Histories).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (notice is null)
            return NotFound();
        if (notice.Status == NoticeStatuses.Cancelled)
        {
            TempData["ErrorMessage"] = "Selected notice is already cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        notice.IsActive = false;
        ChangeStatus(notice, NoticeStatuses.Cancelled, "Cancelled", remarks);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Notice cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("/NoticeManagement/Templates")]
    [RequirePermission("Notice Management.view")]
    public async Task<IActionResult> Templates(CancellationToken ct)
    {
        ViewData["Title"] = "Notice Templates";
        ViewData["ActiveMenu"] = ModuleName;

        var rows = await _db.NoticeTemplates.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new NoticeTemplateRowViewModel
            {
                Id = x.Id,
                TemplateName = x.TemplateName,
                NoticeType = x.NoticeType,
                Subject = x.Subject,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return View(new NoticeTemplateIndexViewModel { Rows = rows });
    }

    private async Task PrepareCreateOptionsAsync(NoticeCreateViewModel model, CancellationToken ct)
    {
        model.TypeOptions = NoticeTypes.Options(model.NoticeType);
        model.TemplateOptions = await _db.NoticeTemplates.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new SelectListItem($"{x.TemplateName} ({NoticeTypes.Display(x.NoticeType)})", x.Id.ToString(), x.Id == model.TemplateId))
            .ToListAsync(ct);

        model.CaseOptions = await _db.ConsumerDisconnectionCases.AsNoTracking()
            .Where(x => x.ConsumerNo == model.ConsumerNo && !x.IsDeleted)
            .OrderByDescending(x => x.NoticeDate)
            .Take(20)
            .Select(x => new SelectListItem($"{x.CaseNo} - {x.Status}", x.Id.ToString(), x.Id == model.RelatedDisconnectionCaseId))
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<NoticeConsumerSearchRowViewModel>> SearchConsumersAsync(NoticeManagementIndexViewModel model, CancellationToken ct)
    {
        var query = _db.ConsumerDetailsMasters.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.ConsNm1 != null && x.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.MobNo != null && x.MobNo.Contains(model.MobileNo));

        return await query.OrderBy(x => x.ConsNo).Take(50).Select(x => new NoticeConsumerSearchRowViewModel
        {
            ConsumerNo = x.ConsNo,
            ConsumerName = x.ConsNm1,
            MobileNo = x.MobNo,
            PropertyNo = x.Sector + "/" + x.BlkNo + "-" + x.FlatNo,
            ConnectionType = x.ConTp,
            DevType = x.DevType
        }).ToListAsync(ct);
    }

    private async Task<IReadOnlyList<NoticeListRowViewModel>> SearchNoticesAsync(NoticeManagementIndexViewModel model, CancellationToken ct)
    {
        var query =
            from notice in _db.ConsumerNotices.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on notice.ConsumerNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where !notice.IsDeleted
            select new { notice, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
            query = query.Where(x => x.notice.NoticeNo.Contains(model.Search) || x.notice.Subject.Contains(model.Search) || x.notice.ConsumerNo.Contains(model.Search) || (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)));
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.notice.ConsumerNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.NoticeType))
            query = query.Where(x => x.notice.NoticeType == model.NoticeType);
        if (!string.IsNullOrWhiteSpace(model.Status))
            query = query.Where(x => x.notice.Status == model.Status);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.notice.NoticeDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.notice.NoticeDate < model.ToDate.Value.Date.AddDays(1));

        return await query.OrderByDescending(x => x.notice.NoticeDate).ThenByDescending(x => x.notice.Id).Take(200).Select(x => new NoticeListRowViewModel
        {
            Id = x.notice.Id,
            NoticeNo = x.notice.NoticeNo,
            ConsumerNo = x.notice.ConsumerNo,
            ConsumerName = x.consumer != null ? x.consumer.ConsNm1 : null,
            MobileNo = x.consumer != null ? x.consumer.MobNo : null,
            PropertyNo = x.consumer != null ? x.consumer.Sector + "/" + x.consumer.BlkNo + "-" + x.consumer.FlatNo : null,
            NoticeType = x.notice.NoticeType,
            Subject = x.notice.Subject,
            NoticeDate = x.notice.NoticeDate,
            DueDate = x.notice.DueDate,
            Status = x.notice.Status,
            AmountDue = x.notice.AmountDue
        }).ToListAsync(ct);
    }

    private async Task<NoticeDetailsViewModel?> BuildDetailsAsync(long id, CancellationToken ct)
    {
        var notice = await _db.ConsumerNotices.AsNoTracking()
            .Include(x => x.Histories.Where(h => !h.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (notice is null)
            return null;

        var consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == notice.ConsumerNo, ct);
        return new NoticeDetailsViewModel
        {
            Id = notice.Id,
            NoticeNo = notice.NoticeNo,
            ConsumerNo = notice.ConsumerNo,
            ConsumerName = consumer?.ConsNm1,
            MobileNo = consumer?.MobNo,
            PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
            Consumer = consumer is null ? null : ToConsumerSummary(consumer),
            TemplateId = notice.TemplateId,
            NoticeType = notice.NoticeType,
            Subject = notice.Subject,
            Body = notice.Body,
            NoticeDate = notice.NoticeDate,
            DueDate = notice.DueDate,
            Status = notice.Status,
            RelatedBillNo = notice.RelatedBillNo,
            RelatedChallanNo = notice.RelatedChallanNo,
            RelatedDisconnectionCaseId = notice.RelatedDisconnectionCaseId,
            AmountDue = notice.AmountDue,
            Remarks = notice.Remarks,
            CreatedByName = notice.CreatedByName,
            CreatedAt = notice.CreatedAt,
            CanIssue = notice.Status == NoticeStatuses.Draft,
            CanCancel = notice.Status != NoticeStatuses.Cancelled,
            Histories = notice.Histories.OrderByDescending(x => x.ActionAt).Select(x => new NoticeHistoryViewModel
            {
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                Action = x.Action,
                Remarks = x.Remarks,
                ActionByName = x.ActionByName,
                ActionAt = x.ActionAt
            }).ToList()
        };
    }

    private void ChangeStatus(ConsumerNotice notice, string toStatus, string action, string? remarks)
    {
        var fromStatus = notice.Status;
        notice.Status = toStatus;
        notice.UpdatedAt = DateTime.Now;
        notice.UpdatedByUserId = CurrentUserId();
        notice.UpdatedByName = CurrentUsername();
        notice.Histories.Add(new ConsumerNoticeHistory
        {
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Action = action,
            Remarks = remarks,
            ActionByUserId = CurrentUserId(),
            ActionByName = CurrentUsername(),
            ActionAt = DateTime.Now
        });
    }

    private void ValidateCreateModel(NoticeCreateViewModel model)
    {
        if (!NoticeTypes.Options().Any(x => x.Value == model.NoticeType))
            ModelState.AddModelError(nameof(model.NoticeType), "Invalid notice type.");
        if (model.DueDate.HasValue && model.DueDate.Value.Date < model.NoticeDate.Date)
            ModelState.AddModelError(nameof(model.DueDate), "Due date cannot be before notice date.");
    }

    private async Task<decimal> CalculateOutstandingAsync(string consumerNo, CancellationToken ct)
    {
        var bills = await _db.JalPrintBillMasters.AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .Select(x => new { x.TotalBillAmt, x.DueAmt, x.MinTotalAmt, x.PaidAmt, x.PaidDate, x.PaidStatus })
            .Take(300)
            .ToListAsync(ct);

        return bills.Sum(x =>
        {
            var total = ToDecimal(x.TotalBillAmt ?? x.DueAmt ?? x.MinTotalAmt ?? 0);
            var paid = x.PaidDate.HasValue || x.PaidStatus == "Y"
                ? (x.PaidAmt.HasValue ? ToDecimal(x.PaidAmt.Value) : total)
                : ToDecimal(x.PaidAmt ?? 0);
            return Math.Max(0, total - paid);
        });
    }

    private async Task<string> GenerateNoticeNoAsync(CancellationToken ct)
    {
        var prefix = $"NT{DateTime.Today:yyyyMM}";
        var existing = await _db.ConsumerNotices.AsNoTracking().Where(x => x.NoticeNo.StartsWith(prefix)).Select(x => x.NoticeNo).ToListAsync(ct);
        var max = existing.Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0).DefaultIfEmpty(0).Max();
        string candidate;
        do
        {
            max++;
            candidate = $"{prefix}{max:D5}";
        }
        while (await _db.ConsumerNotices.AnyAsync(x => x.NoticeNo == candidate, ct));
        return candidate;
    }

    private static string RenderTemplate(string body, ConsumerDetailsMaster consumer, decimal amountDue, DateTime? dueDate, string? billNo, string? challanNo)
    {
        var property = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo);
        return body
            .Replace("{ConsumerNo}", consumer.ConsNo)
            .Replace("{ConsumerName}", consumer.ConsNm1 ?? string.Empty)
            .Replace("{MobileNo}", consumer.MobNo ?? string.Empty)
            .Replace("{PropertyNo}", property)
            .Replace("{AmountDue}", amountDue.ToString("N2"))
            .Replace("{DueDate}", dueDate?.ToString("dd MMM yyyy") ?? string.Empty)
            .Replace("{BillNo}", billNo ?? string.Empty)
            .Replace("{ChallanNo}", challanNo ?? string.Empty);
    }

    private static string DefaultBody(string noticeType) => noticeType switch
    {
        NoticeTypes.DueNotice => "Dear {ConsumerName}, dues of Rs. {AmountDue} are pending against consumer no {ConsumerNo}. Please pay before {DueDate}.",
        NoticeTypes.DisconnectionNotice => "Dear {ConsumerName}, your water connection {ConsumerNo} is liable for disconnection due to pending dues of Rs. {AmountDue}. Please clear before {DueDate}.",
        NoticeTypes.DemandNotice => "Demand notice is issued for consumer no {ConsumerNo}, property {PropertyNo}, for amount Rs. {AmountDue}.",
        NoticeTypes.ReconnectionOrder => "Reconnection order is issued for consumer no {ConsumerNo}, property {PropertyNo}.",
        _ => "Notice for consumer no {ConsumerNo}, {ConsumerName}, property {PropertyNo}."
    };

    private static NoticeConsumerSummaryViewModel ToConsumerSummary(ConsumerDetailsMaster consumer)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            Address = !string.IsNullOrWhiteSpace(consumer.ConsAddress) ? consumer.ConsAddress : BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            DevType = consumer.DevType
        };

    private static void NormalizeCreateModel(NoticeCreateViewModel model)
    {
        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant() ?? string.Empty;
        model.NoticeType = Normalize(model.NoticeType) ?? NoticeTypes.GeneralNotice;
        model.Subject = Normalize(model.Subject) ?? string.Empty;
        model.Body = model.Body?.Trim() ?? string.Empty;
        model.RelatedBillNo = Normalize(model.RelatedBillNo);
        model.RelatedChallanNo = Normalize(model.RelatedChallanNo);
        model.Remarks = Normalize(model.Remarks);
    }

    private string CurrentUsername() => User.FindFirstValue(AppConstants.Claims.Username) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Admin";

    private int? CurrentUserId() => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId), out var value) ? value : null;

    private static decimal ToDecimal(double value) => Convert.ToDecimal(value);

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasAnySearch(params string?[] values) => values.Any(x => !string.IsNullOrWhiteSpace(x));
}
