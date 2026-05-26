namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionStatusChangeRequest
{
    public long ApplicationId { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? ActionBy { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}
