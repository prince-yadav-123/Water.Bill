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
public class NdcController : Controller
{
    private const string ActiveMenu = "NDC / No Dues";

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWorkflowService _workflowService;

    public NdcController(ApplicationDbContext db, IConfiguration configuration, IWorkflowService workflowService)
    {
        _db = db;
        _configuration = configuration;
        _workflowService = workflowService;
    }

    [HttpGet("/Consumer/Ndc")]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken ct)
    {
        ViewData["Title"] = "NDC / No Dues";
        ViewData["ActiveMenu"] = ActiveMenu;

        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var mobileNo = NormalizeNullable(User.FindFirstValue("MobileNo"));

        var query = _db.ConsumerApplyNdcs
            .AsNoTracking()
            .Where(x => linkedConsumerNos.Contains(x.ConsumerNo ?? string.Empty)
                || (!string.IsNullOrWhiteSpace(mobileNo) && x.MobileNo == mobileNo));

        search = NormalizeNullable(search);
        status = NormalizeNullable(status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.ApplicationNo.Contains(search)
                || (x.ConsumerNo != null && x.ConsumerNo.Contains(search))
                || (x.ConsName != null && x.ConsName.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status || x.CurrentStatus == status || x.FinalStatus == status);

        var applications = await query
            .OrderByDescending(x => x.CreatedOn)
            .ThenByDescending(x => x.AutoId)
            .Take(100)
            .ToListAsync(ct);

        return View(new ConsumerNdcIndexViewModel
        {
            Search = search,
            Status = status,
            Applications = applications
        });
    }

    [HttpGet("/Consumer/Ndc/Apply")]
    public async Task<IActionResult> Apply(string? consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Apply NDC";
        ViewData["ActiveMenu"] = ActiveMenu;

        var model = await BuildApplyModelAsync(consumerNo, ct);
        if (model.Consumers.Count == 0)
        {
            TempData["ErrorMessage"] = "No active consumer connection is linked with this login/mobile number.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost("/Consumer/Ndc/Apply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ConsumerNdcApplyViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Apply NDC";
        ViewData["ActiveMenu"] = ActiveMenu;

        model.ConsumerNo = Normalize(model.ConsumerNo);
        model.MobileNo = Normalize(model.MobileNo);
        model.Email = NormalizeNullable(model.Email);
        NormalizeDeclarationFromRequest(model);

        var selectedConsumer = await GetLinkedConsumerAsync(model.ConsumerNo, ct);
        if (selectedConsumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Selected consumer number is not linked with your login/mobile number.");
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(model.DeclarationAccepted), "Please accept the declaration.");

        var documents = await GetNdcDocumentMastersAsync(ct);
        model.Consumers = await BuildConsumerSelectListAsync(model.ConsumerNo, ct);
        model.SelectedConsumer = selectedConsumer is null ? null : MapConsumerDetails(selectedConsumer);
        model.Documents = MergeDocumentInputs(model.Documents, documents);

        ValidateNdcDocuments(model.Documents, Request.Form.Files, documents);
        if (!ModelState.IsValid)
            return View(model);

        var now = DateTime.Now;
        var applicationNo = GenerateApplicationNo();
        var savedFiles = await SaveNdcDocumentsAsync(applicationNo, model.Documents, Request.Form.Files, ct);
        var firstThreePaths = savedFiles.Select(x => x.RelativePath).Take(3).ToArray();
        var division = AppConstants.Divisions.Find(selectedConsumer!.DevType);

        var application = new ConsumerApplyNdc
        {
            ApplicationNo = applicationNo,
            ConsumerNo = selectedConsumer.ConsNo,
            Rid = selectedConsumer.Rid?.ToString(),
            ConsName = BuildConsumerName(selectedConsumer),
            MobileNo = model.MobileNo,
            Email = model.Email ?? selectedConsumer.EmailId,
            DivisionTypeName = division?.Code ?? selectedConsumer.DevType?.ToString(),
            DivisionType = selectedConsumer.DevType,
            Sector = selectedConsumer.Sector,
            Block = selectedConsumer.BlkNo,
            PlotNo = selectedConsumer.FlatNo,
            PlotArea = selectedConsumer.PlotSize?.ToString(),
            PipeSize = selectedConsumer.PipeSize?.ToString(),
            ConsTp = selectedConsumer.ConTp,
            Type = "WEB",
            Status = "Submitted",
            CurrentStatus = "Submitted",
            SuccessStatus = "S",
            CreatedBy = ResolveConsumerUserId(),
            CreatedOn = now,
            LastUpdatedBy = ResolveConsumerUserId(),
            LastUpdatedOn = now,
            Attachment1 = firstThreePaths.ElementAtOrDefault(0),
            Attachment2 = firstThreePaths.ElementAtOrDefault(1),
            Attachment3 = firstThreePaths.ElementAtOrDefault(2),
            BillNo = applicationNo
        };

        _db.ConsumerApplyNdcs.Add(application);
        await _db.SaveChangesAsync(ct);

        foreach (var file in savedFiles)
        {
            _db.NdcDocuments.Add(new NdcDocument
            {
                ConsumerNo = selectedConsumer.ConsNo,
                NdcAutoId = application.AutoId,
                AttachmentName = file.DisplayName,
                AttachmentPath = file.RelativePath,
                Status = true,
                CreatedOn = now
            });
        }

        await _db.SaveChangesAsync(ct);

        await _workflowService.StartWorkflowAsync(
            WorkflowService.ApplicationTypeNdc,
            application.AutoId,
            application.ApplicationNo,
            "Submitted",
            ResolveConsumerUserId(),
            User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
            AppConstants.Roles.Consumer,
            ct);

        TempData["SuccessMessage"] = $"NDC application submitted successfully. Application Number: {application.ApplicationNo}.";
        return RedirectToAction(nameof(Details), new { id = application.AutoId });
    }

    [HttpGet("/Consumer/Ndc/Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "NDC Details";
        ViewData["ActiveMenu"] = ActiveMenu;

        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var mobileNo = NormalizeNullable(User.FindFirstValue("MobileNo"));
        var application = await _db.ConsumerApplyNdcs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AutoId == id
                && (linkedConsumerNos.Contains(x.ConsumerNo ?? string.Empty)
                    || (!string.IsNullOrWhiteSpace(mobileNo) && x.MobileNo == mobileNo)), ct);
        if (application is null)
            return NotFound();

        var workflow = await _db.ApplicationWorkflowInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationType == WorkflowService.ApplicationTypeNdc
                && x.ApplicationId == application.AutoId
                && !x.IsDeleted, ct);

        List<ApplicationWorkflowHistory> history = workflow is null
            ? []
            : await _db.ApplicationWorkflowHistories
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id)
                .OrderBy(x => x.ActionOn)
                .ToListAsync(ct);

        List<ApplicationWorkflowTask> tasks = workflow is null
            ? []
            : await _db.ApplicationWorkflowTasks
                .Include(x => x.Stage)
                .AsNoTracking()
                .Where(x => x.WorkflowInstanceId == workflow.Id && !x.IsDeleted)
                .OrderBy(x => x.AssignedOn)
                .ToListAsync(ct);

        var documents = await _db.NdcDocuments
            .AsNoTracking()
            .Where(x => x.NdcAutoId == application.AutoId && x.Status != false)
            .OrderBy(x => x.AutoId)
            .ToListAsync(ct);

        return View(new ConsumerNdcDetailsViewModel
        {
            Application = application,
            Documents = documents,
            WorkflowHistory = history,
            WorkflowTasks = tasks
        });
    }

    [HttpGet("/Consumer/Ndc/Certificate/{id:int}")]
    public async Task<IActionResult> Certificate(int id, CancellationToken ct)
    {
        var application = await GetOwnedNdcApplicationAsync(id, ct);
        if (application is null)
            return NotFound();

        if (!IsCertificatePrintable(application))
        {
            TempData["ErrorMessage"] = "NDC certificate is available only after final approval.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("Certificate", application);
    }

    [HttpGet("/Consumer/Ndc/Document/{id:int}")]
    public async Task<IActionResult> Document(int id, CancellationToken ct)
    {
        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var document = await _db.NdcDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AutoId == id
                && x.Status != false
                && linkedConsumerNos.Contains(x.ConsumerNo ?? string.Empty), ct);

        if (document is null || string.IsNullOrWhiteSpace(document.AttachmentPath))
            return NotFound();

        var fullPath = Path.Combine(GetNdcStorageBasePath(), document.AttachmentPath.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, ResolveContentType(document.AttachmentPath), document.AttachmentName);
    }

    private async Task<ConsumerNdcApplyViewModel> BuildApplyModelAsync(string? requestedConsumerNo, CancellationToken ct)
    {
        var consumers = await GetLinkedConsumersAsync(ct);
        var normalizedRequestedConsumerNo = NormalizeNullable(requestedConsumerNo);
        var selected = consumers.FirstOrDefault(x => x.ConsNo == normalizedRequestedConsumerNo)
            ?? consumers.FirstOrDefault(x => x.ConsNo == ResolveConsumerNo())
            ?? consumers.FirstOrDefault();

        var documents = await GetNdcDocumentMastersAsync(ct);

        return new ConsumerNdcApplyViewModel
        {
            ConsumerNo = selected?.ConsNo ?? string.Empty,
            MobileNo = selected?.MobNo ?? User.FindFirstValue("MobileNo") ?? string.Empty,
            Email = selected?.EmailId ?? User.FindFirstValue(ClaimTypes.Email),
            Consumers = BuildConsumerSelectList(consumers, selected?.ConsNo),
            SelectedConsumer = selected is null ? null : MapConsumerDetails(selected),
            Documents = documents.Select(x => new ConsumerNdcDocumentInputViewModel
            {
                DocumentId = x.DocumentId,
                DocumentName = x.DocumentName ?? $"Document {x.DocumentId}"
            }).ToList()
        };
    }

    private async Task<List<ConsumerDetailsMaster>> GetLinkedConsumersAsync(CancellationToken ct)
    {
        var primaryConsumerNo = ResolveConsumerNo();
        var mobileNo = NormalizeNullable(User.FindFirstValue("MobileNo"));

        var query = _db.ConsumerDetailsMasters.AsNoTracking().Where(x => x.Status == null || x.Status == 1);
        if (string.IsNullOrWhiteSpace(mobileNo))
            query = query.Where(x => x.ConsNo == primaryConsumerNo);
        else
            query = query.Where(x => x.ConsNo == primaryConsumerNo || x.MobNo == mobileNo);

        return await query
            .OrderByDescending(x => x.ConsNo == primaryConsumerNo)
            .ThenBy(x => x.ConsNo)
            .ToListAsync(ct);
    }

    private async Task<ConsumerApplyNdc?> GetOwnedNdcApplicationAsync(int id, CancellationToken ct)
    {
        var linkedConsumerNos = await GetLinkedConsumerNosAsync(ct);
        var mobileNo = NormalizeNullable(User.FindFirstValue("MobileNo"));
        return await _db.ConsumerApplyNdcs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AutoId == id
                && (linkedConsumerNos.Contains(x.ConsumerNo ?? string.Empty)
                    || (!string.IsNullOrWhiteSpace(mobileNo) && x.MobileNo == mobileNo)), ct);
    }

    private static bool IsCertificatePrintable(ConsumerApplyNdc application)
        => string.Equals(application.FinalStatus, "A", StringComparison.OrdinalIgnoreCase)
            || string.Equals(application.CurrentStatus, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(application.CurrentStatus, "FinalApproved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(application.Status, "A", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(application.CertificateUrl);

    private async Task<List<string>> GetLinkedConsumerNosAsync(CancellationToken ct)
        => (await GetLinkedConsumersAsync(ct)).Select(x => x.ConsNo).ToList();

    private async Task<ConsumerDetailsMaster?> GetLinkedConsumerAsync(string consumerNo, CancellationToken ct)
        => (await GetLinkedConsumersAsync(ct)).FirstOrDefault(x => x.ConsNo == consumerNo);

    private async Task<List<SelectListItem>> BuildConsumerSelectListAsync(string? selectedConsumerNo, CancellationToken ct)
        => BuildConsumerSelectList(await GetLinkedConsumersAsync(ct), selectedConsumerNo);

    private static List<SelectListItem> BuildConsumerSelectList(IEnumerable<ConsumerDetailsMaster> consumers, string? selectedConsumerNo)
        => consumers.Select(x => new SelectListItem(
            $"{x.ConsNo} - {BuildProperty(x)}",
            x.ConsNo,
            string.Equals(x.ConsNo, selectedConsumerNo, StringComparison.OrdinalIgnoreCase))).ToList();

    private async Task<List<MasterDocumentUpload>> GetNdcDocumentMastersAsync(CancellationToken ct)
        => await _db.MasterDocumentUploads
            .AsNoTracking()
            .Where(x => x.DocFor == "ND" && (x.Status == null || x.Status == 1))
            .OrderBy(x => x.DocumentId)
            .ToListAsync(ct);

    private static List<ConsumerNdcDocumentInputViewModel> MergeDocumentInputs(
        IEnumerable<ConsumerNdcDocumentInputViewModel> submitted,
        IReadOnlyList<MasterDocumentUpload> masters)
    {
        var submittedLookup = submitted.ToDictionary(x => x.DocumentId);
        return masters.Select(master =>
        {
            submittedLookup.TryGetValue(master.DocumentId, out var input);
            return new ConsumerNdcDocumentInputViewModel
            {
                DocumentId = master.DocumentId,
                DocumentName = master.DocumentName ?? $"Document {master.DocumentId}",
                DocumentNo = input?.DocumentNo,
                DocumentDate = input?.DocumentDate
            };
        }).ToList();
    }

    private void ValidateNdcDocuments(
        IReadOnlyList<ConsumerNdcDocumentInputViewModel> inputs,
        IFormFileCollection files,
        IReadOnlyList<MasterDocumentUpload> masters)
    {
        foreach (var master in masters)
        {
            var input = inputs.FirstOrDefault(x => x.DocumentId == master.DocumentId);
            var file = files.GetFile(GetFileInputName(master.DocumentId));
            var displayName = master.DocumentName ?? $"Document {master.DocumentId}";

            if (input is null || string.IsNullOrWhiteSpace(input.DocumentNo))
                ModelState.AddModelError(string.Empty, $"{displayName} number is required.");
            if (input?.DocumentDate is null)
                ModelState.AddModelError(string.Empty, $"{displayName} date is required.");
            if (file is null || file.Length == 0)
                ModelState.AddModelError(string.Empty, $"{displayName} upload is required.");
        }

        ValidateDocumentFiles(files);
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

    private async Task<List<SavedNdcDocument>> SaveNdcDocumentsAsync(
        string applicationNo,
        IReadOnlyList<ConsumerNdcDocumentInputViewModel> inputs,
        IFormFileCollection files,
        CancellationToken ct)
    {
        var result = new List<SavedNdcDocument>();
        var basePath = GetNdcStorageBasePath();
        var relativeDirectory = Path.Combine("ndc", applicationNo);
        var directory = Path.Combine(basePath, relativeDirectory);
        Directory.CreateDirectory(directory);

        foreach (var input in inputs)
        {
            var file = files.GetFile(GetFileInputName(input.DocumentId));
            if (file is null || file.Length == 0)
                continue;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeFileName = $"{MakeSafeFileName(input.DocumentName)}-{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(directory, safeFileName);
            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream, ct);

            var dateText = input.DocumentDate?.ToString("dd-MMM-yyyy") ?? "-";
            var displayName = $"{input.DocumentName} | No: {input.DocumentNo} | Date: {dateText}";
            result.Add(new SavedNdcDocument(displayName, Path.Combine(relativeDirectory, safeFileName).Replace('\\', '/')));
        }

        return result;
    }

    private string GetNdcStorageBasePath()
        => _configuration["FileStorage:NdcDocumentBasePath"]
            ?? Path.Combine(_configuration["FileStorage:DocumentBasePath"] ?? "C:\\WaterBillUploads", "NdcDocuments");

    private static string GetFileInputName(int documentId) => $"DocumentFile_{documentId}";

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private void NormalizeDeclarationFromRequest(ConsumerNdcApplyViewModel model)
    {
        if (!Request.HasFormContentType)
            return;

        if (!Request.Form.TryGetValue(nameof(ConsumerNdcApplyViewModel.DeclarationAccepted), out var values))
            return;

        var accepted = values.Any(value =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase)
            || value == "1");

        if (!accepted)
            return;

        model.DeclarationAccepted = true;
        ModelState.Remove(nameof(ConsumerNdcApplyViewModel.DeclarationAccepted));
    }

    private string ResolveConsumerNo()
        => Normalize(User.FindFirstValue("ConsumerNo") ?? string.Empty);

    private static ConsumerNdcConsumerDetailsViewModel MapConsumerDetails(ConsumerDetailsMaster consumer)
    {
        var division = AppConstants.Divisions.Find(consumer.DevType);
        return new ConsumerNdcConsumerDetailsViewModel
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = BuildConsumerName(consumer),
            FatherName = consumer.ConsNm2,
            ConnectionCategory = ResolveConnectionCategory(consumer.ConsCtg),
            ConnectionType = ResolveConnectionType(consumer.ConTp),
            FlatType = consumer.FlatType,
            Address = BuildAddress(consumer),
            Sector = consumer.Sector,
            Block = consumer.BlkNo,
            PlotNo = consumer.FlatNo,
            PlotSize = consumer.PlotSize?.ToString(),
            PipeSize = consumer.PipeSize?.ToString(),
            RegistrationNo = consumer.RegNo,
            EstimationNo = consumer.EstiNo,
            EstimationDate = consumer.EstiDt,
            ConnectionDate = consumer.ConnDt,
            DivisionType = consumer.DevType,
            DivisionName = division?.DisplayText ?? consumer.DevType?.ToString(),
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            Rid = consumer.Rid?.ToString()
        };
    }

    private static string BuildConsumerName(ConsumerDetailsMaster consumer)
        => string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

    private static string BuildProperty(ConsumerDetailsMaster consumer)
        => string.Join(" / ", new[] { consumer.Sector, $"{consumer.BlkNo}-{consumer.FlatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string BuildAddress(ConsumerDetailsMaster consumer)
        => !string.IsNullOrWhiteSpace(consumer.ConsAddress)
            ? consumer.ConsAddress
            : BuildProperty(consumer);

    private static string ResolveConnectionCategory(string? value)
        => value switch
        {
            "R" => "Regular",
            "T" => "Temporary",
            "S" => "Staff",
            "M" => "RMC",
            "CC" => "Court Case",
            null or "" => "-",
            _ => value
        };

    private static string ResolveConnectionType(string? value)
        => value switch
        {
            "R" => "Residential",
            "C" => "Commercial",
            "I" => "Institutional",
            "T" => "Industrial",
            "S" => "Staff",
            "V" => "Village",
            "H" => "Housing",
            "G" => "Group Housing",
            null or "" => "-",
            _ => value
        };

    private static string ResolveContentType(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

    private static string GenerateApplicationNo() => $"NDC{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

    private static string MakeSafeFileName(string value)
    {
        var safe = new string(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch).ToArray()).Trim('-', ' ');
        return string.IsNullOrWhiteSpace(safe) ? "document" : safe;
    }

    private static string Normalize(string value)
        => value.Trim().ToUpperInvariant();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record SavedNdcDocument(string DisplayName, string RelativePath);
}
