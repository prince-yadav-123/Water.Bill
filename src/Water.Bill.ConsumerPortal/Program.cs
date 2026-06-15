using Microsoft.AspNetCore.Authorization;
using Water.Bill.Application.DependencyInjection;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.DependencyInjection;
using Water.Bill.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "WaterBill.PublicNewConnection.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(AppConstants.CookieScheme)
    .AddCookie(AppConstants.CookieScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Unauthorized";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "WaterBill.ConsumerPortal.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseWaterBillExceptionHandling();
app.UseSession();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    var isProtectedEndpoint = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
    var isAuthenticatedRequest = context.User.Identity?.IsAuthenticated == true;

    if (isProtectedEndpoint || isAuthenticatedRequest)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
            return Task.CompletedTask;
        });
    }

    await next();
});
app.UseAuthorization();

app.MapGet("/", (HttpContext context) =>
{
    var target = context.User.Identity?.IsAuthenticated == true
        ? "/Consumer/Dashboard"
        : "/Account/Login";

    return Results.Redirect(target);
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
