using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;

namespace Water.Bill.ConsumerPortal.Controllers;

public abstract class ConsumerPortalControllerBase : Controller
{
    private readonly IErrorLogService _errorLogService;

    protected ConsumerPortalControllerBase(IErrorLogService errorLogService)
    {
        _errorLogService = errorLogService;
    }

    protected Task LogHandledErrorAsync(Exception exception, int statusCode = StatusCodes.Status400BadRequest, CancellationToken ct = default)
    {
        var model = new ErrorLogWriteModel
        {
            CreatedAt = AppTime.IndiaNow,
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.ToString(),
            RequestPath = $"{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
            HttpMethod = HttpContext.Request.Method,
            QueryString = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : null,
            StatusCode = statusCode,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Username = User.Identity?.Name,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            PortalType = AppConstants.PortalTypes.Consumer,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            ControllerName = ControllerContext.ActionDescriptor.ControllerName,
            ActionName = ControllerContext.ActionDescriptor.ActionName,
            TraceId = HttpContext.TraceIdentifier,
            IsHandled = true
        };

        return _errorLogService.TryLogAsync(model, ct);
    }
}
