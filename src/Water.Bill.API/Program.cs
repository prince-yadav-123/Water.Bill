using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Water.Bill.API.Extensions;
using Water.Bill.API.Security;
using Water.Bill.Application.DependencyInjection;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.DependencyInjection;
using Water.Bill.Infrastructure.Extensions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
    var requireHttpsCookies = builder.Configuration.GetValue<bool>("WebSecurity:RequireHttpsCookies");
    var secureCookiePolicy = requireHttpsCookies
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;

    builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<AuthoritySessionCookieEvents>();
    builder.Services.AddControllersWithViews();
    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.Name = "WaterBill.Authority.AntiForgery";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = secureCookiePolicy;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.HeaderName = "X-CSRF-TOKEN";
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthentication()
        .AddCookie(AppConstants.CookieScheme, options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Unauthorized";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "WaterBill.Authority.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = secureCookiePolicy;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.EventsType = typeof(AuthoritySessionCookieEvents);
        });
    builder.Services.AddHealthChecks();
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseForwardedHeaders();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseWaterBillSecurityHeaders();
    app.UseWaterBillExceptionHandling();
    app.UseAuthentication();
    app.Use(async (context, next) =>
    {
        var endpoint = context.GetEndpoint();
        var isProtectedEndpoint = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
        var isAuthenticatedRequest = context.User.Identity?.IsAuthenticated == true;
        var isAuthorityNavigationPage =
            context.Request.Path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/Landing", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/Unauthorized", StringComparison.OrdinalIgnoreCase);

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
