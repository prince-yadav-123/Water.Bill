namespace Water.Bill.Application.Interfaces;

public interface IConsumerSmsSender
{
    Task SendOtpAsync(string mobileNo, string otp, DateTime expiresAt, CancellationToken ct = default);
}
