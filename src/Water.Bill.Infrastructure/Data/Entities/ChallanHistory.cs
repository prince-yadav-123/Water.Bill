namespace Water.Bill.Infrastructure.Data.Entities;

public class ChallanHistory
{
    public long Id { get; set; }

    public long ChallanId { get; set; }

    public string? ChallanNo { get; set; }

    public string? ConsumerNo { get; set; }

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public int? ActionByUserId { get; set; }

    public string? ActionByName { get; set; }

    public DateTime ActionOn { get; set; }

    public bool IsDeleted { get; set; }
}
