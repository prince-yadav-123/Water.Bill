using Microsoft.AspNetCore.Http;

namespace Water.Bill.Infrastructure.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'; " +
        "object-src 'none'; " +
        "img-src 'self' data: blob: https:; " +
        "font-src 'self' data: https:; " +
        "style-src 'self' 'unsafe-inline' https:; " +
        "script-src 'self' 'unsafe-inline' https:; " +
        "connect-src 'self' https: ws: wss:; " +
        "frame-src 'self' https:;";

    private readonly RequestDelegate _next;
    private readonly bool _enableHsts;

    public SecurityHeadersMiddleware(RequestDelegate next, bool enableHsts)
    {
        _next = next;
        _enableHsts = enableHsts;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            headers["Content-Security-Policy"] = ContentSecurityPolicy;

            if (_enableHsts)
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
