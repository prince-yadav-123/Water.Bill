using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.ConsumerWorkflows;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConnectionTypeCategoryChangeController : Controller
{
    private const string ModuleName = AppConstants.Modules.ConnectionTypeCategoryChange;
    private const string AppType = "CTC";
    private readonly ApplicationDbContext _db;

    public ConnectionTypeCategoryChangeController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/ConnectionTypeCategoryChange")]
    [RequirePermission("Connection Type / Category Change.view")]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken ct)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;
        var query = _db.MasterApplicationDetails.AsNoTracking().Where(x => x.AppType == AppType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ApplicationId.Contains(term)
                || (x.ConsNo != null && x.ConsNo.Contains(term))
                || (x.ConName != null && x.ConName.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ApplicationStatus == status);

        return View(new ConsumerChangeListViewModel
        {
            ModuleTitle = ModuleName,
            ModuleDescription = "Create, approve, and track connection type or category change requests.",
            Search = search ?? string.Empty,
            Status = status ?? string.Empty,
            Applications = await query.OrderByDescending(x => x.EnterDate).ThenByDescending(x => x.ApplicationId).Take(250).ToListAsync(ct)
        });
    }

    [HttpGet("/ConnectionTypeCategoryChange/SearchConsumer")]
    [RequirePermission("Connection Type / Category Change.add")]
    public async Task<IActionResult> SearchConsumer(string? search, CancellationToken ct)
    {
        ViewData["Title"] = "Select Consumer";
        ViewData["ActiveMenu"] = ModuleName;
        var model = new ConsumerSearchViewModel { ModuleTitle = ModuleName, CreateAction = nameof(Create), Search = search ?? string.Empty };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            model.Consumers = await _db.ConsumerDetailsMasters.AsNoTracking()
                .Where(x => x.ConsNo.Contains(term)
                    || (x.ConsNm1 != null && x.ConsNm1.Contains(term))
                    || (x.MobNo != null && x.MobNo.Contains(term))
                    || (x.FlatNo != null && x.FlatNo.Contains(term)))
                .OrderBy(x => x.ConsNo)
                .Take(50)
                .ToListAsync(ct);
        }

        return View(model);
    }

    [HttpGet("/ConnectionTypeCategoryChange/Create/{consumerNo}")]
    [RequirePermission("Connection Type / Category Change.add")]
    public async Task<IActionResult> Create(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Create Connection Change";
        ViewData["ActiveMenu"] = ModuleName;
        var consumer = await FindConsumerAsync(consumerNo, ct);
        if (consumer is null)
            return NotFound();

        return View(PrepareCreateModel(new ConnectionChangeCreateViewModel
        {
            ConsumerNo = consumer.ConsNo,
            Consumer = consumer,
            NewConnectionType = consumer.ConTp,
            NewConsumerCategory = consumer.ConsCtg ?? string.Empty,
            TypeChangeDate = DateTime.Today,
            SecurityAmount = consumer.Secu,
            MonthlyRate = ToDecimal(consumer.MonthlyRate)
        }));
    }

    [HttpPost("/ConnectionTypeCategoryChange/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Connection Type / Category Change.add")]
    public async Task<IActionResult> Create(ConnectionChangeCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Connection Change";
        ViewData["ActiveMenu"] = ModuleName;
        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.Consumer = await FindConsumerAsync(model.ConsumerNo, ct);
        if (model.Consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer not found.");
        if (string.IsNullOrWhiteSpace(model.NewConnectionType) && string.IsNullOrWhiteSpace(model.NewConsumerCategory))
            ModelState.AddModelError(nameof(model.NewConsumerCategory), "Select at least one new connection type/category value.");

        if (!ModelState.IsValid || model.Consumer is null)
            return View(PrepareCreateModel(model));

        var appId = await GenerateApplicationIdAsync(ct);
        var division = DivisionCode(model.Consumer.DevType);
        var detail = Encode(new Dictionary<string, string?>
        {
            ["OldConnectionType"] = model.Consumer.ConTp,
            ["NewConnectionType"] = model.NewConnectionType,
            ["OldCategory"] = model.Consumer.ConsCtg,
            ["NewCategory"] = model.NewConsumerCategory,
            ["TypeChangeDate"] = model.TypeChangeDate.ToString("yyyy-MM-dd"),
            ["EstimationNo"] = model.EstimationNo,
            ["EstimationAmount"] = model.EstimationAmount?.ToString("0.##"),
            ["SecurityAmount"] = model.SecurityAmount?.ToString("0.##"),
            ["MonthlyRate"] = model.MonthlyRate?.ToString("0.##"),
            ["Remarks"] = model.Remarks
        });

        var app = new MasterApplicationDetail
        {
            ApplicationId = appId,
            ConsNo = model.ConsumerNo,
            ConName = model.Consumer.ConsNm1,
            ConAddress = model.Consumer.ConsAddress,
            ConPhoneMobile = model.Consumer.MobNo,
            SectorVill = model.Consumer.Sector,
            Block = model.Consumer.BlkNo,
            PlotNo = model.Consumer.FlatNo,
            PlotArea = model.Consumer.PlotSize?.ToString(),
            PipeSize = model.Consumer.PipeSize?.ToString(),
            ConnType = model.NewConnectionType,
            PropertyType = model.NewConsumerCategory,
            PrevConDetail = Truncate($"Old type: {model.Consumer.ConTp}; Old category: {model.Consumer.ConsCtg}", 100),
            Status = 1,
            EnterDate = DateOnly.FromDateTime(DateTime.Today),
            DivName = division,
            ApplicationStatus = "Pending",
            ApplcationStatusDetail = detail,
            Reg = 0,
            CurrentHoldingPer = division,
            AppType = AppType
        };

        _db.MasterApplicationDetails.Add(app);
        AddHistory(appId, division, "Connection type/category change request created. " + model.Remarks, "1");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Connection change application {appId} created.";
        return RedirectToAction(nameof(Details), new { id = appId });
    }

    [HttpGet("/ConnectionTypeCategoryChange/Details/{id}")]
    [RequirePermission("Connection Type / Category Change.view")]
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        ViewData["Title"] = "Connection Change Details";
        ViewData["ActiveMenu"] = ModuleName;
        var model = await BuildDetailsAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost("/ConnectionTypeCategoryChange/Approve")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Connection Type / Category Change.edit")]
    public async Task<IActionResult> Approve(ConsumerChangeActionViewModel model, CancellationToken ct)
    {
        var app = await _db.MasterApplicationDetails.FirstOrDefaultAsync(x => x.ApplicationId == model.ApplicationId && x.AppType == AppType, ct);
        if (app is null)
            return NotFound();
        if (!IsPending(app))
        {
            TempData["ErrorMessage"] = "Only pending applications can be approved.";
            return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
        }

        var consumer = await FindConsumerAsync(app.ConsNo ?? string.Empty, ct);
        if (consumer is null)
            return NotFound();

        var detail = Decode(app.ApplcationStatusDetail);
        consumer.ConTp = NormalizeConnectionTypeCode(Value(detail, "NewConnectionType")) ?? consumer.ConTp;
        consumer.ConsCtg = NormalizeConsumerCategoryCode(Value(detail, "NewCategory")) ?? consumer.ConsCtg;
        consumer.TypeChangeDate = ExtractDate(detail, "TypeChangeDate") ?? DateTime.Today;
        consumer.EstiNo = Value(detail, "EstimationNo") ?? consumer.EstiNo;
        consumer.EstiAmt = ToInt(ExtractDecimal(detail, "EstimationAmount")) ?? consumer.EstiAmt;
        consumer.Secu = ToInt(ExtractDecimal(detail, "SecurityAmount")) ?? consumer.Secu;
        consumer.MonthlyRate = ToDouble(ExtractDecimal(detail, "MonthlyRate")) ?? consumer.MonthlyRate;
        consumer.ModifyDate = DateTime.Now;
        consumer.Userid = CurrentUserIdString();

        app.ApplicationStatus = "Approved";
        app.StatusDate = DateOnly.FromDateTime(DateTime.Today);
        app.ApplcationStatusDetail = app.ApplcationStatusDetail + $";ApprovalRemarks={Sanitize(model.Remarks)}";
        AddHistory(app.ApplicationId, app.DivName, "Approved and applied to consumer master. " + model.Remarks, "2");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Connection type/category updated for consumer {consumer.ConsNo}.";
        return RedirectToAction(nameof(Details), new { id = app.ApplicationId });
    }

    [HttpPost("/ConnectionTypeCategoryChange/Reject")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Connection Type / Category Change.edit")]
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
        app.ApplcationStatusDetail = app.ApplcationStatusDetail + $";RejectionRemarks={Sanitize(model.Remarks)}";
        AddHistory(app.ApplicationId, app.DivName, "Rejected. " + model.Remarks, "3");
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Connection change application rejected.";
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
            Consumer = await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == app.ConsNo, ct),
            DetailValues = Decode(app.ApplcationStatusDetail),
            Histories = await _db.MasterApplicationDetailHistories.AsNoTracking()
                .Where(x => x.ApplicationId == id)
                .OrderByDescending(x => x.SerialNumber)
                .ToListAsync(ct)
        };
    }

    private ConnectionChangeCreateViewModel PrepareCreateModel(ConnectionChangeCreateViewModel model)
    {
        model.ConnectionTypeOptions = ConnectionTypeOptions(model.NewConnectionType);
        model.ConsumerCategoryOptions = ConsumerCategoryOptions(model.NewConsumerCategory);
        return model;
    }

    private async Task<ConsumerDetailsMaster?> FindConsumerAsync(string consumerNo, CancellationToken ct)
        => await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

    private async Task<string> GenerateApplicationIdAsync(CancellationToken ct)
    {
        var ids = await _db.MasterApplicationDetails.AsNoTracking().Where(x => x.AppType == AppType).Select(x => x.ApplicationId).ToListAsync(ct);
        var next = ids.Select(x => int.TryParse(x, out var id) ? id : 40000000).DefaultIfEmpty(40000000).Max() + 1;
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

    private static IReadOnlyList<SelectListItem> ConnectionTypeOptions(string? selected) =>
    [
        Option("R", "Residential", selected),
        Option("C", "Commercial", selected),
        Option("I", "Institutional", selected),
        Option("T", "Industrial", selected),
        Option("S", "Staff", selected),
        Option("V", "Village", selected),
        Option("H", "Housing", selected),
        Option("G", "Group Housing", selected)
    ];

    private static string? NormalizeConnectionTypeCode(string? value)
    {
        var normalized = NormalizeToken(value);
        return normalized switch
        {
            "R" or "RESIDENTIAL" => "R",
            "C" or "COMMERCIAL" => "C",
            "I" or "INSTITUTIONAL" => "I",
            "T" or "INDUSTRIAL" or "INDUSTRY" => "T",
            "S" or "STAFF" => "S",
            "V" or "VILLAGE" => "V",
            "H" or "HOUSING" => "H",
            "G" or "GROUPHOUSING" or "GROUP HOUSING" => "G",
            "CC" or "COURTCASE" or "COURT CASE" => "CC",
            "D" or "DISCONNECTION" or "DISCONNECTED" => "D",
            _ => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 1)]
        };
    }

    private static string? NormalizeConsumerCategoryCode(string? value)
    {
        var normalized = NormalizeToken(value);
        return normalized switch
        {
            "R" or "REGULAR" => "R",
            "T" or "TEMPORARY" => "T",
            "S" or "STAFF" => "S",
            "M" or "RMC" => "M",
            "CC" or "COURTCASE" or "COURT CASE" => "CC",
            "D" or "DISCONNECTION" or "DISCONNECTED" => "D",
            _ => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 10)]
        };
    }

    private static string NormalizeToken(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static IReadOnlyList<SelectListItem> ConsumerCategoryOptions(string? selected) =>
    [
        Option("R", "Regular", selected),
        Option("T", "Temporary", selected),
        Option("S", "Staff", selected),
        Option("M", "RMC", selected),
        Option("CC", "Court Case", selected),
        Option("D", "Disconnected", selected)
    ];

    private static SelectListItem Option(string value, string text, string? selected) => new() { Value = value, Text = $"{text} ({value})", Selected = string.Equals(value, selected, StringComparison.OrdinalIgnoreCase) };
    private static bool IsPending(MasterApplicationDetail app) => string.Equals(app.ApplicationStatus, "Pending", StringComparison.OrdinalIgnoreCase);
    private static string DivisionCode(int? devType) => AppConstants.Divisions.Find(devType)?.Code ?? AppConstants.Divisions.Jal1.Code;
    private string CurrentUserIdString()
    {
        var value = User.FindFirstValue(AppConstants.Claims.UserId) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        return value.Length <= 10 ? value : "0";
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
    private static decimal? ToDecimal(double? value) => value.HasValue ? Convert.ToDecimal(value.Value) : null;
    private static int? ToInt(decimal? value) => value.HasValue ? Convert.ToInt32(value.Value) : null;
    private static double? ToDouble(decimal? value) => value.HasValue ? Convert.ToDouble(value.Value) : null;
    private static decimal? ExtractDecimal(IReadOnlyDictionary<string, string> detail, string key) => decimal.TryParse(Value(detail, key), out var value) ? value : null;
    private static DateTime? ExtractDate(IReadOnlyDictionary<string, string> detail, string key) => DateTime.TryParse(Value(detail, key), out var value) ? value : null;
    private static string? Value(IReadOnlyDictionary<string, string> detail, string key) => detail.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
    private static string Encode(IDictionary<string, string?> values) => string.Join(";", values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={Sanitize(x.Value)}"));
    private static string Sanitize(string? value) => (value ?? string.Empty).Replace(";", ",").Replace("=", "-").Trim();
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
