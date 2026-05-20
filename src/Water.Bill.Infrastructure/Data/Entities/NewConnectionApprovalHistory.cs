namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConnectionApprovalHistory
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Remarks { get; set; }

    public Guid? ActionBy { get; set; }

    public string? ActionByName { get; set; }

    public string? ActionByRole { get; set; }

    public DateTime ActionOn { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual NewConnectionApplication Application { get; set; } = null!;
}
