using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class InAppNotificationSender : IInAppNotificationSender
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public InAppNotificationSender(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<CommunicationSendResult> SendAsync(NotificationRecipient recipient, string title, string message, string purposeKey, string? referenceType, string? referenceId, string? referenceNo, CancellationToken ct = default)
    {
        if (!(_configuration.GetValue<bool?>("Communication:InApp:Enabled") ?? true))
            return CommunicationSendResult.Skipped("In-app notifications are disabled.");

        if (recipient.UserId is null || string.IsNullOrWhiteSpace(recipient.UserType))
            return CommunicationSendResult.Skipped("In-app recipient user was not provided.");

        _db.InAppNotifications.Add(new InAppNotification
        {
            UserType = recipient.UserType,
            UserId = recipient.UserId.Value,
            Title = title,
            Message = message,
            PurposeKey = purposeKey,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNo = referenceNo,
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync(ct);
        return CommunicationSendResult.Sent();
    }
}

