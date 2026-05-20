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
[Route("Consumer/Bills")]
public class BillsController : Controller
{
    private readonly ApplicationDbContext _db;

    public BillsController(ApplicationDbContext db) => _db = db;

    [HttpGet("History")]
    public async Task<IActionResult> History(
        string status = "All",
        string? search = null,
        string? consumerNo = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Bill History";

        var primaryConsumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(primaryConsumerNo))
        {
            return View(new ConsumerBillHistoryViewModel());
        }

        var consumers = await GetLinkedConsumersAsync(primaryConsumerNo, ct);
        var allowedConsumerNos = consumers.Select(x => x.ConsNo).ToList();
        var selectedConsumerNo = !string.IsNullOrWhiteSpace(consumerNo)
                && allowedConsumerNos.Contains(consumerNo.Trim(), StringComparer.OrdinalIgnoreCase)
            ? consumerNo.Trim().ToUpperInvariant()
            : null;

        var query = _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo != null
                && allowedConsumerNos.Contains(x.ConsNo)
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0);

        if (!string.IsNullOrWhiteSpace(selectedConsumerNo))
            query = query.Where(x => x.ConsNo == selectedConsumerNo);

        var bills = await query
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .Take(100)
            .ToListAsync(ct);

        var consumerLookup = consumers.ToDictionary(x => x.ConsNo, StringComparer.OrdinalIgnoreCase);
        var items = bills.Select(bill =>
            {
                consumerLookup.TryGetValue(bill.ConsNo ?? string.Empty, out var consumer);
                return MapHistoryBill(bill, consumer);
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            items = items.Where(x =>
                    x.BillNo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.ConsumerNo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (x.BillPeriod?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    x.Amount.ToString("0.##").Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var normalizedStatus = NormalizeHistoryStatus(status);
        if (normalizedStatus == "Paid")
            items = items.Where(x => x.IsPaid).ToList();
        else if (normalizedStatus == "Due")
            items = items.Where(x => !x.IsPaid).ToList();

        return View(new ConsumerBillHistoryViewModel
        {
            ActiveStatus = normalizedStatus,
            Search = search,
            SelectedConsumerNo = selectedConsumerNo,
            Connections = consumers.Select(x => new ConsumerConnectionFilterViewModel
            {
                ConsumerNo = x.ConsNo,
                Title = BuildPropertySummary(x)
            }).ToList(),
            Bills = items
        });
    }

    [HttpGet("Current")]
    public async Task<IActionResult> Current(CancellationToken ct)
    {
        ViewData["Title"] = "Current Bill";

        var model = await BuildCurrentBillModelAsync(ct);
        return View(model);
    }

    [HttpGet("Pay")]
    public async Task<IActionResult> Pay(
        int step = 1,
        string? amountOption = null,
        double? partialAmount = null,
        string? paymentMethod = null,
        string? paymentIdentifier = null,
        bool saveMethod = true,
        bool setupAutoPay = false,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Pay Bill";

        var model = await BuildPaymentModelAsync(
            step,
            amountOption,
            partialAmount,
            paymentMethod,
            paymentIdentifier,
            saveMethod,
            setupAutoPay,
            ct);

        return View(model);
    }

    [HttpGet("Pay/Confirm")]
    public async Task<IActionResult> Confirm(
        string? amountOption = null,
        double? partialAmount = null,
        string? paymentMethod = null,
        string? paymentIdentifier = null,
        bool saveMethod = true,
        bool setupAutoPay = false,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Confirm Payment";

        var model = await BuildPaymentModelAsync(
            3,
            amountOption,
            partialAmount,
            paymentMethod,
            paymentIdentifier,
            saveMethod,
            setupAutoPay,
            ct);

        return View("Pay", model);
    }

    [HttpPost("Pay/Confirm")]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmPayment(
        string? amountOption,
        double? partialAmount,
        string? paymentMethod,
        string? paymentIdentifier,
        bool saveMethod = true,
        bool setupAutoPay = false)
    {
        TempData["SuccessMessage"] = "Payment gateway integration is not enabled yet. This screen is ready for the next payment integration phase.";

        return RedirectToAction(nameof(Confirm), new
        {
            amountOption,
            partialAmount,
            paymentMethod,
            paymentIdentifier,
            saveMethod,
            setupAutoPay
        });
    }

    [HttpGet("Print/{billNo}")]
    public async Task<IActionResult> Print(string billNo, CancellationToken ct)
    {
        ViewData["Title"] = "Print Bill";

        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
            return RedirectToAction("Login", "Account");

        var linkedConsumers = await GetLinkedConsumersAsync(consumerNo, ct);
        var linkedConsumerNos = linkedConsumers.Select(x => x.ConsNo).ToList();
        var bill = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo != null
                && linkedConsumerNos.Contains(x.ConsNo)
                && x.BillNo == billNo
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0)
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .FirstOrDefaultAsync(ct);

        if (bill is null)
        {
            return View(new ConsumerBillViewModel
            {
                Consumer = linkedConsumers.FirstOrDefault() is null ? null : MapConsumer(linkedConsumers.First()),
                Message = "Bill was not found for this consumer account."
            });
        }

        var consumer = linkedConsumers.FirstOrDefault(x => x.ConsNo == bill.ConsNo) ?? linkedConsumers.FirstOrDefault();

        return View(new ConsumerBillViewModel
        {
            Consumer = consumer is null ? null : MapConsumer(consumer),
            Bill = MapBill(bill)
        });
    }

    private async Task<ConsumerBillViewModel> BuildCurrentBillModelAsync(CancellationToken ct)
    {
        var consumerNo = ResolveConsumerNo();
        if (string.IsNullOrWhiteSpace(consumerNo))
        {
            return new ConsumerBillViewModel
            {
                Message = "We could not identify a consumer number for this login. Please contact support."
            };
        }

        var consumer = await GetConsumerAsync(consumerNo, ct);
        var bill = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo
                && x.BillType != null
                && x.BillCount != null
                && x.BillCount != 0)
            .OrderByDescending(x => x.BillDateTo ?? x.BillDate ?? x.EntryDate)
            .FirstOrDefaultAsync(ct);

        return new ConsumerBillViewModel
        {
            Consumer = consumer is null ? null : MapConsumer(consumer),
            Bill = bill is null ? null : MapBill(bill),
            Message = bill is null ? "No current bill is available for this consumer account." : null
        };
    }

    private async Task<ConsumerBillPaymentViewModel> BuildPaymentModelAsync(
        int step,
        string? amountOption,
        double? partialAmount,
        string? paymentMethod,
        string? paymentIdentifier,
        bool saveMethod,
        bool setupAutoPay,
        CancellationToken ct)
    {
        var current = await BuildCurrentBillModelAsync(ct);
        var normalizedAmountOption = string.Equals(amountOption, "Partial", StringComparison.OrdinalIgnoreCase)
            ? "Partial"
            : "Full";
        var normalizedMethod = NormalizePaymentMethod(paymentMethod);
        var safeStep = Math.Clamp(step, 1, 3);

        return new ConsumerBillPaymentViewModel
        {
            Consumer = current.Consumer,
            Bill = current.Bill,
            Message = current.Message,
            Step = safeStep,
            AmountOption = normalizedAmountOption,
            PartialAmount = partialAmount,
            PaymentMethod = normalizedMethod,
            PaymentIdentifier = paymentIdentifier,
            SaveMethod = saveMethod,
            SetupAutoPay = setupAutoPay,
            ConvenienceFee = 0,
            IsGatewayReady = false
        };
    }

    private async Task<ConsumerDetailsMaster?> GetConsumerAsync(string consumerNo, CancellationToken ct)
        => await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

    private async Task<IReadOnlyList<ConsumerDetailsMaster>> GetLinkedConsumersAsync(string primaryConsumerNo, CancellationToken ct)
    {
        var primary = await GetConsumerAsync(primaryConsumerNo, ct);
        if (primary is null)
            return [];

        var mobile = primary.MobNo?.Trim();
        var email = primary.EmailId?.Trim();

        return await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == primaryConsumerNo
                || (!string.IsNullOrWhiteSpace(mobile) && x.MobNo == mobile)
                || (!string.IsNullOrWhiteSpace(email) && x.EmailId == email))
            .OrderByDescending(x => x.ConsNo == primaryConsumerNo)
            .ThenBy(x => x.ConsNo)
            .Take(10)
            .ToListAsync(ct);
    }

    private string? ResolveConsumerNo()
    {
        var claimConsumerNo = User.FindFirstValue("ConsumerNo")?.Trim();
        if (!string.IsNullOrWhiteSpace(claimConsumerNo))
            return claimConsumerNo.ToUpperInvariant();

        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)?.Trim();
        return !string.IsNullOrWhiteSpace(nameIdentifier) && !Guid.TryParse(nameIdentifier, out _)
            ? nameIdentifier.ToUpperInvariant()
            : null;
    }

    private static ConsumerBillConsumerViewModel MapConsumer(ConsumerDetailsMaster consumer)
    {
        var name = string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }
            .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

        var property = string.Join(" / ", new[] { consumer.Sector, consumer.BlkNo, consumer.FlatNo }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        return new ConsumerBillConsumerViewModel
        {
            ConsumerNo = consumer.ConsNo,
            Name = string.IsNullOrWhiteSpace(name) ? "Consumer" : name,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            Address = consumer.ConsAddress,
            Sector = consumer.Sector,
            Block = consumer.BlkNo,
            FlatNo = consumer.FlatNo,
            PropertyNo = string.IsNullOrWhiteSpace(property) ? consumer.ConsAddress : property,
            ConnectionType = ResolveConnectionType(consumer.ConTp),
            Category = ResolveConsumerCategory(consumer.ConsCtg),
            PipeSize = consumer.PipeSize,
            WaterConsumption = consumer.KiloLitter
        };
    }

    private static ConsumerBillDetailViewModel MapBill(JalPrintBillMaster bill)
    {
        var dueDate = bill.BillDueDate ?? bill.DueDate;
        var totalPayable = bill.TotalBillAmt ?? bill.DueAmt ?? 0;
        var lastPaid = bill.LastPaidAmt ?? bill.PaidAmt ?? 0;

        return new ConsumerBillDetailViewModel
        {
            BillNo = bill.BillNo ?? "Bill",
            ChallanNo = bill.ChallanNo,
            BillDate = bill.BillDate,
            BillDateFrom = bill.BillDateFrom,
            BillDateTo = bill.BillDateTo,
            DueDate = dueDate,
            MinimumRate = bill.MinRate ?? 0,
            CurrentAmount = bill.MinTotalAmt ?? bill.TotalBillAmt ?? 0,
            RebateAmount = bill.BillRebateAmt ?? 0,
            CessAmount = bill.CessAmt ?? 0,
            ArrearAmount = bill.Arear ?? 0,
            ArrearInterest = bill.ArearInt ?? 0,
            LastBillExtra = bill.LastBillExtra ?? 0,
            AdvanceAmount = bill.AdvAmt ?? 0,
            TotalPayableAmount = totalPayable,
            AfterDueDateAmount = bill.AfterDateAmt ?? totalPayable,
            LastPaidAmount = lastPaid,
            EstimatedWaterCharges = bill.MinTotalAmt ?? bill.MinRate ?? 0,
            SewerageCharges = 0,
            MeterRent = 0,
            PaidDate = bill.PaidDate,
            PaymentStatus = ResolveBillPaymentStatus(bill, totalPayable, lastPaid),
            BillPeriod = FormatBillPeriod(bill.BillDateFrom, bill.BillDateTo),
            Division = bill.DivType,
            PrintStatus = bill.PrintStatus
        };
    }

    private static ConsumerBillHistoryItemViewModel MapHistoryBill(JalPrintBillMaster bill, ConsumerDetailsMaster? consumer)
    {
        var totalPayable = bill.TotalBillAmt ?? bill.DueAmt ?? 0;
        var lastPaid = bill.LastPaidAmt ?? bill.PaidAmt ?? 0;
        var status = ResolveBillPaymentStatus(bill, totalPayable, lastPaid);

        return new ConsumerBillHistoryItemViewModel
        {
            BillNo = string.IsNullOrWhiteSpace(bill.BillNo) ? "Bill" : bill.BillNo,
            ConsumerNo = bill.ConsNo ?? string.Empty,
            BillPeriod = FormatBillPeriod(bill.BillDateFrom, bill.BillDateTo),
            Consumption = consumer?.KiloLitter,
            Amount = totalPayable,
            Status = status == "Pending" || status == "Partially paid" ? "Due" : status,
            PaidOn = bill.PaidDate,
            DueDate = bill.BillDueDate ?? bill.DueDate
        };
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

    private static string? FormatBillPeriod(DateTime? from, DateTime? to)
    {
        if (!from.HasValue && !to.HasValue) return null;
        if (!from.HasValue) return $"Till {to:dd MMM yyyy}";
        if (!to.HasValue) return $"From {from:dd MMM yyyy}";
        return $"{from:dd MMM yyyy} to {to:dd MMM yyyy}";
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

    private static string BuildPropertySummary(ConsumerDetailsMaster consumer)
    {
        var parts = new[] { consumer.Sector, consumer.BlkNo, consumer.FlatNo }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var summary = string.Join(" / ", parts);
        return string.IsNullOrWhiteSpace(summary) ? consumer.ConsAddress ?? consumer.ConsNo : summary;
    }

    private static string NormalizeHistoryStatus(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "DUE" => "Due",
        "PAID" => "Paid",
        _ => "All"
    };

    private static string NormalizePaymentMethod(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "CARD" => "CARD",
        "NETBANKING" => "NETBANKING",
        "WALLET" => "WALLET",
        _ => "UPI"
    };
}
