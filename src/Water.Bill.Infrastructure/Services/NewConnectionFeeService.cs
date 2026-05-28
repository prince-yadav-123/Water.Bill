using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionFeeService : INewConnectionFeeService
{
    private readonly ApplicationDbContext _db;

    public NewConnectionFeeService(ApplicationDbContext db) => _db = db;

    public async Task<NewConnectionFeeQuoteDto?> GetFeeAsync(NewConnectionFeeRequestDto request, CancellationToken ct = default)
    {
        var category = Normalize(request.ConnectionCategory);
        var connectionType = Normalize(request.ConnectionType);
        var now = DateTime.Now;

        var query = _db.NewConnectionFeeConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive
                && !x.IsDeleted
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo >= now));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.ConnectionCategory == null || x.ConnectionCategory == category);

        if (!string.IsNullOrWhiteSpace(connectionType))
            query = query.Where(x => x.ConnectionType == null || x.ConnectionType == connectionType);

        if (request.PipeSize.HasValue)
            query = query.Where(x => x.PipeSize == null || x.PipeSize == request.PipeSize.Value);

        if (request.PlotSize.HasValue)
            query = query.Where(x => (x.PlotSizeFrom == null || request.PlotSize.Value >= x.PlotSizeFrom)
                && (x.PlotSizeTo == null || request.PlotSize.Value <= x.PlotSizeTo));

        var match = await query
            .OrderByDescending(x => x.ConnectionCategory != null)
            .ThenByDescending(x => x.ConnectionType != null)
            .ThenByDescending(x => x.PipeSize != null)
            .ThenByDescending(x => x.PlotSizeFrom != null || x.PlotSizeTo != null)
            .ThenByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        if (match is null)
            return null;

        var total = match.TotalAmount > 0
            ? match.TotalAmount
            : match.ApplicationFee + match.ProcessingFee + match.SecurityAmount + match.MeterInstallationFee + match.OtherCharges;

        return new NewConnectionFeeQuoteDto
        {
            ConfigurationId = match.Id,
            ConnectionCategory = match.ConnectionCategory,
            ConnectionType = match.ConnectionType,
            PipeSize = match.PipeSize,
            PlotSizeFrom = match.PlotSizeFrom,
            PlotSizeTo = match.PlotSizeTo,
            ApplicationFee = match.ApplicationFee,
            ProcessingFee = match.ProcessingFee,
            SecurityAmount = match.SecurityAmount,
            MeterInstallationFee = match.MeterInstallationFee,
            OtherCharges = match.OtherCharges,
            TotalAmount = total,
            EffectiveFrom = match.EffectiveFrom,
            EffectiveTo = match.EffectiveTo
        };
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
