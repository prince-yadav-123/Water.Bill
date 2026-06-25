using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.Models.Consumers;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Extensions;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConsumerMasterMaintenanceController : Controller
{
    private const string ModuleName = "Consumer Master Maintenance";
    private readonly ApplicationDbContext _db;
    private readonly IExcelExportService _excelExportService;

    public ConsumerMasterMaintenanceController(ApplicationDbContext db, IExcelExportService excelExportService)
    {
        _db = db;
        _excelExportService = excelExportService;
    }

    [HttpGet("/ConsumerMasterMaintenance")]
    [RequirePermission("Consumer Master Maintenance.view")]
    public async Task<IActionResult> Index(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        int? devType,
        int? status,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = ModuleName;
        ViewData["ActiveMenu"] = ModuleName;
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);

        var model = new ConsumerMasterMaintenanceIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            DevType = devType,
            Status = status ?? 1,
            DivisionOptions = BuildDivisionOptions(devType)
        };

        var paged = await SearchConsumersAsync(model, page, pageSize, ct);
        model.Consumers = paged.Items;
        ViewBag.Pagination = PaginationViewModel.Create(paged);
        return View(model);
    }

    [HttpGet("/ConsumerMasterMaintenance/ExportExcel")]
    [RequirePermission("Consumer Master Maintenance.download")]
    public async Task<IActionResult> ExportExcel(
        string? search,
        string? consumerNo,
        string? consumerName,
        string? mobileNo,
        string? sector,
        string? block,
        string? plotNo,
        int? devType,
        int? status,
        CancellationToken ct = default)
    {
        var model = new ConsumerMasterMaintenanceIndexViewModel
        {
            Search = Normalize(search),
            ConsumerNo = Normalize(consumerNo)?.ToUpperInvariant(),
            ConsumerName = Normalize(consumerName),
            MobileNo = Normalize(mobileNo),
            Sector = Normalize(sector),
            Block = Normalize(block),
            PlotNo = Normalize(plotNo),
            DevType = devType,
            Status = status ?? 1
        };

        var rows = await BuildConsumerSearchQuery(model)
            .OrderBy(x => x.DevType)
            .ThenBy(x => x.Sector)
            .ThenBy(x => x.Block)
            .ThenBy(x => x.PlotNo)
            .Select(x => new ConsumerMasterMaintenanceExportRow
            {
                ConsumerNo = x.ConsumerNo,
                ConsumerName = x.ConsumerName,
                FatherName = x.FatherName,
                MobileNo = x.MobileNo,
                Email = x.Email,
                Sector = x.Sector,
                Block = x.Block,
                PlotNo = x.PlotNo,
                ConnectionType = x.ConnectionType,
                Category = x.Category,
                DevType = x.DevType,
                Status = x.Status == 1 ? "Active" : "Inactive",
                ConnectionDate = x.ConnectionDate
            })
            .ToListAsync(ct);

        var bytes = _excelExportService.Export(new ExcelExportRequest<ConsumerMasterMaintenanceExportRow>
        {
            SheetName = "Consumer Master",
            Rows = rows,
            Columns =
            [
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Consumer No", ValueFactory = x => x.ConsumerNo, Width = 18 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Consumer Name", ValueFactory = x => string.IsNullOrWhiteSpace(x.ConsumerName) ? "-" : x.ConsumerName, Width = 26 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Father / Guardian", ValueFactory = x => string.IsNullOrWhiteSpace(x.FatherName) ? "-" : x.FatherName, Width = 24 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Mobile", ValueFactory = x => string.IsNullOrWhiteSpace(x.MobileNo) ? "-" : x.MobileNo, Width = 18 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Email", ValueFactory = x => string.IsNullOrWhiteSpace(x.Email) ? "-" : x.Email, Width = 28 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Property", ValueFactory = x => BuildPropertyNo(x.Sector, x.Block, x.PlotNo), Width = 18 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Connection Type", ValueFactory = x => string.IsNullOrWhiteSpace(x.ConnectionType) ? "-" : x.ConnectionType, Width = 18 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Category", ValueFactory = x => string.IsNullOrWhiteSpace(x.Category) ? "-" : x.Category, Width = 16 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Division", ValueFactory = x => string.IsNullOrWhiteSpace(AppConstants.Divisions.FormatDisplay(x.DevType)) ? "-" : AppConstants.Divisions.FormatDisplay(x.DevType), Width = 16 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Status", ValueFactory = x => x.Status, Width = 14 },
                new ExcelColumnDefinition<ConsumerMasterMaintenanceExportRow> { Header = "Connection Date", ValueFactory = x => x.ConnectionDate, NumberFormat = "dd mmm yyyy", Width = 18 }
            ]
        });

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"consumer-master-maintenance-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("/ConsumerMasterMaintenance/Details")]
    [RequirePermission("Consumer Master Maintenance.view")]
    public async Task<IActionResult> Details(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Consumer Details";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return BadRequest("Consumer number is required.");

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        if (consumer is null)
            return NotFound();

        var recentBills = await _db.JalPrintBillMasters
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .OrderByDescending(x => x.BillDate)
            .ThenByDescending(x => x.BillDateTo)
            .Take(5)
            .Select(x => new ConsumerMasterBillRowViewModel
            {
                BillNo = x.BillNo,
                BillDate = x.BillDate,
                BillDateFrom = x.BillDateFrom,
                BillDateTo = x.BillDateTo,
                TotalAmount = x.TotalBillAmt,
                PaidAmount = x.PaidAmt,
                PaidDate = x.PaidDate
            })
            .ToListAsync(ct);

        var recentChallans = await _db.Challans
            .AsNoTracking()
            .Where(x => x.ConsNo == consumerNo)
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.Id)
            .Take(5)
            .Select(x => new ConsumerMasterChallanRowViewModel
            {
                Id = x.Id,
                ChallanNo = x.RecpNo ?? x.ReceiptId ?? x.ReceiptId1,
                GeneratedOn = x.EntryDate,
                Amount = x.PaidAmt ?? x.BillAmt,
                PaidDate = x.PayDate,
                Status = x.Status
            })
            .ToListAsync(ct);

        return View(ToDetailsModel(consumer, recentBills, recentChallans));
    }

    [HttpGet("/ConsumerMasterMaintenance/Edit")]
    [RequirePermission("Consumer Master Maintenance.edit")]
    public async Task<IActionResult> Edit(string consumerNo, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Consumer";
        ViewData["ActiveMenu"] = ModuleName;

        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return BadRequest("Consumer number is required.");

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);

        if (consumer is null)
            return NotFound();

        var model = ToFormModel(consumer);
        await PrepareFormOptionsAsync(model, ct);
        return View(model);
    }

    [HttpPost("/ConsumerMasterMaintenance/Edit")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Master Maintenance.edit")]
    public async Task<IActionResult> Edit(ConsumerMasterMaintenanceFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Consumer";
        ViewData["ActiveMenu"] = ModuleName;

        model.ConsumerNo = Normalize(model.ConsumerNo)?.ToUpperInvariant() ?? string.Empty;
        NormalizeForm(model);
        ValidateForm(model);

        var consumer = string.IsNullOrWhiteSpace(model.ConsumerNo)
            ? null
            : await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == model.ConsumerNo, ct);

        if (consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer not found.");

        if (!ModelState.IsValid || consumer is null)
        {
            await PrepareFormOptionsAsync(model, ct);
            return View(model);
        }

        var legacyUser = CurrentUsernameForLegacy();
        ApplyForm(consumer, model, legacyUser);
        _db.ConsumerDetailsTrans.Add(new ConsumerDetailsTran
        {
            ConsNo = consumer.ConsNo,
            CalDate = DateTime.Today.ToString("dd/MM/yyyy"),
            Status = consumer.Status,
            Userid = legacyUser,
            EntryDate = DateTime.Now,
            DevType = consumer.DevType
        });
        _db.Auditlogs.Add(new Auditlog
        {
            Timestamp = DateTime.Now,
            UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
            Username = User.Identity?.Name,
            Action = 2,
            Module = ModuleName,
            EntityId = consumer.ConsNo,
            Details = "Consumer master details updated.",
            Success = true,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Consumer details updated successfully.";
        return RedirectToAction(nameof(Details), new { consumerNo = model.ConsumerNo });
    }

    [HttpPost("/ConsumerMasterMaintenance/ToggleStatus")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Master Maintenance.delete")]
    public async Task<IActionResult> ToggleStatus(string consumerNo, CancellationToken ct)
    {
        consumerNo = Normalize(consumerNo)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(consumerNo))
            return BadRequest("Consumer number is required.");

        var consumer = await _db.ConsumerDetailsMasters.FirstOrDefaultAsync(x => x.ConsNo == consumerNo, ct);
        if (consumer is null)
            return NotFound();

        var makeActive = consumer.Status != 1;
        consumer.Status = makeActive ? 1 : 0;
        consumer.ModifyDate = DateTime.Now;
        consumer.DeleteDate = makeActive ? null : DateTime.Now;
        consumer.Userid = CurrentUsernameForLegacy();

        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = makeActive
            ? "Consumer activated successfully."
            : "Consumer deactivated successfully.";

        return RedirectToAction(nameof(Details), new { consumerNo });
    }

    private async Task<PagedResult<ConsumerMasterMaintenanceListItemViewModel>> SearchConsumersAsync(
        ConsumerMasterMaintenanceIndexViewModel model,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var paged = await BuildConsumerSearchQuery(model)
            .OrderBy(x => x.DevType)
            .ThenBy(x => x.Sector)
            .ThenBy(x => x.Block)
            .ThenBy(x => x.PlotNo)
            .ToPagedResultAsync(page, pageSize, ct);

        return new PagedResult<ConsumerMasterMaintenanceListItemViewModel>
        {
            Items = paged.Items.Select(x => new ConsumerMasterMaintenanceListItemViewModel
            {
                ConsumerNo = x.ConsumerNo,
                ConsumerName = x.ConsumerName,
                FatherName = x.FatherName,
                MobileNo = x.MobileNo,
                Email = x.Email,
                PropertyNo = BuildPropertyNo(x.Sector, x.Block, x.PlotNo),
                ConnectionType = x.ConnectionType,
                Category = x.Category,
                DevType = x.DevType,
                Status = x.Status,
                ConnectionDate = x.ConnectionDate,
                ModifiedOn = x.ModifiedOn
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private IQueryable<ConsumerMasterMaintenanceSearchProjection> BuildConsumerSearchQuery(ConsumerMasterMaintenanceIndexViewModel model)
    {
        var query = _db.ConsumerDetailsMasters.AsNoTracking().AsQueryable();

        if (model.Status.HasValue && model.Status.Value >= 0)
            query = query.Where(x => x.Status == model.Status.Value);
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var search = model.Search;
            query = query.Where(x =>
                x.ConsNo.Contains(search) ||
                (x.ConsNm1 != null && x.ConsNm1.Contains(search)) ||
                (x.ConsNm2 != null && x.ConsNm2.Contains(search)) ||
                (x.MobNo != null && x.MobNo.Contains(search)) ||
                (x.EmailId != null && x.EmailId.Contains(search)) ||
                (x.Sector != null && x.Sector.Contains(search)) ||
                (x.BlkNo != null && x.BlkNo.Contains(search)) ||
                (x.FlatNo != null && x.FlatNo.Contains(search)));
        }
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
        if (model.DevType.HasValue && model.DevType.Value != AppConstants.Divisions.AllDivision.DevType)
            query = query.Where(x => x.DevType == model.DevType.Value);

        return query.Select(x => new ConsumerMasterMaintenanceSearchProjection
        {
            ConsumerNo = x.ConsNo,
            ConsumerName = x.ConsNm1,
            FatherName = x.ConsNm2,
            MobileNo = x.MobNo,
            Email = x.EmailId,
            Sector = x.Sector,
            Block = x.BlkNo,
            PlotNo = x.FlatNo,
            ConnectionType = x.ConTp,
            Category = x.ConsCtg,
            DevType = x.DevType,
            Status = x.Status,
            ConnectionDate = x.ConnDt,
            ModifiedOn = x.ModifyDate
        });
    }

    private sealed class ConsumerMasterMaintenanceSearchProjection
    {
        public string ConsumerNo { get; set; } = string.Empty;
        public string? ConsumerName { get; set; }
        public string? FatherName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Sector { get; set; }
        public string? Block { get; set; }
        public string? PlotNo { get; set; }
        public string? ConnectionType { get; set; }
        public string? Category { get; set; }
        public int? DevType { get; set; }
        public int? Status { get; set; }
        public DateTime? ConnectionDate { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    private sealed class ConsumerMasterMaintenanceExportRow
    {
        public string ConsumerNo { get; set; } = string.Empty;
        public string? ConsumerName { get; set; }
        public string? FatherName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Sector { get; set; }
        public string? Block { get; set; }
        public string? PlotNo { get; set; }
        public string? ConnectionType { get; set; }
        public string? Category { get; set; }
        public int? DevType { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ConnectionDate { get; set; }
    }

    private async Task PrepareFormOptionsAsync(ConsumerMasterMaintenanceFormViewModel model, CancellationToken ct)
    {
        model.DivisionOptions = BuildDivisionOptions(model.DevType);
        model.PipeSizeOptions = await BuildPipeSizeOptionsAsync(model.PipeSize, ct);
        model.ConnectionTypeOptions = await BuildConnectionTypeOptionsAsync(model.ConnectionType, ct);
        model.CategoryOptions = BuildConsumerCategoryOptions(model.Category);
        model.FlatTypeOptions = await BuildFlatTypeOptionsAsync(model.FlatType, ct);
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildPipeSizeOptionsAsync(int? selected, CancellationToken ct)
    {
        var values = await _db.PipeSizeMasters.AsNoTracking()
            .Where(x => x.Status == null || x.Status == 1)
            .Select(x => x.PipeSize)
            .Where(x => x.HasValue)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

        if (selected.HasValue && !values.Contains(selected.Value))
            values.Add(selected.Value);

        return values
            .Where(x => x.HasValue)
            .Select(x => new SelectListItem(x!.Value.ToString(), x.Value.ToString(), selected == x.Value))
            .ToList();
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildConnectionTypeOptionsAsync(string? selected, CancellationToken ct)
    {
        var oldValues = await _db.MasterConnectionTypeDetails.AsNoTracking()
            .Where(x => x.Status == null || x.Status == "1")
            .Select(x => new { Code = x.ConId, Name = x.ConName })
            .ToListAsync(ct);

        var items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["R"] = "Residential",
            ["C"] = "Commercial",
            ["I"] = "Institutional",
            ["T"] = "Industrial",
            ["S"] = "Staff",
            ["V"] = "Village",
            ["H"] = "Housing",
            ["G"] = "Group Housing"
        };

        foreach (var row in oldValues.Where(x => !string.IsNullOrWhiteSpace(x.Code)))
        {
            var code = row.Code.Trim();
            if (code.Length > 1)
                continue;
            items[code] = string.IsNullOrWhiteSpace(row.Name) ? code : row.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(selected) && !items.ContainsKey(selected))
            items[selected] = selected;

        return items
            .OrderBy(x => x.Value)
            .Select(x => new SelectListItem($"{x.Value} ({x.Key})", x.Key, string.Equals(selected, x.Key, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private async Task<IReadOnlyList<SelectListItem>> BuildFlatTypeOptionsAsync(string? selected, CancellationToken ct)
    {
        var values = await _db.ConsumerDetailsMasters.AsNoTracking()
            .Where(x => x.FlatType != null && x.FlatType != "")
            .Select(x => x.FlatType!)
            .Distinct()
            .OrderBy(x => x)
            .Take(30)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(selected) && !values.Any(x => string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
            values.Add(selected);

        return values
            .Select(x => new SelectListItem(x, x, string.Equals(selected, x, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static IReadOnlyList<SelectListItem> BuildConsumerCategoryOptions(string? selected)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["R"] = "Regular",
            ["T"] = "Temporary",
            ["S"] = "Staff",
            ["M"] = "RMC",
            ["CC"] = "Court Case",
            ["D"] = "Disconnected"
        };

        if (!string.IsNullOrWhiteSpace(selected) && !values.ContainsKey(selected))
            values[selected] = selected;

        return values
            .Select(x => new SelectListItem($"{x.Value} ({x.Key})", x.Key, string.Equals(selected, x.Key, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static List<SelectListItem> BuildDivisionOptions(int? selected)
        => AppConstants.Divisions.Options
            .Where(x => x.DevType != AppConstants.Divisions.AllDivision.DevType)
            .Select(x => new SelectListItem(x.DisplayText, x.DevType.ToString(), selected == x.DevType))
            .ToList();

    private static ConsumerMasterMaintenanceDetailsViewModel ToDetailsModel(
        ConsumerDetailsMaster consumer,
        IReadOnlyList<ConsumerMasterBillRowViewModel> bills,
        IReadOnlyList<ConsumerMasterChallanRowViewModel> challans)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            Address = consumer.ConsAddress,
            PropertyNo = BuildPropertyNo(consumer.Sector, consumer.BlkNo, consumer.FlatNo),
            Sector = consumer.Sector,
            Block = consumer.BlkNo,
            FlatNo = consumer.FlatNo,
            PlotSize = consumer.PlotSize,
            PipeSize = consumer.PipeSize,
            FlatType = consumer.FlatType,
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            DevType = consumer.DevType,
            RegistrationNo = consumer.RegNo,
            ConnectionDate = consumer.ConnDt,
            MonthlyRate = consumer.MonthlyRate,
            MonthlyCharges = consumer.MaonthyCharges,
            CessAmount = consumer.CessAmt,
            EstimateNo = consumer.EstiNo,
            EstimateAmount = consumer.EstiAmt,
            SecurityAmount = consumer.Secu,
            VillageName = consumer.VillgaeName,
            KhasraNo = consumer.KhasraNo,
            Purpose = consumer.PurposeCon,
            OtherConnection = consumer.OtherCon,
            Narration = consumer.Narration,
            Status = consumer.Status,
            EntryDate = consumer.EntryDate,
            ModifiedOn = consumer.ModifyDate,
            RecentBills = bills,
            RecentChallans = challans
        };

    private static ConsumerMasterMaintenanceFormViewModel ToFormModel(ConsumerDetailsMaster consumer)
        => new()
        {
            ConsumerNo = consumer.ConsNo,
            ConsumerName = consumer.ConsNm1,
            FatherName = consumer.ConsNm2,
            MobileNo = consumer.MobNo,
            Email = consumer.EmailId,
            Address = consumer.ConsAddress,
            DevType = consumer.DevType,
            Sector = consumer.Sector,
            Block = consumer.BlkNo,
            FlatNo = consumer.FlatNo,
            PlotSize = consumer.PlotSize,
            PipeSize = consumer.PipeSize,
            FlatType = consumer.FlatType,
            ConnectionType = consumer.ConTp,
            Category = consumer.ConsCtg,
            RegistrationNo = consumer.RegNo,
            ConnectionDate = consumer.ConnDt,
            TypeChangeDate = consumer.TypeChangeDate,
            EstimateNo = consumer.EstiNo,
            EstimateAmount = consumer.EstiAmt,
            SecurityAmount = consumer.Secu,
            EstimateDate = consumer.EstiDt,
            MonthlyRate = consumer.MonthlyRate,
            MonthlyCharges = consumer.MaonthyCharges,
            CessAmount = consumer.CessAmt,
            Purpose = consumer.PurposeCon,
            OtherConnection = consumer.OtherCon,
            KhasraNo = consumer.KhasraNo,
            VillageName = consumer.VillgaeName,
            VillageId = consumer.VillgaeId,
            IssueOfficer = consumer.IssueOfficer,
            PlotMapId = consumer.PlotMapId,
            KiloLiter = consumer.KiloLitter,
            Narration = consumer.Narration,
            IsActive = consumer.Status == 1
        };

    private static void ApplyForm(ConsumerDetailsMaster consumer, ConsumerMasterMaintenanceFormViewModel model, string userId)
    {
        consumer.ConsNm1 = model.ConsumerName;
        consumer.ConsNm2 = model.FatherName;
        consumer.MobNo = model.MobileNo;
        consumer.EmailId = model.Email;
        consumer.ConsAddress = model.Address;
        consumer.DevType = model.DevType;
        consumer.Sector = model.Sector;
        consumer.BlkNo = model.Block;
        consumer.FlatNo = model.FlatNo;
        consumer.PlotSize = model.PlotSize;
        consumer.PipeSize = model.PipeSize;
        consumer.FlatType = model.FlatType;
        consumer.ConTp = model.ConnectionType;
        consumer.ConsCtg = model.Category;
        consumer.RegNo = model.RegistrationNo;
        consumer.ConnDt = model.ConnectionDate;
        consumer.TypeChangeDate = model.TypeChangeDate;
        consumer.EstiNo = model.EstimateNo;
        consumer.EstiAmt = model.EstimateAmount;
        consumer.Secu = model.SecurityAmount;
        consumer.EstiDt = model.EstimateDate;
        consumer.MonthlyRate = model.MonthlyRate;
        consumer.MaonthyCharges = model.MonthlyCharges;
        consumer.CessAmt = model.CessAmount;
        consumer.PurposeCon = model.Purpose;
        consumer.OtherCon = model.OtherConnection;
        consumer.KhasraNo = model.KhasraNo;
        consumer.VillgaeName = model.VillageName;
        consumer.VillgaeId = model.VillageId;
        consumer.IssueOfficer = model.IssueOfficer;
        consumer.PlotMapId = model.PlotMapId;
        consumer.KiloLitter = model.KiloLiter;
        consumer.Narration = model.Narration;
        consumer.Status = model.IsActive ? 1 : 0;
        consumer.DeleteDate = model.IsActive ? null : consumer.DeleteDate ?? DateTime.Now;
        consumer.ModifyDate = DateTime.Now;
        consumer.Userid = userId;
    }

    private void ValidateForm(ConsumerMasterMaintenanceFormViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.MobileNo) && model.MobileNo.Any(x => !char.IsDigit(x)))
            ModelState.AddModelError(nameof(model.MobileNo), "Mobile number should contain digits only.");
        if (model.PlotSize is < 0)
            ModelState.AddModelError(nameof(model.PlotSize), "Plot size cannot be negative.");
        if (model.PipeSize is < 0)
            ModelState.AddModelError(nameof(model.PipeSize), "Pipe size cannot be negative.");
        if (model.MonthlyRate is < 0)
            ModelState.AddModelError(nameof(model.MonthlyRate), "Monthly rate cannot be negative.");
        if (model.MonthlyCharges is < 0)
            ModelState.AddModelError(nameof(model.MonthlyCharges), "Monthly charges cannot be negative.");
        if (model.CessAmount is < 0)
            ModelState.AddModelError(nameof(model.CessAmount), "Cess amount cannot be negative.");
    }

    private static void NormalizeForm(ConsumerMasterMaintenanceFormViewModel model)
    {
        model.ConsumerName = Normalize(model.ConsumerName);
        model.FatherName = Normalize(model.FatherName);
        model.MobileNo = Normalize(model.MobileNo);
        model.Email = Normalize(model.Email);
        model.Address = Normalize(model.Address);
        model.Sector = Normalize(model.Sector);
        model.Block = Normalize(model.Block);
        model.FlatNo = Normalize(model.FlatNo);
        model.FlatType = Normalize(model.FlatType)?.ToUpperInvariant();
        model.ConnectionType = Normalize(model.ConnectionType)?.ToUpperInvariant();
        model.Category = Normalize(model.Category)?.ToUpperInvariant();
        model.RegistrationNo = Normalize(model.RegistrationNo);
        model.EstimateNo = Normalize(model.EstimateNo);
        model.Purpose = Normalize(model.Purpose);
        model.OtherConnection = Normalize(model.OtherConnection);
        model.KhasraNo = Normalize(model.KhasraNo);
        model.VillageName = Normalize(model.VillageName);
        model.IssueOfficer = Normalize(model.IssueOfficer);
        model.PlotMapId = Normalize(model.PlotMapId);
        model.KiloLiter = Normalize(model.KiloLiter);
        model.Narration = Normalize(model.Narration);
    }

    private string CurrentUsernameForLegacy()
    {
        var value = User.FindFirstValue(AppConstants.Claims.Username)
            ?? User.Identity?.Name
            ?? "System";
        return value.Length > 10 ? value[..10] : value;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildPropertyNo(string? sector, string? block, string? flatNo)
        => string.Join("/", new[] { sector, $"{block}-{flatNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
