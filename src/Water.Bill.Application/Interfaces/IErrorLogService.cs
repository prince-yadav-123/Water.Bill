using Water.Bill.Application.Models;

namespace Water.Bill.Application.Interfaces;

public interface IErrorLogService
{
    Task<bool> TryLogAsync(ErrorLogWriteModel model, CancellationToken ct = default);
}
