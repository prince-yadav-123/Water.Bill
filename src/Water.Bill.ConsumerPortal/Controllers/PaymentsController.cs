using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Water.Bill.Application.DTOs.Payments;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.ConsumerPortal.Controllers;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("Payments")]
public class PaymentsController : Controller
{
    private readonly IConsumerPaymentService _paymentService;

    public PaymentsController(IConsumerPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [AcceptVerbs("GET", "POST")]
    [Route("AxisResponse")]
    public async Task<IActionResult> AxisResponse([FromForm] string? msg, [FromQuery] string? q, CancellationToken ct)
    {
        var rawMessage = string.IsNullOrWhiteSpace(msg) ? q : msg;
        var result = await _paymentService.ProcessAxisResponseAsync(rawMessage ?? string.Empty, BuildActorContext(), ct);

        TempData[result.Success ? "SuccessMessage" : result.IsPending ? "InfoMessage" : "ErrorMessage"] =
            result.Message ?? (result.Success ? "Payment processed successfully." : "Payment could not be completed.");

        if (string.Equals(result.PaymentKind, PaymentReferenceKinds.Bill, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Current", "Bills");

        if (string.Equals(result.PaymentKind, PaymentReferenceKinds.Challan, StringComparison.OrdinalIgnoreCase)
            && result.LocalEntityId.HasValue)
        {
            return RedirectToAction(
                result.Success ? "Success" : "Details",
                "Challans",
                new { id = result.LocalEntityId.Value });
        }

        if (string.Equals(result.PaymentKind, PaymentReferenceKinds.NewConnection, StringComparison.OrdinalIgnoreCase)
            && result.LocalEntityId.HasValue)
        {
            if (result.IsPublicFlow)
            {
                return RedirectToAction(
                    result.Success ? "Details" : "Payment",
                    "PublicNewConnection",
                    new { id = result.LocalEntityId.Value });
            }

            return RedirectToAction(
                result.Success ? "Details" : "Payment",
                "NewConnection",
                new { id = result.LocalEntityId.Value });
        }

        return Redirect("/");
    }

    private PaymentActorContextDto BuildActorContext()
        => new()
        {
            UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
            UserName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "Payment Gateway",
            UserRole = User.FindFirstValue(ClaimTypes.Role),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };
}
