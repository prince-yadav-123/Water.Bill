using Water.Bill.Application.DTOs.Consumer;

namespace Water.Bill.Application.Interfaces;

public interface IConsumerOtpService
{
    Task<ConsumerOtpRequestResult> RequestOtpAsync(string consumerNo, CancellationToken ct = default);

    Task<ConsumerOtpRequestResult> RequestOtpByMobileAsync(string mobileNo, CancellationToken ct = default);

    Task<ConsumerOtpVerifyResult> VerifyOtpAsync(string consumerNo, string otp, CancellationToken ct = default);
}
