using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Water.Bill.Core.Common;

namespace Water.Bill.API.Controllers.Mvc;

public class LandingController : Controller
{
    private readonly IConfiguration _configuration;

    public LandingController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet("/")]
    [HttpGet("/Landing")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index()
    {
        var cookieAuth = await HttpContext.AuthenticateAsync(AppConstants.CookieScheme);
        if (cookieAuth.Succeeded)
            return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Noida Water Billing System";
        ViewData["AuthorityLoginUrl"] = _configuration["PortalUrls:AuthorityLogin"] ?? "/Account/Login";
        var consumerLoginUrl = _configuration["PortalUrls:ConsumerLogin"] ?? "https://localhost:7040/Account/Login";
        ViewData["ConsumerLoginUrl"] = consumerLoginUrl;
        ViewData["PublicNewConnectionStartUrl"] = _configuration["PortalUrls:PublicNewConnectionStart"]
            ?? consumerLoginUrl.Replace("/Account/Login", "/NewConnection/Start", StringComparison.OrdinalIgnoreCase);
        return View();
    }
}
