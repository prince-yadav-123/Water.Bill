using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.API.Filters;

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
            context.Result = BuildForbiddenResult(context, _permission);
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var (module, action) = Parse(_permission);
        if (!await permissionService.HasPermissionAsync(roleId, module, action))
            context.Result = BuildForbiddenResult(context, $"{module}.{action}");
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
                message = "You do not have permission to access this module."
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
}
