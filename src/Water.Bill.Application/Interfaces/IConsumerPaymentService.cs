using Water.Bill.Application.DTOs.Payments;

namespace Water.Bill.Application.Interfaces;

public interface IConsumerPaymentService
{
    Task<PaymentInitiationResultDto> InitiateBillPaymentAsync(
        PaymentInitiationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentInitiationResultDto?> GetInitiatedPaymentAsync(
        string jalReferenceId,
        string consumerNo,
        CancellationToken cancellationToken = default);
}
