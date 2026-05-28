using Water.Bill.Application.DTOs.PublicNewConnection;

namespace Water.Bill.Application.Interfaces;

public interface IPublicNewConnectionOtpService
{
    Task<PublicOtpRequestResult> RequestOtpAsync(string mobileNumber, CancellationToken ct = default);

    Task<PublicOtpVerifyResult> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken ct = default);
}
