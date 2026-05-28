using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.Application.Interfaces;

public interface INewConnectionApplicationService
{
    Task<NewConnectionApplicationDetailsDto> SubmitAsync(NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompletePublicApplicationAsync(long id, string mobileNumber, NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompleteConsumerApplicationAsync(long id, string consumerNo, int? consumerUserId, NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompletePublicPaymentAsync(long id, string mobileNumber, NewConnectionPaymentRequestDto request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompleteConsumerPaymentAsync(long id, string consumerNo, int? consumerUserId, NewConnectionPaymentRequestDto request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> TrackAsync(string applicationNo, string mobileNumber, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetConsumerApplicationsAsync(string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> GetConsumerApplicationDetailsAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetPublicApplicationsByMobileAsync(string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> GetPublicApplicationDetailsAsync(long id, string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationFormDto?> GetPublicContinuationFormAsync(long id, string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationFormDto?> GetConsumerContinuationFormAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<NewConnectionFeeQuoteDto?> GetApplicationFeeAsync(long applicationId, CancellationToken ct = default);

    Task UpdateApplicationStatusAsync(NewConnectionStatusChangeRequest request, CancellationToken ct = default);
}
