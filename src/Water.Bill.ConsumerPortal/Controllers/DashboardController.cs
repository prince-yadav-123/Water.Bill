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

        var bills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.BillType != null && x.BillCount != null && x.BillCount != 0)
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .Take(5)
            .ToListAsync(ct);

        var latestBill = bills.FirstOrDefault();
        var recentBills = bills
            .Select(MapRecentBill)
            .ToList();

        var recentPayments = await GetRecentPaymentsAsync(consumerNo, ct);
        var model = new ConsumerDashboardViewModel
        {
            ConsumerNo = consumerNo,
            Consumer = consumer is null ? null : MapConsumer(consumer),
            CurrentBill = latestBill is null ? null : MapBill(latestBill),
            RecentBills = recentBills,
            RecentPayments = recentPayments,
            Metrics = BuildMetrics(consumer, latestBill, recentBills, recentPayments)
        };

        model.Alerts = BuildAlerts(model);
        return View(model);
    }

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
            .Take(6)
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

        var offlinePayments = await _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .OrderByDescending(x => x.PayDate ?? x.EntryDate)
            .Take(6)
            .Select(x => new RecentPaymentViewModel
            {
                ReferenceNo = x.RecpNo ?? x.ReceiptId ?? x.ReceiptId1 ?? "Offline payment",
                PaymentDate = x.PayDate ?? x.EntryDate,
                Amount = x.PaidAmt ?? x.BillAmt ?? 0,
                Source = "Offline",
                Status = string.IsNullOrWhiteSpace(x.Status) ? "Recorded" : x.Status,
                BankName = x.BnkCd ?? x.BrNm
            })
            .ToListAsync(ct);

        return onlinePayments
            .Concat(offlinePayments)
            .OrderByDescending(x => x.PaymentDate ?? DateTime.MinValue)
            .Take(3)
            .ToList();
    }

    private static ConsumerSummaryViewModel MapConsumer(ConsumerDetailsMaster consumer)
    {
        var name = string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }
            .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

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
        IReadOnlyList<RecentBillViewModel> recentBills,
        IReadOnlyList<RecentPaymentViewModel> recentPayments)
    {
        var lastPayment = recentPayments
            .OrderByDescending(x => x.PaymentDate ?? DateTime.MinValue)
            .FirstOrDefault();

        return new DashboardMetricViewModel
        {
            TotalDue = latestBill?.TotalBillAmt ?? latestBill?.DueAmt ?? 0,
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
        else
        {
            if (string.IsNullOrWhiteSpace(model.Consumer.MobileNo) || string.IsNullOrWhiteSpace(model.Consumer.Email))
            {
                alerts.Add(new DashboardAlertViewModel
                {
                    Type = "info",
                    Title = "Contact details incomplete",
                    Message = "Please keep your mobile number and email updated for bill and payment alerts."
                });
            }
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
                Message = "Your bill due date has passed. Please review the bill details before payment."
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
        if (!from.HasValue) return $"Till {to:dd MMM yyyy}";
        if (!to.HasValue) return $"From {from:dd MMM yyyy}";
        return $"{from:dd MMM yyyy} to {to:dd MMM yyyy}";
    }

    private static string ResolveBillPaymentStatus(JalPrintBillMaster bill, double totalPayable, double lastPaid)
    {
        if (string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(bill.PaidStatus, "1", StringComparison.OrdinalIgnoreCase))
            return "Paid";

        if (lastPaid > 0 && totalPayable <= 0)
            return "Paid";

        if (lastPaid > 0)
            return "Partially paid";

        return "Pending";
    }

    private static string ResolveTransactionStatus(JalnoidaBankpayMaster master, JalnoidaBankpayTran? tran)
    {
        if (string.Equals(master.Paymentstatus, "Y", StringComparison.OrdinalIgnoreCase))
            return "Success";

        var status = tran?.AuthStatus ?? tran?.Tcntype ?? master.Paymentstatus ?? master.Status;
        return string.IsNullOrWhiteSpace(status) ? "Pending" : status;
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
