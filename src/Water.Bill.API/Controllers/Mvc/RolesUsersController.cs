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

    [RequirePermission("Roles & Users.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Roles & Users";
        var model = new RolesUsersViewModel
        {
            Roles = await _db.Approles.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct),
            Users = await _db.Appusers.Include(x => x.Role).Where(x => !x.IsDeleted).OrderBy(x => x.FullName).ToListAsync(ct)
        };
        return View(model);
    }

    [HttpGet, RequirePermission("Roles & Users.add")]
    public IActionResult CreateRole()
    {
        ViewData["Title"] = "Create Role";
        return View(new Approle());
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.add")]
    public async Task<IActionResult> CreateRole(Approle model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Role name is required.");
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;
        _db.Approles.Add(model);
        await _db.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RolesUsers, model.Id.ToString(), "Role created.", ct: ct);
        TempData["SuccessMessage"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequirePermission("Roles & Users.edit")]
    public async Task<IActionResult> EditRole(Guid id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Role";
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.edit")]
    public async Task<IActionResult> EditRole(Guid id, Approle model, CancellationToken ct)
    {
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is null) return NotFound();
        role.Name = model.Name;
        role.Description = model.Description;
        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RolesUsers, id.ToString(), "Role updated.", ct: ct);
        TempData["SuccessMessage"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.delete")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken ct)
    {
        var role = await _db.Approles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is not null)
        {
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await _auditLogService.LogAsync(AuditAction.RoleChanged, AppConstants.Modules.RolesUsers, id.ToString(), "Role deleted.", ct: ct);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequirePermission("Roles & Users.add")]
    public async Task<IActionResult> CreateUser(CancellationToken ct)
    {
        ViewData["Title"] = "Create User";
        await PopulateRoles(ct);
        return View(new UserFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.add")]
    public async Task<IActionResult> CreateUser(UserFormViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        if (!ModelState.IsValid)
        {
            await PopulateRoles(ct);
            return View(model);
        }

        var user = new Appuser
        {
            Id = Guid.NewGuid(),
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
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequirePermission("Roles & Users.edit")]
    public async Task<IActionResult> EditUser(Guid id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit User";
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

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.edit")]
    public async Task<IActionResult> EditUser(Guid id, UserFormViewModel model, CancellationToken ct)
    {
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.delete")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is not null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("Roles & Users.view")]
    public async Task<IActionResult> Permissions(Guid? roleId, CancellationToken ct)
    {
        ViewData["Title"] = "Permission Matrix";
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

    [HttpPost, ValidateAntiForgeryToken, RequirePermission("Roles & Users.edit")]
    public async Task<IActionResult> SavePermissions(Guid roleId, CancellationToken ct)
    {
        var modules = await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var incoming = modules.Select(module =>
        {
            var key = module.Id.ToString("N");
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
        => ViewBag.Roles = await _db.Approles.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct);
}
