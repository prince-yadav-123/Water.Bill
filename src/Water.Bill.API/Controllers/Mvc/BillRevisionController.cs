using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Billing;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class BillRevisionController : Controller
{
    private readonly ApplicationDbContext _db;

    public BillRevisionController(ApplicationDbContext db) => _db = db;

    [HttpGet("/BillRevision")]
    [RequirePermission("Advanced Bill Revision / Reversal.view")]
    public async Task<IActionResult> Index(string? search, string? consumerNo, string? billNo, CancellationToken ct)
    {
        ViewData["Title"] = "Advanced Bill Revision / Reversal";
        ViewData["ActiveMenu"] = "Advanced Bill Revision / Reversal";
        search = Normalize(search);
        consumerNo = Normalize(consumerNo)?.ToUpperInvariant();
        billNo = Normalize(billNo)?.ToUpperInvariant();
        var query = _db.JalPrintBillMasters.AsNoTracking().Where(x => x.BillNo != null);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.BillNo!.Contains(search) || (x.ConsNo != null && x.ConsNo.Contains(search)));
        if (!string.IsNullOrWhiteSpace(consumerNo))
            query = query.Where(x => x.ConsNo == consumerNo);
        if (!string.IsNullOrWhiteSpace(billNo))
            query = query.Where(x => x.BillNo == billNo);
        var bills = await query.OrderByDescending(x => x.BillDate ?? x.EntryDate).Take(200).ToListAsync(ct);
        var consumerNos = bills.Select(x => x.ConsNo).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        var consumers = await _db.ConsumerDetailsMasters.AsNoTracking().Where(x => consumerNos.Contains(x.ConsNo)).ToDictionaryAsync(x => x.ConsNo, ct);
        return View(new BillRevisionIndexViewModel
        {
            Search = search,
            ConsumerNo = consumerNo,
            BillNo = billNo,
            Bills = bills.Select(x => ToRow(x, consumers)).ToList()
        });
    }

    [HttpGet("/BillRevision/Details")]
    [RequirePermission("Advanced Bill Revision / Reversal.view")]
    public async Task<IActionResult> Details(string billNo, CancellationToken ct)
    {
        ViewData["Title"] = "Bill Revision Details";
        ViewData["ActiveMenu"] = "Advanced Bill Revision / Reversal";
        billNo = Normalize(billNo)?.ToUpperInvariant() ?? string.Empty;
        var bill = await _db.JalPrintBillMasters.AsNoTracking().FirstOrDefaultAsync(x => x.BillNo == billNo, ct);
        if (bill is null)
            return NotFound();
        var consumers = await _db.ConsumerDetailsMasters.AsNoTracking().Where(x => x.ConsNo == bill.ConsNo).ToDictionaryAsync(x => x.ConsNo, ct);
        return View(new BillRevisionDetailsViewModel
        {
            Bill = ToRow(bill, consumers),
            CanReverse = bill.Status == "1" && bill.PaidDate == null && bill.PaidStatus != "Y"
        });
    }

    [HttpPost("/BillRevision/Reverse")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Advanced Bill Revision / Reversal.edit")]
    public async Task<IActionResult> Reverse(BillReverseRequestViewModel model, CancellationToken ct)
    {
        model.BillNo = Normalize(model.BillNo)?.ToUpperInvariant() ?? string.Empty;
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { billNo = model.BillNo });

        var bill = await _db.JalPrintBillMasters.AsNoTracking().FirstOrDefaultAsync(x => x.BillNo == model.BillNo, ct);
        if (bill is null)
            return NotFound();
        if (bill.PaidDate.HasValue || bill.PaidStatus == "Y")
        {
            TempData["ErrorMessage"] = "Paid bills cannot be reversed from this screen.";
            return RedirectToAction(nameof(Details), new { billNo = model.BillNo });
        }
        if (bill.Status != "1")
        {
            TempData["ErrorMessage"] = "Only active generated bills can be reversed.";
            return RedirectToAction(nameof(Details), new { billNo = model.BillNo });
        }

        var user = CurrentUsername();
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            await InsertBillLogSnapshotAsync(bill, $"REV:{Trim(model.Reason, 16)}", user, ct);
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [jal_print_bill_master]
SET [STATUS] = '0',
    [BILL_COUNT] = 0,
    [PRINT_STATUS] = 0,
    [update_record] = 'R',
    [USERID] = {user},
    [Challan_Content] = CONCAT(COALESCE([Challan_Content], ''), ' | Reversed: ', {Trim(model.Reason, 120)})
WHERE [BILL_NO] = {model.BillNo}
  AND ([paid_date] IS NULL)
  AND (COALESCE([PAID_STATUS], 'N') <> 'Y')
  AND [STATUS] = '1';", ct);
            _db.Auditlogs.Add(BuildAudit("Advanced Bill Revision / Reversal", model.BillNo, 3, $"Bill reversed. Reason: {model.Reason}", true));
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        TempData["SuccessMessage"] = "Bill reversed successfully. Snapshot inserted into old bill log table.";
        return RedirectToAction(nameof(Details), new { billNo = model.BillNo });
    }

    private async Task InsertBillLogSnapshotAsync(JalPrintBillMaster b, string updateRecord, string user, CancellationToken ct)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO [jal_print_bill_master_log]
