using Water.Bill.Application.DTOs.Consumer;

namespace Water.Bill.Application.Interfaces;

public interface IPimsConsumerInfoService
{
    Task<ConsumerPimsContactResult> GetDetailsByRidAsync(long rid, CancellationToken ct = default);
}
