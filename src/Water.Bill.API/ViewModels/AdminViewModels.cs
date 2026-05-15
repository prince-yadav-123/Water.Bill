using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.DTOs.Menu;
using Water.Bill.Application.DTOs.Security;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.API.ViewModels;

public class RolesUsersViewModel
{
    public IReadOnlyList<Approle> Roles { get; set; } = [];
    public IReadOnlyList<Appuser> Users { get; set; } = [];
}

public class UserFormViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public Guid RoleId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PermissionMatrixViewModel
{
    public IReadOnlyList<Approle> Roles { get; set; } = [];
    public Approle? SelectedRole { get; set; }
    public IReadOnlyList<Rolepermission> Permissions { get; set; } = [];
    public IReadOnlyList<string> Modules { get; set; } = [];
}

public class MenuIndexViewModel
{
    public IReadOnlyList<Menuitem> Items { get; set; } = [];
}

public class MenuFormViewModel
{
    public Menuitem Item { get; set; } = new();
    public IReadOnlyList<Menuitem> ParentItems { get; set; } = [];
}

public class SecuritySettingsViewModel
{
    public SecuritySettingsDto Settings { get; set; } = new();
}
