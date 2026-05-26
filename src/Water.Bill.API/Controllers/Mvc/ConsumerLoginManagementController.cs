using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Water.Bill.API.Filters;
using Water.Bill.API.ViewModels;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.API.Controllers.Mvc;

[Authorize(AuthenticationSchemes = AppConstants.CookieScheme)]
public class ConsumerLoginManagementController : Controller
{
    private const string ConsumerNoCollation = "utf8mb4_0900_ai_ci";
    private readonly ApplicationDbContext _db;

    public ConsumerLoginManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/ConsumerLoginManagement")]
    [RequirePermission("Consumer Login Management.view")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Consumer Login Management";
        ViewData["ActiveMenu"] = "Consumer Login Management";

        var users = await _db.ConsumerUsers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ConsumerNo)
            .ThenBy(x => x.Username)
            .ToListAsync(ct);

        var model = new List<ConsumerLoginUserListItemViewModel>();
        foreach (var user in users)
        {
            var consumer = await FindConsumerAsync(user.ConsumerNo, ct);
            model.Add(new ConsumerLoginUserListItemViewModel
            {
                Id = user.Id,
                ConsumerNo = user.ConsumerNo,
                ConsumerName = GetConsumerName(consumer),
                ConsumerMobileNo = consumer?.MobNo,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        return View(model);
    }

    [HttpGet("/ConsumerLoginManagement/Create")]
    [RequirePermission("Consumer Login Management.add")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Consumer Login";
        ViewData["ActiveMenu"] = "Consumer Login Management";
        return View(new ConsumerLoginUserFormViewModel());
    }

    [HttpPost("/ConsumerLoginManagement/Create")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Login Management.add")]
    public async Task<IActionResult> Create(ConsumerLoginUserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Consumer Login Management";
        await ValidateFormAsync(model, isCreate: true, currentId: null, ct);
        if (!ModelState.IsValid)
            return View(model);

        var user = new ConsumerUser
        {
            ConsumerNo = NormalizeConsumerNo(model.ConsumerNo),
            Username = model.Username.Trim(),
            Email = NormalizeOptional(model.Email),
            PasswordHash = AuthService.HashPassword(model.Password!),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _db.ConsumerUsers.Add(user);
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Consumer login user created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/ConsumerLoginManagement/Edit/{id:int}")]
    [RequirePermission("Consumer Login Management.edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Consumer Login";
        ViewData["ActiveMenu"] = "Consumer Login Management";
        var user = await _db.ConsumerUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();

        var consumer = await FindConsumerAsync(user.ConsumerNo, ct);
        return View(new ConsumerLoginUserFormViewModel
        {
            Id = user.Id,
            ConsumerNo = user.ConsumerNo,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            ConsumerName = GetConsumerName(consumer),
            ConsumerMobileNo = consumer?.MobNo
        });
    }

    [HttpPost("/ConsumerLoginManagement/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Login Management.edit")]
    public async Task<IActionResult> Edit(int id, ConsumerLoginUserFormViewModel model, CancellationToken ct)
    {
        ViewData["ActiveMenu"] = "Consumer Login Management";
        var user = await _db.ConsumerUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();

        model.Id = id;
        await ValidateFormAsync(model, isCreate: false, currentId: id, ct);
        if (!ModelState.IsValid)
            return View(model);

        user.ConsumerNo = NormalizeConsumerNo(model.ConsumerNo);
        user.Username = model.Username.Trim();
        user.Email = NormalizeOptional(model.Email);
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = AuthService.HashPassword(model.Password);
            user.FailedLoginCount = 0;
            user.LockoutUntil = null;
        }

        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Consumer login user updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/ConsumerLoginManagement/Details/{id:int}")]
    [RequirePermission("Consumer Login Management.view")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Consumer Login Details";
        ViewData["ActiveMenu"] = "Consumer Login Management";
        var user = await _db.ConsumerUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();

        var consumer = await FindConsumerAsync(user.ConsumerNo, ct);
        return View(new ConsumerLoginUserDetailsViewModel
        {
            Id = user.Id,
            ConsumerNo = user.ConsumerNo,
            ConsumerName = GetConsumerName(consumer),
            ConsumerMobileNo = consumer?.MobNo,
            ConsumerEmail = consumer?.EmailId,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            FailedLoginCount = user.FailedLoginCount,
            LockoutUntil = user.LockoutUntil,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }

    [HttpPost("/ConsumerLoginManagement/ToggleStatus/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequirePermission("Consumer Login Management.delete")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
    {
        var user = await _db.ConsumerUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = user.IsActive
            ? "Consumer login user activated."
            : "Consumer login user deactivated.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateFormAsync(ConsumerLoginUserFormViewModel model, bool isCreate, int? currentId, CancellationToken ct)
    {
        model.ConsumerNo = NormalizeConsumerNo(model.ConsumerNo);
        model.Username = (model.Username ?? string.Empty).Trim();
        model.Email = NormalizeOptional(model.Email);

        if (string.IsNullOrWhiteSpace(model.ConsumerNo))
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number is required.");
        if (string.IsNullOrWhiteSpace(model.Username))
            ModelState.AddModelError(nameof(model.Username), "Username is required.");
        if (isCreate && string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Password and confirm password do not match.");

        var consumer = string.IsNullOrWhiteSpace(model.ConsumerNo)
            ? null
            : await FindConsumerAsync(model.ConsumerNo, ct);

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo) && consumer is null)
            ModelState.AddModelError(nameof(model.ConsumerNo), "Consumer number not found.");

        model.ConsumerName = GetConsumerName(consumer);
        model.ConsumerMobileNo = consumer?.MobNo;

        if (!string.IsNullOrWhiteSpace(model.ConsumerNo))
        {
            var hasActiveConsumerAccount = await _db.ConsumerUsers.AnyAsync(x =>
                !x.IsDeleted &&
                x.IsActive &&
                x.ConsumerNo == model.ConsumerNo &&
                (!currentId.HasValue || x.Id != currentId.Value), ct);

            if (hasActiveConsumerAccount)
                ModelState.AddModelError(nameof(model.ConsumerNo), "An active login account already exists for this consumer number.");
        }

        if (!string.IsNullOrWhiteSpace(model.Username))
        {
            var usernameExists = await _db.ConsumerUsers.AnyAsync(x =>
                !x.IsDeleted &&
                x.Username == model.Username &&
                (!currentId.HasValue || x.Id != currentId.Value), ct);

            if (usernameExists)
                ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        }

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var emailExists = await _db.ConsumerUsers.AnyAsync(x =>
                !x.IsDeleted &&
                x.Email == model.Email &&
                (!currentId.HasValue || x.Id != currentId.Value), ct);

            if (emailExists)
                ModelState.AddModelError(nameof(model.Email), "Email already exists.");
        }
    }

    private async Task<ConsumerDetailsMaster?> FindConsumerAsync(string consumerNo, CancellationToken ct)
    {
        var normalizedConsumerNo = NormalizeConsumerNo(consumerNo);
        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            return null;

        return await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => EF.Functions.Collate(x.ConsNo, ConsumerNoCollation) == normalizedConsumerNo, ct);
    }

    private static string NormalizeConsumerNo(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetConsumerName(ConsumerDetailsMaster? consumer)
    {
        if (consumer is null) return "Consumer not found";

        var name = string.Join(" ", new[] { consumer.ConsNm1, consumer.ConsNm2 }
            .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

        return string.IsNullOrWhiteSpace(name) ? consumer.ConsNo : name;
    }
}
