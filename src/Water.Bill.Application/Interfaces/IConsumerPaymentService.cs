using Water.Bill.Application.DTOs.Payments;

namespace Water.Bill.Application.Interfaces;

public interface IConsumerPaymentService
{
    bool IsDevelopmentMode();

    Task<PaymentInitiationResultDto> InitiateBillPaymentAsync(
        PaymentInitiationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentInitiationResultDto> InitiatePaymentAsync(
        PaymentInitiationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentInitiationResultDto?> GetInitiatedPaymentAsync(
        string jalReferenceId,
        string? consumerNo,
        CancellationToken cancellationToken = default);

    Task<PaymentProcessingResultDto> ProcessDevelopmentSuccessAsync(
        string jalReferenceId,
        PaymentActorContextDto actor,
        CancellationToken cancellationToken = default);

    Task<PaymentProcessingResultDto> ProcessAxisResponseAsync(
        string rawMessage,
        PaymentActorContextDto actor,
        CancellationToken cancellationToken = default);
}
