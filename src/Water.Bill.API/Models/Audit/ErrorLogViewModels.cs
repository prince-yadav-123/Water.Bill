namespace Water.Bill.API.Models.Audit;

public class ErrorLogIndexViewModel
{
    public string? Search { get; set; }
    public string? ExceptionType { get; set; }
    public int? StatusCode { get; set; }
    public string? PortalType { get; set; }
    public string? Username { get; set; }
    public bool? IsHandled { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public IReadOnlyList<string> ExceptionTypes { get; set; } = [];
    public IReadOnlyList<ErrorLogRowViewModel> Rows { get; set; } = [];
}

public class ErrorLogRowViewModel
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RequestPath { get; set; }
    public int StatusCode { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public bool IsHandled { get; set; }
}

public class ErrorLogDetailsViewModel
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? QueryString { get; set; }
    public int StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? Username { get; set; }
    public string? UserId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string? TraceId { get; set; }
    public bool IsHandled { get; set; }
}
