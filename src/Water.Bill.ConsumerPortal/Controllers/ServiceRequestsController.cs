using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class ServiceRequestsController : Controller
{
    private const string ActiveMenu = "Support & Service Requests";
    private const string NameTransferAppType = "TRN";
    private const string ConnectionChangeAppType = "CTC";

    private readonly ApplicationDbContext _db;
    private readonly IWorkflowService _workflowService;

    public ServiceRequestsController(ApplicationDbContext db, IWorkflowService workflowService)
    {
        _db = db;
        _workflowService = workflowService;
    }

    [HttpGet("/Consumer/ServiceRequests")]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken ct)
    {
        ViewData["Title"] = "My Service Requests";
        ViewData["ActiveMenu"] = ActiveMenu;

        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var query = _db.MasterApplicationDetails
            .AsNoTracking()
            .Where(x => (x.AppType == NameTransferAppType || x.AppType == ConnectionChangeAppType)
                && linkedConsumerNos.Contains(x.ConsNo ?? string.Empty));

        search = NormalizeNullable(search);
        status = NormalizeNullable(status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.ApplicationId.Contains(search)
                || (x.ConsNo != null && x.ConsNo.Contains(search))
                || (x.ConName != null && x.ConName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ApplicationStatus == status);

        return View(new ConsumerServiceRequestListViewModel
        {
            Search = search,
            Status = status,
            Applications = await query
                .OrderByDescending(x => x.EnterDate)
                .ThenByDescending(x => x.ApplicationId)
                .Take(100)
                .ToListAsync(ct)
        });
    }

    [HttpGet("/Consumer/ServiceRequests/NameTransfer")]
    public async Task<IActionResult> NameTransfer(string? consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Name Transfer / Mutation";
        ViewData["ActiveMenu"] = ActiveMenu;

        var model = await BuildNameTransferModelAsync(consumerNo, ct);
        if (model.Consumers.Count == 0)
        {
            TempData["ErrorMessage"] = "No active consumer connection is linked with this login/mobile number.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost("/Consumer/ServiceRequests/NameTransfer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NameTransfer(NameTransferRequestViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Name Transfer / Mutation";
        ViewData["ActiveMenu"] = ActiveMenu;
        NormalizeDeclarationFromRequest(model);
        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.MobileNo = Normalize(model.MobileNo);

        var selectedConsumer = await GetLinkedConsumerAsync(model.ConsumerNo, ct);
        if (selectedConsumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Selected consumer number is not linked with your login/mobile number.");
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(model.DeclarationAccepted), "Please accept the declaration.");

        if (!ModelState.IsValid || selectedConsumer is null)
        {
            await PopulateCommonModelAsync(model, model.ConsumerNo, ct);
            return View(model);
        }

        var appId = await GenerateApplicationIdAsync(NameTransferAppType, 30000000, ct);
        var division = DivisionCode(selectedConsumer.DevType);
        var detail = Encode(new Dictionary<string, string?>
        {
            ["OldName"] = selectedConsumer.ConsNm1,
            ["OldFather"] = selectedConsumer.ConsNm2,
            ["NewName"] = model.NewConsumerName,
            ["NewFather"] = model.NewFatherName,
            ["Mobile"] = model.MobileNo,
            ["TransferFee"] = model.TransferFee?.ToString("0.##"),
            ["SecurityAmount"] = model.SecurityAmount?.ToString("0.##"),
            ["ChallanNo"] = model.ChallanNo,
            ["ChallanDate"] = model.ChallanDate?.ToString("yyyy-MM-dd"),
            ["Remarks"] = model.Remarks,
            ["AppliedBy"] = "ConsumerPortal"
        });

        var application = new MasterApplicationDetail
        {
            ApplicationId = appId,
            ConsNo = selectedConsumer.ConsNo,
            ConName = model.NewConsumerName.Trim(),
            ConAddress = selectedConsumer.ConsAddress,
            ConPhoneMobile = model.MobileNo,
            SectorVill = selectedConsumer.Sector,
            Block = selectedConsumer.BlkNo,
            PlotNo = selectedConsumer.FlatNo,
            PlotArea = selectedConsumer.PlotSize?.ToString(),
            PipeSize = selectedConsumer.PipeSize?.ToString(),
            PrevConDetail = Truncate($"Old: {selectedConsumer.ConsNm1}; New: {model.NewConsumerName}", 100),
            Status = 1,
            EnterDate = DateOnly.FromDateTime(DateTime.Today),
            DivName = division,
            ApplicationStatus = "Submitted",
            ApplcationStatusDetail = detail,
            Reg = 0,
            CurrentHoldingPer = division,
            AppType = NameTransferAppType
        };

        _db.MasterApplicationDetails.Add(application);
        AddHistory(appId, division, "Name transfer request submitted by consumer. " + model.Remarks, "1");
        await _db.SaveChangesAsync(ct);

        await _workflowService.StartWorkflowAsync(
            WorkflowService.ApplicationTypeNameTransfer,
            long.Parse(appId),
            appId,
            "Submitted",
            ResolveConsumerUserId(),
            User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
            AppConstants.Roles.Consumer,
            ct);

        TempData["SuccessMessage"] = $"Name transfer application submitted successfully. Application Number: {appId}.";
        return RedirectToAction(nameof(Details), new { id = appId });
    }

    [HttpGet("/Consumer/ServiceRequests/ConnectionChange")]
    public async Task<IActionResult> ConnectionChange(string? consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Connection Type / Category Change";
        ViewData["ActiveMenu"] = ActiveMenu;

        var model = await BuildConnectionChangeModelAsync(consumerNo, ct);
        if (model.Consumers.Count == 0)
        {
            TempData["ErrorMessage"] = "No active consumer connection is linked with this login/mobile number.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost("/Consumer/ServiceRequests/ConnectionChange")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConnectionChange(ConnectionChangeRequestViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Connection Type / Category Change";
        ViewData["ActiveMenu"] = ActiveMenu;
        NormalizeDeclarationFromRequest(model);
        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.MobileNo = Normalize(model.MobileNo);

        var selectedConsumer = await GetLinkedConsumerAsync(model.ConsumerNo, ct);
        if (selectedConsumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Selected consumer number is not linked with your login/mobile number.");
        if (string.IsNullOrWhiteSpace(model.NewConnectionType) && string.IsNullOrWhiteSpace(model.NewConsumerCategory))
            ModelState.AddModelError(nameof(model.NewConsumerCategory), "Select at least one new connection type/category value.");
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(model.DeclarationAccepted), "Please accept the declaration.");

        if (!ModelState.IsValid || selectedConsumer is null)
        {
            await PopulateCommonModelAsync(model, model.ConsumerNo, ct);
            PrepareConnectionOptions(model);
            return View(model);
        }

        var appId = await GenerateApplicationIdAsync(ConnectionChangeAppType, 40000000, ct);
        var division = DivisionCode(selectedConsumer.DevType);
        var detail = Encode(new Dictionary<string, string?>
        {
            ["OldConnectionType"] = selectedConsumer.ConTp,
            ["NewConnectionType"] = model.NewConnectionType,
            ["OldCategory"] = selectedConsumer.ConsCtg,
            ["NewCategory"] = model.NewConsumerCategory,
            ["TypeChangeDate"] = model.TypeChangeDate.ToString("yyyy-MM-dd"),
            ["EstimationNo"] = model.EstimationNo,
            ["EstimationAmount"] = model.EstimationAmount?.ToString("0.##"),
            ["SecurityAmount"] = model.SecurityAmount?.ToString("0.##"),
            ["MonthlyRate"] = model.MonthlyRate?.ToString("0.##"),
            ["Remarks"] = model.Remarks,
            ["AppliedBy"] = "ConsumerPortal"
        });

        var application = new MasterApplicationDetail
        {
            ApplicationId = appId,
            ConsNo = selectedConsumer.ConsNo,
            ConName = selectedConsumer.ConsNm1,
            ConAddress = selectedConsumer.ConsAddress,
            ConPhoneMobile = model.MobileNo,
            SectorVill = selectedConsumer.Sector,
            Block = selectedConsumer.BlkNo,
            PlotNo = selectedConsumer.FlatNo,
            PlotArea = selectedConsumer.PlotSize?.ToString(),
            PipeSize = selectedConsumer.PipeSize?.ToString(),
            ConnType = model.NewConnectionType,
            PropertyType = model.NewConsumerCategory,
            PrevConDetail = Truncate($"Old type: {selectedConsumer.ConTp}; Old category: {selectedConsumer.ConsCtg}", 100),
            Status = 1,
            EnterDate = DateOnly.FromDateTime(DateTime.Today),
            DivName = division,
            ApplicationStatus = "Submitted",
            ApplcationStatusDetail = detail,
            Reg = 0,
            CurrentHoldingPer = division,
            AppType = ConnectionChangeAppType
        };

        _db.MasterApplicationDetails.Add(application);
        AddHistory(appId, division, "Connection type/category change request submitted by consumer. " + model.Remarks, "1");
        await _db.SaveChangesAsync(ct);

        await _workflowService.StartWorkflowAsync(
            WorkflowService.ApplicationTypeConnectionChange,
            long.Parse(appId),
            appId,
            "Submitted",
            ResolveConsumerUserId(),
            User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
            AppConstants.Roles.Consumer,
            ct);

        TempData["SuccessMessage"] = $"Connection change application submitted successfully. Application Number: {appId}.";
        return RedirectToAction(nameof(Details), new { id = appId });
    }

    [HttpGet("/Consumer/ServiceRequests/Details/{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        ViewData["Title"] = "Service Request Details";
        ViewData["ActiveMenu"] = ActiveMenu;

        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var application = await _db.MasterApplicationDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationId == id
                && (x.AppType == NameTransferAppType || x.AppType == ConnectionChangeAppType)
                && linkedConsumerNos.Contains(x.ConsNo ?? string.Empty), ct);
        if (application is null)
            return NotFound();

        var applicationType = application.AppType == NameTransferAppType
            ? WorkflowService.ApplicationTypeNameTransfer
            : WorkflowService.ApplicationTypeConnectionChange;
        var workflow = await _db.ApplicationWorkflowInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationType == applicationType
                && x.ApplicationNo == application.ApplicationId
                && !x.IsDeleted, ct);

        List<ApplicationWorkflowHistory> workflowHistory = workflow is null
            ? []
            : await _db.ApplicationWorkflowHistories
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id)
                .OrderBy(x => x.ActionOn)
                .ToListAsync(ct);

        List<ApplicationWorkflowTask> workflowTasks = workflow is null
            ? []
            : await _db.ApplicationWorkflowTasks
                .Include(x => x.Stage)
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id && !x.IsDeleted)
                .OrderBy(x => x.AssignedOn)
                .ToListAsync(ct);

        return View(new ConsumerServiceRequestDetailsViewModel
        {
            Application = application,
            DetailValues = Decode(application.ApplcationStatusDetail),
            Histories = await _db.MasterApplicationDetailHistories
                .AsNoTracking()
                .Where(x => x.ApplicationId == id)
                .OrderByDescending(x => x.SerialNumber)
                .ToListAsync(ct),
            WorkflowHistory = workflowHistory,
            WorkflowTasks = workflowTasks
        });
    }

    private async Task<NameTransferRequestViewModel> BuildNameTransferModelAsync(string? requestedConsumerNo, CancellationToken ct)
    {
        var model = new NameTransferRequestViewModel();
        await PopulateCommonModelAsync(model, requestedConsumerNo, ct);
        if (model.SelectedConsumer is not null)
        {
            model.MobileNo = model.SelectedConsumer.MobNo ?? User.FindFirstValue("MobileNo") ?? string.Empty;
            model.NewConsumerName = model.SelectedConsumer.ConsNm1 ?? string.Empty;
            model.NewFatherName = model.SelectedConsumer.ConsNm2;
        }
        return model;
    }

    private async Task<ConnectionChangeRequestViewModel> BuildConnectionChangeModelAsync(string? requestedConsumerNo, CancellationToken ct)
    {
        var model = new ConnectionChangeRequestViewModel();
        await PopulateCommonModelAsync(model, requestedConsumerNo, ct);
        if (model.SelectedConsumer is not null)
        {
            model.MobileNo = model.SelectedConsumer.MobNo ?? User.FindFirstValue("MobileNo") ?? string.Empty;
            model.NewConnectionType = model.SelectedConsumer.ConTp;
            model.NewConsumerCategory = model.SelectedConsumer.ConsCtg;
            model.SecurityAmount = model.SelectedConsumer.Secu.HasValue ? (decimal?)Convert.ToDecimal(model.SelectedConsumer.Secu.Value) : null;
            model.MonthlyRate = model.SelectedConsumer.MonthlyRate.HasValue ? Convert.ToDecimal(model.SelectedConsumer.MonthlyRate.Value) : null;
        }
        PrepareConnectionOptions(model);
        return model;
    }

    private async Task PopulateCommonModelAsync(ConsumerServiceRequestFormViewModel model, string? requestedConsumerNo, CancellationToken ct)
    {
        var consumers = await GetLinkedConsumersAsync(ct);
        var selectedConsumerNo = NormalizeNullable(requestedConsumerNo);
        var selected = consumers.FirstOrDefault(x => x.ConsNo == selectedConsumerNo)
            ?? consumers.FirstOrDefault(x => x.ConsNo == ResolveConsumerNo())
            ?? consumers.FirstOrDefault();

        model.ConsumerNo = selected?.ConsNo ?? model.ConsumerNo;
        model.MobileNo = selected?.MobNo ?? User.FindFirstValue("MobileNo") ?? model.MobileNo;
        model.Consumers = consumers.Select(x => new SelectListItem(
            $"{x.ConsNo} - {BuildConsumerName(x)} - {BuildProperty(x)}",
            x.ConsNo,
            string.Equals(x.ConsNo, selected?.ConsNo, StringComparison.OrdinalIgnoreCase))).ToList();
        model.SelectedConsumer = selected;
    }

    private void PrepareConnectionOptions(ConnectionChangeRequestViewModel model)
    {
        model.ConnectionTypeOptions = ConnectionTypeOptions(model.NewConnectionType);
        model.ConsumerCategoryOptions = ConsumerCategoryOptions(model.NewConsumerCategory);
    }

    private async Task<List<ConsumerDetailsMaster>> GetLinkedConsumersAsync(CancellationToken ct)
    {
        var primaryConsumerNo = ResolveConsumerNo();
        var mobileNo = NormalizeNullable(User.FindFirstValue("MobileNo"));
        var query = _db.ConsumerDetailsMasters.AsNoTracking().Where(x => x.Status == null || x.Status == 1);

        query = string.IsNullOrWhiteSpace(mobileNo)
            ? query.Where(x => x.ConsNo == primaryConsumerNo)
            : query.Where(x => x.ConsNo == primaryConsumerNo || x.MobNo == mobileNo);

        return await query
            .OrderByDescending(x => x.ConsNo == primaryConsumerNo)
            .ThenBy(x => x.ConsNo)
            .ToListAsync(ct);
    }

    private async Task<List<string>> GetLinkedConsumerNosAsync(CancellationToken ct)
        => (await GetLinkedConsumersAsync(ct)).Select(x => x.ConsNo).ToList();

    private async Task<ConsumerDetailsMaster?> GetLinkedConsumerAsync(string consumerNo, CancellationToken ct)
        => (await GetLinkedConsumersAsync(ct)).FirstOrDefault(x => x.ConsNo == consumerNo);

    private async Task<string> GenerateApplicationIdAsync(string appType, int seed, CancellationToken ct)
    {
        var ids = await _db.MasterApplicationDetails
            .AsNoTracking()
            .Where(x => x.AppType == appType)
            .Select(x => x.ApplicationId)
            .ToListAsync(ct);
        var next = ids.Select(x => int.TryParse(x, out var id) ? id : seed).DefaultIfEmpty(seed).Max() + 1;
        return next.ToString();
    }

    private void AddHistory(string appId, string? division, string remark, string status)
    {
        var next = (_db.MasterApplicationDetailHistories
            .Where(x => x.ApplicationId == appId)
            .Select(x => x.SerialNumber ?? 0)
            .DefaultIfEmpty()
            .Max()) + 1;

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

    private void NormalizeDeclarationFromRequest(ConsumerServiceRequestFormViewModel model)
    {
        if (!Request.HasFormContentType)
            return;
        if (!Request.Form.TryGetValue(nameof(ConsumerServiceRequestFormViewModel.DeclarationAccepted), out var values))
            return;

        if (values.Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) || value == "1"))
        {
            model.DeclarationAccepted = true;
            ModelState.Remove(nameof(ConsumerServiceRequestFormViewModel.DeclarationAccepted));
        }
    }

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private string ResolveConsumerNo()
        => Normalize(User.FindFirstValue("ConsumerNo") ?? string.Empty);

    private static string DivisionCode(int? devType)
        => AppConstants.Divisions.Find(devType)?.Code ?? AppConstants.Divisions.Jal1.Code;

    private static string BuildConsumerName(ConsumerDetailsMaster consumer)
        => string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

    private static string BuildProperty(ConsumerDetailsMaster consumer)
        => string.Join(" / ", new[] { consumer.Sector, $"{consumer.BlkNo}-{consumer.FlatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

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

    private static IReadOnlyList<SelectListItem> ConsumerCategoryOptions(string? selected) =>
    [
        Option("R", "Regular", selected),
        Option("T", "Temporary", selected),
        Option("S", "Staff", selected),
        Option("M", "RMC", selected),
        Option("CC", "Court Case", selected),
        Option("D", "Disconnected", selected)
    ];

    private static SelectListItem Option(string value, string text, string? selected)
        => new() { Value = value, Text = $"{text} ({value})", Selected = string.Equals(value, selected, StringComparison.OrdinalIgnoreCase) };

    private static string Encode(IDictionary<string, string?> values)
        => string.Join(";", values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={Sanitize(x.Value)}"));

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

    private static string Sanitize(string? value)
        => (value ?? string.Empty).Replace(";", ",").Replace("=", "-").Trim();

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private static string Normalize(string value)
        => value.Trim().ToUpperInvariant();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
