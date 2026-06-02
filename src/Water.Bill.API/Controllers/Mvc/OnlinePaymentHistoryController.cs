using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Payments;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
[RequirePermission("Online Payment History.view")]
public class OnlinePaymentHistoryController : Controller
{
    private readonly ApplicationDbContext _db;

    public OnlinePaymentHistoryController(ApplicationDbContext db) => _db = db;

    [HttpGet("/OnlinePaymentHistory")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? jalRefId,
        string? transactionNo,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        ViewData["Title"] = "Online Payment History";
        ViewData["ActiveMenu"] = "Online Payment History";

        var model = new OnlinePaymentHistoryIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo),
            JalRefId = Normalize(jalRefId),
            TransactionNo = Normalize(transactionNo),
            Status = Normalize(status),
            FromDate = fromDate,
            ToDate = toDate,
            HasSearched = HasSearch(search, consumerNo, jalRefId, transactionNo, status, fromDate, toDate)
        };

        if (!model.HasSearched)
            return View(model);

        var query = _db.JalnoidaBankpayMasters.AsNoTracking();
        var transactionRefIds = new List<string>();

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search;
            transactionRefIds.AddRange(await _db.JalnoidaBankpayTrans
                .AsNoTracking()
                .Where(x => x.Jalrefid != null
                    && x.TrxReferenceNo != null
                    && x.TrxReferenceNo.Contains(term))
                .Select(x => x.Jalrefid!)
                .Distinct()
                .Take(300)
                .ToListAsync(ct));

            query = query.Where(x =>
                x.Jalrefid.Contains(term)
                || (x.Consid != null && x.Consid.Contains(term))
                || (x.ConsName != null && x.ConsName.Contains(term))
                || (x.MobileNo != null && x.MobileNo.Contains(term))
                || transactionRefIds.Contains(x.Jalrefid));
        }
        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            query = query.Where(x => x.Consid != null && x.Consid.Contains(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.JalRefId))
            query = query.Where(x => x.Jalrefid.Contains(model.JalRefId));
        if (!string.IsNullOrWhiteSpace(model.TransactionNo))
        {
            var transactionRefs = await _db.JalnoidaBankpayTrans
                .AsNoTracking()
                .Where(x => x.Jalrefid != null
                    && x.TrxReferenceNo != null
                    && x.TrxReferenceNo.Contains(model.TransactionNo))
                .Select(x => x.Jalrefid!)
                .Distinct()
                .Take(300)
                .ToListAsync(ct);

            query = query.Where(x => transactionRefs.Contains(x.Jalrefid));
        }
        if (!string.IsNullOrWhiteSpace(model.Status))
        {
            var statusTransactionRefs = await _db.JalnoidaBankpayTrans
                .AsNoTracking()
                .Where(x => x.Jalrefid != null && x.AuthStatus == model.Status)
                .Select(x => x.Jalrefid!)
                .Distinct()
                .Take(300)
                .ToListAsync(ct);

            query = query.Where(x => x.Paymentstatus == model.Status || statusTransactionRefs.Contains(x.Jalrefid));
        }
        if (fromDate.HasValue)
            query = query.Where(x => x.EntryDate != null && x.EntryDate.Value.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.EntryDate != null && x.EntryDate.Value.Date <= toDate.Value.Date);

        var masters = await query
            .OrderByDescending(x => x.EntryDate)
            .Take(300)
            .ToListAsync(ct);

        var jalRefIds = masters
            .Select(x => x.Jalrefid)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var latestTransactions = jalRefIds.Count == 0
            ? new Dictionary<string, JalnoidaBankpayTran>()
            : (await _db.JalnoidaBankpayTrans
                .AsNoTracking()
                .Where(x => x.Jalrefid != null && jalRefIds.Contains(x.Jalrefid))
                .OrderByDescending(x => x.EntryDate)
                .ToListAsync(ct))
            .GroupBy(x => x.Jalrefid!)
            .ToDictionary(x => x.Key, x => x.First());

        model.Items = masters
            .Select(x =>
            {
                latestTransactions.TryGetValue(x.Jalrefid, out var tran);

                return new OnlinePaymentHistoryRowViewModel
                {
                    JalRefId = x.Jalrefid,
                    ConsumerNo = x.Consid,
                    ConsumerName = x.ConsName,
                    PropertyNo = x.ConsProperty,
                    BillNo = x.BillNo,
                    ChallanNo = x.ChallanNo,
                    Amount = x.Payamount,
                    Bank = x.DepositBank,
                    PaymentStatus = x.Paymentstatus,
                    TransactionStatus = tran?.AuthStatus,
                    TransactionNo = tran?.TrxReferenceNo,
                    EntryDate = x.EntryDate
                };
            })
            .ToList();

        return View(model);
    }

    [HttpGet("/OnlinePaymentHistory/Details/{jalRefId}")]
    public async Task<IActionResult> Details(string jalRefId, CancellationToken ct)
    {
        ViewData["Title"] = "Payment Transaction Details";
        ViewData["ActiveMenu"] = "Online Payment History";

        jalRefId = Normalize(jalRefId) ?? string.Empty;
        var master = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Jalrefid == jalRefId, ct);
        if (master is null)
            return NotFound();

        var transactions = await _db.JalnoidaBankpayTrans
            .AsNoTracking()
            .Where(x => x.Jalrefid == jalRefId)
            .OrderByDescending(x => x.EntryDate)
            .Select(x => new OnlinePaymentTransactionViewModel
            {
                MerchantId = x.MerchantId,
                TransactionReferenceNo = x.TrxReferenceNo,
                BankReferenceNo = x.BankReferenceNo,
                TransactionAmount = x.TxnAmount,
                BankId = x.BankId,
                TransactionDate = x.TxnDate,
                AuthStatus = x.AuthStatus,
                ErrorStatus = x.ErrorStatus,
                ErrorDescription = x.ErrorDescription,
                EntryDate = x.EntryDate
            })
            .ToListAsync(ct);

        var hasSuccess = transactions.Any(x => IsSuccessfulStatus(x.AuthStatus));
        var bill = string.IsNullOrWhiteSpace(master.BillNo)
            ? null
            : await _db.JalPrintBillMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillNo == master.BillNo && x.ConsNo == master.Consid, ct);
        var isBillPaid = bill?.PaidDate.HasValue == true
            || string.Equals(bill?.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase)
            || (bill?.PaidAmt.GetValueOrDefault() > 0);
        var canReconcile = hasSuccess
            && !string.IsNullOrWhiteSpace(master.BillNo)
            && bill is not null
            && !isBillPaid;

        return View(new OnlinePaymentHistoryDetailsViewModel
        {
            JalRefId = master.Jalrefid,
            ConsumerNo = master.Consid,
            ConsumerName = master.ConsName,
            PropertyNo = master.ConsProperty,
            DateFrom = master.DateFrom,
            DateTo = master.DateTo,
            Amount = master.Payamount,
            Email = master.EmailId,
            MobileNo = master.MobileNo,
            DepositBank = master.DepositBank,
            PaymentStatus = master.Paymentstatus,
            BillNo = master.BillNo,
            ChallanNo = master.ChallanNo,
            DueDate = master.DueDate,
            BillNdc = master.BillNdc,
            EntryDate = master.EntryDate,
            Transactions = transactions,
            HasSuccessfulGatewayResponse = hasSuccess,
            IsBillPaid = isBillPaid,
            CanReconcileBillPayment = canReconcile,
            ReconciliationStatus = ResolveReconciliationStatus(master, bill, hasSuccess, isBillPaid)
        });
    }

    [HttpPost("/OnlinePaymentHistory/Reconcile/{jalRefId}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Online Payment History.edit")]
    public async Task<IActionResult> Reconcile(string jalRefId, CancellationToken ct)
    {
        jalRefId = Normalize(jalRefId) ?? string.Empty;
        var strategy = _db.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                var master = await _db.JalnoidaBankpayMasters.FirstOrDefaultAsync(x => x.Jalrefid == jalRefId, ct)
                    ?? throw new InvalidOperationException("Payment reference not found.");

                var latestTransaction = await _db.JalnoidaBankpayTrans
                    .AsNoTracking()
                    .Where(x => x.Jalrefid == jalRefId)
                    .OrderByDescending(x => x.EntryDate)
                    .FirstOrDefaultAsync(ct);

                if (!IsSuccessfulStatus(latestTransaction?.AuthStatus))
                    throw new InvalidOperationException("Only successful gateway transactions can be reconciled.");
                if (string.IsNullOrWhiteSpace(master.BillNo))
                    throw new InvalidOperationException("This payment reference is not linked with a bill number.");

                var bill = await _db.JalPrintBillMasters.FirstOrDefaultAsync(x => x.BillNo == master.BillNo && x.ConsNo == master.Consid, ct)
                    ?? throw new InvalidOperationException("Linked bill was not found.");

                if (bill.PaidDate.HasValue || string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Linked bill is already marked as paid.");

                var amount = master.Payamount ?? ParseDouble(latestTransaction?.TxnAmount) ?? bill.TotalBillAmt ?? bill.DueAmt ?? 0;
                var paidOn = ParseDate(latestTransaction?.TxnDate) ?? latestTransaction?.EntryDate ?? DateTime.Now;
                bill.PaidDate = paidOn;
                bill.PaidAmt = amount;
                bill.PaidStatus = "Y";
                bill.PaymentType ??= 1;
                bill.BankCode = master.DepositBank ?? latestTransaction?.BankId ?? bill.BankCode;
                bill.PaymentMode ??= 1;
                bill.Diff = Math.Round((bill.TotalBillAmt ?? bill.DueAmt ?? amount) - amount, 2);
                bill.UpdateRecord = TrimToLength($"Reconciled {master.Jalrefid}", 20);

                master.Paymentstatus = "Y";
                master.Status = "1";
                _db.Auditlogs.Add(new Auditlog
                {
                    Timestamp = DateTime.Now,
                    UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
                    Username = User.Identity?.Name,
                    Action = 2,
                    Module = "Online Payment History",
                    EntityId = master.Jalrefid,
                    Details = $"Payment reconciled for bill {master.BillNo}.",
                    Success = true,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });

            TempData["SuccessMessage"] = "Payment reconciled and linked bill marked as paid.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { jalRefId });
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasSearch(params object?[] values)
        => values.Any(x => x switch
        {
            null => false,
            string value => !string.IsNullOrWhiteSpace(value),
            _ => true
        });

    private static bool IsSuccessfulStatus(string? value)
        => value?.Trim().Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true
           || value?.Trim().Equals("SUC000", StringComparison.OrdinalIgnoreCase) == true
           || value?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true
           || value?.Trim().Equals("S", StringComparison.OrdinalIgnoreCase) == true;

    private static string ResolveReconciliationStatus(JalnoidaBankpayMaster master, JalPrintBillMaster? bill, bool hasSuccess, bool isBillPaid)
    {
        if (!hasSuccess)
            return "Gateway response is not successful.";
        if (string.IsNullOrWhiteSpace(master.BillNo))
            return "Payment reference is not linked with a bill.";
        if (bill is null)
            return "Linked bill was not found.";
        return isBillPaid ? "Bill is already marked as paid." : "Ready for reconciliation.";
    }

    private static double? ParseDouble(string? value)
        => double.TryParse(value, out var result) ? result : null;

    private static DateTime? ParseDate(string? value)
        => DateTime.TryParse(value, out var result) ? result : null;

    private static string? TrimToLength(string? value, int length)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, length)];
}
