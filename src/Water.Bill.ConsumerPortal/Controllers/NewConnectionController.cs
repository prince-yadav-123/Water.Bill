using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.DTOs.Payments;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.Filters;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Security;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
[RequirePermission("Consumer New Connection.view")]
public class NewConnectionController : ConsumerPortalControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly INewConnectionApplicationService _service;
    private readonly INewConnectionLookupService _lookupService;
    private readonly INewConnectionFeeService _feeService;
    private readonly IConsumerPaymentService _paymentService;

    public NewConnectionController(
        IConfiguration configuration,
        INewConnectionApplicationService service,
        INewConnectionLookupService lookupService,
        INewConnectionFeeService feeService,
        IConsumerPaymentService paymentService,
        IErrorLogService errorLogService)
        : base(errorLogService)
    {
        _configuration = configuration;
        _service = service;
        _lookupService = lookupService;
        _feeService = feeService;
        _paymentService = paymentService;
    }

    [HttpGet("/Consumer/NewConnection/Apply")]
    [RequirePermission("Consumer New Connection.add")]
    public async Task<IActionResult> Apply(CancellationToken ct)
    {
        ViewData["Title"] = "New Connection";
        ViewData["ActiveMenu"] = "New Connection";
        ViewData["FormAction"] = nameof(Apply);
        var model = new NewConnectionApplicationFormDto
        {
            MobileNumber = User.FindFirstValue("MobileNo"),
            EmailId = User.FindFirstValue(ClaimTypes.Email)
        };
        await LoadLookupDataAsync(model, ct);
        return View(model);
    }

    [HttpPost("/Consumer/NewConnection/Apply")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer New Connection.add")]
    public async Task<IActionResult> Apply(NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        ViewData["Title"] = "New Connection";
        ViewData["ActiveMenu"] = "New Connection";
        ViewData["FormAction"] = nameof(Apply);
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);

        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View(model);

        ValidateRequiredDocuments(Request.Form.Files, await GetRequiredDocumentTypeNamesAsync(ct));
        if (!ModelState.IsValid)
            return View(model);

        var consumerNo = ResolveConsumerNo();
        var consumerUserId = ResolveConsumerUserId();
        var applicationNo = GenerateApplicationNo();

        try
        {
            var savedDocuments = await SaveDocumentsAsync(Request.Form.Files, applicationNo, ct);
            var result = await _service.SubmitAsync(new NewConnectionSubmitRequest
            {
                Form = model,
                Documents = savedDocuments,
                ApplicationNo = applicationNo,
                IsPublicApplication = false,
                SubmittedByConsumerNo = consumerNo,
                SubmittedByConsumerUserId = consumerUserId,
                ActionBy = consumerUserId,
                ActionByName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
                ActionByRole = AppConstants.Roles.Consumer,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                TargetStatus = "PendingPayment",
                StatusAction = "FeeCalculated",
                StatusRemarks = "Application fee calculated and payment is pending.",
                FeeQuote = await ResolveFeeAsync(model, ct),
                StartWorkflow = false
            }, ct);

            return RedirectToAction(nameof(Payment), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("/Consumer/NewConnection/Lookups/Blocks")]
    public async Task<IActionResult> Blocks(string sectorId, CancellationToken ct)
        => Json((await _lookupService.GetSectorContextAsync(sectorId, ct)).Blocks);

    [HttpGet("/Consumer/NewConnection/Lookups/SectorContext")]
    public async Task<IActionResult> SectorContext(string sectorId, CancellationToken ct)
        => Json(await _lookupService.GetSectorContextAsync(sectorId, ct));

    [HttpGet("/Consumer/NewConnection/Lookups/ConnectionSubTypes")]
    public async Task<IActionResult> ConnectionSubTypes(string connectionCategoryId, int? devType, CancellationToken ct)
        => Json(await _lookupService.GetConnectionSubTypesAsync(connectionCategoryId, devType, ct));

    [HttpGet("/Consumer/NewConnection/FeePreview")]
    public async Task<IActionResult> FeePreview([FromQuery] NewConnectionFeeRequestDto request, CancellationToken ct)
    {
        var fee = await _feeService.GetFeeAsync(request, ct);
        return fee is null ? NotFound() : Json(fee);
    }

    [HttpGet("/Consumer/NewConnection/MyApplications")]
    public async Task<IActionResult> MyApplications(CancellationToken ct)
    {
        ViewData["Title"] = "My Applications";
        ViewData["ActiveMenu"] = "My Applications";
        var applications = await _service.GetConsumerApplicationsAsync(ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        return View(applications);
    }

    [HttpGet("/Consumer/NewConnection/Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Application Details";
        ViewData["ActiveMenu"] = "My Applications";
        var details = await _service.GetConsumerApplicationDetailsAsync(id, ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        if (details is null)
            return NotFound();

        return View(details);
    }

    [HttpGet("/Consumer/NewConnection/Continue/{id:long}")]
    [RequirePermission("Consumer New Connection.edit")]
    public async Task<IActionResult> Continue(long id, CancellationToken ct)
    {
        var model = await _service.GetConsumerContinuationFormAsync(id, ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        if (model is null)
            return NotFound();

        ViewData["Title"] = "Complete New Connection";
        ViewData["ActiveMenu"] = "My Applications";
        ViewData["FormAction"] = nameof(Continue);
        ViewData["FormRouteId"] = id;
        ViewData["ExistingFeeQuote"] = await _service.GetApplicationFeeAsync(id, ct);
        var existing = await _service.GetConsumerApplicationDetailsAsync(id, ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        ViewData["ExistingDocumentTypes"] = existing?.Documents.Select(x => x.DocumentType).ToArray() ?? [];
        await LoadLookupDataAsync(model, ct);
        return View("Apply", model);
    }

    [HttpPost("/Consumer/NewConnection/Continue/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer New Connection.edit")]
    public async Task<IActionResult> Continue(long id, NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        var consumerUserId = ResolveConsumerUserId();
        var existing = await _service.GetConsumerApplicationDetailsAsync(id, consumerNo, consumerUserId, ct);
        if (existing is null || !existing.CanContinue)
            return NotFound();

        ViewData["Title"] = "Complete New Connection";
        ViewData["ActiveMenu"] = "My Applications";
        ViewData["FormAction"] = nameof(Continue);
        ViewData["FormRouteId"] = id;
        ViewData["ExistingFeeQuote"] = await _service.GetApplicationFeeAsync(id, ct);
        ViewData["ExistingDocumentTypes"] = existing.Documents.Select(x => x.DocumentType).ToArray();
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);

        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View("Apply", model);

        ValidateRequiredDocuments(Request.Form.Files, await GetRequiredDocumentTypeNamesAsync(ct), existing.Documents.Select(x => x.DocumentType).ToArray());
        if (!ModelState.IsValid)
            return View("Apply", model);

        var fee = await _service.GetApplicationFeeAsync(id, ct)
            ?? await _feeService.GetFeeAsync(new NewConnectionFeeRequestDto
            {
                ConnectionCategory = model.ConnectionCategory,
                ConnectionType = model.ConnectionType,
                PipeSize = model.PipeSize,
                PlotSize = model.PlotSize
            }, ct);

        if (fee is null)
        {
            ModelState.AddModelError(string.Empty, "Fee configuration is not available for the selected connection details. Please contact support.");
            return View("Apply", model);
        }

        try
        {
            var savedDocuments = await SaveDocumentsAsync(Request.Form.Files, existing.ApplicationNo, ct);
            var result = await _service.CompleteConsumerApplicationAsync(id, consumerNo, consumerUserId, new NewConnectionSubmitRequest
            {
                Form = model,
                Documents = savedDocuments,
                ApplicationNo = existing.ApplicationNo,
                IsPublicApplication = false,
                SubmittedByConsumerNo = consumerNo,
                SubmittedByConsumerUserId = consumerUserId,
                ActionBy = consumerUserId,
                ActionByName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
                ActionByRole = AppConstants.Roles.Consumer,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                TargetStatus = "PendingPayment",
                StatusAction = "FeeCalculated",
                StatusRemarks = existing.ApplicationStatus == "Draft"
                    ? "Application reviewed and fee calculated. Payment is pending."
                    : "Application updated and fee calculated. Payment is pending.",
                FeeQuote = fee,
                StartWorkflow = false
            }, ct);

            return RedirectToAction(nameof(Payment), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Apply", model);
        }
    }

    [HttpGet("/Consumer/NewConnection/Resubmit/{id:long}")]
    [RequirePermission("Consumer New Connection.edit")]
    public async Task<IActionResult> Resubmit(long id, CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        var consumerUserId = ResolveConsumerUserId();

        var details = await _service.GetConsumerApplicationDetailsAsync(id, consumerNo, consumerUserId, ct);
        if (details is null || !details.CanResubmit)
            return NotFound();

        var model = await _service.GetConsumerResubmitFormAsync(id, consumerNo, consumerUserId, ct);
        if (model is null) return NotFound();

        ViewData["Title"] = "Correct & Resubmit Application";
        ViewData["ActiveMenu"] = "My Applications";
        ViewData["FormAction"] = nameof(Resubmit);
        ViewData["FormRouteId"] = id;
        ViewData["SentBackRemarks"] = details.SentBackRemarks;
        ViewData["SentBackAt"] = details.SentBackAt;
        ViewData["ApplicationNo"] = details.ApplicationNo;
        ViewData["IsResubmit"] = true;
        ViewData["LockMobileNumber"] = true;
        ViewData["ExistingDocumentTypes"] = details.Documents.Select(x => x.DocumentType).ToArray();
        await LoadLookupDataAsync(model, ct);
        return View("Resubmit", model);
    }

    [HttpPost("/Consumer/NewConnection/Resubmit/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer New Connection.edit")]
    public async Task<IActionResult> Resubmit(long id, NewConnectionApplicationFormDto model, string? applicantRemarks, CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        var consumerUserId = ResolveConsumerUserId();

        var details = await _service.GetConsumerApplicationDetailsAsync(id, consumerNo, consumerUserId, ct);
        if (details is null || !details.CanResubmit)
            return NotFound();

        ViewData["Title"] = "Correct & Resubmit Application";
        ViewData["ActiveMenu"] = "My Applications";
        ViewData["FormAction"] = nameof(Resubmit);
        ViewData["FormRouteId"] = id;
        ViewData["SentBackRemarks"] = details.SentBackRemarks;
        ViewData["SentBackAt"] = details.SentBackAt;
        ViewData["ApplicationNo"] = details.ApplicationNo;
        ViewData["IsResubmit"] = true;
        ViewData["LockMobileNumber"] = true;
        ViewData["ExistingDocumentTypes"] = details.Documents.Select(x => x.DocumentType).ToArray();
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);

        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);
        if (!ModelState.IsValid)
            return View("Resubmit", model);

        // Documents: existing are kept; new uploads supplement/replace
        var newDocuments = await SaveDocumentsAsync(Request.Form.Files, details.ApplicationNo, ct);

        try
        {
            var result = await _service.ResubmitApplicationAsync(
                id, consumerNo, consumerUserId, applicantRemarks, newDocuments, ct);

            TempData["SuccessMessage"] = $"Application {result.ApplicationNo} resubmitted successfully. It has been returned to the authority for review.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Resubmit", model);
        }
    }

    private string ResolveConsumerNo()
        => (User.FindFirstValue("ConsumerNo") ?? string.Empty).Trim().ToUpperInvariant();

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet("/Consumer/NewConnection/Payment/{id:long}")]
    public async Task<IActionResult> Payment(long id, int step = 1, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Pay Application Fee";
        ViewData["ActiveMenu"] = "My Applications";
        ViewData["IsDevelopmentPayment"] = _paymentService.IsDevelopmentMode();

        var model = await BuildPaymentModelAsync(id, step, paymentMethod, paymentIdentifier, ct);
        if (model is null)
            return NotFound();

        return View("~/Views/NewConnection/Payment.cshtml", model);
    }

    [HttpGet("/Consumer/NewConnection/Payment/{id:long}/Confirm")]
    public async Task<IActionResult> ConfirmPayment(long id, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
        => await Payment(id, 3, paymentMethod, paymentIdentifier, ct);

    [HttpPost("/Consumer/NewConnection/Payment/{id:long}/Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentPost(long id, string? paymentMethod, string? paymentIdentifier, CancellationToken ct)
    {
        var model = await BuildPaymentModelAsync(id, 3, paymentMethod, paymentIdentifier, ct);
        if (model is null)
            return NotFound();

        var result = await _paymentService.InitiatePaymentAsync(new PaymentInitiationRequestDto
        {
            ConsumerNo = ResolveConsumerNo(),
            ConsumerName = model.Application.ApplicantName,
            ConsumerProperty = $"{model.Application.Sector} / {model.Application.Block} / {model.Application.FlatNo}",
            MobileNo = model.Application.MobileNumber,
            Email = model.Application.EmailId,
            BillNo = model.Application.ApplicationNo,
            ChallanNo = id.ToString(),
            Amount = Convert.ToDouble(model.Fee.TotalAmount),
            GatewayCode = paymentMethod ?? "AX",
            BillOrNdc = PaymentReferenceKinds.NewConnection,
            ContextId = id.ToString(),
            ContextReferenceNo = model.Application.ApplicationNo,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        }, ct);

        if (!result.Success || string.IsNullOrWhiteSpace(result.JalReferenceId))
        {
            TempData["ErrorMessage"] = result.Message ?? "Payment reference could not be created.";
            return RedirectToAction(nameof(Payment), new { id, step = 3, paymentMethod, paymentIdentifier });
        }

        if (_paymentService.IsDevelopmentMode())
        {
            var processed = await _paymentService.ProcessDevelopmentSuccessAsync(result.JalReferenceId, BuildPaymentActorContext(), ct);
            TempData[processed.Success ? "SuccessMessage" : "ErrorMessage"] = processed.Message
                ?? (processed.Success ? "Application payment simulated successfully." : "Application payment simulation failed.");
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(PaymentStarted), new { referenceId = result.JalReferenceId });
    }

    [HttpGet("/Consumer/NewConnection/PaymentStarted/{referenceId}")]
    public async Task<IActionResult> PaymentStarted(string referenceId, CancellationToken ct)
    {
        ViewData["Title"] = "Payment Initiated";
        ViewData["ActiveMenu"] = "My Applications";

        var result = await _paymentService.GetInitiatedPaymentAsync(referenceId, ResolveConsumerNo(), ct);
        if (result is null)
        {
            TempData["ErrorMessage"] = "Payment reference was not found for this application.";
            return RedirectToAction(nameof(MyApplications));
        }

        ViewData["IsPublicFlow"] = false;
        return View("~/Views/NewConnection/PaymentStarted.cshtml", result);
    }

    private async Task<NewConnectionPaymentViewModel?> BuildPaymentModelAsync(long id, int step, string? paymentMethod, string? paymentIdentifier, CancellationToken ct)
    {
        var application = await _service.GetConsumerApplicationDetailsAsync(id, ResolveConsumerNo(), ResolveConsumerUserId(), ct);
        if (application is null || !application.CanContinue)
            return null;

        var fee = await _service.GetApplicationFeeAsync(id, ct)
            ?? await _feeService.GetFeeAsync(new NewConnectionFeeRequestDto
            {
                ConnectionCategory = application.ConnectionCategory,
                ConnectionType = application.ConnectionType,
                PipeSize = application.PipeSize,
                PlotSize = application.PlotSize
            }, ct);

        if (fee is null)
            return null;

        return new NewConnectionPaymentViewModel
        {
            Application = application,
            Fee = fee,
            Step = Math.Clamp(step, 1, 3),
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "UPI" : paymentMethod,
            PaymentIdentifier = paymentIdentifier
        };
    }

    private async Task<NewConnectionFeeQuoteDto> ResolveFeeAsync(NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        var fee = await _feeService.GetFeeAsync(new NewConnectionFeeRequestDto
        {
            ConnectionCategory = model.ConnectionCategory,
            ConnectionType = model.ConnectionType,
            PipeSize = model.PipeSize,
            PlotSize = model.PlotSize
        }, ct);

        return fee ?? throw new InvalidOperationException("Fee configuration is not available for the selected connection details. Please contact support.");
    }

    private PaymentActorContextDto BuildPaymentActorContext()
        => new()
        {
            UserId = ResolveConsumerUserId(),
            UserName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Consumer",
            UserRole = AppConstants.Roles.Consumer,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

    private async Task<IReadOnlyList<NewConnectionDocumentInputDto>> SaveDocumentsAsync(IFormFileCollection files, string applicationNo, CancellationToken ct)
    {
        var result = new List<NewConnectionDocumentInputDto>();
        var storageRoot = _configuration["FileStorage:DocumentBasePath"];
        if (string.IsNullOrWhiteSpace(storageRoot))
            throw new InvalidOperationException("Document storage path is not configured.");

        var options = FileUploadSecurityHelper.BuildOptions(_configuration);
        var uploadRoot = Path.Combine(storageRoot, applicationNo);
        Directory.CreateDirectory(uploadRoot);

        foreach (var file in files.Where(x => x.Length > 0))
        {
            if (!FileUploadSecurityHelper.TryValidate(file, options, out var errorMessage))
                throw new InvalidOperationException(errorMessage!);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeOriginalName = MakeSafeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var safeName = $"{safeOriginalName}-{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadRoot, safeName);
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream, ct);

            result.Add(new NewConnectionDocumentInputDto
            {
                DocumentType = ResolveDocumentType(file.Name),
                FileName = Path.GetFileName(file.FileName),
                FilePath = $"{applicationNo}/{safeName}",
                ContentType = FileUploadSecurityHelper.ResolveSafeContentType(file.FileName),
                FileSize = file.Length
            });
        }

        return result;
    }

    private static string ResolveDocumentType(string input)
    {
        var normalized = input.Replace("Documents_", string.Empty).Replace("_", " ").Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Other" : normalized;
    }

    private void ValidateRequiredDocuments(IFormFileCollection files, IReadOnlyCollection<string> requiredDocumentTypes, IReadOnlyCollection<string>? existingDocumentTypes = null)
    {
        var uploadedTypes = files
            .Where(x => x.Length > 0)
            .Select(x => ResolveDocumentType(x.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (existingDocumentTypes is not null)
        {
            foreach (var existingDocumentType in existingDocumentTypes)
                uploadedTypes.Add(existingDocumentType);
        }

        foreach (var requiredType in requiredDocumentTypes)
        {
            if (!uploadedTypes.Contains(requiredType))
                ModelState.AddModelError(string.Empty, $"{requiredType} is required.");
        }
    }

    private async Task LoadLookupDataAsync(NewConnectionApplicationFormDto? model, CancellationToken ct)
    {
        var lookups = await _lookupService.GetLookupDataAsync(ct: ct);
        lookups.ConnectionCategories = [];
        lookups.PipeSizes = [];
        lookups.ConnectionSubTypes = [];
        lookups.Villages = [];

        if (!string.IsNullOrWhiteSpace(model?.Sector))
        {
            var sectorContext = await _lookupService.GetSectorContextAsync(model.Sector, ct);
            lookups.ConnectionCategories = sectorContext.ConnectionCategories;
            lookups.PipeSizes = sectorContext.PipeSizes;
            lookups.Villages = sectorContext.Villages;

            if (!string.IsNullOrWhiteSpace(model.ConnectionCategory))
                lookups.ConnectionSubTypes = await _lookupService.GetConnectionSubTypesAsync(model.ConnectionCategory, sectorContext.DevType, ct);

            ViewData["DivisionDisplay"] = sectorContext.DivisionDisplay;
        }

        ViewData["LookupData"] = lookups;
        ViewData["DocumentTypeOptions"] = lookups.DocumentTypes.ToArray();
        ViewData["DocumentTypes"] = lookups.DocumentTypes.Select(x => x.Text).ToArray();
        ViewData["BlocksUrl"] = Url.Action(nameof(Blocks), "NewConnection");
        ViewData["SectorContextUrl"] = Url.Action(nameof(SectorContext), "NewConnection");
        ViewData["ConnectionSubTypesUrl"] = Url.Action(nameof(ConnectionSubTypes), "NewConnection");
        ViewData["FeePreviewUrl"] = Url.Action(nameof(FeePreview), "NewConnection");
    }

    private async Task<IReadOnlyList<string>> GetRequiredDocumentTypeNamesAsync(CancellationToken ct)
    {
        var documentTypes = await _lookupService.GetDocumentTypesAsync(ct);
        return documentTypes.Where(x => x.IsRequired).Select(x => x.Text).ToArray();
    }

    private async Task PopulateSectorDevTypeAsync(NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Sector))
        {
            model.DevType = null;
            return;
        }

        model.DevType = await _lookupService.GetSectorDevTypeAsync(model.Sector, ct);
        if (!model.DevType.HasValue)
            ModelState.AddModelError(nameof(model.Sector), "Division could not be determined for the selected Sector.");
    }

    private void NormalizeDeclarationFromRequest(NewConnectionApplicationFormDto model)
    {
        if (!Request.HasFormContentType)
            return;

        if (!Request.Form.TryGetValue(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), out var values))
            return;

        var accepted = values.Any(value =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase)
            || value == "1");

        if (!accepted)
            return;

        model.DeclarationAccepted = true;
        ModelState.Remove(nameof(NewConnectionApplicationFormDto.DeclarationAccepted));
    }

    private void ValidateDeclaration(NewConnectionApplicationFormDto model)
    {
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), "Please accept the declaration.");
    }

    private static string GenerateApplicationNo() => $"NC{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

    private static string MakeSafeFileName(string fileName)
    {
        var safe = new string(fileName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch).ToArray()).Trim('-', ' ');
        return string.IsNullOrWhiteSpace(safe) ? "document" : safe;
    }
}
