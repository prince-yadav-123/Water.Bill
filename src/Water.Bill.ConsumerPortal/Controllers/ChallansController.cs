using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.ConsumerPortal.Filters;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
[RequirePermission("Consumer Challans.view")]
[Route("Consumer/Challans")]
public class ChallansController : Controller
{
    private readonly ApplicationDbContext _db;

    public ChallansController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string status = "Pending", string? search = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "My Challans";
        ViewData["ActiveMenu"] = "My Challans";

        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction("Login", "Account");

        var normalizedStatus = NormalizeStatus(status);
        var query = _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo);

        var rows = await query
            .OrderByDescending(x => x.EntryDate ?? x.DueDt)
            .Take(200)
            .ToListAsync(ct);

        var items = rows
            .Select(MapListItem)
            .Where(x => normalizedStatus switch
            {
                "Pending" => x.Status == ChallanStatus.PendingPayment,
                "Paid" => x.Status == ChallanStatus.Paid,
                "Cancelled" => x.Status == ChallanStatus.Cancelled,
                _ => true
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            items = items.Where(x =>
                    x.ChallanNo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Purpose.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Amount.ToString("0.##").Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return View(new ConsumerChallanIndexViewModel
        {
            ActiveStatus = normalizedStatus,
            Search = search,
            Challans = items
        });
    }

    [HttpGet("Details/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Challan Details";
        ViewData["ActiveMenu"] = "My Challans";

        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpGet("Print/{id:long}")]
    public async Task<IActionResult> Print(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Print Challan";
        ViewData["ActiveMenu"] = "My Challans";

        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpGet("Pay/{id:long}")]
    public async Task<IActionResult> Pay(long id, int step = 1, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Pay Challan";
        ViewData["ActiveMenu"] = "My Challans";

        var model = await BuildPaymentModelAsync(id, step, paymentMethod, paymentIdentifier, ct);
        if (model is null)
            return NotFound();

        if (!model.CanPay)
        {
            TempData["ErrorMessage"] = "Only pending challans can be paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("Pay", model);
    }

    [HttpGet("Pay/{id:long}/Confirm")]
    public async Task<IActionResult> Confirm(long id, string? paymentMethod = null, string? paymentIdentifier = null, CancellationToken ct = default)
        => await Pay(id, 3, paymentMethod, paymentIdentifier, ct);

    [HttpPost("Pay/{id:long}/Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(long id, string? paymentMethod, string? paymentIdentifier, CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction("Login", "Account");

        paymentMethod = NormalizePaymentMethod(paymentMethod);
        paymentIdentifier = Normalize(paymentIdentifier);

        var challan = await _db.Challans.FirstOrDefaultAsync(x => x.Id == id && x.ConsNo == consumerNo, ct);
        if (challan is null)
            return NotFound();

        var currentStatus = ResolveStatus(challan);
        if (currentStatus != ChallanStatus.PendingPayment)
        {
            TempData["ErrorMessage"] = "This challan is not pending for payment.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var amount = ResolvePayableAmount(challan);
        if (amount <= 0)
        {
            TempData["ErrorMessage"] = "Payable amount is not available for this challan.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var now = DateTime.Now;
        var referenceNo = string.IsNullOrWhiteSpace(paymentIdentifier)
            ? $"TEST-{now:yyyyMMddHHmmss}-{challan.Id}"
            : paymentIdentifier;

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            challan.PayDate = now;
            challan.PaidAmt = amount;
            challan.Status = "1";
            challan.ChallanStatus = 1;
            challan.Upd = "PAID";
            challan.BankId = challan.BankId ?? "ONLINE";
            challan.BnkCd = challan.BnkCd ?? "ONLINE";
            challan.BrNm = challan.BrNm ?? "Simulated Online Payment";

            _db.ChallanPaymentHistories.Add(new ChallanPaymentHistory
            {
                ChallanId = challan.Id,
                ChallanNo = challan.RecpNo ?? challan.ReceiptId,
                ConsumerNo = challan.ConsNo,
                SourceBillNo = string.Equals(challan.RevBilFr, "BILL", StringComparison.OrdinalIgnoreCase) ? challan.BillId : null,
                Amount = amount,
                PaymentDate = now,
                PaymentMode = paymentMethod,
                BankCode = challan.BnkCd,
                BankName = challan.BrNm,
                TransactionReferenceNo = referenceNo,
                Remarks = "Simulated consumer challan payment for current phase.",
                PostedByUserId = ResolveConsumerUserId(),
                PostedByName = User.Identity?.Name ?? consumerNo,
                PostedOn = now
            });

            _db.ChallanHistories.Add(new ChallanHistory
            {
                ChallanId = challan.Id,
                ChallanNo = challan.RecpNo ?? challan.ReceiptId,
                ConsumerNo = challan.ConsNo,
                FromStatus = currentStatus,
                ToStatus = ChallanStatus.Paid,
                Action = "ConsumerPayment",
                Remarks = $"Consumer simulated payment completed. Ref: {referenceNo}",
                ActionByUserId = ResolveConsumerUserId(),
                ActionByName = User.Identity?.Name ?? consumerNo,
                ActionOn = now
            });

            await _db.SaveChangesAsync(ct);

            if (string.Equals(challan.RevBilFr, "BILL", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(challan.BillId)
                && !string.IsNullOrWhiteSpace(challan.ConsNo))
            {
                await MarkSourceBillPaidAsync(challan, amount, now, paymentMethod, ct);
            }

            await tx.CommitAsync(ct);
        });

        TempData["SuccessMessage"] = $"Challan {challan.RecpNo ?? challan.ReceiptId} paid successfully. Reference: {referenceNo}";
        return RedirectToAction(nameof(Success), new { id });
    }

    [HttpGet("Success/{id:long}")]
    public async Task<IActionResult> Success(long id, CancellationToken ct)
    {
        ViewData["Title"] = "Payment Successful";
        ViewData["ActiveMenu"] = "My Challans";

        var model = await BuildDetailsAsync(id, ct);
        if (model is null)
            return NotFound();

        return View(model);
    }

    private async Task<ConsumerChallanDetailsViewModel?> BuildDetailsAsync(long id, CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return null;

        var challan = await _db.Challans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.ConsNo == consumerNo, ct);
        if (challan is null)
            return null;

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        var payments = await _db.ChallanPaymentHistories
            .AsNoTracking()
            .Where(x => x.ChallanId == challan.Id && !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate)
            .Select(x => new ConsumerChallanPaymentHistoryItemViewModel
            {
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                BankName = x.BankName ?? x.BankCode,
                TransactionReferenceNo = x.TransactionReferenceNo
            })
            .ToListAsync(ct);

        return MapDetails(challan, consumer, payments);
    }

    private async Task<ConsumerChallanPaymentViewModel?> BuildPaymentModelAsync(long id, int step, string? paymentMethod, string? paymentIdentifier, CancellationToken ct)
    {
        var details = await BuildDetailsAsync(id, ct);
        if (details is null)
            return null;

        return new ConsumerChallanPaymentViewModel
        {
            Id = details.Id,
            ChallanNo = details.ChallanNo,
            ConsumerNo = details.ConsumerNo,
            ConsumerName = details.ConsumerName,
            MobileNo = details.MobileNo,
            PropertyNo = details.PropertyNo,
            Address = details.Address,
            Purpose = details.Purpose,
            SourceBillNo = details.SourceBillNo,
            Amount = details.Amount,
            BillAmount = details.BillAmount,
            Surcharge = details.Surcharge,
            NdcAmount = details.NdcAmount,
            ConnectionCharge = details.ConnectionCharge,
            OtherCharge = details.OtherCharge,
            BillPeriodFrom = details.BillPeriodFrom,
            BillPeriodTo = details.BillPeriodTo,
            DueDate = details.DueDate,
            GeneratedDate = details.GeneratedDate,
            PaidDate = details.PaidDate,
            Status = details.Status,
            BankCode = details.BankCode,
            BankName = details.BankName,
            Payments = details.Payments,
            Step = Math.Clamp(step, 1, 3),
            PaymentMethod = NormalizePaymentMethod(paymentMethod),
            PaymentIdentifier = paymentIdentifier,
            ConvenienceFee = 0
        };
    }

    private async Task MarkSourceBillPaidAsync(Challan challan, double amount, DateTime paymentDate, string? paymentMethod, CancellationToken ct)
    {
        var consumerNo = challan.ConsNo ?? string.Empty;
        var billNo = challan.BillId ?? string.Empty;
        var challanNo = challan.RecpNo ?? challan.ReceiptId ?? string.Empty;
        var bankCode = challan.BnkCd;

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [jal_print_bill_master]
SET [paid_date] = {paymentDate},
    [paid_amt] = {amount},
    [PAID_STATUS] = 'Y',
    [CHALLAN_NO] = {challanNo},
    [BANK_CODE] = {bankCode}
WHERE [CONS_NO] = {consumerNo}
  AND [BILL_NO] = {billNo}
  AND [STATUS] = '1';", ct);
    }

    private static ConsumerChallanListItemViewModel MapListItem(Challan challan)
        => new()
        {
            Id = challan.Id,
            ChallanNo = BuildChallanNo(challan),
            ConsumerNo = challan.ConsNo ?? string.Empty,
            Purpose = PurposeDisplayFromCode(challan.RevBilFr),
            Amount = ResolvePayableAmount(challan),
            GeneratedDate = challan.EntryDate,
            DueDate = challan.DueDt,
            PaidDate = challan.PayDate,
            Status = ResolveStatus(challan)
        };

    private static ConsumerChallanDetailsViewModel MapDetails(
        Challan challan,
        ConsumerDetailsMaster? consumer,
        IReadOnlyList<ConsumerChallanPaymentHistoryItemViewModel> payments)
        => new()
        {
            Id = challan.Id,
            ChallanNo = BuildChallanNo(challan),
            ConsumerNo = challan.ConsNo ?? string.Empty,
            ConsumerName = consumer?.ConsNm1 ?? challan.DeposeterName,
            MobileNo = consumer?.MobNo,
            PropertyNo = BuildPropertyNo(challan.Sec ?? consumer?.Sector, challan.Blk ?? consumer?.BlkNo, challan.FlatNo ?? consumer?.FlatNo),
            Address = challan.Address ?? consumer?.ConsAddress,
            Purpose = PurposeDisplayFromCode(challan.RevBilFr),
            SourceBillNo = challan.BillId,
            Amount = ResolvePayableAmount(challan),
            BillAmount = challan.BillAmt ?? 0,
            Surcharge = challan.Surcharge ?? 0,
            NdcAmount = challan.Noc ?? 0,
            ConnectionCharge = challan.ConnCharge ?? 0,
            OtherCharge = challan.PanalityCharges ?? 0,
            BillPeriodFrom = challan.BlPerFr,
            BillPeriodTo = challan.BlPerTo,
            DueDate = challan.DueDt,
            GeneratedDate = challan.EntryDate,
            PaidDate = challan.PayDate,
            Status = ResolveStatus(challan),
            BankCode = challan.BnkCd ?? challan.BankId,
            BankName = challan.BrNm,
            Payments = payments,
            PaymentMode = payments.FirstOrDefault()?.PaymentMode,
            TransactionReferenceNo = payments.FirstOrDefault()?.TransactionReferenceNo
        };

    private string? ResolveConsumerNo()
    {
        var claimConsumerNo = User.FindFirstValue("ConsumerNo")?.Trim();
        if (!string.IsNullOrWhiteSpace(claimConsumerNo))
            return claimConsumerNo.ToUpperInvariant();

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim();
        return !string.IsNullOrWhiteSpace(nameIdentifier) && !int.TryParse(nameIdentifier, out _)
            ? nameIdentifier.ToUpperInvariant()
            : null;
    }

    private int? ResolveConsumerUserId()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    private static string ResolveStatus(Challan challan)
    {
        if (challan.Status == "0" || challan.ChallanStatus == 0)
            return ChallanStatus.Cancelled;

        return challan.PayDate.HasValue ? ChallanStatus.Paid : ChallanStatus.PendingPayment;
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

    private static string NormalizeStatus(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "PAID" => "Paid",
        "CANCELLED" => "Cancelled",
        "ALL" => "All",
        _ => "Pending"
    };

    private static string NormalizePaymentMethod(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "AX" or "AXIS" or "BILLDESK" => "AX",
        "IC" or "ICICI" => "IC",
        "HD" or "HDFC" => "HD",
        "PT" or "PAYTM" => "PT",
        "CARD" => "CARD",
        "NETBANKING" => "NETBANKING",
        "WALLET" => "WALLET",
        "UPI" => "UPI",
        _ => "UPI"
    };

    private static int? PaymentModeCode(string? mode) => mode switch
    {
        "CASH" => 1,
        "CHEQUE" => 2,
        "DEMANDDRAFT" => 3,
        "BANKTRANSFER" => 4,
        "UPI" => 5,
        "CARD" => 6,
        "AX" or "IC" or "HD" or "PT" or "NETBANKING" or "WALLET" => 5,
        _ => null
    };

    private static string PurposeDisplayFromCode(string? code) => code?.Trim().ToUpperInvariant() switch
    {
        "NDC" => "NDC / No Dues fee",
        "NEWCONN" => "New Connection fee",
        "OTHER" => "Other service charge",
        "BILL" => "Existing consumer bill / due",
        _ => "Challan"
    };

    private static string BuildChallanNo(Challan challan)
        => challan.RecpNo ?? challan.ReceiptId ?? challan.ReceiptId1 ?? $"CH-{challan.Id}";

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static class ChallanStatus
    {
        public const string PendingPayment = "PendingPayment";
        public const string Paid = "Paid";
        public const string Cancelled = "Cancelled";
    }
}
