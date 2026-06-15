using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Enums;

namespace Water.Bill.ConsumerPortal.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission) => _permission = permission;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var roleIdClaim = user.FindFirstValue("RoleId");
        if (!int.TryParse(roleIdClaim, out var roleId))
        {
            await LogUnauthorizedAttemptAsync(context, _permission);
            context.Result = BuildForbiddenResult(context, _permission);
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var (module, action) = Parse(_permission);
        if (!await permissionService.HasPermissionAsync(roleId, module, action))
        {
            await LogUnauthorizedAttemptAsync(context, $"{module}.{action}");
            context.Result = BuildForbiddenResult(context, $"{module}.{action}");
        }
    }

    private static (string module, string action) Parse(string permission)
    {
        var lastDot = permission.LastIndexOf('.');
        return lastDot > 0
            ? (permission[..lastDot].Trim(), permission[(lastDot + 1)..].Trim())
            : (permission.Trim(), "view");
    }

    private static IActionResult BuildForbiddenResult(AuthorizationFilterContext context, string permission)
    {
        var request = context.HttpContext.Request;
        var acceptHeader = request.Headers.Accept.ToString();
        var isAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
            || (acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                && !acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase));

        if (isAjax)
        {
            return new JsonResult(new
            {
                error = "Forbidden",
                message = "You do not have permission to access this service."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var returnUrl = $"{request.Path}{request.QueryString}";
        return new RedirectToActionResult("Index", "Unauthorized", new
        {
            permission,
            returnUrl
        });
    }

    private static async Task LogUnauthorizedAttemptAsync(AuthorizationFilterContext context, string permission)
    {
        try
        {
            var auditLogService = context.HttpContext.RequestServices.GetRequiredService<IAuditLogService>();
            var request = context.HttpContext.Request;
            await auditLogService.LogAsync(
                AuditAction.PermissionChanged,
                module: "Authorization",
                details: $"Blocked direct access to {request.Path}{request.QueryString} for permission {permission}.",
                success: false,
                ct: context.HttpContext.RequestAborted);
        }
        catch
        {
            // Never let audit logging failure block the permission restriction flow.
        }
    }
}
