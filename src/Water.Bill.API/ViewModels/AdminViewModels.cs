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
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int RoleId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ConsumerLoginUserListItemViewModel
{
    public int Id { get; set; }
    public string ConsumerNo { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string? ConsumerMobileNo { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class ConsumerLoginUserFormViewModel
{
    public int Id { get; set; }
    public string ConsumerNo { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ConsumerName { get; set; }
    public string? ConsumerMobileNo { get; set; }
}

public class ConsumerLoginUserDetailsViewModel
{
    public int Id { get; set; }
    public string ConsumerNo { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string? ConsumerMobileNo { get; set; }
    public string? ConsumerEmail { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PermissionMatrixViewModel
{
    public IReadOnlyList<Approle> Roles { get; set; } = [];
    public Approle? SelectedRole { get; set; }
    public IReadOnlyList<Rolepermission> Permissions { get; set; } = [];
    public IReadOnlyList<PermissionModule> Modules { get; set; } = [];
}

public class MenuIndexViewModel
{
    public IReadOnlyList<Menuitem> Items { get; set; } = [];
}

public class MenuFormViewModel
{
    public Menuitem Item { get; set; } = new();
    public IReadOnlyList<Menuitem> ParentItems { get; set; } = [];
    public IReadOnlyList<PermissionModule> PermissionModules { get; set; } = [];
}

public class MenuReorderViewModel
{
    public int Id { get; set; }
    public int Order { get; set; }
    public int? ParentId { get; set; }
}

public class SecuritySettingsViewModel
{
    public SecuritySettingsDto Settings { get; set; } = new();
}

public class MasterListViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<MasterColumnViewModel> Columns { get; set; } = [];
    public IReadOnlyList<MasterRowViewModel> Rows { get; set; } = [];
}

public class MasterColumnViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class MasterRowViewModel
{
    public string Key { get; set; } = string.Empty;
    public Dictionary<string, string?> Values { get; set; } = [];
    public bool IsActive { get; set; }
}

public class MasterFormViewModel
{
    public string Key { get; set; } = string.Empty;
    public string? RowKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsEdit { get; set; }
    public IReadOnlyList<MasterFieldViewModel> Fields { get; set; } = [];
}

public class MasterFieldViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string InputType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public bool IsReadOnly { get; set; }
    public IReadOnlyList<MasterOptionViewModel> Options { get; set; } = [];
}

public class MasterOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

