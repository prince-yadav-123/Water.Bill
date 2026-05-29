using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            Transactions = transactions
        });
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
}
