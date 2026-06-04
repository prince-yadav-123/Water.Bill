namespace Water.Bill.API.Models.Audit;

public class OperatorAuditIndexViewModel
{
    public string? Search { get; set; }
    public string? Module { get; set; }
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 0;
    public IReadOnlyList<OperatorAuditRowViewModel> Rows { get; set; } = [];
}

public class OperatorAuditRowViewModel
{
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public int Action { get; set; }
    public string? Module { get; set; }
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
    public bool Success { get; set; }
}
