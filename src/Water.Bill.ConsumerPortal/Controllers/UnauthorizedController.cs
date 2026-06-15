using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Core.Common;

namespace Water.Bill.ConsumerPortal.Controllers;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class UnauthorizedController : Controller
{
    [HttpGet("/Unauthorized")]
    public IActionResult Index(string? permission, string? returnUrl)
    {
        ViewData["Title"] = "Access Denied";
        ViewBag.Permission = permission;
        ViewBag.ReturnUrl = returnUrl;
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
