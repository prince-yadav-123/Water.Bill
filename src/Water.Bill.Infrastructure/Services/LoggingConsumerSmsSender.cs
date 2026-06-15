using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class LoggingConsumerSmsSender : IConsumerSmsSender
{
    private readonly ILogger<LoggingConsumerSmsSender> _logger;
    private readonly IHostEnvironment _environment;

    public LoggingConsumerSmsSender(ILogger<LoggingConsumerSmsSender> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task SendOtpAsync(string mobileNo, string otp, DateTime expiresAt, CancellationToken ct = default)
    {
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("Consumer Portal OTP generated for {MobileNo}. Expires at {ExpiresAt:u}", mobileNo, expiresAt);
        }
        else
        {
            _logger.LogWarning("No SMS provider is configured. Consumer Portal OTP SMS was not sent to {MobileNo}.", mobileNo);
        }

        return Task.CompletedTask;
    }
}
