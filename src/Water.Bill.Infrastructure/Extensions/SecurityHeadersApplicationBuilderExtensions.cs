using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Water.Bill.Infrastructure.Middleware;

namespace Water.Bill.Infrastructure.Extensions;

public static class SecurityHeadersApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWaterBillSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>(!app.ApplicationServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment());
}
