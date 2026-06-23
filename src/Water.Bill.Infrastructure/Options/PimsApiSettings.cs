namespace Water.Bill.Infrastructure.Options;

public class PimsApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string GetDetailsByRidEndpoint { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
