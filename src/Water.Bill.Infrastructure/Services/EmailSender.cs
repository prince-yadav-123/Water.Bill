using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration) => _configuration = configuration;

    public async Task<CommunicationSendResult> SendAsync(string toEmail, string? toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var section = _configuration.GetSection("Communication:Email");
        var provider = section["Provider"];
        var host = section["Host"];
        var fromEmail = section["FromEmail"];
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            return CommunicationSendResult.Skipped("Email provider is not configured.");

        if (!string.Equals(provider, "Smtp", StringComparison.OrdinalIgnoreCase))
            return CommunicationSendResult.Skipped($"Email provider '{provider}' is not supported yet.");

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

            using var client = new SmtpClient(host, int.TryParse(section["Port"], out var port) ? port : 587)
            {
                EnableSsl = bool.TryParse(section["EnableSsl"], out var ssl) && ssl
            };

            var username = section["Username"];
            var password = section["Password"];
            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, password);

            await client.SendMailAsync(message, ct);
            return CommunicationSendResult.Sent();
        }
        catch (Exception ex)
        {
            return CommunicationSendResult.Failed(ex.Message);
        }
    }
}

