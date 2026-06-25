using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Water.Bill.API.Filters;
using Water.Bill.API.Models;
using Water.Bill.API.ViewModels;
using Water.Bill.Application.DTOs.Menu;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;
using Water.Bill.Core.Common;
using Water.Bill.Core.Enums;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Extensions;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class RolesUsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;
    private readonly ISessionService _sessionService;
    private readonly IMemoryCache _cache;
    private readonly IExcelExportService _excelExportService;

    public RolesUsersController(
        ApplicationDbContext db,
        IPermissionService permissionService,
        IAuditLogService auditLogService,
        ISessionService sessionService,
        IMemoryCache cache,
        IExcelExportService excelExportService)
    {
        _db = db;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
        _sessionService = sessionService;
        _cache = cache;
        _excelExportService = excelExportService;
    }

    public IActionResult Index()
        => RedirectToAction(nameof(Roles));

    [HttpGet("/Roles")]
    [RequirePermission("Role Management.view")]
    public async Task<IActionResult> Roles(
        string? search = null,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Role Management";
        ViewData["ActiveMenu"] = "Role Management";
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);
        ViewBag.Search = search;

        var query = _db.Approles.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var paged = await query
            .OrderBy(x => x.Name)
            .ToPagedResultAsync(page, pageSize, ct);
        var userCounts = await _db.Appusers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.RoleId)
            .Select(x => new { RoleId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, ct);

        ViewBag.UserCounts = userCounts;
        ViewBag.Pagination = PaginationViewModel.Create(paged);
        return View("Roles", paged.Items);
    }

    [HttpGet("/Users")]
    [RequirePermission("User Management.view")]
    public async Task<IActionResult> Users(
        string? search = null,
        int page = 1,
        int pageSize = 0,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "User Management";
        ViewData["ActiveMenu"] = "User Management";
        pageSize = PagingConstants.Validate(pageSize == 0 ? PagingConstants.DefaultPageSize : pageSize);
        page = PagingConstants.ValidatePage(page);
        ViewBag.Search = search;

        var query = BuildUserListQuery(search);
        var paged = await query.OrderBy(x => x.FullName).ToPagedResultAsync(page, pageSize, ct);
        ViewBag.Pagination = PaginationViewModel.Create(paged);
        return View("Users", paged.Items.ToList());
    }

    [HttpGet("/Users/ExportExcel")]
    [RequirePermission("User Management.download")]
    public async Task<IActionResult> ExportUsersExcel(string? search = null, CancellationToken ct = default)
    {
        var rows = await BuildUserListQuery(search)
            .OrderBy(x => x.FullName)
            .Select(x => new UserExportRow
            {
                FullName = x.FullName,
                Username = x.Username,
                RoleName = x.Role != null ? x.Role.Name : null,
                DivisionDevType = x.DivisionDevType,
                Mobile = x.PhoneNumber,
                Email = x.Email,
                IsActive = x.IsActive == true
            })
            .ToListAsync(ct);

        var bytes = _excelExportService.Export(new ExcelExportRequest<UserExportRow>
        {
            SheetName = "User Management",
            Rows = rows,
            Columns =
            [
                new ExcelColumnDefinition<UserExportRow> { Header = "Full Name", ValueFactory = x => x.FullName, Width = 28 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Username", ValueFactory = x => x.Username, Width = 22 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Role", ValueFactory = x => string.IsNullOrWhiteSpace(x.RoleName) ? "Unassigned" : x.RoleName, Width = 22 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Division", ValueFactory = x => string.IsNullOrWhiteSpace(AppConstants.Divisions.FormatDisplay(x.DivisionDevType)) ? "-" : AppConstants.Divisions.FormatDisplay(x.DivisionDevType), Width = 18 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Mobile", ValueFactory = x => string.IsNullOrWhiteSpace(x.Mobile) ? "-" : x.Mobile, Width = 18 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Email", ValueFactory = x => string.IsNullOrWhiteSpace(x.Email) ? "-" : x.Email, Width = 30 },
                new ExcelColumnDefinition<UserExportRow> { Header = "Status", ValueFactory = x => x.IsActive ? "Active" : "Inactive", Width = 14 }
            ]
        });

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"user-management-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("/RolesUsers/Combined")]
    [RequirePermission("Role Management.view")]
    public async Task<IActionResult> Combined(CancellationToken ct)
    {
        ViewData["Title"] = "Roles & Users";
        var model = new RolesUsersViewModel
        {
            Roles = await GetAuthorityRolesAsync(ct),
            Users = await _db.Appusers.AsNoTracking().Include(x => x.Role).Where(x => !x.IsDeleted).OrderBy(x => x.FullName).ToListAsync(ct)
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
        var role = await _db.Approles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
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
        var role = await _db.Approles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (role is null) return NotFound();

        ViewBag.UserCount = await _db.Appusers.AsNoTracking().CountAsync(x => x.RoleId == id && !x.IsDeleted, ct);
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
        PopulateDivisionOptions();
        return View(new UserFormViewModel());
    }

    [HttpPost("/Users/Create"), ValidateAntiForgeryToken, RequirePermission("User Management.add")]
    public async Task<IActionResult> CreateUser(UserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "User Management";
        NormalizeUserForm(model);
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Password and confirm password do not match.");
        if (!model.DivisionDevType.HasValue)
            ModelState.AddModelError(nameof(model.DivisionDevType), "Division is required.");
        if (await IsConsumerRoleAsync(model.RoleId, ct))
            ModelState.AddModelError(nameof(model.RoleId), "Consumer role cannot be assigned to Authority users.");
        if (!ModelState.IsValid)
        {
            await PopulateRoles(ct);
            PopulateDivisionOptions();
            return View(model);
        }

        var user = new Appuser
        {
            FullName = model.FullName,
            Email = model.Email,
            Username = model.Username,
            PasswordHash = AuthService.HashPassword(model.Password!),
            RoleId = model.RoleId,
            DivisionDevType = model.DivisionDevType,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _db.Appusers.Add(user);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (TryAddUserSaveErrors(ex, model))
        {
            await PopulateRoles(ct);
            PopulateDivisionOptions();
            return View(model);
        }
        TempData["SuccessMessage"] = "User created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("/Users/Edit/{id:int}"), RequirePermission("User Management.edit")]
    public async Task<IActionResult> EditUser(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit User";
        ViewData["ActiveMenu"] = "User Management";
        var user = await _db.Appusers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();
        await PopulateRoles(ct);
        PopulateDivisionOptions();
        return View(new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username,
            RoleId = user.RoleId,
            DivisionDevType = user.DivisionDevType,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive == true
        });
    }

    [HttpPost("/Users/Edit/{id:int}"), ValidateAntiForgeryToken, RequirePermission("User Management.edit")]
    public async Task<IActionResult> EditUser(int id, UserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "User Management";
        NormalizeUserForm(model);
        var user = await _db.Appusers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Password and confirm password do not match.");
        if (!model.DivisionDevType.HasValue)
            ModelState.AddModelError(nameof(model.DivisionDevType), "Division is required.");
        if (await IsConsumerRoleAsync(model.RoleId, ct))
            ModelState.AddModelError(nameof(model.RoleId), "Consumer role cannot be assigned to Authority users.");
        if (!ModelState.IsValid)
        {
            await PopulateRoles(ct);
            PopulateDivisionOptions();
            return View(model);
        }
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Username = model.Username;
        user.RoleId = model.RoleId;
        user.DivisionDevType = model.DivisionDevType;
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        var passwordChanged = false;
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = AuthService.HashPassword(model.Password);
            user.PasswordChangedAt = DateTime.UtcNow;
            passwordChanged = true;
        }
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (TryAddUserSaveErrors(ex, model))
        {
            await PopulateRoles(ct);
            PopulateDivisionOptions();
            return View(model);
        }

        if (passwordChanged)
        {
            await _sessionService.RevokeAllSessionsAsync(user.Id, "PasswordChanged", ct);

            var currentUserIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdText, out var currentUserId) && currentUserId == user.Id)
            {
                await HttpContext.SignOutAsync(AppConstants.CookieScheme);
                Response.Cookies.Delete("WaterBill.Authority.Auth");
                Response.Cookies.Append(
                    "WaterBill.Authority.AuthMessage",
                    Uri.EscapeDataString("Your password was changed. Please login again."),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(5)
                    });

                return RedirectToAction("Login", "Account");
            }
        }

        TempData["SuccessMessage"] = passwordChanged
            ? "User updated. Active sessions have been signed out."
            : "User updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("/Users/Details/{id:int}"), RequirePermission("User Management.view")]
    public async Task<IActionResult> UserDetails(int id, CancellationToken ct)
    {
        ViewData["Title"] = "User Details";
        ViewData["ActiveMenu"] = "User Management";
        var user = await _db.Appusers
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
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
        var roles = await GetAuthorityRolesAsync(ct);
        var selectedRole = roleId.HasValue ? roles.FirstOrDefault(x => x.Id == roleId.Value) : roles.FirstOrDefault();
        var selectedPortalScope = selectedRole is not null && await IsConsumerRoleAsync(selectedRole.Id, ct)
            ? AppConstants.PortalScopes.Consumer
            : AppConstants.PortalScopes.Authority;
        var modules = await _db.PermissionModules
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted && EF.Property<string>(x, "PortalScope") == selectedPortalScope)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        var permissions = selectedRole is null
            ? []
            : await _db.Rolepermissions
                .AsNoTracking()
                .Include(x => x.PermissionModule)
                .Where(x => x.RoleId == selectedRole.Id && !x.IsDeleted)
                .ToListAsync(ct);
        return View(new PermissionMatrixViewModel
        {
            Roles = roles,
            SelectedRole = selectedRole,
            Permissions = permissions,
            Modules = modules,
            SelectedPortalScope = selectedPortalScope,
            SelectedPortalLabel = AppConstants.PortalScopes.DisplayName(selectedPortalScope)
        });
    }

    [HttpPost("/RolePermissions/Save"), ValidateAntiForgeryToken, RequirePermission("Role Permission.edit")]
    public async Task<IActionResult> SavePermissions(int roleId, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Role Permission";
        var portalScope = await IsConsumerRoleAsync(roleId, ct)
            ? AppConstants.PortalScopes.Consumer
            : AppConstants.PortalScopes.Authority;
        var modules = await _db.PermissionModules
            .Where(x => x.IsActive && !x.IsDeleted && EF.Property<string>(x, "PortalScope") == portalScope)
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
        await _permissionService.SavePermissionsAsync(roleId, incoming, modules.Select(x => x.Id).ToArray(), ct);
        TempData["SuccessMessage"] = "Permissions saved.";
        return RedirectToAction(nameof(Permissions), new { roleId });
    }

    private async Task PopulateRoles(CancellationToken ct)
        => ViewBag.Roles = await GetAuthorityRolesAsync(ct);

    private async Task<IReadOnlyList<Approle>> GetAuthorityRolesAsync(CancellationToken ct)
        => await _cache.GetOrCreateAsync("lookup:authority-roles", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Approles
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Name != AppConstants.Roles.Consumer)
                .OrderBy(x => x.Name)
                .ToListAsync(ct);
        }) ?? [];

    private void PopulateDivisionOptions()
        => ViewBag.DivisionOptions = AppConstants.Divisions.Options;

    private static void NormalizeUserForm(UserFormViewModel model)
    {
        model.FullName = model.FullName?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.Username = model.Username?.Trim() ?? string.Empty;
        model.PhoneNumber = model.PhoneNumber?.Trim();
    }

    private bool TryAddUserSaveErrors(DbUpdateException ex, UserFormViewModel model)
    {
        var root = ex.GetBaseException();
        if (root is Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            if (sqlEx.Number is 2601 or 2627)
            {
                var message = sqlEx.Message.Contains("Username", StringComparison.OrdinalIgnoreCase)
                    ? "Username already exists."
                    : sqlEx.Message.Contains("Email", StringComparison.OrdinalIgnoreCase)
                        ? "Email already exists."
                        : "A record with the same values already exists.";
                ModelState.AddModelError(string.Empty, message);
                return true;
            }

            if (sqlEx.Number == 547)
            {
                ModelState.AddModelError(string.Empty, "Could not save user changes because one of the referenced records is no longer available.");
                return true;
            }
        }

        ModelState.AddModelError(string.Empty, "Could not save user changes. Please verify the entered values and try again.");
        return true;
    }

    private async Task<bool> IsConsumerRoleAsync(int roleId, CancellationToken ct)
        => await _db.Approles.AnyAsync(x =>
            x.Id == roleId &&
            !x.IsDeleted &&
            x.Name == AppConstants.Roles.Consumer, ct);

    private IQueryable<Appuser> BuildUserListQuery(string? search)
    {
        var query = _db.Appusers
            .Include(x => x.Role)
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => x.FullName.Contains(s)
                || x.Username.Contains(s)
                || x.Email.Contains(s));
        }

        return query;
    }

    private sealed class UserExportRow
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public int? DivisionDevType { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}
