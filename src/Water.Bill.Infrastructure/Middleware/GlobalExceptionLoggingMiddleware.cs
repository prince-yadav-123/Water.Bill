using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;

namespace Water.Bill.Infrastructure.Middleware;

public class GlobalExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public GlobalExceptionLoggingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionLoggingMiddleware> logger,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment hostEnvironment)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var isHandled = false;
        var portalType = DetectPortalType(context);
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception, "Unhandled exception captured for {Path}. TraceId: {TraceId}", context.Request.Path, traceId);

        using (var scope = _scopeFactory.CreateScope())
        {
            var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
            await errorLogService.TryLogAsync(BuildLogModel(context, exception, statusCode, isHandled, portalType, traceId));
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started before error page could be rendered. TraceId: {TraceId}", traceId);
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        if (WantsJson(context.Request))
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(BuildJsonResponse(traceId));
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(BuildHtmlResponse(
            portalType,
            traceId,
            context.User.Identity?.IsAuthenticated == true,
            _hostEnvironment.ApplicationName ?? string.Empty));
    }

    private ErrorLogWriteModel BuildLogModel(
        HttpContext context,
        Exception exception,
        int statusCode,
        bool isHandled,
        string portalType,
        string traceId)
    {
        var routeValues = context.Request.RouteValues;
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null;
        var username = context.User.Identity?.Name ?? context.User.FindFirstValue("FullName");
        var userId = context.User.FindFirstValue("UserId")
            ?? context.User.FindFirstValue(AppConstants.Claims.UserId)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return new ErrorLogWriteModel
        {
            CreatedAt = DateTime.UtcNow,
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            RequestPath = context.Request.Path.Value,
            HttpMethod = context.Request.Method,
            QueryString = queryString,
            StatusCode = statusCode,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Username = username,
            UserId = userId,
            PortalType = portalType,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            ControllerName = routeValues.TryGetValue("controller", out var controller) ? controller?.ToString() : null,
            ActionName = routeValues.TryGetValue("action", out var action) ? action?.ToString() : null,
            TraceId = traceId,
            IsHandled = isHandled
        };
    }

    private string DetectPortalType(HttpContext context)
    {
        var path = context.Request.Path;
        var appName = _hostEnvironment.ApplicationName ?? string.Empty;
        var isApiRequest = path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);

        if (isApiRequest)
            return AppConstants.PortalTypes.Api;

        if (appName.Contains("ConsumerPortal", StringComparison.OrdinalIgnoreCase))
            return context.User.Identity?.IsAuthenticated == true
                ? AppConstants.PortalTypes.Consumer
                : AppConstants.PortalTypes.Public;

        if (appName.Contains("Water.Bill.API", StringComparison.OrdinalIgnoreCase))
            return context.User.Identity?.IsAuthenticated == true
                ? AppConstants.PortalTypes.Admin
                : AppConstants.PortalTypes.Public;

        return AppConstants.PortalTypes.Unknown;
    }

    private static bool WantsJson(HttpRequest request)
    {
        var accept = request.Headers.Accept.ToString();
        var isAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (isAjax)
            return true;

        if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            && !accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            return true;

        return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildJsonResponse(string traceId)
        => $$"""
        {"error":"ServerError","message":"Something went wrong while processing your request. Please try again later or contact support.","traceId":"{{JsonEscape(traceId)}}"}
        """;

    private static string BuildHtmlResponse(string portalType, string traceId, bool isAuthenticated, string applicationName)
    {
        var isConsumerApp = applicationName.Contains("ConsumerPortal", StringComparison.OrdinalIgnoreCase);
        var palette = portalType switch
        {
            AppConstants.PortalTypes.Admin => ("#0f172a", "#2563eb", "#f8fbff", isAuthenticated ? "/Dashboard" : "/Account/Login", isAuthenticated ? "Go to Dashboard" : "Go to Login"),
            AppConstants.PortalTypes.Consumer => ("#0b3a53", "#0ea5e9", "#f4fbff", isAuthenticated ? "/Consumer/Dashboard" : "/Account/Login", isAuthenticated ? "Go to Dashboard" : "Go to Login"),
            _ => ("#1e293b", "#2563eb", "#f8fafc", isConsumerApp ? "/NewConnection/Start" : "/", isConsumerApp ? "Go to Public Services" : "Go to Home")
        };

        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>Something went wrong</title>
            <style>
                :root {
                    --wb-title: {{palette.Item1}};
                    --wb-accent: {{palette.Item2}};
                    --wb-bg: {{palette.Item3}};
                }
                * { box-sizing: border-box; }
                body {
                    margin: 0;
                    font-family: "Segoe UI", Arial, sans-serif;
                    background: linear-gradient(180deg, var(--wb-bg) 0%, #eef4fb 100%);
                    color: #0f172a;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 24px;
                }
                .wb-error-card {
                    width: min(100%, 640px);
                    background: #fff;
                    border: 1px solid #dbe6f3;
                    border-radius: 20px;
                    padding: 40px 36px;
                    box-shadow: 0 22px 60px rgba(15, 23, 42, 0.10);
                    text-align: center;
                }
                .wb-error-icon {
                    width: 74px;
                    height: 74px;
                    margin: 0 auto 18px;
                    border-radius: 22px;
                    background: color-mix(in srgb, var(--wb-accent) 12%, white);
                    color: var(--wb-accent);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 36px;
                    font-weight: 800;
                }
                h1 {
                    margin: 0 0 10px;
                    font-size: 32px;
                    color: var(--wb-title);
                }
                p {
                    margin: 0;
                    color: #5b6b7e;
                    line-height: 1.65;
                    font-size: 15px;
                }
                .wb-trace {
                    display: inline-flex;
                    margin-top: 22px;
                    padding: 8px 14px;
                    border-radius: 999px;
                    background: #f8fafc;
                    border: 1px solid #e2e8f0;
                    color: #475569;
                    font-size: 12px;
                    font-family: Consolas, "Courier New", monospace;
                }
                .wb-actions {
                    display: flex;
                    justify-content: center;
                    gap: 12px;
                    flex-wrap: wrap;
                    margin-top: 28px;
                }
                .wb-btn {
                    text-decoration: none;
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    min-height: 42px;
                    padding: 0 18px;
                    border-radius: 12px;
                    border: 1px solid #cdd9e8;
                    color: #0f172a;
                    font-weight: 600;
                    font-size: 14px;
                    background: #fff;
                }
                .wb-btn-primary {
                    background: var(--wb-accent);
                    border-color: var(--wb-accent);
                    color: #fff;
                }
                @media (max-width: 640px) {
                    .wb-error-card { padding: 28px 22px; border-radius: 16px; }
                    h1 { font-size: 28px; }
                    .wb-actions { flex-direction: column; }
                    .wb-btn { width: 100%; }
                }
            </style>
        </head>
        <body>
            <div class="wb-error-card">
                <div class="wb-error-icon">!</div>
                <h1>Something went wrong</h1>
                <p>Something went wrong while processing your request. Please try again later or contact support.</p>
                <div class="wb-trace">Trace ID: {{WebUtility.HtmlEncode(traceId)}}</div>
                <div class="wb-actions">
                    <a class="wb-btn" href="javascript:history.back()">Go Back</a>
                    <a class="wb-btn wb-btn-primary" href="{{palette.Item4}}">{{palette.Item5}}</a>
                </div>
            </div>
        </body>
        </html>
        """;
    }

    private static string JsonEscape(string value)
        => string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
