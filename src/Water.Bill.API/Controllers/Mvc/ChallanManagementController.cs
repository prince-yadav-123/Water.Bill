using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Challans;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.API.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Extensions;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ChallanManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICommunicationService _communicationService;

    public ChallanManagementController(ApplicationDbContext db, ICommunicationService communicationService)
    {
        _db = db;
        _communicationService = communicationService;
    }

    [HttpGet("/ChallanManagement")]
    [RequirePermission("Challan Management.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Challan Management";
        ViewData["ActiveMenu"] = "Challan Management";
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);

        var model = new ChallanManagementIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            Status = Normalize(status),
            FromDate = fromDate,
            ToDate = toDate
        };

        var (challans, totalCount) = await SearchChallansPagedAsync(model, page, pageSize, ct);
        model.Challans = challans;
        ViewBag.Pagination = PaginationViewModel.Create(new Application.Models.PagedResult<ChallanListRowViewModel>
        {
            Items = challans,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });

        return View(model);
    }

    [HttpGet("/ChallanManagement/Create")]
    [RequirePermission("Challan Management.add")]
    public async Task<IActionResult> Create(
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        string? purpose,
        string? billNo,
        CancellationToken ct)
    {
        ViewData["Title"] = "Generate Challan";
        ViewData["ActiveMenu"] = "Challan Management";

        consumerNo = Normalize(consumerNo);
        if (string.IsNullOrWhiteSpace(consumerNo))
        {
            var searchModel = new ChallanCreateViewModel
            {
                ConsumerName = Normalize(consumerName),
                MobileNo = Normalize(mobileNo),
                Sector = Normalize(sector),
                Block = Normalize(block),
                PlotNo = Normalize(plotNo),
                Purpose = Normalize(purpose) ?? ChallanPurposes.ExistingBillDue,
                BillNo = Normalize(billNo)
            };
            searchModel.HasConsumerSearch = HasAnySearch(searchModel.ConsumerName, searchModel.MobileNo, searchModel.Sector, searchModel.Block, searchModel.PlotNo);
            searchModel.Consumers = searchModel.HasConsumerSearch
                ? await SearchConsumersAsync(ToIndexSearchModel(searchModel), ct)
                : [];
            await PrepareCreateModelAsync(searchModel, null, ct);
            return View(searchModel);
        }

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
        if (consumer is null)
        {
            var searchModel = new ChallanCreateViewModel
            {
                ConsumerNo = consumerNo,
                ConsumerName = Normalize(consumerName),
                MobileNo = Normalize(mobileNo),
                Sector = Normalize(sector),
                Block = Normalize(block),
                PlotNo = Normalize(plotNo),
                Purpose = Normalize(purpose) ?? ChallanPurposes.ExistingBillDue,
                BillNo = Normalize(billNo),
                HasConsumerSearch = true
            };
            searchModel.Consumers = await SearchConsumersAsync(ToIndexSearchModel(searchModel), ct);
            await PrepareCreateModelAsync(searchModel, null, ct);
            return View(searchModel);
        }

        var selectedPurpose = Normalize(purpose) ?? ChallanPurposes.ExistingBillDue;
        var model = new ChallanCreateViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = ToConsumerSummary(consumer),
            Purpose = selectedPurpose,
            BillNo = Normalize(billNo),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo)
        };

        await PrepareCreateModelAsync(model, consumer, ct);
        return View(model);
    }

    [HttpPost("/ChallanManagement/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Challan Management.add")]
    public async Task<IActionResult> Create(ChallanCreateViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Generate Challan";
        ViewData["ActiveMenu"] = "Challan Management";

        model.ConsumerNo = Normalize(model.ConsumerNo) ?? string.Empty;
        model.Purpose = Normalize(model.Purpose) ?? ChallanPurposes.ExistingBillDue;
        model.BankCode = Normalize(model.BankCode);
        model.BillNo = Normalize(model.BillNo);
        model.Remarks = Normalize(model.Remarks);

        var consumer = await _db.ConsumerDetailsMasters
            .FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);
        if (consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Selected consumer was not found.");

        if (!ChallanPurposes.Options().Any(x => x.Value == model.Purpose))
            ModelState.AddModelError(nameof(model.Purpose), "Please select a valid challan purpose.");

        JalPrintBillMaster? sourceBill = null;
        if (consumer is not null && model.Purpose == ChallanPurposes.ExistingBillDue)
        {
            sourceBill = await GetSourceBillAsync(consumer.ConsNo, model.BillNo, ct);
            if (sourceBill is null)
                ModelState.AddModelError(nameof(model.BillNo), "Please select a valid generated bill for bill due challan.");
        }

        if (!ModelState.IsValid || consumer is null)
        {
            await PrepareCreateModelAsync(model, consumer, ct);
            return View(model);
        }

        var bank = await FindBankAsync(model.BankCode, ct);
        var now = DateTime.Now;
        var challanNo = await GenerateChallanNoAsync(consumer.DevType, ct);
        var amount = model.Amount.GetValueOrDefault();
        var purposeCode = PurposeCode(model.Purpose);
        var heads = BuildAmountHeads(model.Purpose, amount, sourceBill);

        var challan = new Challan
        {
            ReceiptId = challanNo,
            ReceiptId1 = challanNo,
            RecpNo = challanNo,
            ConsNo = consumer.ConsNo,
            FlatNo = consumer.FlatNo,
            Blk = consumer.BlkNo,
            Sec = consumer.Sector,
            BlPerFr = model.BillPeriodFrom,
            BlPerTo = model.BillPeriodTo,
            DueDt = model.DueDate,
            BillAmt = heads.BillAmount,
            Surcharge = heads.Surcharge,
            PaidAmt = 0,
            PayDate = null,
            Arrear = heads.Arrear,
            Credit = 0,
            Noc = heads.NocAmount,
            Rmc = 0,
            Secu = 0,
            TFee = 0,
            Css = "0",
            BnkCd = model.BankCode,
            BrNm = BuildBankName(bank),
            RevBilFr = purposeCode,
            DevType = consumer.DevType,
            Gst = 0,
            ConnCharge = heads.ConnectionCharge,
            PanalityCharges = heads.OtherCharge,
            BankId = model.BankCode,
            DeposeterName = consumer.ConsNm1,
            BillId = model.Purpose == ChallanPurposes.ExistingBillDue ? sourceBill?.BillNo : purposeCode,
            Status = "1",
            EntryDate = now,
            ChallanVia = "ADMIN",
            ChallanStatus = 1,
            Userid = CurrentUsername(),
            Address = BuildAddress(consumer),
            RT = 0,
            Disconnection = 0,
            Reconnection = 0,
            Rid = consumer.Rid?.ToString()
        };

        _db.Challans.Add(challan);
        await _db.SaveChangesAsync(ct);
        await AddHistoryAsync(challan, null, ChallanStatuses.PendingPayment, "Generated", $"Challan generated for {ChallanPurposes.Display(model.Purpose)}.", ct);

        await _communicationService.SendAsync(
            CommunicationPurposes.ChallanGenerated,
            new NotificationRecipient
            {
                Name = consumer.ConsNm1,
                Email = consumer.EmailId,
                Mobile = consumer.MobNo
            },
            new Dictionary<string, string?>
            {
                ["ConsumerName"] = consumer.ConsNm1,
                ["ConsumerNo"] = consumer.ConsNo,
                ["ChallanNo"] = challanNo,
                ["Purpose"] = ChallanPurposes.Display(model.Purpose),
                ["Amount"] = amount.ToString("0.00"),
                ["Status"] = ChallanStatuses.PendingPayment,
                ["Date"] = now.ToString("dd MMM yyyy")
            },
            NotificationChannelOptions.For(CommunicationChannels.Email, CommunicationChannels.Sms),
            "Challan",
            challan.Id.ToString(),
            challanNo,
            ct);

        TempData["SuccessMessage"] = $"Challan {challanNo} generated successfully.";
        return RedirectToAction(nameof(Details), new { id = challan.Id });
    }

    [HttpPost("/ChallanManagement/Cancel/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Challan Management.edit")]
    public async Task<IActionResult> Cancel(long id, string? cancelReason, CancellationToken ct)
    {
        cancelReason = Normalize(cancelReason);
        if (string.IsNullOrWhiteSpace(cancelReason))
        {
            TempData["ErrorMessage"] = "Cancellation reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var challan = await _db.Challans.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (challan is null)
            return NotFound();

        var currentStatus = ResolveStatus(challan);
        if (currentStatus == ChallanStatuses.Paid)
        {
            TempData["ErrorMessage"] = "Paid challan cannot be cancelled from this screen.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (currentStatus == ChallanStatuses.Cancelled)
        {
            TempData["ErrorMessage"] = "Selected challan is already cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        challan.Status = "0";
        challan.ChallanStatus = 0;
        challan.Upd = "CANCEL";
        challan.Err = "CANCEL";
        await _db.SaveChangesAsync(ct);
        await AddHistoryAsync(challan, currentStatus, ChallanStatuses.Cancelled, "Cancelled", cancelReason, ct);

        TempData["SuccessMessage"] = "Challan cancelled successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/ChallanManagement/PostPayment/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Challan Management.edit")]
    public async Task<IActionResult> PostPayment(long id, [Bind(Prefix = "PaymentPost")] ChallanPaymentPostViewModel model, CancellationToken ct)
    {
        model.BankCode = Normalize(model.BankCode);
        model.TransactionReferenceNo = Normalize(model.TransactionReferenceNo);
        model.Remarks = Normalize(model.Remarks);
        model.PaymentMode = Normalize(model.PaymentMode) ?? ChallanPaymentModes.Cash;

        var challan = await _db.Challans.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (challan is null)
            return NotFound();

        var currentStatus = ResolveStatus(challan);
        if (currentStatus != ChallanStatuses.PendingPayment)
        {
            TempData["ErrorMessage"] = "Only pending payment challans can be posted as paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var payableAmount = ResolvePayableAmount(challan);
        if (model.Amount.GetValueOrDefault() < payableAmount)
            ModelState.AddModelError(nameof(model.Amount), $"Paid amount cannot be less than payable amount Rs. {payableAmount:N2}.");

        if (!ChallanPaymentModes.Options().Any(x => x.Value == model.PaymentMode))
            ModelState.AddModelError(nameof(model.PaymentMode), "Please select a valid payment mode.");

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id });
        }

        var bank = await FindBankAsync(model.BankCode, ct);
        var bankName = BuildBankName(bank);

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            challan.PayDate = model.PaymentDate;
            challan.PaidAmt = model.Amount;
            challan.BnkCd = model.BankCode ?? challan.BnkCd;
            challan.BankId = model.BankCode ?? challan.BankId;
            challan.BrNm = bankName ?? challan.BrNm;
            challan.Status = "1";
            challan.ChallanStatus = 1;
            challan.Upd = "PAID";

            _db.ChallanPaymentHistories.Add(new ChallanPaymentHistory
            {
                ChallanId = challan.Id,
                ChallanNo = challan.RecpNo ?? challan.ReceiptId,
                ConsumerNo = challan.ConsNo,
                SourceBillNo = challan.RevBilFr == "BILL" ? challan.BillId : null,
                Amount = model.Amount.GetValueOrDefault(),
                PaymentDate = model.PaymentDate,
                PaymentMode = model.PaymentMode,
                BankCode = model.BankCode,
                BankName = bankName,
                TransactionReferenceNo = model.TransactionReferenceNo,
                Remarks = model.Remarks,
                PostedByUserId = CurrentUserId(),
                PostedByName = CurrentUsername(),
                PostedOn = DateTime.Now
            });

            await _db.SaveChangesAsync(ct);

            if (challan.RevBilFr == "BILL" && !string.IsNullOrWhiteSpace(challan.BillId) && !string.IsNullOrWhiteSpace(challan.ConsNo))
                await MarkSourceBillPaidAsync(challan, model, ct);

            await AddHistoryAsync(challan, currentStatus, ChallanStatuses.Paid, "PaymentPosted",
                $"Payment posted by {ChallanPaymentModes.Display(model.PaymentMode)}. Ref: {model.TransactionReferenceNo ?? "-"}", ct);

            await tx.CommitAsync(ct);
        });

        TempData["SuccessMessage"] = "Payment posted successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("/ChallanManagement/Details/{id:long}")]
    [RequirePermission("Challan Management.view")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Challan Details";
        ViewData["ActiveMenu"] = "Challan Management";

        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpGet("/ChallanManagement/Print/{id:long}")]
    [RequirePermission("Challan Management.print")]
    public async Task<IActionResult> Print(long id, CancellationToken ct)
    {
        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();
        if (model.Status == ChallanStatuses.Cancelled)
            return BadRequest("Cancelled challan cannot be printed.");

        ViewData["Title"] = "Print Challan";
        ViewData["ActiveMenu"] = "Challan Management";
        return View(model);
    }

    [HttpGet("/ChallanManagement/Receipt/{id:long}")]
    [RequirePermission("Challan Management.print")]
    public async Task<IActionResult> Receipt(long id, CancellationToken ct)
    {
        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();
        if (model.Status != ChallanStatuses.Paid)
            return BadRequest("Receipt can be printed only after payment is posted.");

        ViewData["Title"] = "Print Receipt";
        ViewData["ActiveMenu"] = "Challan Management";
        return View(model);
    }

    [HttpGet("/ChallanManagement/PaymentHistory")]
    [RequirePermission("Challan Management.view")]
    public async Task<IActionResult> PaymentHistory(
        string? search,
        string? consumerNo,
        string? challanNo,
        string? paymentMode,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = "Challan Payment History";
        ViewData["ActiveMenu"] = "Challan Management";

        var model = new ChallanPaymentHistoryIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo),
            ChallanNo = Normalize(challanNo),
            PaymentMode = Normalize(paymentMode),
            FromDate = fromDate,
            ToDate = toDate,
            PaymentModeOptions = ChallanPaymentModes.Options(paymentMode).ToList()
        };

        var query =
            from payment in _db.ChallanPaymentHistories.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on payment.ConsumerNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where !payment.IsDeleted
            select new { payment, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x =>
                (x.payment.ChallanNo != null && x.payment.ChallanNo.Contains(model.Search)) ||
                (x.payment.ConsumerNo != null && x.payment.ConsumerNo.Contains(model.Search)) ||
                (x.payment.TransactionReferenceNo != null && x.payment.TransactionReferenceNo.Contains(model.Search)) ||
                (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)));
        }

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.payment.ConsumerNo != null && x.payment.ConsumerNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ChallanNo))
            query = query.Where(x => x.payment.ChallanNo != null && x.payment.ChallanNo.StartsWith(model.ChallanNo));
        if (!string.IsNullOrWhiteSpace(model.PaymentMode))
            query = query.Where(x => x.payment.PaymentMode == model.PaymentMode);
        if (model.FromDate.HasValue)
            query = query.Where(x => x.payment.PaymentDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.payment.PaymentDate < model.ToDate.Value.Date.AddDays(1));

        model.Payments = await query
            .OrderByDescending(x => x.payment.PaymentDate)
            .ThenByDescending(x => x.payment.Id)
            .Take(200)
            .Select(x => new ChallanPaymentHistoryRowViewModel
            {
                Id = x.payment.Id,
                ChallanId = x.payment.ChallanId,
                ChallanNo = x.payment.ChallanNo,
                ConsumerNo = x.payment.ConsumerNo,
                ConsumerName = x.consumer != null ? x.consumer.ConsNm1 : null,
                SourceBillNo = x.payment.SourceBillNo,
                Amount = x.payment.Amount,
                PaymentDate = x.payment.PaymentDate,
                PaymentMode = x.payment.PaymentMode,
                BankName = x.payment.BankName,
                TransactionReferenceNo = x.payment.TransactionReferenceNo,
                PostedByName = x.payment.PostedByName,
                PostedOn = x.payment.PostedOn
            })
            .ToListAsync(ct);

        return View(model);
    }

    private async Task<IReadOnlyList<ChallanConsumerSearchRowViewModel>> SearchConsumersAsync(ChallanManagementIndexViewModel model, CancellationToken ct)
    {
        var query = _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.Status == 1);

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.ConsNm1 != null && x.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.MobNo != null && x.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            query = query.Where(x => x.Sector != null && x.Sector.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            query = query.Where(x => x.BlkNo != null && x.BlkNo.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            query = query.Where(x => x.FlatNo != null && x.FlatNo.StartsWith(model.PlotNo));

        return await query
            .OrderBy(x => x.ConsNo)
            .Take(50)
            .Select(x => new ChallanConsumerSearchRowViewModel
            {
                ConsumerNo = x.ConsNo,
                ConsumerName = x.ConsNm1,
                MobileNo = x.MobNo,
                PropertyNo = x.Sector + "/" + x.BlkNo + "-" + x.FlatNo,
                ConnectionType = x.ConTp,
                DevType = x.DevType
            })
            .ToListAsync(ct);
    }

    private static ChallanManagementIndexViewModel ToIndexSearchModel(ChallanCreateViewModel model) => new()
    {
        ConsumerNo = model.ConsumerNo,
        ConsumerName = model.ConsumerName,
        MobileNo = model.MobileNo,
        Sector = model.Sector,
        Block = model.Block,
        PlotNo = model.PlotNo
    };

    private async Task<(IReadOnlyList<ChallanListRowViewModel> Items, int TotalCount)> SearchChallansPagedAsync(
        ChallanManagementIndexViewModel model, int page, int pageSize, CancellationToken ct)
    {
        var baseQ =
            from challan in _db.Challans.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on challan.ConsNo equals consumer.ConsNo into cj
            from consumer in cj.DefaultIfEmpty()
            where challan.Status != null
            select new { challan, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
            baseQ = baseQ.Where(x => (x.challan.RecpNo != null && x.challan.RecpNo.Contains(model.Search)) || (x.challan.ConsNo != null && x.challan.ConsNo.Contains(model.Search)) || (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)) || (x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.Search)));
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo)) baseQ = baseQ.Where(x => x.challan.ConsNo != null && x.challan.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName)) baseQ = baseQ.Where(x => x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo)) baseQ = baseQ.Where(x => x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector)) baseQ = baseQ.Where(x => x.challan.Sec != null && x.challan.Sec.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block)) baseQ = baseQ.Where(x => x.challan.Blk != null && x.challan.Blk.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo)) baseQ = baseQ.Where(x => x.challan.FlatNo != null && x.challan.FlatNo.StartsWith(model.PlotNo));
        if (model.FromDate.HasValue) baseQ = baseQ.Where(x => x.challan.EntryDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue) baseQ = baseQ.Where(x => x.challan.EntryDate < model.ToDate.Value.Date.AddDays(1));

        var orderedQ = baseQ.OrderByDescending(x => x.challan.EntryDate).ThenByDescending(x => x.challan.Id);
        var totalCount = await orderedQ.CountAsync(ct);
        var entities = await orderedQ.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var rows = entities.Select(x => new ChallanListRowViewModel
        {
            Id = x.challan.Id, ChallanNo = x.challan.RecpNo ?? x.challan.ReceiptId,
            ConsumerNo = x.challan.ConsNo, ConsumerName = x.consumer?.ConsNm1, MobileNo = x.consumer?.MobNo,
            PropertyNo = x.challan.Sec + "/" + x.challan.Blk + "-" + x.challan.FlatNo,
            Purpose = x.challan.RevBilFr, Amount = ResolvePayableAmount(x.challan),
            Status = ResolveStatus(x.challan), GeneratedOn = x.challan.EntryDate
        }).ToList();

        IReadOnlyList<ChallanListRowViewModel> result = string.IsNullOrWhiteSpace(model.Status)
            ? rows : rows.Where(x => x.Status.Equals(model.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        return (result, totalCount);
    }

    private IQueryable<dynamic> BuildChallanQuery(ChallanManagementIndexViewModel model)
    {
        var query =
            from challan in _db.Challans.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on challan.ConsNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where challan.Status != null
            select new { challan, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x =>
                (x.challan.RecpNo != null && x.challan.RecpNo.Contains(model.Search)) ||
                (x.challan.ReceiptId != null && x.challan.ReceiptId.Contains(model.Search)) ||
                (x.challan.ConsNo != null && x.challan.ConsNo.Contains(model.Search)) ||
                (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)) ||
                (x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.Search)));
        }
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.challan.ConsNo != null && x.challan.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            query = query.Where(x => x.challan.Sec != null && x.challan.Sec.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            query = query.Where(x => x.challan.Blk != null && x.challan.Blk.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            query = query.Where(x => x.challan.FlatNo != null && x.challan.FlatNo.StartsWith(model.PlotNo));
        if (model.FromDate.HasValue)
            query = query.Where(x => x.challan.EntryDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.challan.EntryDate < model.ToDate.Value.Date.AddDays(1));

        return (IQueryable<dynamic>)query;
    }

    private IReadOnlyList<ChallanListRowViewModel> MapChallanRows(IEnumerable<dynamic> entities, string? statusFilter = null)
    {
        var rows = entities
            .Select(x => new ChallanListRowViewModel
            {
                Id = x.challan.Id,
                ChallanNo = x.challan.RecpNo ?? x.challan.ReceiptId,
                ConsumerNo = x.challan.ConsNo,
                ConsumerName = x.consumer != null ? x.consumer.ConsNm1 : null,
                MobileNo = x.consumer != null ? x.consumer.MobNo : null,
                PropertyNo = x.challan.Sec + "/" + x.challan.Blk + "-" + x.challan.FlatNo,
                Purpose = x.challan.RevBilFr,
                Amount = ResolvePayableAmount(x.challan),
                Status = ResolveStatus(x.challan),
                GeneratedOn = x.challan.EntryDate,
                DevType = null
            })
            .ToList();

        if (string.IsNullOrWhiteSpace(statusFilter))
            return rows;
        return rows.Where(x => x.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async Task<IReadOnlyList<ChallanListRowViewModel>> SearchChallansAsync(ChallanManagementIndexViewModel model, CancellationToken ct)
    {
        var query =
            from challan in _db.Challans.AsNoTracking()
            join consumer in _db.ConsumerDetailsMasters.AsNoTracking() on challan.ConsNo equals consumer.ConsNo into consumerJoin
            from consumer in consumerJoin.DefaultIfEmpty()
            where challan.Status != null
            select new { challan, consumer };

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x =>
                (x.challan.RecpNo != null && x.challan.RecpNo.Contains(model.Search)) ||
                (x.challan.ReceiptId != null && x.challan.ReceiptId.Contains(model.Search)) ||
                (x.challan.ConsNo != null && x.challan.ConsNo.Contains(model.Search)) ||
                (x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.Search)) ||
                (x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.Search)));
        }
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.challan.ConsNo != null && x.challan.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.ConsumerName))
            query = query.Where(x => x.consumer != null && x.consumer.ConsNm1 != null && x.consumer.ConsNm1.Contains(model.ConsumerName));
        if (!string.IsNullOrWhiteSpace(model.MobileNo))
            query = query.Where(x => x.consumer != null && x.consumer.MobNo != null && x.consumer.MobNo.Contains(model.MobileNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            query = query.Where(x => x.challan.Sec != null && x.challan.Sec.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            query = query.Where(x => x.challan.Blk != null && x.challan.Blk.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            query = query.Where(x => x.challan.FlatNo != null && x.challan.FlatNo.StartsWith(model.PlotNo));
        if (model.FromDate.HasValue)
            query = query.Where(x => x.challan.EntryDate >= model.FromDate.Value.Date);
        if (model.ToDate.HasValue)
            query = query.Where(x => x.challan.EntryDate < model.ToDate.Value.Date.AddDays(1));

        var entities = await query
            .OrderByDescending(x => x.challan.EntryDate)
            .ThenByDescending(x => x.challan.Id)
            .Take(100)
            .ToListAsync(ct);

        return MapChallanRows(entities, model.Status);
    }

    private async Task PrepareCreateModelAsync(ChallanCreateViewModel model, ConsumerDetailsMaster? consumer, CancellationToken ct)
    {
        model.Consumer = consumer is null ? null : ToConsumerSummary(consumer);
        model.PurposeOptions = ChallanPurposes.Options(model.Purpose).ToList();
        model.BankOptions = await BuildBankOptionsAsync(model.BankCode, ct);
        model.BillOptions = consumer is null ? [] : await BuildBillOptionsAsync(consumer.ConsNo, model.BillNo, ct);

        if (consumer is null || model.Amount.HasValue)
            return;

        if (model.Purpose == ChallanPurposes.ExistingBillDue)
        {
            var bill = await GetSourceBillAsync(consumer.ConsNo, model.BillNo, ct);

            if (bill is not null)
            {
                model.BillNo ??= bill.BillNo;
                model.Amount = bill.TotalBillAmt ?? bill.DueAmt ?? bill.MinTotalAmt;
                model.BillPeriodFrom = bill.BillDateFrom;
                model.BillPeriodTo = bill.BillDateTo;
                model.DueDate = bill.BillDueDate ?? bill.DueDate;
                model.SuggestedAmountNote = $"Amount suggested from latest bill {(bill.BillNo ?? string.Empty)}.";
            }
        }
        else if (model.Purpose == ChallanPurposes.NdcNoDuesFee)
        {
            var noc = await _db.MasterNocAmts
                .AsNoTracking()
                .Where(x => x.Status == "1" && x.ConMainId == consumer.ConTp)
                .FirstOrDefaultAsync(ct);

            if (noc?.NocAmt > 0)
            {
                model.Amount = noc.NocAmt;
                model.SuggestedAmountNote = "Amount suggested from NDC amount master.";
            }
        }
    }

    private async Task<ChallanDetailsViewModel?> BuildDetailsAsync(long id, CancellationToken ct)
    {
        var challan = await _db.Challans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (challan is null)
            return null;

        var consumer = !string.IsNullOrWhiteSpace(challan.ConsNo)
            ? await _db.ConsumerDetailsMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ConsNo == challan.ConsNo, ct)
            : null;

        return new ChallanDetailsViewModel
        {
            Id = challan.Id,
            ChallanNo = challan.RecpNo ?? challan.ReceiptId,
            ReceiptNo = challan.ReceiptId,
            Purpose = PurposeDisplayFromCode(challan.RevBilFr ?? challan.BillId),
            SourceBillNo = challan.RevBilFr == "BILL" ? challan.BillId : null,
            Consumer = consumer is null ? ToConsumerSummary(challan) : ToConsumerSummary(consumer),
            BillPeriodFrom = challan.BlPerFr,
            BillPeriodTo = challan.BlPerTo,
            DueDate = challan.DueDt,
            GeneratedOn = challan.EntryDate,
            BillAmount = challan.BillAmt ?? 0,
            Surcharge = challan.Surcharge ?? 0,
            Arrear = challan.Arrear ?? 0,
            NocAmount = challan.Noc ?? 0,
            ConnectionCharge = challan.ConnCharge ?? 0,
            PenaltyCharge = challan.PanalityCharges ?? 0,
            TotalAmount = ResolvePayableAmount(challan),
            BankCode = challan.BnkCd ?? challan.BankId,
            BankName = challan.BrNm,
            Status = ResolveStatus(challan),
            GeneratedBy = challan.Userid,
            Remarks = challan.BillId,
            CanCancel = ResolveStatus(challan) == ChallanStatuses.PendingPayment,
            CanPostPayment = ResolveStatus(challan) == ChallanStatuses.PendingPayment,
            PaymentPost = new ChallanPaymentPostViewModel
            {
                ChallanId = challan.Id,
                Amount = ResolvePayableAmount(challan),
                PaymentDate = DateTime.Today,
                BankCode = challan.BnkCd ?? challan.BankId,
                PaymentModeOptions = ChallanPaymentModes.Options(ChallanPaymentModes.Cash).ToList(),
                BankOptions = await BuildBankOptionsAsync(challan.BnkCd ?? challan.BankId, ct)
            },
            Payments = await _db.ChallanPaymentHistories
                .AsNoTracking()
                .Where(x => x.ChallanId == challan.Id && !x.IsDeleted)
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.Id)
                .Select(x => new ChallanPaymentHistoryRowViewModel
                {
                    Id = x.Id,
                    ChallanId = x.ChallanId,
                    ChallanNo = x.ChallanNo,
                    ConsumerNo = x.ConsumerNo,
                    SourceBillNo = x.SourceBillNo,
                    Amount = x.Amount,
                    PaymentDate = x.PaymentDate,
                    PaymentMode = x.PaymentMode,
                    BankName = x.BankName,
                    TransactionReferenceNo = x.TransactionReferenceNo,
                    PostedByName = x.PostedByName,
                    PostedOn = x.PostedOn
                })
                .ToListAsync(ct),
            Histories = await _db.ChallanHistories
                .AsNoTracking()
                .Where(x => x.ChallanId == challan.Id && !x.IsDeleted)
                .OrderByDescending(x => x.ActionOn)
                .Select(x => new ChallanHistoryRowViewModel
                {
                    Action = x.Action,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Remarks = x.Remarks,
                    ActionByName = x.ActionByName,
                    ActionOn = x.ActionOn
                })
                .ToListAsync(ct)
        };
    }

    private async Task<List<SelectListItem>> BuildBillOptionsAsync(string consumerNo, string? selected, CancellationToken ct)
    {
        var bills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo
                && x.Status == "1"
                && x.BillNo != null
                && x.PaidDate == null
                && (x.PaidStatus == null || x.PaidStatus != "Y"))
            .OrderByDescending(x => x.BillDate)
            .ThenByDescending(x => x.BillDateTo)
            .Take(20)
            .ToListAsync(ct);

        return bills
            .Select(x =>
            {
                var amount = x.TotalBillAmt ?? x.DueAmt ?? x.MinTotalAmt ?? 0;
                var text = $"{x.BillNo} - {x.BillDateFrom:dd MMM yyyy} to {x.BillDateTo:dd MMM yyyy} - Rs. {amount:N2}";
                return new SelectListItem(text, x.BillNo, x.BillNo == selected);
            })
            .ToList();
    }

    private async Task<JalPrintBillMaster?> GetSourceBillAsync(string consumerNo, string? billNo, CancellationToken ct)
    {
        var query = _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo
                && x.Status == "1"
                && x.BillNo != null
                && x.PaidDate == null
                && (x.PaidStatus == null || x.PaidStatus != "Y"));

        if (!string.IsNullOrWhiteSpace(billNo))
            query = query.Where(x => x.BillNo == billNo);

        return await query
            .OrderByDescending(x => x.BillDate)
            .ThenByDescending(x => x.BillDateTo)
            .FirstOrDefaultAsync(ct);
    }

    private static ChallanAmountHeads BuildAmountHeads(string purpose, double amount, JalPrintBillMaster? bill)
    {
        if (purpose == ChallanPurposes.ExistingBillDue)
        {
            return new ChallanAmountHeads(
                bill?.MinTotalAmt ?? amount,
                0,
                bill?.Arear ?? 0,
                0,
                0,
                0);
        }

        return purpose switch
        {
            ChallanPurposes.NdcNoDuesFee => new ChallanAmountHeads(0, 0, 0, amount, 0, 0),
            ChallanPurposes.NewConnectionFee => new ChallanAmountHeads(0, 0, 0, 0, amount, 0),
            ChallanPurposes.OtherServiceCharge => new ChallanAmountHeads(0, 0, 0, 0, 0, amount),
            _ => new ChallanAmountHeads(amount, 0, 0, 0, 0, 0)
        };
    }

    private async Task AddHistoryAsync(Challan challan, string? fromStatus, string toStatus, string action, string? remarks, CancellationToken ct)
    {
        _db.ChallanHistories.Add(new ChallanHistory
        {
            ChallanId = challan.Id,
            ChallanNo = challan.RecpNo ?? challan.ReceiptId,
            ConsumerNo = challan.ConsNo,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Action = action,
            Remarks = remarks,
            ActionByUserId = CurrentUserId(),
            ActionByName = CurrentUsername(),
            ActionOn = DateTime.Now
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task MarkSourceBillPaidAsync(Challan challan, ChallanPaymentPostViewModel payment, CancellationToken ct)
    {
        var consumerNo = challan.ConsNo ?? string.Empty;
        var billNo = challan.BillId ?? string.Empty;
        var challanNo = challan.RecpNo ?? challan.ReceiptId ?? string.Empty;
        var amount = payment.Amount.GetValueOrDefault();
        var bankCode = payment.BankCode;

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE `jal_print_bill_master`
SET `paid_date` = {payment.PaymentDate},
    `paid_amt` = {amount},
    `PAID_STATUS` = 'Y',
    `CHALLAN_NO` = {challanNo},
    `BANK_CODE` = {bankCode}
WHERE `CONS_NO` = {consumerNo}
  AND `BILL_NO` = {billNo}
  AND `STATUS` = '1';", ct);
    }

    private async Task<string> GenerateChallanNoAsync(int? devType, CancellationToken ct)
    {
        var prefix = (devType is > 0 ? devType.Value : 1).ToString();
        var marker = prefix + "-";
        var receiptIds = await _db.Challans
            .AsNoTracking()
            .Where(x => x.ReceiptId != null && x.ReceiptId.StartsWith(marker))
            .OrderByDescending(x => x.Id)
            .Select(x => x.ReceiptId!)
            .Take(300)
            .ToListAsync(ct);

        var max = receiptIds
            .Select(x => long.TryParse(x[marker.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(2000000000)
            .Max();

        var next = max < 2000000000 ? 2000000001 : max + 1;
        string candidate;
        do
        {
            candidate = $"{prefix}-{next}";
            next++;
        }
        while (await _db.Challans.AnyAsync(x => x.ReceiptId == candidate || x.RecpNo == candidate, ct));

        return candidate;
    }

    private async Task<List<SelectListItem>> BuildBankOptionsAsync(string? selected, CancellationToken ct)
    {
        var banks = await _db.BankMasters
            .AsNoTracking()
            .Where(x => (x.Status == null || x.Status == 1) && (x.IsActive == null || x.IsActive == 1 || x.IsActive1 == 1))
            .OrderBy(x => x.BankName)
            .ThenBy(x => x.BranchName)
            .Take(200)
            .ToListAsync(ct);

        return banks
            .Select(x =>
            {
                var value = BankValue(x);
                return new SelectListItem(BuildBankName(x) ?? value, value, value == selected);
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToList();
    }

    private async Task<BankMaster?> FindBankAsync(string? bankCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            return null;

        if (int.TryParse(bankCode, out var bankId))
        {
            var byId = await _db.BankMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == bankId, ct);
            if (byId is not null)
                return byId;
        }

        return await _db.BankMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BankBranchCode == bankCode || x.Prefix == bankCode, ct);
    }

    private static ChallanConsumerSummaryViewModel ToConsumerSummary(ConsumerDetailsMaster consumer)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            Address = BuildAddress(consumer),
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            FlatType = consumer.FlatType,
            PipeSize = consumer.PipeSize,
            PlotSize = consumer.PlotSize,
            DevType = consumer.DevType
        };

    private static ChallanConsumerSummaryViewModel ToConsumerSummary(Challan challan)
        => new()
        {
            ConsumerNo = challan.ConsNo ?? string.Empty,
            PropertyNo = BuildPropertyNo(challan.Sec, challan.Blk, challan.FlatNo),
            Address = challan.Address,
            DevType = challan.DevType.HasValue ? Convert.ToInt32(challan.DevType.Value) : null
        };

    private static string? BuildBankName(BankMaster? bank)
    {
        if (bank is null)
            return null;

        return string.Join(" - ", new[] { bank.BankName, bank.BranchName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string BankValue(BankMaster bank)
        => !string.IsNullOrWhiteSpace(bank.BankBranchCode)
            ? bank.BankBranchCode
            : (!string.IsNullOrWhiteSpace(bank.Prefix) ? bank.Prefix : bank.Id.ToString());

    private static string PurposeCode(string? purpose) => purpose switch
    {
        ChallanPurposes.NdcNoDuesFee => "NDC",
        ChallanPurposes.NewConnectionFee => "NEWCONN",
        ChallanPurposes.OtherServiceCharge => "OTHER",
        _ => "BILL"
    };

    private static string PurposeDisplayFromCode(string? code) => code switch
    {
        "NDC" => ChallanPurposes.Display(ChallanPurposes.NdcNoDuesFee),
        "NEWCONN" => ChallanPurposes.Display(ChallanPurposes.NewConnectionFee),
        "OTHER" => ChallanPurposes.Display(ChallanPurposes.OtherServiceCharge),
        _ => ChallanPurposes.Display(ChallanPurposes.ExistingBillDue)
    };

    private static int? PaymentModeCode(string? mode) => mode switch
    {
        ChallanPaymentModes.Cash => 1,
        ChallanPaymentModes.Cheque => 2,
        ChallanPaymentModes.DemandDraft => 3,
        ChallanPaymentModes.BankTransfer => 4,
        ChallanPaymentModes.Upi => 5,
        ChallanPaymentModes.Card => 6,
        _ => null
    };

    private static string ResolveStatus(Challan challan)
    {
        if (challan.Status == "0" || challan.ChallanStatus == 0)
            return ChallanStatuses.Cancelled;

        return challan.PayDate.HasValue ? ChallanStatuses.Paid : ChallanStatuses.PendingPayment;
    }

    private static double ResolvePayableAmount(Challan challan)
    {
        if (challan.PayDate.HasValue)
            return challan.PaidAmt ?? 0;

        var amount = (challan.BillAmt ?? 0)
            + (challan.Surcharge ?? 0)
            + (challan.Arrear ?? 0)
            + (challan.Noc ?? 0)
            + (challan.ConnCharge ?? 0)
            + (challan.PanalityCharges ?? 0)
            + (challan.Secu ?? 0)
            + (challan.TFee ?? 0)
            + (challan.Rmc ?? 0)
            + (challan.Gst ?? 0)
            - (challan.Credit ?? 0);

        return amount > 0
            ? amount
            : challan.PaidAmt ?? 0;
    }

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? BuildAddress(ConsumerDetailsMaster consumer)
        => !string.IsNullOrWhiteSpace(consumer.ConsAddress)
            ? consumer.ConsAddress
            : BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo);

    private string CurrentUsername()
        => User.FindFirstValue("FullName")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.Identity?.Name
            ?? "Admin";

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasAnySearch(params string?[] values)
        => values.Any(x => !string.IsNullOrWhiteSpace(x));

    private sealed record ChallanAmountHeads(
        double BillAmount,
        double Surcharge,
        double Arrear,
        double NocAmount,
        double ConnectionCharge,
        double OtherCharge);
}
