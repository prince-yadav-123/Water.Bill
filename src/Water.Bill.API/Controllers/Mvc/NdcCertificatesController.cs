using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.Models.Ndc;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
[RequirePermission("NDC Certificate Management.view")]
public class NdcCertificatesController : Controller
{
    private readonly ApplicationDbContext _db;

    public NdcCertificatesController(ApplicationDbContext db) => _db = db;

    [HttpGet("/NdcCertificates")]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewData["Title"] = "NDC Certificate Management";
        ViewData["ActiveMenu"] = "NDC Certificate Management";

        search = Normalize(search);
        var query = _db.ConsumerApplyNdcs
            .AsNoTracking()
            .Where(x => x.AutoId > 0 && (x.FinalStatus == "A" || x.CurrentStatus == "Approved" || x.CertificateUrl != null));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.ApplicationNo.Contains(search)
                || (x.ConsumerNo != null && x.ConsumerNo.Contains(search))
                || (x.ConsName != null && x.ConsName.Contains(search))
                || (x.MobileNo != null && x.MobileNo.Contains(search)));

        var rows = await query
            .OrderByDescending(x => x.CompletedDate ?? x.LastUpdatedOn ?? x.CreatedOn)
            .Take(200)
            .ToListAsync(ct);

        return View(new NdcCertificateIndexViewModel
        {
            Search = search,
            Items = rows.Select(x => new NdcApplicationListItemViewModel
            {
                AutoId = x.AutoId,
                ApplicationNo = x.ApplicationNo,
                ConsumerNo = x.ConsumerNo,
                ConsumerName = x.ConsName,
                MobileNo = x.MobileNo,
                PropertyNo = BuildProperty(x.Sector, x.Block, x.PlotNo),
                Division = x.DivisionTypeName,
                Status = "Approved",
                Amount = x.Amount,
                CreatedOn = x.CompletedDate ?? x.LastUpdatedOn ?? x.CreatedOn
            }).ToList()
        });
    }

    [HttpGet("/NdcCertificates/Print/{id:int}")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Print NDC Certificate";
        ViewData["ActiveMenu"] = "NDC Certificate Management";

        var application = await _db.ConsumerApplyNdcs.AsNoTracking().FirstOrDefaultAsync(x => x.AutoId == id, ct);
        if (application is null)
            return NotFound();
        if (application.FinalStatus != "A" && application.CurrentStatus != "Approved" && string.IsNullOrWhiteSpace(application.CertificateUrl))
            return BadRequest("NDC certificate is available only after final approval.");

        return View(application);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildProperty(string? sector, string? block, string? plotNo)
        => string.Join(" / ", new[] { sector, $"{block}-{plotNo}".Trim('-') }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
