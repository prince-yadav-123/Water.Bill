using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.DTOs.Payments;
using Water.Bill.Application.Interfaces;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Infrastructure.Security;

namespace Water.Bill.ConsumerPortal.Controllers;

public class PublicNewConnectionController : ConsumerPortalControllerBase
{
    private const string SessionMobileKey = "PublicNewConnection.VerifiedMobile";
    private const string SessionVerifiedAtKey = "PublicNewConnection.VerifiedAt";
    private const string SessionModeKey = "PublicNewConnection.Mode";
    private const string PublicFlowViewDataKey = "IsPublicNewConnectionFlow";
    private readonly IConfiguration _configuration;
    private readonly IPublicNewConnectionOtpService _otpService;
    private readonly INewConnectionApplicationService _applicationService;
    private readonly INewConnectionLookupService _lookupService;
    private readonly INewConnectionFeeService _feeService;
    private readonly IConsumerPaymentService _paymentService;

    public PublicNewConnectionController(
        IConfiguration configuration,
        IPublicNewConnectionOtpService otpService,
        INewConnectionApplicationService applicationService,
        INewConnectionLookupService lookupService,
        INewConnectionFeeService feeService,
        IConsumerPaymentService paymentService,
        IErrorLogService errorLogService)
        : base(errorLogService)
    {
        _configuration = configuration;
        _otpService = otpService;
        _applicationService = applicationService;
        _lookupService = lookupService;
        _feeService = feeService;
        _paymentService = paymentService;
    }

    [HttpGet("/NewConnection/Start")]
    public IActionResult Start(string? mode = null)
    {
        ViewData["Title"] = "Verify Mobile";
        ViewData["IsTrackOnly"] = IsTrackOnly(mode);
        MarkPublicVerificationFlow();
        return View();
    }

