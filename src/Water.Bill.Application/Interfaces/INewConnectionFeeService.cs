using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.Application.Interfaces;

public interface INewConnectionFeeService
{
    Task<NewConnectionFeeQuoteDto?> GetFeeAsync(NewConnectionFeeRequestDto request, CancellationToken ct = default);
}
