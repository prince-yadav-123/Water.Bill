using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.Infrastructure.Services;

public class NewConnectionLookupService : INewConnectionLookupService
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public NewConnectionLookupService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

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

    public async Task<NewConnectionSectorContextDto> GetSectorContextAsync(string sectorId, CancellationToken ct = default)
    {
        var normalizedSector = Normalize(sectorId);
        if (normalizedSector is null)
            return new NewConnectionSectorContextDto();

        var devType = await GetSectorDevTypeAsync(normalizedSector, ct);

        return new NewConnectionSectorContextDto
        {
            DevType = devType,
            DivisionDisplay = AppConstants.Divisions.FormatDisplay(devType),
            Blocks = await GetBlocksBySectorAsync(normalizedSector, devType, ct),
            ConnectionCategories = devType.HasValue ? await GetConnectionCategoriesAsync(devType, ct) : [],
            PipeSizes = devType.HasValue ? await GetPipeSizesAsync(devType, ct) : [],
            Villages = devType.HasValue ? await GetVillagesAsync(devType, ct) : []
        };
    }

    public async Task<int?> GetSectorDevTypeAsync(string sectorId, CancellationToken ct = default)
    {
        var normalizedSector = Normalize(sectorId);
        if (normalizedSector is null)
            return null;

        return await GetCachedAsync(
            CacheKey("sector-devtype", normalizedSector),
            TimeSpan.FromMinutes(30),
            async () => await _db.SectorDetails
                .AsNoTracking()
                .Where(x => x.SectorId == normalizedSector
                    && (x.Status == null || x.Status == 1))
                .Select(x => x.DevType)
                .FirstOrDefaultAsync(ct));
    }

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetBlocksBySectorAsync(string sectorId, int? devType = null, CancellationToken ct = default)
    {
        var normalizedSector = Normalize(sectorId);
        if (normalizedSector is null)
            return [];

        return await GetCachedAsync(
            CacheKey("sector-blocks", normalizedSector, devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.BlockDetails
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
                .ToListAsync(ct));
    }

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionSubTypesAsync(string connectionCategoryId, int? devType = null, CancellationToken ct = default)
    {
        var normalizedCategory = Normalize(connectionCategoryId);
        if (normalizedCategory is null)
            return [];

        return await GetCachedAsync(
            CacheKey("connection-subtypes", normalizedCategory, devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.MasterConnectionTypeDetailsTrans
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
                .ToListAsync(ct));
    }

    public async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetDocumentTypesAsync(CancellationToken ct = default)
        => await GetCachedAsync(
            CacheKey("document-types"),
            TimeSpan.FromMinutes(30),
            async () => await _db.MasterDocumentUploads
                .AsNoTracking()
                .Where(x => (x.Status == null || x.Status == 1)
                    && x.DocumentName != null
                    && (x.DocFor == null || x.DocFor == "NCH" || x.DocFor == "NC"))
                .OrderBy(x => x.DocumentId)
                .Select(x => new NewConnectionLookupOptionDto
                {
                    Value = x.DocumentName!,
                    Text = x.DocumentName!,
                    IsRequired = (x.IsMandatory ?? 0) == 1
                })
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetSectorsAsync(int? devType, CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("sectors", devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.SectorDetails
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
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetPipeSizesAsync(int? devType, CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("pipe-sizes", devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.PipeSizeMasters
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
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionCategoriesAsync(int? devType, CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("connection-categories", devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.MasterConnectionTypeDetails
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
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionTypesAsync(CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("connection-types"),
            TimeSpan.FromMinutes(30),
            async () => await _db.ConnectionTypeMsts
                .AsNoTracking()
                .Where(x => x.ConnectionName != null && (x.Status == null || x.Status == true))
                .OrderBy(x => x.ConnectionName)
                .Select(x => new NewConnectionLookupOptionDto
                {
                    Value = x.ConnectionMainId ?? x.AutoId.ToString(),
                    Text = x.ConnectionName!
                })
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetAllConnectionSubTypesAsync(int? devType, CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("all-connection-subtypes", devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.MasterConnectionTypeDetailsTrans
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
                .ToListAsync(ct));

    private async Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetVillagesAsync(int? devType, CancellationToken ct)
        => await GetCachedAsync(
            CacheKey("villages", devType),
            TimeSpan.FromMinutes(20),
            async () => await _db.VillageDetails
                .AsNoTracking()
                .Where(x => (x.Status == null || x.Status == 1)
                    && (!devType.HasValue || x.DevType == devType.Value))
                .OrderBy(x => x.VillageName)
                .Select(x => new NewConnectionLookupOptionDto
                {
                    Value = x.VillageId.HasValue ? x.VillageId.Value.ToString() : x.VillageName,
                    Text = x.VillageName
                })
                .ToListAsync(ct));

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private async Task<T> GetCachedAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
            return cachedValue;

        var value = await factory();
        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            Priority = CacheItemPriority.Low
        });

        return value;
    }

    private static string CacheKey(string segment, params object?[] parts)
    {
        var values = new List<string> { "new-connection-lookups", segment };
        foreach (var part in parts)
            values.Add(part?.ToString() ?? "null");
        return string.Join(":", values);
    }
}
