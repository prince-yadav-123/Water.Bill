namespace Water.Bill.Application.DTOs.Communication;

public sealed class NotificationRecipient
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? UserType { get; set; }
    public long? UserId { get; set; }

    public NotificationRecipient()
    {
    }

    public NotificationRecipient(long userId, string? name, string? email)
    {
        UserId = userId;
        Name = name;
        Email = email;
    }

    public NotificationRecipient(long userId, string? name, string? email, string? mobile, string? userType)
    {
        UserId = userId;
        Name = name;
        Email = email;
        Mobile = mobile;
        UserType = userType;
    }
}

public sealed class NotificationChannelOptions
{
    public bool Email { get; set; }
    public bool Sms { get; set; }
    public bool WhatsApp { get; set; }
    public bool InApp { get; set; }

    public static NotificationChannelOptions None => new();

    public static NotificationChannelOptions For(params string[] channels)
    {
        var options = new NotificationChannelOptions();
        foreach (var channel in channels)
        {
            if (string.Equals(channel, CommunicationChannels.Email, StringComparison.OrdinalIgnoreCase))
                options.Email = true;
            if (string.Equals(channel, CommunicationChannels.Sms, StringComparison.OrdinalIgnoreCase))
                options.Sms = true;
            if (string.Equals(channel, CommunicationChannels.WhatsApp, StringComparison.OrdinalIgnoreCase))
                options.WhatsApp = true;
            if (string.Equals(channel, CommunicationChannels.InApp, StringComparison.OrdinalIgnoreCase))
                options.InApp = true;
        }

        return options;
    }
}

public static class CommunicationChannels
{
    public const string Email = "Email";
    public const string Sms = "SMS";
    public const string WhatsApp = "WhatsApp";
    public const string InApp = "InApp";

    public static readonly IReadOnlyList<string> All = [Email, Sms, WhatsApp, InApp];
}

public static class CommunicationPurposes
{
    public const string AuthorityLoginOtp = "AuthorityLoginOtp";
    public const string ConsumerOtp = "ConsumerOtp";
    public const string PublicNewConnectionOtp = "PublicNewConnectionOtp";
    public const string NewConnectionSubmitted = "NewConnectionSubmitted";
    public const string NewConnectionApproved = "NewConnectionApproved";
    public const string FinalConsumerCreated = "FinalConsumerCreated";
    public const string ApprovalStageAssigned = "ApprovalStageAssigned";
    public const string QueryRaised = "QueryRaised";
    public const string QueryResolved = "QueryResolved";
    public const string ChallanGenerated = "ChallanGenerated";
    public const string PaymentSuccess = "PaymentSuccess";
    public const string PaymentFailed = "PaymentFailed";
    public const string NdcSubmitted = "NdcSubmitted";
    public const string NdcApproved = "NdcApproved";
}
