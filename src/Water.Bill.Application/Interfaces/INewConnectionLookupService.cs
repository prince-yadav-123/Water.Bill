using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.Application.Interfaces;

public interface INewConnectionLookupService
{
    Task<NewConnectionLookupDataDto> GetLookupDataAsync(int? devType = null, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetBlocksBySectorAsync(string sectorId, int? devType = null, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetConnectionSubTypesAsync(string connectionCategoryId, int? devType = null, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionLookupOptionDto>> GetDocumentTypesAsync(CancellationToken ct = default);
}
