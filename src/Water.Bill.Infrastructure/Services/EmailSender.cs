using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CommunicationSendResult> SendAsync(string toEmail, string? toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var section = _configuration.GetSection("Communication:Email");
        var provider = section["Provider"];
        var host = section["Host"];
        var fromEmail = section["FromEmail"];
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(provider)) missing.Add("Provider");
            if (string.IsNullOrWhiteSpace(host)) missing.Add("Host");
            if (string.IsNullOrWhiteSpace(fromEmail)) missing.Add("FromEmail");

            var reason = $"Email provider is not configured. Missing: {string.Join(", ", missing)}.";
            _logger.LogWarning(
                "Email send skipped. Recipient={RecipientEmail}, Subject={Subject}, MissingConfig={MissingConfig}",
                toEmail,
                subject,
                string.Join(", ", missing));
            return CommunicationSendResult.Skipped(reason);
        }

        if (!string.Equals(provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Email send skipped. Unsupported provider {Provider} for recipient {RecipientEmail}.",
                provider,
                toEmail);
            return CommunicationSendResult.Skipped($"Email provider '{provider}' is not supported yet.");
        }

        var port = int.TryParse(section["Port"], out var configuredPort) ? configuredPort : 587;
        var enableSsl = bool.TryParse(section["EnableSsl"], out var ssl) && ssl;
        var username = section["Username"];

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, section["FromName"] ?? "Noida Water Billing"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            var password = section["Password"];
            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, password);

            await client.SendMailAsync(message, ct);
            _logger.LogInformation(
                "Email sent successfully. Host={Host}, Port={Port}, EnableSsl={EnableSsl}, FromEmail={FromEmail}, RecipientEmail={RecipientEmail}, Subject={Subject}",
                host,
                port,
                enableSsl,
                fromEmail,
                toEmail,
                subject);
            return CommunicationSendResult.Sent();
        }
        catch (Exception ex)
        {
            var combinedMessage = ex.InnerException is null
                ? ex.Message
                : $"{ex.Message} | Inner: {ex.InnerException.Message}";

            _logger.LogError(
                ex,
                "Email send failed. Host={Host}, Port={Port}, EnableSsl={EnableSsl}, FromEmail={FromEmail}, RecipientEmail={RecipientEmail}, Subject={Subject}, UsernameConfigured={UsernameConfigured}",
                host,
                port,
                enableSsl,
                fromEmail,
                toEmail,
                subject,
                !string.IsNullOrWhiteSpace(username));

            return CommunicationSendResult.Failed(combinedMessage);
        }
    }
}
