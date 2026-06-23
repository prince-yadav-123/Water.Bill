namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CommunicationChannelSetting
{
    public int Id { get; set; }

    public string ChannelName { get; set; } = null!;

    public bool IsEnabled { get; set; } = true;

    public string ConfigurationJson { get; set; } = "{}";

    public int? CreatedByUserId { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public string? UpdatedByName { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
