using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.Application.Interfaces;

public interface INewConnectionApplicationService
{
    Task<NewConnectionApplicationDetailsDto> SubmitAsync(NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> TrackAsync(string applicationNo, string mobileNumber, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetConsumerApplicationsAsync(string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> GetConsumerApplicationDetailsAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task UpdateApplicationStatusAsync(NewConnectionStatusChangeRequest request, CancellationToken ct = default);
}