([BILL_NO], [CONS_NO], [BILL_DATE], [BILL_DUE_DATE], [BILL_DATE_FROM], [BILL_DATE_TO], [MIN_RATE], [MIN_TOTAL_AMT],
 [BILL_REBATE_PER], [BILL_REBATE_AMT], [CESS_AMT], [AREAR], [AREAR_TEXT], [AREAR_INT], [AREAR_INT_TEXT], [LAST_BILL_EXTRA],
 [TOTAL_BILL_AMT], [BEFORE_DATE], [AFTER_DATE], [AFTER_DATE_AMT], [Div_type], [STATUS], [ENTRY_DATE], [Due_date],
 [due_amt], [paid_date], [paid_amt], [diff], [PAID_STATUS], [new_record], [update_record], [bill_after_sep_amt], [adv_amt],
 [PRINT_STATUS], [OLD_RATE], [BILL_TYPE], [LAST_PAID_AMT], [BILL_COUNT], [SCHEME_ID], [BILL_PERCENTAGE], [USERID], [DEV_TYPE],
 [PAYMENT_TYPE], [CHALLAN_NO], [BANK_CODE], [Challan_Content], [Rid], [PaymentMode], [Part_Amt])
VALUES
({b.BillNo}, {b.ConsNo}, {b.BillDate}, {b.BillDueDate}, {b.BillDateFrom}, {b.BillDateTo}, {b.MinRate}, {b.MinTotalAmt},
 {b.BillRebatePer}, {b.BillRebateAmt}, {b.CessAmt}, {b.Arear}, {b.ArearText}, {b.ArearInt}, {b.ArearIntText}, {b.LastBillExtra},
 {b.TotalBillAmt}, {b.BeforeDate}, {b.AfterDate}, {b.AfterDateAmt}, {b.DivType}, {b.Status}, {DateTime.Now}, {b.DueDate},
 {b.DueAmt}, {b.PaidDate}, {b.PaidAmt}, {b.Diff}, {b.PaidStatus}, {b.NewRecord}, {updateRecord}, {b.BillAfterSepAmt}, {b.AdvAmt},
 {b.PrintStatus}, {b.OldRate}, {b.BillType}, {b.LastPaidAmt}, {b.BillCount}, {b.SchemeId}, {b.BillPercentage}, {user}, {b.DevType},
 {b.PaymentType}, {b.ChallanNo}, {b.BankCode}, {b.ChallanContent}, {b.Rid}, {b.PaymentMode}, {b.PartAmt});", ct);
    }

    private Auditlog BuildAudit(string module, string entityId, int action, string details, bool success) => new()
    {
        Timestamp = DateTime.Now,
        UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
        Username = User.Identity?.Name,
        Module = module,
        EntityId = entityId,
        Action = action,
        Details = details,
        Success = success,
        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = Request.Headers.UserAgent.ToString()
    };

    private static BillRevisionRowViewModel ToRow(JalPrintBillMaster x, IDictionary<string, ConsumerDetailsMaster> consumers)
    {
        consumers.TryGetValue(x.ConsNo ?? string.Empty, out var consumer);
        return new BillRevisionRowViewModel
        {
            BillNo = x.BillNo,
            ConsumerNo = x.ConsNo,
            ConsumerName = consumer?.ConsNm1,
            PropertyNo = consumer is null ? null : $"{consumer.Sector}/{consumer.BlkNo}-{consumer.FlatNo}".Trim('/', '-'),
            BillDate = x.BillDate,
            BillDateFrom = x.BillDateFrom,
            BillDateTo = x.BillDateTo,
            TotalAmount = x.TotalBillAmt ?? x.DueAmt,
            PaidAmount = x.PaidAmt,
            PaidDate = x.PaidDate,
            Status = x.Status == "0" ? "Reversed" : x.PaidStatus == "Y" ? "Paid" : "Active"
        };
    }

    private string CurrentUsername()
    {
        var value = User.FindFirstValue(AppConstants.Claims.Username) ?? User.Identity?.Name ?? "System";
        return value.Length > 10 ? value[..10] : value;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Trim(string value, int len) => value.Trim()[..Math.Min(value.Trim().Length, len)];
}