    [HttpPost("/NewConnection/Start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(string mobileNumber, string? mode, CancellationToken ct)
    {
        ViewData["Title"] = "Verify Mobile";
        ViewData["IsTrackOnly"] = IsTrackOnly(mode);
        MarkPublicVerificationFlow();
        try
        {
            var result = await _otpService.RequestOtpAsync(mobileNumber, ct);
            TempData["OtpMessage"] = $"OTP sent to {result.MaskedMobileNumber}.";

            return RedirectToAction(nameof(VerifyOtp), new { mobileNumber = result.MobileNumber, mode = NormalizeMode(mode) });
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
            ModelState.AddModelError(nameof(mobileNumber), ex.Message);
            return View((object?)mobileNumber);
        }
    }

    [HttpGet("/NewConnection/VerifyOtp")]
    public IActionResult VerifyOtp(string mobileNumber, string? mode = null)
    {
        ViewData["Title"] = "Verify OTP";
        ViewData["FlowMode"] = NormalizeMode(mode);
        ViewData["IsTrackOnly"] = IsTrackOnly(mode);
        MarkPublicVerificationFlow();
        return View((object?)mobileNumber);
    }

    [HttpPost("/NewConnection/VerifyOtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(string mobileNumber, string otp, string? mode, CancellationToken ct)
    {
        ViewData["Title"] = "Verify OTP";
        ViewData["FlowMode"] = NormalizeMode(mode);
        ViewData["IsTrackOnly"] = IsTrackOnly(mode);
        MarkPublicVerificationFlow();
        try
        {
            var result = await _otpService.VerifyOtpAsync(mobileNumber, otp, ct);
            HttpContext.Session.SetString(SessionMobileKey, result.MobileNumber);
            HttpContext.Session.SetString(SessionVerifiedAtKey, result.VerifiedAt.ToString("O"));
            HttpContext.Session.SetString(SessionModeKey, NormalizeMode(mode));
            var applications = await _applicationService.GetPublicApplicationsByMobileAsync(result.MobileNumber, ct);
            if (applications.Count == 0)
            {
                if (IsTrackOnly(mode))
                {
                    TempData["InfoMessage"] = "No applications were found for this mobile number.";
                    return RedirectToAction(nameof(MyApplications));
                }

                return RedirectToAction(nameof(Apply));
            }

            return RedirectToAction(nameof(MyApplications));
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
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
        MarkPublicVerificationFlow();
        ViewData["IsTrackOnly"] = IsTrackOnlySession();
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
        MarkPublicVerificationFlow();
        var model = new NewConnectionApplicationFormDto
        {
            MobileNumber = mobile
        };
        await LoadLookupDataAsync(model, ct);
        return View("~/Views/NewConnection/Apply.cshtml", model);
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
        MarkPublicVerificationFlow();
        model.MobileNumber = mobile;
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);
        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        ValidateRequiredDocuments(Request.Form.Files, await GetRequiredDocumentTypeNamesAsync(ct));
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
            await LogHandledErrorAsync(ex);
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
        MarkPublicVerificationFlow();
        model.MobileNumber = mobile;
        await LoadLookupDataAsync(model, ct);
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
        MarkPublicVerificationFlow();
        model.MobileNumber = mobile;
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);
        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);

        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Apply.cshtml", model);

        ValidateRequiredDocuments(Request.Form.Files, await GetRequiredDocumentTypeNamesAsync(ct), existing.Documents.Select(x => x.DocumentType).ToArray());
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
            await LogHandledErrorAsync(ex);
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
        MarkPublicVerificationFlow();
        return View("~/Views/NewConnection/Details.cshtml", details);
    }

    [HttpGet("/NewConnection/Payment/{id:long}")]
    public async Task<IActionResult> Payment(long id, int step = 1, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        ViewData["IsDevelopmentPayment"] = _paymentService.IsDevelopmentMode();
        MarkPublicVerificationFlow();
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

        var result = await _paymentService.InitiatePaymentAsync(new PaymentInitiationRequestDto
        {
            ConsumerNo = $"NC{id}",
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
            if (!result.Success)
                await LogHandledErrorAsync(new InvalidOperationException(result.Message ?? "Payment reference could not be created."));
            return RedirectToAction(nameof(Payment), new { id, step = 3, paymentMethod, paymentIdentifier });
        }

        if (_paymentService.IsDevelopmentMode())
        {
            var processed = await _paymentService.ProcessDevelopmentSuccessAsync(result.JalReferenceId, BuildPaymentActorContext());
            TempData[processed.Success ? "SuccessMessage" : "ErrorMessage"] = processed.Message
                ?? (processed.Success ? "Application payment simulated successfully." : "Application payment simulation failed.");
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(PaymentStarted), new { referenceId = result.JalReferenceId });
    }

    [HttpGet("/NewConnection/PaymentStarted/{referenceId}")]
    public async Task<IActionResult> PaymentStarted(string referenceId, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        ViewData["Title"] = "Payment Initiated";
        ViewData["IsPublicFlow"] = true;
        MarkPublicVerificationFlow();
        var result = await _paymentService.GetInitiatedPaymentAsync(referenceId, null, ct);
        if (result is null)
        {
            TempData["ErrorMessage"] = "Payment reference was not found for this application.";
            return RedirectToAction(nameof(MyApplications));
        }

        return View("~/Views/NewConnection/PaymentStarted.cshtml", result);
    }

    [HttpGet("/NewConnection/Resubmit/{id:long}")]
    public async Task<IActionResult> Resubmit(long id, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var details = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        if (details is null || !details.CanResubmit)
            return NotFound();

        var model = await _applicationService.GetPublicResubmitFormAsync(id, mobile, ct);
        if (model is null) return NotFound();

        model.MobileNumber = mobile;
        ViewData["Title"] = "Correct & Resubmit Application";
        ViewData["FormAction"] = nameof(Resubmit);
        ViewData["FormRouteId"] = id;
        ViewData["SentBackRemarks"] = details.SentBackRemarks;
        ViewData["SentBackAt"] = details.SentBackAt;
        ViewData["ApplicationNo"] = details.ApplicationNo;
        ViewData["IsResubmit"] = true;
        ViewData["ExistingDocumentTypes"] = details.Documents.Select(x => x.DocumentType).ToArray();
        MarkPublicVerificationFlow();
        await LoadLookupDataAsync(model, ct);
        return View("~/Views/NewConnection/Resubmit.cshtml", model);
    }

    [HttpPost("/NewConnection/Resubmit/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit(long id, NewConnectionApplicationFormDto model, string? applicantRemarks, CancellationToken ct)
    {
        var mobile = GetVerifiedMobile();
        if (mobile is null)
            return RedirectToAction(nameof(Start));

        var details = await _applicationService.GetPublicApplicationDetailsAsync(id, mobile, ct);
        if (details is null || !details.CanResubmit)
            return NotFound();

        model.MobileNumber = mobile;
        ViewData["Title"] = "Correct & Resubmit Application";
        ViewData["FormAction"] = nameof(Resubmit);
        ViewData["FormRouteId"] = id;
        ViewData["SentBackRemarks"] = details.SentBackRemarks;
        ViewData["SentBackAt"] = details.SentBackAt;
        ViewData["ApplicationNo"] = details.ApplicationNo;
        ViewData["IsResubmit"] = true;
        ViewData["ExistingDocumentTypes"] = details.Documents.Select(x => x.DocumentType).ToArray();
        MarkPublicVerificationFlow();
        await PopulateSectorDevTypeAsync(model, ct);
        await LoadLookupDataAsync(model, ct);

        NormalizeDeclarationFromRequest(model);
        ValidateDeclaration(model);
        if (!ModelState.IsValid)
            return View("~/Views/NewConnection/Resubmit.cshtml", model);

        var newDocuments = await SaveDocumentsAsync(Request.Form.Files, details.ApplicationNo, ct);

        try
        {
            var result = await _applicationService.ResubmitPublicApplicationAsync(
                id, mobile, applicantRemarks, newDocuments, ct);

            TempData["SuccessMessage"] = $"Application {result.ApplicationNo} resubmitted successfully. It has been returned to the authority for review.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            await LogHandledErrorAsync(ex);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/NewConnection/Resubmit.cshtml", model);
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
        => Json((await _lookupService.GetSectorContextAsync(sectorId, ct)).Blocks);

    [HttpGet("/NewConnection/Lookups/SectorContext")]
    public async Task<IActionResult> SectorContext(string sectorId, CancellationToken ct)
        => Json(await _lookupService.GetSectorContextAsync(sectorId, ct));

    [HttpGet("/NewConnection/Lookups/ConnectionSubTypes")]
    public async Task<IActionResult> ConnectionSubTypes(string connectionCategoryId, int? devType, CancellationToken ct)
        => Json(await _lookupService.GetConnectionSubTypesAsync(connectionCategoryId, devType, ct));

    private string? GetVerifiedMobile() => HttpContext.Session.GetString(SessionMobileKey);

    private bool IsTrackOnlySession()
        => IsTrackOnly(HttpContext.Session.GetString(SessionModeKey));

    private static bool IsTrackOnly(string? mode)
        => string.Equals(mode, "track", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeMode(string? mode)
        => IsTrackOnly(mode) ? "track" : "apply";

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
        ViewData["BlocksUrl"] = Url.Action(nameof(Blocks), "PublicNewConnection");
        ViewData["SectorContextUrl"] = Url.Action(nameof(SectorContext), "PublicNewConnection");
        ViewData["ConnectionSubTypesUrl"] = Url.Action(nameof(ConnectionSubTypes), "PublicNewConnection");
        ViewData["FeePreviewUrl"] = Url.Action(nameof(FeePreview), "PublicNewConnection");
        ViewData["LockMobileNumber"] = true;
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

    private PaymentActorContextDto BuildPaymentActorContext()
        => new()
        {
            UserName = GetVerifiedMobile() ?? "PublicApplicant",
            UserRole = "PublicApplicant",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

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

    private void ValidateRequiredDocuments(IFormFileCollection files, IReadOnlyCollection<string> requiredDocumentTypes, IReadOnlyCollection<string>? existingDocumentTypes = null)
    {
        var uploadedTypes = files.Where(x => x.Length > 0).Select(x => ResolveDocumentType(x.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
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

    private void MarkPublicVerificationFlow()
        => ViewData[PublicFlowViewDataKey] = true;
}
