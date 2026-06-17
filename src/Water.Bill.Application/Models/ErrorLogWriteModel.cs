using Water.Bill.Core.Common;

namespace Water.Bill.Application.Models;

public sealed class ErrorLogWriteModel
{
    public DateTime CreatedAt { get; init; } = AppTime.IndiaNow;
    public string ExceptionType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? StackTrace { get; init; }
    public string? RequestPath { get; init; }
    public string? HttpMethod { get; init; }
    public string? QueryString { get; init; }
    public int StatusCode { get; init; }
    public string? IpAddress { get; init; }
    public string? Username { get; init; }
    public string? UserId { get; init; }
    public string PortalType { get; init; } = string.Empty;
    public string? UserAgent { get; init; }
    public string? ControllerName { get; init; }
    public string? ActionName { get; init; }
    public string? TraceId { get; init; }
    public bool IsHandled { get; init; }
}
