using System.Security.Claims;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.ConsumerPortal.TagHelpers;

[HtmlTargetElement(Attributes = "require-permission")]
public class PermissionTagHelper : TagHelper
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    [HtmlAttributeName("require-permission")]
    public string Permission { get; set; } = string.Empty;

    public PermissionTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            output.SuppressOutput();
            return;
        }

        var roleIdClaim = user.FindFirstValue("RoleId");
        if (!int.TryParse(roleIdClaim, out var roleId))
        {
            output.SuppressOutput();
            return;
        }

        var (module, action) = ParsePermission(Permission);
        if (!await _permissionService.HasPermissionAsync(roleId, module, action))
        {
            output.SuppressOutput();
            return;
        }

        output.Attributes.RemoveAll("require-permission");
    }

    private static (string module, string action) ParsePermission(string permission)
    {
        var lastDot = permission.LastIndexOf('.');
        return lastDot > 0
            ? (permission[..lastDot].Trim(), permission[(lastDot + 1)..].Trim())
            : (permission.Trim(), "view");
    }
}
