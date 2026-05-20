namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionApprovalHistoryDto
{
    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public DateTime ActionOn { get; set; }
}
