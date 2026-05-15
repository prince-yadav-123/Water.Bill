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
        if (!Guid.TryParse(roleIdClaim, out var roleId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var (module, action) = Parse(_permission);
        if (!await permissionService.HasPermissionAsync(roleId, module, action))
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }

    private static (string module, string action) Parse(string permission)
    {
        var lastDot = permission.LastIndexOf('.');
        return lastDot > 0
            ? (permission[..lastDot].Trim(), permission[(lastDot + 1)..].Trim())
            : (permission.Trim(), "view");
    }
}
