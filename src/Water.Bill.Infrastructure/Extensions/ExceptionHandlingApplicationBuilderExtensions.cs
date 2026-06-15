using Microsoft.AspNetCore.Builder;
using Water.Bill.Infrastructure.Middleware;

namespace Water.Bill.Infrastructure.Extensions;

public static class ExceptionHandlingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWaterBillExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionLoggingMiddleware>();
}
