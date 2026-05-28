using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;

namespace Water.Bill.ConsumerPortal.Controllers;

public class PublicNewConnectionController : Controller
{
    private const string SessionMobileKey = "PublicNewConnection.VerifiedMobile";
    private const string SessionVerifiedAtKey = "PublicNewConnection.VerifiedAt";
    private static readonly string[] RequiredDocumentTypes = ["ID Proof", "Address Proof", "Property Document"];

    private readonly IConfiguration _configuration;
    private readonly IPublicNewConnectionOtpService _otpService;
    private readonly INewConnectionApplicationService _applicationService;
    private readonly INewConnectionLookupService _lookupService;
    private readonly INewConnectionFeeService _feeService;

    public PublicNewConnectionController(
        IConfiguration configuration,
        IPublicNewConnectionOtpService otpService,
        INewConnectionApplicationService applicationService,
        INewConnectionLookupService lookupService,
        INewConnectionFeeService feeService)
    {
        _configuration = configuration;
        _otpService = otpService;
        _applicationService = applicationService;
        _lookupService = lookupService;
        _feeService = feeService;
    }

    [HttpGet("/NewConnection/Start")]
    public IActionResult Start()
    {
        ViewData["Title"] = "Verify Mobile";
        return View();
    }

    [HttpPost("/NewConnection/Start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(string mobileNumber, CancellationToken ct)
    {
        ViewData["Title"] = "Verify Mobile";
        try
        {
            var result = await _otpService.RequestOtpAsync(mobileNumber, ct);
            TempData["OtpMessage"] = $"OTP sent to {result.MaskedMobileNumber}.";
            if (!string.IsNullOrWhiteSpace(result.DevelopmentOtp))
                TempData["DevelopmentOtp"] = result.DevelopmentOtp;

            return RedirectToAction(nameof(VerifyOtp), new { mobileNumber = result.MobileNumber });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(mobileNumber), ex.Message);
            return View((object?)mobileNumber);
        }
    }

    [HttpGet("/NewConnection/VerifyOtp")]
    public IActionResult VerifyOtp(string mobileNumber)
    {
        ViewData["Title"] = "Verify OTP";
        return View((object?)mobileNumber);
    }

    [HttpPost("/NewConnection/VerifyOtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(string mobileNumber, string otp, CancellationToken ct)
    {
        ViewData["Title"] = "Verify OTP";
        try
        {
            var result = await _otpService.VerifyOtpAsync(mobileNumber, otp, ct);
            HttpContext.Session.SetString(SessionMobileKey, result.MobileNumber);
            HttpContext.Session.SetString(SessionVerifiedAtKey, result.VerifiedAt.ToString("O"));
            var applications = await _applicationService.GetPublicApplicationsByMobileAsync(result.MobileNumber, ct);
            if (applications.Count == 0)
                return RedirectToAction(nameof(Apply));

            return RedirectToAction(nameof(MyApplications));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View((object?)mobileNumber);
        }
    }

    [HttpGet("/NewConnection/MyApplications")]
    public async Task<IActionResult> MyApplications(CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        ViewData["Title"] = "My Applications";
        ViewData["IsPublicNewConnection"] = true;
        var applications = await _applicationService.GetPublicApplicationsByMobileAsync(mobile, ct);
        return View("~/Views/NewConnection/MyApplications.cshtml", applications);
    }

    [HttpGet("/NewConnection/Apply")]
    public async Task<IActionResult> Apply(CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        ViewData["Title"] = "Apply for New Connection";
        ViewData["FormAction"] = nameof(Apply);
        await LoadLookupDataAsync(ct);
        return View("~/Views/NewConnection/Apply.cshtml", new NewConnectionApplicationFormDto
        {
            MobileNumber = mobile
        });
    }

    [HttpPost("/NewConnection/Apply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        ViewData["Title"] = "Apply for New Connection";
        ViewData["FormAction"] = nameof(Apply);
        await LoadLookupDataAsync(ct);
        model.MobileNumber = mobile;
        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        ValidateRequiredDocuments(Request.Form.Files, await GetDocumentTypeNamesAsync(ct));
        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        var fee = await _feeService.GetFeeAsync(new NewConnectionFeeRequestDto
        {
            ConnectionCategory = model.ConnectionCategory,
            ConnectionType = model.ConnectionType,
            PipeSize = model.PipeSize,
            PlotSize = model.PlotSize
        }, ct);

        if (fee is null)
        {
            ModelState.AddModelError(string.Empty, "Fee configuration is not available for the selected connection details. Please contact support.");
            return View("~/Views/NewConnection/Apply.cshtml", model);
        }

        var applicationNo = GenerateApplicationNo();
        try
        {
            var savedDocuments = await SaveDocumentsAsync(Request.Form.Files, applicationNo, ct);
            var result = await _applicationService.SubmitAsync(new NewConnectionSubmitRequest
            {
                Form = model,
                Documents = savedDocuments,
                ApplicationNo = applicationNo,
                IsPublicApplication = true,
                ActionByName = mobile,
                ActionByRole = "PublicApplicant",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                TargetStatus = "PendingPayment",
                StatusAction = "FeeCalculated",
                StatusRemarks = "Application fee calculated and payment is pending.",
                FeeQuote = fee,
                StartWorkflow = false
            }, ct);

            return RedirectToAction(nameof(Payment), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/NewConnection/Apply.cshtml", model);
        }
    }

    [HttpGet("/NewConnection/Continue/{id:long}")]
    public async Task<IActionResult> Continue(long id, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var model = await _applicationService.GetPublicContinuationFormAsync(id, mobile, ct);
        if (model is null)
            return NotFound();

        var existing = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        var existingFee = await _applicationService.GetApplicationFeeAsync(id, ct);
        ViewData["Title"] = "Complete New Connection";
        ViewData["FormAction"] = nameof(Continue);
        ViewData["FormRouteId"] = id;
        ViewData["ExistingFeeQuote"] = existingFee;
        ViewData["ExistingDocumentTypes"] = existing?.Documents.Select(x => x.DocumentType).ToArray() ?? [];
        await LoadLookupDataAsync(ct);
        model.MobileNumber = mobile;
        return View("~/Views/NewConnection/Apply.cshtml", model);
    }

    [HttpPost("/NewConnection/Continue/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Continue(long id, NewConnectionApplicationFormDto model, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var existing = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        if (existing is null || !existing.CanContinue)
            return NotFound();

        ViewData["Title"] = "Complete New Connection";
        ViewData["FormAction"] = nameof(Continue);
        ViewData["FormRouteId"] = id;
        ViewData["ExistingFeeQuote"] = await _applicationService.GetApplicationFeeAsync(id, ct);
        ViewData["ExistingDocumentTypes"] = existing.Documents.Select(x => x.DocumentType).ToArray();
        await LoadLookupDataAsync(ct);
        model.MobileNumber = mobile;
        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        ValidateRequiredDocuments(Request.Form.Files, await GetDocumentTypeNamesAsync(ct), existing.Documents.Select(x => x.DocumentType).ToArray());
        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        var fee = await _applicationService.GetApplicationFeeAsync(id, ct)
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
            return View("~/Views/NewConnection/Apply.cshtml", model);
        }

        try
        {
            var savedDocuments = await SaveDocumentsAsync(Request.Form.Files, existing.ApplicationNo, ct);
            var result = await _applicationService.CompletePublicApplicationAsync(id, mobile, new NewConnectionSubmitRequest
            {
                Form = model,
                Documents = savedDocuments,
                ApplicationNo = existing.ApplicationNo,
                IsPublicApplication = true,
                ActionByName = mobile,
                ActionByRole = "PublicApplicant",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                TargetStatus = "PendingPayment",
                StatusAction = "FeeCalculated",
                StatusRemarks = "Application reviewed and fee calculated. Payment is pending.",
                FeeQuote = fee,
                StartWorkflow = false
            }, ct);

            return RedirectToAction(nameof(Payment), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/NewConnection/Apply.cshtml", model);
        }
    }

    [HttpGet("/NewConnection/Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var details = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        if (details is null)
            return NotFound();

        ViewData["Title"] = "Application Details";
        return View("~/Views/NewConnection/Details.cshtml", details);
    }

    [HttpGet("/NewConnection/Payment/{id:long}")]
    public async Task<IActionResult> Payment(long id, int step = 1, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var model = await BuildPaymentModelAsync(id, mobile, true, step, paymentMethod, paymentIdentifier, ct);
        if (model is null)
            return NotFound();

        ViewData["Title"] = "Pay Application Fee";
        return View("~/Views/NewConnection/Payment.cshtml", model);
    }

    [HttpGet("/NewConnection/Payment/{id:long}/Confirm")]
    public async Task<IActionResult> ConfirmPayment(long id, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
        => await Payment(id, 3, paymentMethod, paymentIdentifier, ct);

    [HttpPost("/NewConnection/Payment/{id:long}/Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentPost(long id, string? paymentMethod, string? paymentIdentifier, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var model = await BuildPaymentModelAsync(id, mobile, true, 3, paymentMethod, paymentIdentifier, ct);
        if (model is null)
            return NotFound();

        try
        {
            var result = await _applicationService.CompletePublicPaymentAsync(id, mobile, new NewConnectionPaymentRequestDto
            {
                FeeQuote = model.Fee,
                ActionByName = mobile,
                ActionByRole = "PublicApplicant",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                PaymentMethod = paymentMethod,
                PaymentIdentifier = paymentIdentifier,
                StartWorkflow = true
            }, ct);

            TempData["SuccessMessage"] = $"Application submitted successfully. Application Number: {result.ApplicationNo}.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Payment), new { id, step = 3, paymentMethod, paymentIdentifier });
        }
    }

    [HttpGet("/NewConnection/FeePreview")]
    public async Task<IActionResult> FeePreview([FromQuery] NewConnectionFeeRequestDto request, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return Unauthorized();

        var fee = await _feeService.GetFeeAsync(request, ct);
        return fee is null ? NotFound() : Json(fee);
    }

    [HttpGet("/NewConnection/Lookups/Blocks")]
    public async Task<IActionResult> Blocks(string sectorId, CancellationToken ct)
        => Json(await _lookupService.GetBlocksBySectorAsync(sectorId, ResolveDevType(), ct));

    [HttpGet("/NewConnection/Lookups/ConnectionSubTypes")]
    public async Task<IActionResult> ConnectionSubTypes(string connectionCategoryId, CancellationToken ct)
        => Json(await _lookupService.GetConnectionSubTypesAsync(connectionCategoryId, ResolveDevType(), ct));

    private string? GetVerifiedMobile() => HttpContext.Session.GetString(SessionMobileKey);

    private async Task LoadLookupDataAsync(CancellationToken ct)
    {
        var lookups = await _lookupService.GetLookupDataAsync(ResolveDevType(), ct);
        ViewData["LookupData"] = lookups;
        ViewData["DocumentTypes"] = lookups.DocumentTypes.Select(x => x.Text).ToArray();
        ViewData["BlocksUrl"] = Url.Action(nameof(Blocks), "PublicNewConnection");
        ViewData["ConnectionSubTypesUrl"] = Url.Action(nameof(ConnectionSubTypes), "PublicNewConnection");
        ViewData["FeePreviewUrl"] = Url.Action(nameof(FeePreview), "PublicNewConnection");
        ViewData["LockMobileNumber"] = true;
    }

    private async Task<IReadOnlyList<string>> GetDocumentTypeNamesAsync(CancellationToken ct)
    {
        var documentTypes = await _lookupService.GetDocumentTypesAsync(ct);
        return documentTypes.Select(x => x.Text).ToArray();
    }

    private int? ResolveDevType()
        => int.TryParse(_configuration["NewConnection:DefaultDevType"], out var devType) ? devType : null;

    private async Task<NewConnectionPaymentViewModel?> BuildPaymentModelAsync(
        long id,
        string mobile,
        bool isPublicFlow,
        int step,
        string? paymentMethod,
        string? paymentIdentifier,
        CancellationToken ct)
    {
        var application = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        if (application is null || !application.CanContinue)
            return null;

        var fee = await _applicationService.GetApplicationFeeAsync(id, ct)
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
            PaymentIdentifier = paymentIdentifier,
            IsPublicFlow = isPublicFlow
        };
    }

    private async Task<IReadOnlyList<NewConnectionDocumentInputDto>> SaveDocumentsAsync(IFormFileCollection files, string applicationNo, CancellationToken ct)
    {
        var result = new List<NewConnectionDocumentInputDto>();
        var storageRoot = _configuration["FileStorage:DocumentBasePath"];
        if (string.IsNullOrWhiteSpace(storageRoot))
            throw new InvalidOperationException("Document storage path is not configured.");

        var maxUploadSizeMb = int.TryParse(_configuration["FileStorage:MaxUploadSizeMb"], out var configuredMaxUploadSizeMb)
            ? configuredMaxUploadSizeMb
            : 5;
        var maxBytes = maxUploadSizeMb * 1024L * 1024L;
        var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        if (allowedExtensions.Length == 0)
            allowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        var uploadRoot = Path.Combine(storageRoot, applicationNo);
        Directory.CreateDirectory(uploadRoot);

        foreach (var file in files.Where(x => x.Length > 0))
        {
            if (file.Length > maxBytes)
                throw new InvalidOperationException($"Each document must be {maxUploadSizeMb} MB or smaller.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF, JPG, JPEG, and PNG documents are allowed.");

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
                ContentType = file.ContentType,
                FileSize = file.Length
            });
        }

        return result;
    }

    private void ValidateRequiredDocuments(IFormFileCollection files, IReadOnlyCollection<string> configuredDocumentTypes, IReadOnlyCollection<string>? existingDocumentTypes = null)
    {
        var uploadedTypes = files.Where(x => x.Length > 0).Select(x => ResolveDocumentType(x.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (existingDocumentTypes is not null)
        {
            foreach (var existingDocumentType in existingDocumentTypes)
                uploadedTypes.Add(existingDocumentType);
        }

        foreach (var requiredType in RequiredDocumentTypes.Where(requiredType => configuredDocumentTypes.Contains(requiredType, StringComparer.OrdinalIgnoreCase)))
        {
            if (!uploadedTypes.Contains(requiredType))
                ModelState.AddModelError(string.Empty, $"{requiredType} is required.");
        }
    }

    private void NormalizeDeclarationFromRequest(NewConnectionApplicationFormDto model)
    {
        if (!Request.HasFormContentType || !Request.Form.TryGetValue(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), out var values))
            return;

        if (!values.Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) || value == "1"))
            return;

        model.DeclarationAccepted = true;
        ModelState.Remove(nameof(NewConnectionApplicationFormDto.DeclarationAccepted));
    }

    private void ValidateDeclaration(NewConnectionApplicationFormDto model)
    {
        if (!model.DeclarationAccepted)
            ModelState.AddModelError(nameof(NewConnectionApplicationFormDto.DeclarationAccepted), "Please accept the declaration.");
    }

    private static string ResolveDocumentType(string input)
    {
        var normalized = input.Replace("Documents_", string.Empty).Replace("_", " ").Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Other" : normalized;
    }

    private static string GenerateApplicationNo() => $"NC{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

    private static string MakeSafeFileName(string fileName)
    {
        var safe = new string(fileName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch).ToArray()).Trim('-', ' ');
        return string.IsNullOrWhiteSpace(safe) ? "document" : safe;
    }
}
