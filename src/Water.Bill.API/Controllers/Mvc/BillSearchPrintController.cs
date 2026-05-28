using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Billing;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class BillSearchPrintController : Controller
{
    private readonly ApplicationDbContext _db;

    public BillSearchPrintController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/BillSearchPrint")]
    [RequirePermission("Bill Search & Print.view")]
    public async Task<IActionResult> Index(
        string? consumerNo,
        string? billNo,
        string? sector,
        string? block,
        string? plotNo,
        int? devType,
        CancellationToken ct)
    {
        ViewData["Title"] = "Bill Search & Print";
        ViewData["ActiveMenu"] = "Bill Search & Print";

        var model = new BillSearchPrintIndexViewModel
        {
            ConsumerNo = Normalize(consumerNo),
            BillNo = Normalize(billNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            DevType = devType,
            DivisionOptions = BuildDivisionOptions(devType),
            HasSearched = HasAnySearch(consumerNo, billNo, sector, block, plotNo, devType)
        };

        if (!model.HasSearched)
            return View(model);

        model.Consumers = await SearchAsync(model, ct);
        return View(model);
    }

    [HttpGet("/BillSearchPrint/Print")]
    [RequirePermission("Bill Search & Print.print")]
    public async Task<IActionResult> Print(string consumerNo, string billNo, CancellationToken ct)
    {
        ViewData["Title"] = "Print Bill";
        ViewData["ActiveMenu"] = "Bill Search & Print";

        consumerNo = Normalize(consumerNo) ?? string.Empty;
        billNo = Normalize(billNo) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo) || string.IsNullOrWhiteSpace(billNo))
            return BadRequest("Consumer number and bill number are required.");

        var bill = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo && x.BillNo == billNo)
            .OrderByDescending(x => x.BillDate)
            .FirstOrDefaultAsync(ct);
        if (bill is null)
            return NotFound();

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        return View(new BillPrintViewModel
        {
            ConsumerNo = consumerNo,
            BillNo = billNo,
            ConsumerName = consumer?.ConsNm1,
            MobileNo = consumer?.MobNo,
            Email = consumer?.EmailId,
            PropertyNo = BuildPropertyNo(consumer?.Sector, consumer?.BlkNo, consumer?.FlatNo),
            Address = consumer?.ConsAddress,
            ConnectionType = consumer?.ConTp,
            Category = consumer?.ConsCtg,
            FlatType = consumer?.FlatType,
            PipeSize = consumer?.PipeSize,
            DevType = consumer?.DevType ?? bill.DevType,
            BillDate = bill.BillDate,
            BillDueDate = bill.BillDueDate,
            BillDateFrom = bill.BillDateFrom,
            BillDateTo = bill.BillDateTo,
            MinimumRate = bill.MinRate,
            MinimumTotalAmount = bill.MinTotalAmt,
            CessAmount = bill.CessAmt,
            ArrearAmount = bill.Arear,
            ArrearInterest = bill.ArearInt,
            RebateAmount = bill.BillRebateAmt,
            AdvanceAmount = bill.AdvAmt,
            LastPaidAmount = bill.LastPaidAmt,
            TotalBillAmount = bill.TotalBillAmt,
            ChallanNo = bill.ChallanNo,
            BankCode = bill.BankCode,
            ChallanContent = bill.ChallanContent
        });
    }

    private async Task<IReadOnlyList<BillSearchPrintConsumerViewModel>> SearchAsync(BillSearchPrintIndexViewModel model, CancellationToken ct)
    {
        var hasConsumerFilters = !string.IsNullOrWhiteSpace(model.ConsumerNo)
            || !string.IsNullOrWhiteSpace(model.Sector)
            || !string.IsNullOrWhiteSpace(model.Block)
            || !string.IsNullOrWhiteSpace(model.PlotNo)
            || (model.DevType.HasValue && model.DevType.Value != AppConstants.Divisions.AllDivision.DevType);

        var consumersQuery = _db.ConsumerDetailsMasters
            .AsNoTracking()
            .Where(x => x.Status == 1);

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
            consumersQuery = consumersQuery.Where(x => x.ConsNo.StartsWith(model.ConsumerNo));
        if (!string.IsNullOrWhiteSpace(model.Sector))
            consumersQuery = consumersQuery.Where(x => x.Sector != null && x.Sector.StartsWith(model.Sector));
        if (!string.IsNullOrWhiteSpace(model.Block))
            consumersQuery = consumersQuery.Where(x => x.BlkNo != null && x.BlkNo.StartsWith(model.Block));
        if (!string.IsNullOrWhiteSpace(model.PlotNo))
            consumersQuery = consumersQuery.Where(x => x.FlatNo != null && x.FlatNo.StartsWith(model.PlotNo));
        if (model.DevType.HasValue && model.DevType.Value != AppConstants.Divisions.AllDivision.DevType)
            consumersQuery = consumersQuery.Where(x => x.DevType == model.DevType.Value);

        List<ConsumerDetailsMaster> consumers = hasConsumerFilters
            ? await consumersQuery.OrderBy(x => x.ConsNo).Take(100).ToListAsync(ct)
            : [];

        var consumerNos = consumers.Select(x => x.ConsNo).Distinct().ToList();
        var billsQuery = _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.Status == "1" && x.ConsNo != null && x.BillNo != null);

        if (!string.IsNullOrWhiteSpace(model.BillNo))
        {
            billsQuery = billsQuery.Where(x => x.BillNo!.Contains(model.BillNo));
            if (hasConsumerFilters)
                billsQuery = billsQuery.Where(x => consumerNos.Contains(x.ConsNo!));
        }
        else
        {
            if (consumerNos.Count == 0)
                return [];
            billsQuery = billsQuery.Where(x => consumerNos.Contains(x.ConsNo!));
        }

        var bills = await billsQuery
            .OrderByDescending(x => x.BillDate)
            .ThenByDescending(x => x.BillDateTo)
            .Take(300)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(model.BillNo) && consumerNos.Count == 0)
        {
            var billConsumerNos = bills.Where(x => !string.IsNullOrWhiteSpace(x.ConsNo)).Select(x => x.ConsNo!).Distinct().ToList();
            consumers = await _db.ConsumerDetailsMasters
                .AsNoTracking()
                .Where(x => billConsumerNos.Contains(x.ConsNo))
                .OrderBy(x => x.ConsNo)
                .ToListAsync(ct);
        }

        var billLookup = bills
            .Where(x => !string.IsNullOrWhiteSpace(x.ConsNo))
            .GroupBy(x => x.ConsNo!)
            .ToDictionary(x => x.Key, x => x.Select(b => new BillSearchPrintBillViewModel
            {
                BillNo = b.BillNo ?? string.Empty,
                BillDateFrom = b.BillDateFrom,
                BillDateTo = b.BillDateTo,
                BillDate = b.BillDate,
                DueDate = b.BillDueDate,
                BillAmount = b.MinTotalAmt,
                TotalBillAmount = b.TotalBillAmt,
                ChallanNo = b.ChallanNo,
                HasPrintableContent = !string.IsNullOrWhiteSpace(b.ChallanContent)
            }).ToList());

        return consumers
            .Where(x => string.IsNullOrWhiteSpace(model.BillNo) || billLookup.ContainsKey(x.ConsNo))
            .Select(x => new BillSearchPrintConsumerViewModel
            {
                ConsumerNo = x.ConsNo,
                ConsumerName = x.ConsNm1,
                MobileNo = x.MobNo,
                PropertyNo = BuildPropertyNo(x.Sector, x.BlkNo, x.FlatNo),
                ConnectionDate = x.ConnDt,
                DevType = x.DevType,
                Bills = billLookup.TryGetValue(x.ConsNo, out var rows) ? rows : []
            })
            .ToList();
    }

    private static List<SelectListItem> BuildDivisionOptions(int? selected)
        => AppConstants.Divisions.Options
            .Where(x => x.DevType != AppConstants.Divisions.AllDivision.DevType)
            .Select(x => new SelectListItem(x.DisplayText, x.DevType.ToString(), selected == x.DevType))
            .ToList();

    private static bool HasAnySearch(params object?[] values)
        => values.Any(x => x switch
        {
            null => false,
            string value => !string.IsNullOrWhiteSpace(value),
            int => true,
            _ => true
        });

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
