using Water.Bill.Application.DTOs.NewConnection;

namespace Water.Bill.Application.Interfaces;

public interface INewConnectionApplicationService
{
    Task<NewConnectionApplicationDetailsDto> SubmitAsync(NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompletePublicApplicationAsync(long id, string mobileNumber, NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompleteConsumerApplicationAsync(long id, string consumerNo, int? consumerUserId, NewConnectionSubmitRequest request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompletePublicPaymentAsync(long id, string mobileNumber, NewConnectionPaymentRequestDto request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> CompleteConsumerPaymentAsync(long id, string consumerNo, int? consumerUserId, NewConnectionPaymentRequestDto request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto> FinalizeGatewayPaymentAsync(long id, NewConnectionPaymentRequestDto request, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> TrackAsync(string applicationNo, string mobileNumber, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetConsumerApplicationsAsync(string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> GetConsumerApplicationDetailsAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<IReadOnlyList<NewConnectionApplicationSummaryDto>> GetPublicApplicationsByMobileAsync(string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationDetailsDto?> GetPublicApplicationDetailsAsync(long id, string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationFormDto?> GetPublicContinuationFormAsync(long id, string mobileNumber, CancellationToken ct = default);

    Task<NewConnectionApplicationFormDto?> GetConsumerContinuationFormAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    /// <summary>Loads form data for resubmission — public applicant (no ContinuableStatuses check).</summary>
    Task<NewConnectionApplicationFormDto?> GetPublicResubmitFormAsync(long id, string mobileNumber, CancellationToken ct = default);

    /// <summary>Loads form data for resubmission — consumer portal (no ContinuableStatuses check).</summary>
    Task<NewConnectionApplicationFormDto?> GetConsumerResubmitFormAsync(long id, string consumerNo, int? consumerUserId, CancellationToken ct = default);

    Task<NewConnectionFeeQuoteDto?> GetApplicationFeeAsync(long applicationId, CancellationToken ct = default);

    Task UpdateApplicationStatusAsync(NewConnectionStatusChangeRequest request, CancellationToken ct = default);

    /// <summary>
    /// Resubmit a Send-Back application (consumer portal — logged-in consumer) without payment.
    /// </summary>
    Task<NewConnectionApplicationDetailsDto> ResubmitApplicationAsync(
        long id,
        string consumerNo,
        int? consumerUserId,
        string? applicantRemarks,
        IReadOnlyList<NewConnectionDocumentInputDto> newDocuments,
        CancellationToken ct = default);

    /// <summary>
    /// Resubmit a Send-Back application (public applicant — mobile OTP verified) without payment.
    /// </summary>
    Task<NewConnectionApplicationDetailsDto> ResubmitPublicApplicationAsync(
        long id,
        string mobileNumber,
        string? applicantRemarks,
        IReadOnlyList<NewConnectionDocumentInputDto> newDocuments,
        CancellationToken ct = default);
}
