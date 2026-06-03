using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.ConsumerPortal.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme, Roles = AppConstants.Roles.Consumer)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    [HttpGet("/Consumer/Dashboard")]
    [HttpGet("/Dashboard")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";
        ViewData["ActiveMenu"] = "Dashboard";

        var consumerNo = await ResolveConsumerNoAsync(ct);
        if (string.IsNullOrWhiteSpace(consumerNo))
        {
            return View(new ConsumerDashboardViewModel
            {
                Alerts =
                [
                    new DashboardAlertViewModel
                    {
                        Type = "warning",
                        Title = "Consumer account not linked",
                        Message = "We could not identify a consumer number for this login. Please contact support."
                    }
                ]
            });
        }

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        // Top 5 bills for display
        var bills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.BillType != null && x.BillCount != null && x.BillCount != 0)
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .Take(5)
            .ToListAsync(ct);

        var latestBill = bills.FirstOrDefault();
        var recentBills = bills.Select(MapRecentBill).ToList();

        // Sum ALL unpaid bills for accurate total due
        var totalUnpaidDue = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0
                && (x.TotalBillAmt ?? 0) > 0
                && x.PaidStatus != "Y"
                && x.PaidStatus != "1")
            .SumAsync(x => (double?)x.TotalBillAmt, ct) ?? 0;

        var unpaidBillCount = await _db.JalPrintBillMasters
            .AsNoTracking()
            .CountAsync(x => x.ConsNo == consumerNo
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0
                && (x.TotalBillAmt ?? 0) > 0
                && x.PaidStatus != "Y"
                && x.PaidStatus != "1", ct);

        var recentPayments = await GetRecentPaymentsAsync(consumerNo, ct);
        var latestChallans = await GetLatestChallansAsync(consumerNo, ct);
        var querySummary = await GetQuerySummaryAsync(consumerNo, ct);
        var billingTrend = await GetBillingTrendAsync(consumerNo, ct);

        var model = new ConsumerDashboardViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = consumer is null ? null : MapConsumer(consumer),
            CurrentBill = latestBill is null ? null : MapBill(latestBill),
            TotalUnpaidDue = totalUnpaidDue,
            UnpaidBillCount = unpaidBillCount,
            RecentBills = recentBills,
            RecentPayments = recentPayments,
            LatestChallans = latestChallans,
            QuerySummary = querySummary,
            BillingTrend = billingTrend,
            Metrics = BuildMetrics(consumer, latestBill, totalUnpaidDue, recentPayments, recentBills)
        };

        model.Alerts = BuildAlerts(model);
        return View(model);
    }

    // ─── Data fetchers ─────────────────────────────────────────────────────

    private async Task<string?> ResolveConsumerNoAsync(CancellationToken ct)
    {
        var claimConsumerNo = User.FindFirstValue("ConsumerNo")?.Trim();
        if (!string.IsNullOrWhiteSpace(claimConsumerNo))
            return claimConsumerNo.ToUpperInvariant();

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim();
        if (!string.IsNullOrWhiteSpace(nameIdentifier) && !int.TryParse(nameIdentifier, out _))
            return nameIdentifier.ToUpperInvariant();

        var email = User.FindFirstValue(ClaimTypes.Email)?.Trim();
        var username = User.Identity?.Name?.Trim();

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x =>
                (!string.IsNullOrWhiteSpace(email) && x.EmailId == email) ||
                (!string.IsNullOrWhiteSpace(username) && (x.ConsNo == username || x.MobNo == username || x.EmailId == username)))
            .OrderBy(x => x.ConsNo)
            .FirstOrDefaultAsync(ct);

        return consumer?.ConsNo?.Trim().ToUpperInvariant();
    }

    private async Task<IReadOnlyList<RecentPaymentViewModel>> GetRecentPaymentsAsync(string consumerNo, CancellationToken ct)
    {
        var onlineMasters = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .Where(x => x.Consid == consumerNo)
            .OrderByDescending(x => x.EntryDate)
            .Take(8)
            .ToListAsync(ct);

        var refs = onlineMasters.Select(x => x.Jalrefid).ToList();
        var onlineTransactions = await _db.JalnoidaBankpayTrans
            .AsNoTracking()
            .Where(x => x.Jalrefid != null && refs.Contains(x.Jalrefid))
            .ToListAsync(ct);

        var onlinePayments = onlineMasters.Select(master =>
        {
            var tran = onlineTransactions.FirstOrDefault(x => x.Jalrefid == master.Jalrefid);
            return new RecentPaymentViewModel
            {
                ReferenceNo = master.Jalrefid,
                PaymentDate = tran?.EntryDate ?? master.EntryDate,
                Amount = master.Payamount ?? ParseAmount(tran?.TxnAmount),
                Source = "Online",
                Status = ResolveTransactionStatus(master, tran),
                BankName = master.DepositBank
            };
        });

        var postedChallanPayments = await _db.ChallanPaymentHistories
            .AsNoTracking()
            .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate)
            .Take(8)
            .Select(x => new RecentPaymentViewModel
            {
                ReferenceNo = x.TransactionReferenceNo ?? x.ChallanNo ?? "Challan payment",
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                Source = "Challan",
                Status = "Paid",
                BankName = x.BankName ?? x.BankCode
            })
            .ToListAsync(ct);

        var postedChallanNos = (await _db.ChallanPaymentHistories
            .AsNoTracking()
            .Where(x => x.ConsumerNo == consumerNo && !x.IsDeleted && x.ChallanNo != null)
            .Select(x => x.ChallanNo)
            .ToListAsync(ct))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var legacyOfflinePayments = await _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.PayDate != null)
            .OrderByDescending(x => x.PayDate)
            .Take(8)
            .Select(x => new RecentPaymentViewModel
            {
                ReferenceNo = x.RecpNo ?? x.ReceiptId ?? x.ReceiptId1 ?? "Offline payment",
                PaymentDate = x.PayDate,
                Amount = x.PaidAmt ?? x.BillAmt ?? 0,
                Source = "Offline",
                Status = "Paid",
                BankName = x.BnkCd ?? x.BrNm
            })
            .ToListAsync(ct);

        return onlinePayments
            .Concat(postedChallanPayments)
            .Concat(legacyOfflinePayments.Where(x => string.IsNullOrWhiteSpace(x.ReferenceNo) || !postedChallanNos.Contains(x.ReferenceNo)))
            .OrderByDescending(x => x.PaymentDate ?? DateTime.MinValue)
            .Take(5)
            .ToList();
    }

    private async Task<IReadOnlyList<ConsumerChallanRowViewModel>> GetLatestChallansAsync(string consumerNo, CancellationToken ct)
    {
        var rows = await _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .OrderByDescending(x => x.EntryDate)
            .Take(5)
            .ToListAsync(ct);

        return rows.Select(x => new ConsumerChallanRowViewModel
        {
            ChallanId = x.Id,
            ChallanNo = x.ReceiptId1 ?? x.ReceiptId ?? x.RecpNo ?? $"CH-{x.Id}",
            Purpose = ResolveChallanPurpose(x),
            Amount = x.BillAmt ?? x.Noc ?? x.TFee ?? x.ConnCharge ?? x.Disconnection ?? x.Reconnection ?? 0,
            Status = ResolveChallanStatus(x),
            GeneratedOn = x.EntryDate
        }).ToList();
    }

    private async Task<ConsumerQuerySummaryViewModel> GetQuerySummaryAsync(string consumerNo, CancellationToken ct)
    {
        var hasTable = await TableExistsAsync("consumersupportqueries", ct);
        if (!hasTable) return new ConsumerQuerySummaryViewModel();

        var stats = await _db.ConsumerSupportQueries
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ConsumerNo == consumerNo)
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(ct);

        return new ConsumerQuerySummaryViewModel
        {
            Open = stats.FirstOrDefault(x => x.Status == "Open")?.Count ?? 0,
            InProgress = stats.FirstOrDefault(x => x.Status == "InProgress")?.Count ?? 0,
            Resolved = stats.FirstOrDefault(x => x.Status == "Resolved")?.Count ?? 0,
            Closed = stats.FirstOrDefault(x => x.Status == "Closed")?.Count ?? 0
        };
    }

    private async Task<IReadOnlyList<BillingYearTrendViewModel>> GetBillingTrendAsync(string consumerNo, CancellationToken ct)
    {
        var startYear = DateTime.Today.Year - 4;
        var rows = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0
                && x.BillDate.HasValue
                && x.BillDate.Value.Year >= startYear)
            .GroupBy(x => x.BillDate!.Value.Year)
            .Select(x => new
            {
                Year = x.Key,
                Billed = (double?)x.Sum(b => b.TotalBillAmt) ?? 0,
                Paid = (double?)x.Sum(b => b.PaidAmt) ?? 0
            })
            .OrderBy(x => x.Year)
            .ToListAsync(ct);

        return Enumerable.Range(startYear, 5).Select(year =>
        {
            var row = rows.FirstOrDefault(x => x.Year == year);
            return new BillingYearTrendViewModel
            {
                Year = year,
                BilledAmount = row?.Billed ?? 0,
                PaidAmount = row?.Paid ?? 0
            };
        }).ToList();
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND LOWER(table_name) = LOWER(@tableName);";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    // ─── Mappers ────────────────────────────────────────────────────────────

    private static ConsumerSummaryViewModel MapConsumer(ConsumerDetailsMaster consumer)
    {
        var name = (consumer.ConsNm1 ?? string.Empty).Trim();
        return new ConsumerSummaryViewModel
        {
            ConsumerNo = consumer.ConsNo,
            Name = string.IsNullOrWhiteSpace(name) ? "Consumer" : name,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            Sector = consumer.Sector,
            Block = consumer.BlkNo,
            FlatNo = consumer.FlatNo,
            PlotNo = consumer.PlotMapId,
            Address = consumer.ConsAddress,
            Category = ResolveConsumerCategory(consumer.ConsCtg),
            ConnectionType = ResolveConnectionType(consumer.ConTp),
            PropertySummary = BuildPropertySummary(consumer)
        };
    }

    private static CurrentBillSummaryViewModel MapBill(JalPrintBillMaster bill)
    {
        var dueDate = bill.BillDueDate ?? bill.DueDate;
        var totalPayable = bill.TotalBillAmt ?? bill.DueAmt ?? 0;
        var lastPaid = bill.LastPaidAmt ?? bill.PaidAmt ?? 0;
        var now = DateTime.Today;
        return new CurrentBillSummaryViewModel
        {
            BillNo = bill.BillNo,
            ChallanNo = bill.ChallanNo,
            BillDate = bill.BillDate,
            BillDateFrom = bill.BillDateFrom,
            BillDateTo = bill.BillDateTo,
            DueDate = dueDate,
            CurrentAmount = bill.MinTotalAmt ?? bill.TotalBillAmt ?? 0,
            ArrearAmount = (bill.Arear ?? 0) + (bill.ArearInt ?? 0),
            TotalPayableAmount = totalPayable,
            LastPaidAmount = lastPaid,
            PaymentStatus = ResolveBillPaymentStatus(bill, totalPayable, lastPaid),
            BillPeriod = FormatBillPeriod(bill.BillDateFrom, bill.BillDateTo),
            IsOverdue = dueDate.HasValue && dueDate.Value.Date < now && totalPayable > 0,
            IsDueSoon = dueDate.HasValue && dueDate.Value.Date >= now && dueDate.Value.Date <= now.AddDays(7) && totalPayable > 0
        };
    }

    private static RecentBillViewModel MapRecentBill(JalPrintBillMaster bill)
    {
        var totalPayable = bill.TotalBillAmt ?? bill.DueAmt ?? 0;
        var lastPaid = bill.LastPaidAmt ?? bill.PaidAmt ?? 0;
        return new RecentBillViewModel
        {
            BillNo = string.IsNullOrWhiteSpace(bill.BillNo) ? "Bill" : bill.BillNo,
            BillPeriod = FormatBillPeriod(bill.BillDateFrom, bill.BillDateTo),
            DueDate = bill.BillDueDate ?? bill.DueDate,
            Amount = totalPayable,
            Status = ResolveBillPaymentStatus(bill, totalPayable, lastPaid)
        };
    }

    private static DashboardMetricViewModel BuildMetrics(
        ConsumerDetailsMaster? consumer,
        JalPrintBillMaster? latestBill,
        double totalUnpaidDue,
        IReadOnlyList<RecentPaymentViewModel> recentPayments,
        IReadOnlyList<RecentBillViewModel> recentBills)
    {
        var lastPayment = recentPayments.OrderByDescending(x => x.PaymentDate ?? DateTime.MinValue).FirstOrDefault();
        return new DashboardMetricViewModel
        {
            TotalDue = totalUnpaidDue,
            LastPaymentAmount = lastPayment?.Amount ?? latestBill?.LastPaidAmt ?? latestBill?.PaidAmt ?? 0,
            LastPaymentDate = lastPayment?.PaymentDate ?? latestBill?.PaidDate,
            ServiceStatus = ResolveServiceStatus(consumer?.Status),
            RecentBillCount = recentBills.Count
        };
    }

    private static IReadOnlyList<DashboardAlertViewModel> BuildAlerts(ConsumerDashboardViewModel model)
    {
        var alerts = new List<DashboardAlertViewModel>();

        if (model.Consumer is null)
        {
            alerts.Add(new DashboardAlertViewModel
            {
                Type = "warning",
                Title = "Consumer profile not found",
                Message = "No consumer profile details were found for this login."
            });
        }
        else if (string.IsNullOrWhiteSpace(model.Consumer.MobileNo) || string.IsNullOrWhiteSpace(model.Consumer.Email))
        {
            alerts.Add(new DashboardAlertViewModel
            {
                Type = "info",
                Title = "Contact details incomplete",
                Message = "Please keep your mobile number and email updated for bill and payment alerts."
            });
        }

        if (model.CurrentBill is null)
        {
            alerts.Add(new DashboardAlertViewModel
            {
                Type = "info",
                Title = "No current bill found",
                Message = "There is no current bill available for this consumer account."
            });
        }
        else if (model.CurrentBill.IsOverdue)
        {
            alerts.Add(new DashboardAlertViewModel
            {
                Type = "danger",
                Title = "Bill overdue",
                Message = "Your bill due date has passed. Please review and pay your dues immediately."
            });
        }
        else if (model.CurrentBill.IsDueSoon)
        {
            alerts.Add(new DashboardAlertViewModel
            {
                Type = "warning",
                Title = "Bill due soon",
                Message = "Your current bill is due within the next 7 days."
            });
        }

        return alerts;
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static string BuildPropertySummary(ConsumerDetailsMaster consumer)
    {
        var parts = new[] { consumer.Sector, consumer.BlkNo, consumer.FlatNo }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var summary = string.Join(" / ", parts);
        return string.IsNullOrWhiteSpace(summary) ? consumer.ConsAddress ?? string.Empty : summary;
    }

    private static string? FormatBillPeriod(DateTime? from, DateTime? to)
    {
        if (!from.HasValue && !to.HasValue) return null;
        if (!from.HasValue) return $"Till {to:MMM yyyy}";
        if (!to.HasValue) return $"From {from:MMM yyyy}";
        return $"{from:MMM yyyy} – {to:MMM yyyy}";
    }

    private static string ResolveBillPaymentStatus(JalPrintBillMaster bill, double totalPayable, double lastPaid)
    {
        if (string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bill.PaidStatus, "1", StringComparison.OrdinalIgnoreCase))
            return "Paid";
        if (lastPaid > 0 && totalPayable <= 0) return "Paid";
        if (lastPaid > 0) return "Partial";
        return "Pending";
    }

    private static string ResolveTransactionStatus(JalnoidaBankpayMaster master, JalnoidaBankpayTran? tran)
    {
        if (string.Equals(master.Paymentstatus, "Y", StringComparison.OrdinalIgnoreCase)) return "Success";
        var status = tran?.AuthStatus ?? tran?.Tcntype ?? master.Paymentstatus ?? master.Status;
        return string.IsNullOrWhiteSpace(status) ? "Pending" : status;
    }

    private static string ResolveChallanPurpose(Challan x)
    {
        if ((x.BillAmt ?? 0d) > 0) return "Bill Due";
        if ((x.Noc ?? 0d) > 0) return "NDC Fee";
        if ((x.TFee ?? 0d) > 0) return "Transfer Fee";
        if ((x.ConnCharge ?? 0d) > 0) return "Connection";
        if ((x.Disconnection ?? 0d) > 0) return "Disconnection";
        if ((x.Reconnection ?? 0d) > 0) return "Reconnection";
        return "Service Charge";
    }

    private static string ResolveChallanStatus(Challan x)
    {
        if (x.PayDate.HasValue || (x.PaidAmt ?? 0d) > 0 || string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            return "Paid";
        if (!string.IsNullOrWhiteSpace(x.Status)) return x.Status;
        return "Generated";
    }

    private static string ResolveConnectionType(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "I" => "Institutional",
        "C" => "Commercial",
        "R" or "S" => "Residential",
        "T" => "Industrial",
        "V" => "Village",
        "G" => "Group Housing",
        "H" => "Housing",
        null or "" => "Not available",
        _ => value!
    };

    private static string ResolveConsumerCategory(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "R" => "Regular",
        "T" => "Temporary",
        null or "" => "Not available",
        _ => value!
    };

    private static string ResolveServiceStatus(int? status) => status switch
    {
        null => "Not available",
        0 => "Inactive",
        1 => "Active",
        _ => "Active"
    };

    private static double ParseAmount(string? value)
        => double.TryParse(value, out var amount) ? amount : 0;
}
