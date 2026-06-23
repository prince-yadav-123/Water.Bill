using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ICommunicationConfigurationService _communicationConfigurationService;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ICommunicationConfigurationService communicationConfigurationService, ILogger<EmailSender> logger)
    {
        _communicationConfigurationService = communicationConfigurationService;
        _logger = logger;
    }

    public async Task<CommunicationSendResult> SendAsync(string toEmail, string? toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var settings = await _communicationConfigurationService.GetEmailSettingsAsync(ct);
        var isEnabled = settings.IsEnabled;
        if (!isEnabled)
        {
            _logger.LogInformation(
                "Email send skipped because Communication:Email:Enabled is false. Recipient={RecipientEmail}, Subject={Subject}",
                toEmail,
                subject);
            return CommunicationSendResult.Skipped("Email sending is disabled in configuration.");
        }

        var provider = settings.Provider;
        var host = settings.Host;
        var fromEmail = settings.FromEmail;
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

        var port = settings.Port;
        var enableSsl = settings.EnableSsl;
        var username = settings.Username;

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, string.IsNullOrWhiteSpace(settings.FromName) ? "Noida Water Billing" : settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            var password = settings.Password;
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
