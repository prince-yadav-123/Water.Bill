using Water.Bill.Application.DTOs.Consumer;

namespace Water.Bill.Application.Interfaces;

public interface IConsumerMobileRegistrationService
{
    Task<ConsumerMobileRegistrationEligibilityResult> CheckEligibilityAsync(string consumerNo, CancellationToken ct = default);

    Task<ConsumerOtpRequestResult> RequestOtpAsync(string consumerNo, string mobileNo, CancellationToken ct = default);

    Task UpdateMobileAsync(string consumerNo, string mobileNo, string otp, CancellationToken ct = default);
}
