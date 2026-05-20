using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionLookupService : INewConnectionLookupService
{
    private readonly ApplicationDbContext _db;

    public NewConnectionLookupService(ApplicationDbContext db) => _db = db;

    public async Task<NewConnectionLookupDataDto> GetLookupDataAsync(int? devType = null, CancellationToken ct = default)
        => new()
        {
            Sectors = await GetSectorsAsync(devType, ct),
            PipeSizes = await GetPipeSizesAsync(devType, ct),
            ConnectionCategories = await GetConnectionCategoriesAsync(devType, ct),
            ConnectionTypes = await GetConnectionTypesAsync(ct),
            ConnectionSubTypes = await GetAllConnectionSubTypesAsync(devType, ct),
            DocumentTypes = await GetDocumentTypesAsync(ct),
            Villages = await GetVillagesAsync(devType, ct)
        };

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetBlocksBySectorAsync(string sectorId, int? devType = null, CancellationToken ct = default)
    {
        var normalizedSector = Normalize(sectorId);
        if (normalizedSector is null)
            return [];

        return await _db.BlockDetails
            .AsNoTracking()
            .Where(x => x.SectorId == normalizedSector
                && (x.Status == null || x.Status == 1)
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.Block)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.Block,
                Text = x.Block,
                ParentValue = x.SectorId
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionSubTypesAsync(string connectionCategoryId, int? devType = null, CancellationToken ct = default)
    {
        var normalizedCategory = Normalize(connectionCategoryId);
        if (normalizedCategory is null)
            return [];

        return await _db.MasterConnectionTypeDetailsTrans
            .AsNoTracking()
            .Where(x => x.ConId == normalizedCategory
                && (x.Status == null || x.Status == "1")
                && (!devType.HasValue || x.DevType == devType.Value)
                && x.SubConName != null)
            .OrderBy(x => x.SubConName)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.SubConName!,
                Text = x.SubConName!,
                ParentValue = x.ConId
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetDocumentTypesAsync(CancellationToken ct = default)
        => await _db.MasterDocumentUploads
            .AsNoTracking()
            .Where(x => (x.Status == null || x.Status == 1)
                && x.DocumentName != null
                && (x.DocFor == null || x.DocFor == "NCH"))
            .OrderBy(x => x.DocumentId)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.DocumentName!,
                Text = x.DocumentName!
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetSectorsAsync(int? devType, CancellationToken ct)
        => await _db.SectorDetails
            .AsNoTracking()
            .Where(x => x.SectorNo != null
                && (x.Status == null || x.Status == 1)
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.OrderBy ?? int.MaxValue)
            .ThenBy(x => x.SectorNo)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.SectorId,
                Text = x.SectorNo!
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetPipeSizesAsync(int? devType, CancellationToken ct)
        => await _db.PipeSizeMasters
            .AsNoTracking()
            .Where(x => x.PipeSize != null
                && (x.Status == null || x.Status == 1)
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.PipeSize)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.PipeSize!.Value.ToString(),
                Text = $"{x.PipeSize} inch"
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionCategoriesAsync(int? devType, CancellationToken ct)
        => await _db.MasterConnectionTypeDetails
            .AsNoTracking()
            .Where(x => x.ConName != null
                && (x.Status == null || x.Status == "1")
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.ConName)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.ConMainId ?? x.ConId,
                Text = x.ConName!
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionTypesAsync(CancellationToken ct)
        => await _db.ConnectionTypeMsts
            .AsNoTracking()
            .Where(x => x.ConnectionName != null && (x.Status == null || x.Status == true))
            .OrderBy(x => x.ConnectionName)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.ConnectionMainId ?? x.AutoId.ToString(),
                Text = x.ConnectionName!
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetAllConnectionSubTypesAsync(int? devType, CancellationToken ct)
        => await _db.MasterConnectionTypeDetailsTrans
            .AsNoTracking()
            .Where(x => x.SubConName != null
                && (x.Status == null || x.Status == "1")
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.SubConName)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.SubConName!,
                Text = x.SubConName!,
                ParentValue = x.ConId
            })
            .ToListAsync(ct);

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetVillagesAsync(int? devType, CancellationToken ct)
        => await _db.VillageDetails
            .AsNoTracking()
            .Where(x => (x.Status == null || x.Status == 1)
                && (!devType.HasValue || x.DevType == devType.Value))
            .OrderBy(x => x.VillageName)
            .Select(x => new NewConnectionLookupOptionDto
            {
                Value = x.VillageId.HasValue ? x.VillageId.Value.ToString() : x.VillageName,
                Text = x.VillageName
            })
            .ToListAsync(ct);

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
