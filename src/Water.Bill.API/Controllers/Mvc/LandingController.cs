using Microsoft.AspNetCore.Mvc;

namespace Water.Bill.API.Controllers.Mvc;

public class LandingController : Controller
{
    private readonly IConfiguration _configuration;

    public LandingController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet("/")]
    [HttpGet("/Landing")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Noida Water Billing System";
        ViewData["AuthorityLoginUrl"] = _configuration["PortalUrls:AuthorityLogin"] ?? "/Account/Login";
        ViewData["ConsumerLoginUrl"] = _configuration["PortalUrls:ConsumerLogin"] ?? "https://localhost:7040/Account/Login";
        return View();
    }
}
