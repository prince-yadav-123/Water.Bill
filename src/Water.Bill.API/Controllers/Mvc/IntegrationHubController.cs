using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class IntegrationHubController : Controller
{
    private readonly ICommunicationConfigurationService _communicationConfigurationService;
    private readonly IAuditLogService _auditLogService;

    public IntegrationHubController(
        ICommunicationConfigurationService communicationConfigurationService,
        IAuditLogService auditLogService)
    {
        _communicationConfigurationService = communicationConfigurationService;
        _auditLogService = auditLogService;
    }

    [HttpGet, RequirePermission("Integration Hub.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Integration Hub";
        ViewData["ActiveMenu"] = AppConstants.Modules.IntegrationHub;
        var channels = await _communicationConfigurationService.GetAllAsync(ct);
        return View(channels);
    }

    [HttpGet, RequirePermission("Integration Hub.edit")]
    public async Task<IActionResult> Edit(string channel, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Integration Hub";
        ViewData["ActiveMenu"] = AppConstants.Modules.IntegrationHub;

        if (string.IsNullOrWhiteSpace(channel))
            return RedirectToAction(nameof(Index));

        try
        {
            return View(await _communicationConfigurationService.GetAsync(channel, ct));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Integration Hub.edit")]
    public async Task<IActionResult> Edit(string channel, CommunicationChannelSettingsDto model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Integration Hub";
        ViewData["ActiveMenu"] = AppConstants.Modules.IntegrationHub;

        model.ChannelName = string.IsNullOrWhiteSpace(channel) ? model.ChannelName : channel;

        if (string.IsNullOrWhiteSpace(model.ChannelName))
            ModelState.AddModelError(nameof(model.ChannelName), "Channel name is required.");

        if (string.IsNullOrWhiteSpace(model.ConfigurationJson))
            ModelState.AddModelError(nameof(model.ConfigurationJson), "Configuration JSON is required.");

        if (!ModelState.IsValid)
        {
            model.DisplayName = GetDisplayName(model.ChannelName);
            return View(model);
        }

        try
        {
            var saved = await _communicationConfigurationService.SaveAsync(
                model,
                CurrentUserId(),
                CurrentUserName(),
                ct);

            await _auditLogService.LogAsync(
                AuditAction.CommunicationSettingsChanged,
                AppConstants.Modules.IntegrationHub,
                saved.Id.ToString(),
                $"Communication settings updated for {saved.ChannelName}.",
                ct: ct);

            TempData["SuccessMessage"] = $"{saved.DisplayName} settings saved.";
            return RedirectToAction(nameof(Edit), new { channel = saved.ChannelName });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(model.ConfigurationJson), ex.Message);
            model.DisplayName = GetDisplayName(model.ChannelName);
            return View(model);
        }
    }

    private int? CurrentUserId()
        => int.TryParse(User.FindFirstValue(AppConstants.Claims.UserId) ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var value) ? value : null;

    private string? CurrentUserName()
        => User.FindFirstValue(AppConstants.Claims.Username)
           ?? User.FindFirstValue(ClaimTypes.Name)
           ?? User.Identity?.Name
           ?? "Admin";

    private static string GetDisplayName(string? channelName)
    {
        var normalized = channelName?.Trim();
        if (string.Equals(normalized, CommunicationChannels.Email, StringComparison.OrdinalIgnoreCase))
            return "Email";
        if (string.Equals(normalized, CommunicationChannels.Sms, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Text Message", StringComparison.OrdinalIgnoreCase))
            return "SMS / Text Message";
        if (string.Equals(normalized, CommunicationChannels.WhatsApp, StringComparison.OrdinalIgnoreCase))
            return "WhatsApp";

        return normalized ?? "Communication";
    }
}
