namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ErrorLog
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ExceptionType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? StackTrace { get; set; }

    public string? RequestPath { get; set; }

    public string? HttpMethod { get; set; }

    public string? QueryString { get; set; }

    public int StatusCode { get; set; }

    public string? IpAddress { get; set; }

    public string? Username { get; set; }

    public string? UserId { get; set; }

    public string PortalType { get; set; } = null!;

    public string? UserAgent { get; set; }

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public string? TraceId { get; set; }

    public bool IsHandled { get; set; }
}
