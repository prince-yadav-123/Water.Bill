namespace Water.Bill.Application.DTOs.Communication;

public sealed class CommunicationChannelSettingsDto
{
    public int Id { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public string? LastUpdatedByName { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public bool IsConfiguredInDb { get; set; }
}

public sealed class EmailCommunicationSettings
{
    public bool IsEnabled { get; set; } = true;
    public string Provider { get; set; } = "Smtp";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Noida Water Billing";
    public bool EnableSsl { get; set; } = true;
    public string FooterText { get; set; } = "This is an automated message from Noida Water Billing System.";
}

public sealed class SmsCommunicationSettings
{
    public bool IsEnabled { get; set; } = true;
    public string Provider { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string PeId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "91";
    public string Msg91AuthKey { get; set; } = string.Empty;
    public string Route { get; set; } = "4";
    public string DefaultOtp { get; set; } = string.Empty;
}

public sealed class WhatsAppCommunicationSettings
{
    public bool IsEnabled { get; set; } = true;
    public string Provider { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderPhoneNo { get; set; } = string.Empty;
}
