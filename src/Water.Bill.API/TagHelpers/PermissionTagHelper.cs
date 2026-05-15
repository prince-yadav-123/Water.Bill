using System.Security.Claims;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.API.TagHelpers;

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
        if (user?.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(user.FindFirstValue("RoleId"), out var roleId))
        {
            output.SuppressOutput();
            return;
        }

        var lastDot = Permission.LastIndexOf('.');
        var module = lastDot > 0 ? Permission[..lastDot].Trim() : Permission.Trim();
        var action = lastDot > 0 ? Permission[(lastDot + 1)..].Trim() : "view";
        if (!await _permissionService.HasPermissionAsync(roleId, module, action))
            output.SuppressOutput();
        else
            output.Attributes.RemoveAll("require-permission");
    }
}
