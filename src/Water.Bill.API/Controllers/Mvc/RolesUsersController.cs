using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.DTOs.Menu;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class RolesUsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;

    public RolesUsersController(ApplicationDbContext db, IPermissionService permissionService, IAuditLogService auditLogService)
    {
        _db = db;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
    }

    public IActionResult Index()
        => RedirectToAction(nameof(Roles));

    [HttpGet("/Roles")]
    [RequirePermission("Role Management.view")]
    public async Task<IActionResult> Roles(CancellationToken ct)
    {
        ViewData["Title"] = "Role Management";
        ViewData["ActiveMenu"] = "Role Management";
        var roles = await _db.Approles.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct);
        var userCounts = await _db.Appusers
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.RoleId)
            .Select(x => new { RoleId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, ct);

        ViewBag.UserCounts = userCounts;
        return View("Roles", roles);
    }

    [HttpGet("/Users")]
    [RequirePermission("User Management.view")]
    public async Task<IActionResult> Users(CancellationToken ct)
    {
        ViewData["Title"] = "User Management";
        ViewData["ActiveMenu"] = "User Management";
        var users = await _db.Appusers
            .Include(x => x.Role)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .ToListAsync(ct);

        return View("Users", users);
    }

    [HttpGet("/RolesUsers/Combined")]
    [RequirePermission("Role Management.view")]
    public async Task<IActionResult> Combined(CancellationToken ct)
    {
        ViewData["Title"] = "Roles & Users";
        var model = new RolesUsersViewModel
        {
            Roles = await _db.Approles.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct),
            Users = await _db.Appusers.Include(x => x.Role).Where(x => !x.IsDeleted).OrderBy(x => x.FullName).ToListAsync(ct)
        };
        return View(model);
    }

    [HttpGet("/Roles/Create"), RequirePermission("Role Management.add")]
    public IActionResult CreateRole()
    {
        ViewData["Title"] = "Create Role";
        ViewData["ActiveMenu"] = "Role Management";
        return View(new Approle());
    }

    [HttpPost("/Roles/Create"), ValidateAntiForgeryToken, RequirePermission("Role Management.add")]
    public async Task<IActionResult> CreateRole(Approle model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Role Management";
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Role name is required.");
        if (!ModelState.IsValid) return View(model);

        model.CreatedAt = DateTime.UtcNow;
        _db.Approles.Add(model);
        await _db.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RoleManagement, model.Id.ToString(), "Role created.", ct: ct);
        TempData["SuccessMessage"] = "Role created.";
        return RedirectToAction(nameof(Roles));
    }

    [HttpGet("/Roles/Edit/{id:int}"), RequirePermission("Role Management.edit")]
    public async Task<IActionResult> EditRole(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Role";
        ViewData["ActiveMenu"] = "Role Management";
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost("/Roles/Edit/{id:int}"), ValidateAntiForgeryToken, RequirePermission("Role Management.edit")]
    public async Task<IActionResult> EditRole(int id, Approle model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Role Management";
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is null) return NotFound();
        role.Name = model.Name;
        role.Description = model.Description;
        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RoleManagement, id.ToString(), "Role updated.", ct: ct);
        TempData["SuccessMessage"] = "Role updated.";
        return RedirectToAction(nameof(Roles));
    }

    [HttpGet("/Roles/Details/{id:int}"), RequirePermission("Role Management.view")]
    public async Task<IActionResult> RoleDetails(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Role Details";
        ViewData["ActiveMenu"] = "Role Management";
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is null) return NotFound();

        ViewBag.UserCount = await _db.Appusers.CountAsync(x => x.RoleId == id && !x.IsDeleted, ct);
        return View("RoleDetails", role);
    }

    [HttpPost("/Roles/Delete/{id:int}"), ValidateAntiForgeryToken, RequirePermission("Role Management.delete")]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken ct)
    {
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is not null)
        {
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RoleManagement, id.ToString(), "Role deleted.", ct: ct);
        }
        return RedirectToAction(nameof(Roles));
    }

    [HttpGet("/Users/Create"), RequirePermission("User Management.add")]
    public async Task<IActionResult> CreateUser(CancellationToken ct)
    {
        ViewData["Title"] = "Create User";
        ViewData["ActiveMenu"] = "User Management";
        await PopulateRoles(ct);
        return View(new UserFormViewModel());
    }

    [HttpPost("/Users/Create"), ValidateAntiForgeryToken, RequirePermission("User Management.add")]
    public async Task<IActionResult> CreateUser(UserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "User Management";
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        if (await IsConsumerRoleAsync(model.RoleId, ct))
            ModelState.AddModelError(nameof(model.RoleId), "Consumer role cannot be assigned to Authority users.");
        if (!ModelState.IsValid)
        {
            await PopulateRoles(ct);
            return View(model);
        }

        var user = new Appuser
        {
            FullName = model.FullName,
            Email = model.Email,
            Username = model.Username,
            PasswordHash = AuthService.HashPassword(model.Password!),
            RoleId = model.RoleId,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _db.Appusers.Add(user);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "User created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("/Users/Edit/{id:int}"), RequirePermission("User Management.edit")]
    public async Task<IActionResult> EditUser(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit User";
        ViewData["ActiveMenu"] = "User Management";
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();
        await PopulateRoles(ct);
        return View(new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username,
            RoleId = user.RoleId,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive == true
        });
    }

    [HttpPost("/Users/Edit/{id:int}"), ValidateAntiForgeryToken, RequirePermission("User Management.edit")]
    public async Task<IActionResult> EditUser(int id, UserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "User Management";
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();
        if (await IsConsumerRoleAsync(model.RoleId, ct))
            ModelState.AddModelError(nameof(model.RoleId), "Consumer role cannot be assigned to Authority users.");
        if (!ModelState.IsValid)
        {
            await PopulateRoles(ct);
            return View(model);
        }
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Username = model.Username;
        user.RoleId = model.RoleId;
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = AuthService.HashPassword(model.Password);
            user.PasswordChangedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "User updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("/Users/Details/{id:int}"), RequirePermission("User Management.view")]
    public async Task<IActionResult> UserDetails(int id, CancellationToken ct)
    {
        ViewData["Title"] = "User Details";
        ViewData["ActiveMenu"] = "User Management";
        var user = await _db.Appusers.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return user is null ? NotFound() : View("UserDetails", user);
    }

    [HttpPost("/Users/Delete/{id:int}"), ValidateAntiForgeryToken, RequirePermission("User Management.delete")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is not null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("/RolePermissions")]
    [HttpGet("/RolesUsers/Permissions")]
    [RequirePermission("Role Permission.view")]
    public async Task<IActionResult> Permissions(int? roleId, CancellationToken ct)
    {
        ViewData["Title"] = "Role Permission";
        ViewData["ActiveMenu"] = "Role Permission";
        var roles = await _db.Approles.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct);
        var selectedRole = roleId.HasValue ? roles.FirstOrDefault(x => x.Id == roleId.Value) : roles.FirstOrDefault();
        var modules = await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        var permissions = selectedRole is null
            ? []
            : await _db.Rolepermissions
                .Include(x => x.PermissionModule)
                .Where(x => x.RoleId == selectedRole.Id && !x.IsDeleted)
                .ToListAsync(ct);
        return View(new PermissionMatrixViewModel
        {
            Roles = roles,
            SelectedRole = selectedRole,
            Permissions = permissions,
            Modules = modules
        });
    }

    [HttpPost("/RolePermissions/Save"), ValidateAntiForgeryToken, RequirePermission("Role Permission.edit")]
    public async Task<IActionResult> SavePermissions(int roleId, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Role Permission";
        var modules = await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var incoming = modules.Select(module =>
        {
            var key = module.Id.ToString();
            bool Has(string action) => Request.Form.ContainsKey($"perm_{key}_{action}");
            return new RolePermissionDto
            {
                RoleId = roleId,
                ModuleId = module.Id,
                Module = module.Name,
                CanSeeMenu = Has("SeeMenu"),
                CanView = Has("View"),
                CanAdd = Has("Add"),
                CanEdit = Has("Edit"),
                CanDelete = Has("Delete"),
                CanDownload = Has("Download"),
                CanExport = Has("Export"),
                CanApprove = Has("Approve"),
                CanForward = Has("Forward"),
                CanPrint = Has("Print")
            };
        });
        await _permissionService.SavePermissionsAsync(roleId, incoming, ct);
        TempData["SuccessMessage"] = "Permissions saved.";
        return RedirectToAction(nameof(Permissions), new { roleId });
    }

    private async Task PopulateRoles(CancellationToken ct)
        => ViewBag.Roles = await _db.Approles
            .Where(x => !x.IsDeleted && x.Name != AppConstants.Roles.Consumer)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    private async Task<bool> IsConsumerRoleAsync(int roleId, CancellationToken ct)
        => await _db.Approles.AnyAsync(x =>
            x.Id == roleId &&
            !x.IsDeleted &&
            x.Name == AppConstants.Roles.Consumer, ct);
}
