using Serilog;
using Microsoft.AspNetCore.Authorization;
using Water.Bill.API.Extensions;
using Water.Bill.Application.DependencyInjection;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllersWithViews();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthentication()
        .AddCookie(AppConstants.CookieScheme, options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "WaterBill.Authority.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.Use(async (context, next) =>
    {
        var endpoint = context.GetEndpoint();
        var isProtectedEndpoint = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
        var isAuthenticatedRequest = context.User.Identity?.IsAuthenticated == true;
        var isAuthorityNavigationPage =
            context.Request.Path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/Landing", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase);

        if (isProtectedEndpoint || isAuthenticatedRequest || isAuthorityNavigationPage)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
                return Task.CompletedTask;
            });
        }

        await next();
    });
    app.UseAuthorization();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Landing}/{action=Index}/{id?}");
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Water.Bill API terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
