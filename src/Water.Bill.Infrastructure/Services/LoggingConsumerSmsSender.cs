using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class LoggingConsumerSmsSender : IConsumerSmsSender
{
    private readonly ILogger<LoggingConsumerSmsSender> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ISmsSender _smsSender;

    public LoggingConsumerSmsSender(ILogger<LoggingConsumerSmsSender> logger, IHostEnvironment environment, ISmsSender smsSender)
    {
        _logger = logger;
        _environment = environment;
        _smsSender = smsSender;
    }

    public async Task SendOtpAsync(string mobileNo, string otp, DateTime expiresAt, CancellationToken ct = default)
    {
        var message = $"Your Water.Bill OTP is {otp}. It is valid until {expiresAt:dd MMM yyyy hh:mm tt}. Do not share it with anyone.";
        var result = await _smsSender.SendAsync(mobileNo, message, null, ct);

        if (result.Status == "Sent")
        {
            _logger.LogInformation("Consumer Portal OTP sent for {MobileNo}. Expires at {ExpiresAt:u}", mobileNo, expiresAt);
            return;
        }

        if (_environment.IsDevelopment())
        {
            _logger.LogInformation(
                "Consumer Portal OTP dispatch completed with status {Status} for {MobileNo}. Details: {Details}",
                result.Status,
                mobileNo,
                result.ErrorMessage ?? "none");
            return;
        }

        _logger.LogWarning(
            "Consumer Portal OTP SMS was not sent to {MobileNo}. Status={Status}, Details={Details}",
            mobileNo,
            result.Status,
            result.ErrorMessage ?? "none");
    }
}
