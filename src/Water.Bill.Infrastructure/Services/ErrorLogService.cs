using Microsoft.Extensions.Logging;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(ApplicationDbContext db, ILogger<ErrorLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> TryLogAsync(ErrorLogWriteModel model, CancellationToken ct = default)
    {
        try
        {
            _db.ErrorLogs.Add(new ErrorLog
            {
                CreatedAt = model.CreatedAt,
                ExceptionType = Trim(model.ExceptionType, 200) ?? "Exception",
                Message = Trim(SensitiveDataRedactionHelper.Redact(model.Message), 2000) ?? "Unhandled exception occurred.",
                StackTrace = SensitiveDataRedactionHelper.Redact(model.StackTrace),
                RequestPath = Trim(SensitiveDataRedactionHelper.Redact(model.RequestPath), 500),
                HttpMethod = Trim(model.HttpMethod, 10),
                QueryString = Trim(SensitiveDataRedactionHelper.Redact(model.QueryString), 2000),
                StatusCode = model.StatusCode,
                IpAddress = Trim(model.IpAddress, 64),
                Username = Trim(model.Username, 150),
                UserId = Trim(model.UserId, 100),
                PortalType = Trim(model.PortalType, 20) ?? "Unknown",
                UserAgent = Trim(model.UserAgent, 1000),
                ControllerName = Trim(model.ControllerName, 150),
                ActionName = Trim(model.ActionName, 150),
                TraceId = Trim(model.TraceId, 100),
                IsHandled = model.IsHandled
            });

            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist error log for path {Path}", SensitiveDataRedactionHelper.Redact(model.RequestPath));
            return false;
        }
    }

    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
