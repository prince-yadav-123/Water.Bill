namespace Water.Bill.Application.DTOs.Consumer;

public class ConsumerAccountLoginResult
{
    public int Id { get; set; }
    public string ConsumerNo { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? MobileNo { get; set; }
    public string? Username { get; set; }
    public int? ConsumerRoleId { get; set; }
}

