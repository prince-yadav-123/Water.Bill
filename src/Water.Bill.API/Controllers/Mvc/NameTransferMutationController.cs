using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.ConsumerWorkflows;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class NameTransferMutationController : Controller
{
    private const string ModuleName = AppConstants.Modules.NameTransferMutation;
    private const string AppType = "TRN";
    private readonly ApplicationDbContext _db;

    public NameTransferMutationController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/NameTransferMutation")]
    [RequirePermission("Name Transfer / Mutation.view")]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;
        var query = _db.MasterApplicationDetails.AsNoTracking().Where(x => x.AppType == AppType);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ApplicationId.Contains(term) || (x.ConsNo != null && x.ConsNo.Contains(term)) || (x.ConName != null && x.ConName.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ApplicationStatus == status);

        return View(new ConsumerChangeListViewModel
        {
            ModuleTitle = ModuleName,
            ModuleDescription = "Create, approve, and track consumer name transfer / mutation requests.",
            Search = search ?? string.Empty,
            Status = status ?? string.Empty,
            Applications = await query.OrderByDescending(x => x.EnterDate).ThenByDescending(x => x.ApplicationId).Take(250).ToListAsync(ct)
        });
    }

    [HttpGet("/NameTransferMutation/SearchConsumer")]
    [RequirePermission("Name Transfer / Mutation.add")]
    public async Task<IActionResult> SearchConsumer(string? search, CancellationToken ct)
    {
        ViewData["Title"] = "Select Consumer";
        ViewData["ActiveMenu"] = ModuleName;
        var model = new ConsumerSearchViewModel { ModuleTitle = ModuleName, CreateAction = nameof(Create), Search = search ?? string.Empty };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            model.Consumers = await _db.ConsumerDetailsMasters.AsNoTracking()
                .Where(x => x.ConsNo.Contains(term) || (x.ConsNm1 != null && x.ConsNm1.Contains(term)) || (x.MobNo != null && x.MobNo.Contains(term)) || (x.FlatNo != null && x.FlatNo.Contains(term)))
                .OrderBy(x => x.ConsNo)
                .Take(50)
                .ToListAsync(ct);
        }
        return View(model);
    }

    [HttpGet("/NameTransferMutation/Create/{consumerNo}")]
    [RequirePermission("Name Transfer / Mutation.add")]
    public async Task<IActionResult> Create(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Create Name Transfer";
        ViewData["ActiveMenu"] = ModuleName;
        var consumer = await FindConsumerAsync(consumerNo, ct);
        if (consumer is null)
            return NotFound();
        return View(new NameTransferCreateViewModel
        {
            ConsumerNo = consumer.ConsNo,
            Consumer = consumer,
            MobileNo = consumer.MobNo
        });
    }

    [HttpPost("/NameTransferMutation/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Name Transfer / Mutation.add")]
    public async Task<IActionResult> Create(NameTransferCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Name Transfer";
        ViewData["ActiveMenu"] = ModuleName;
        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.Consumer = await FindConsumerAsync(model.ConsumerNo, ct);
        if (model.Consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer not found.");
        if (!ModelState.IsValid || model.Consumer is null)
            return View(model);

        var appId = await GenerateApplicationIdAsync(ct);
        var requestDetail = Encode(new Dictionary<string, string?>
        {
            ["OldName"] = model.Consumer.ConsNm1,
            ["OldFather"] = model.Consumer.ConsNm2,
            ["NewName"] = model.NewConsumerName,
            ["NewFather"] = model.NewFatherName,
            ["Mobile"] = model.MobileNo,
            ["TransferFee"] = model.TransferFee?.ToString("0.##"),
            ["SecurityAmount"] = model.SecurityAmount?.ToString("0.##"),
            ["ChallanNo"] = model.ChallanNo,
            ["ChallanDate"] = model.ChallanDate?.ToString("yyyy-MM-dd"),
            ["Remarks"] = model.Remarks
        });
        var prev = $"Old: {model.Consumer.ConsNm1}; New: {model.NewConsumerName}";
        var app = new MasterApplicationDetail
        {
            ApplicationId = appId,
            ConsNo = model.ConsumerNo,
            ConName = model.NewConsumerName.Trim(),
            ConAddress = model.Consumer.ConsAddress,
            ConPhoneMobile = NormalizeNullable(model.MobileNo),
            SectorVill = model.Consumer.Sector,
            Block = model.Consumer.BlkNo,
            PlotNo = model.Consumer.FlatNo,
            PlotArea = model.Consumer.PlotSize?.ToString(),
            PipeSize = model.Consumer.PipeSize?.ToString(),
            PrevConDetail = Truncate(prev, 100),
            Status = 1,
            EnterDate = DateOnly.FromDateTime(DateTime.Today),
            DivName = DivisionCode(model.Consumer.DevType),
            ApplicationStatus = "Pending",
            ApplcationStatusDetail = requestDetail,
            Reg = 0,
            CurrentHoldingPer = DivisionCode(model.Consumer.DevType),
            AppType = AppType
        };
        _db.MasterApplicationDetails.Add(app);
        AddHistory(appId, app.DivName, "Name transfer request created. " + model.Remarks, "1");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Name transfer application {appId} created.";
        return RedirectToAction(nameof(Details), new { id = appId });
    }

    [HttpGet("/NameTransferMutation/Details/{id}")]
    [RequirePermission("Name Transfer / Mutation.view")]
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        ViewData["Title"] = "Name Transfer Details";
        ViewData["ActiveMenu"] = ModuleName;
        var model = await BuildDetailsAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost("/NameTransferMutation/Approve")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Name Transfer / Mutation.edit")]
    public async Task<IActionResult> Approve(ConsumerChangeActionViewModel model, CancellationToken ct)
    {
        var app = await _db.MasterApplicationDetails.FirstOrDefaultAsync(x => x.ApplicationId == model.ApplicationId && x.AppType == AppType, ct);
        if (app is null)
            return NotFound();
        var consumer = await FindConsumerAsync(app.ConsNo ?? string.Empty, ct);
        if (consumer is null)
            return NotFound();
        if (!IsPending(app))
        {
            TempData["ErrorMessage"] = "Only pending applications can be approved.";
            return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
        }

        var oldName = consumer.ConsNm1;
        var oldFather = consumer.ConsNm2;
        consumer.ConsNm1 = app.ConName;
        var detail = Decode(app.ApplcationStatusDetail);
        consumer.ConsNm2 = Value(detail, "NewFather") ?? consumer.ConsNm2;
        consumer.MobNo = app.ConPhoneMobile ?? consumer.MobNo;
        consumer.ModifyDate = DateTime.Now;
        consumer.Userid = CurrentUserIdString();

        _db.ConsumerTransfers.Add(new ConsumerTransfer
        {
            ConsNo = consumer.ConsNo,
            ConsNm = app.ConName,
            ConsFnm = consumer.ConsNm2,
            TransDate = DateTime.Now,
            TransAmt = ToDouble(ExtractDecimal(detail, "TransferFee")),
            ChallanNo = Value(detail, "ChallanNo"),
            ChallanDate = ExtractDate(detail, "ChallanDate") ?? DateTime.Now,
            Secu = ToDouble(ExtractDecimal(detail, "SecurityAmount")) ?? 0,
            Status = 1,
            Userid = CurrentUserIdString(),
            EntryDate = DateTime.Now,
            DevType = consumer.DevType
        });

        app.ApplicationStatus = "Approved";
        app.StatusDate = DateOnly.FromDateTime(DateTime.Today);
        app.ApplcationStatusDetail = $"Approved. Old name: {oldName}, old father/name2: {oldFather}. {model.Remarks}";
        AddHistory(app.ApplicationId, app.DivName, "Approved and applied to consumer master. " + model.Remarks, "2");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Name transfer approved for consumer {consumer.ConsNo}.";
        return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
    }

    [HttpPost("/NameTransferMutation/Reject")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Name Transfer / Mutation.edit")]
    public async Task<IActionResult> Reject(ConsumerChangeActionViewModel model, CancellationToken ct)
    {
        var app = await _db.MasterApplicationDetails.FirstOrDefaultAsync(x => x.ApplicationId == model.ApplicationId && x.AppType == AppType, ct);
        if (app is null)
            return NotFound();
        if (!IsPending(app))
        {
            TempData["ErrorMessage"] = "Only pending applications can be rejected.";
            return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
        }
        app.ApplicationStatus = "Rejected";
        app.StatusDate = DateOnly.FromDateTime(DateTime.Today);
        app.ApplcationStatusDetail = model.Remarks;
        AddHistory(app.ApplicationId, app.DivName, "Rejected. " + model.Remarks, "3");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Name transfer application rejected.";
        return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
    }

    private async Task<ConsumerChangeDetailsViewModel?> BuildDetailsAsync(string id, CancellationToken ct)
    {
        var app = await _db.MasterApplicationDetails.AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == id && x.AppType == AppType, ct);
        if (app is null)
            return null;
        return new ConsumerChangeDetailsViewModel
        {
            ModuleTitle = ModuleName,
            BackAction = nameof(Index),
            Application = app,
            Consumer = await FindConsumerAsync(app.ConsNo ?? string.Empty, ct),
            DetailValues = Decode(app.ApplcationStatusDetail),
            Histories = await _db.MasterApplicationDetailHistories.AsNoTracking().Where(x => x.ApplicationId == id).OrderByDescending(x => x.SerialNumber).ToListAsync(ct)
        };
    }

    private async Task<ConsumerDetailsMaster?> FindConsumerAsync(string consumerNo, CancellationToken ct)
        => await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

    private async Task<string> GenerateApplicationIdAsync(CancellationToken ct)
    {
        var ids = await _db.MasterApplicationDetails.AsNoTracking().Where(x => x.AppType == AppType).Select(x => x.ApplicationId).ToListAsync(ct);
        var next = ids.Select(x => int.TryParse(x, out var id) ? id : 30000000).DefaultIfEmpty(30000000).Max() + 1;
        return next.ToString();
    }

    private void AddHistory(string appId, string? division, string remark, string status)
    {
        var next = (_db.MasterApplicationDetailHistories.Where(x => x.ApplicationId == appId).Select(x => x.SerialNumber ?? 0).DefaultIfEmpty().Max()) + 1;
        _db.MasterApplicationDetailHistories.Add(new MasterApplicationDetailHistory
        {
            ApplicationId = appId,
            SerialNumber = next,
            Division = division,
            CurrentHoldingPer = division,
            ForwardDate = DateOnly.FromDateTime(DateTime.Today),
            Remark = Truncate(remark, 200),
            Flag = "N",
            CurentStatus = status,
            Status = "1"
        });
    }

    private static bool IsPending(MasterApplicationDetail app) => string.Equals(app.ApplicationStatus, "Pending", StringComparison.OrdinalIgnoreCase);
    private static string DivisionCode(int? devType) => AppConstants.Divisions.Find(devType)?.Code ?? AppConstants.Divisions.Jal1.Code;
    private string CurrentUserIdString()
    {
        var value = User.FindFirstValue(AppConstants.Claims.UserId) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        return value.Length <= 10 ? value : "0";
    }
    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
    private static double? ToDouble(decimal? value) => value.HasValue ? Convert.ToDouble(value.Value) : null;
    private static decimal? ExtractDecimal(IReadOnlyDictionary<string, string> detail, string key) => decimal.TryParse(Value(detail, key), out var value) ? value : null;
    private static DateTime? ExtractDate(IReadOnlyDictionary<string, string> detail, string key) => DateTime.TryParse(Value(detail, key), out var value) ? value : null;
    private static string? Value(IReadOnlyDictionary<string, string> detail, string key) => detail.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
    private static string Encode(IDictionary<string, string?> values) => string.Join(";", values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={x.Value!.Replace(";", ",").Replace("=", "-")}"));
    private static Dictionary<string, string> Decode(string? text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
            return result;

        foreach (var item in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = item.Split('=', 2);
            if (parts.Length == 2)
                result[parts[0]] = parts[1];
        }

        return result;
    }
}
