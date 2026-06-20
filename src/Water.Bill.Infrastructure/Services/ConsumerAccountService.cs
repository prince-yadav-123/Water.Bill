using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Consumer;
using Water.Bill.Application.Interfaces;
using Water.Bill.Core.Common;
using Water.Bill.Infrastructure.Data;

namespace Water.Bill.Infrastructure.Services;

public class ConsumerAccountService : IConsumerAccountService
{
    private readonly ApplicationDbContext _db;

    public ConsumerAccountService(ApplicationDbContext db) => _db = db;

    public async Task<ConsumerAccountLoginResult> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default)
    {
        var login = (usernameOrEmail ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            throw new UnauthorizedAccessException("Invalid username/email or password.");

        var normalizedLogin = login.ToLowerInvariant();
        var user = await _db.ConsumerUsers
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                (x.Username.ToLower() == normalizedLogin ||
                 (x.Email != null && x.Email.ToLower() == normalizedLogin)), ct);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid username/email or password.");

        if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Consumer account is locked. Please try again later.");

        if (!user.IsActive || AuthService.HashPassword(password) != user.PasswordHash)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);

            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        var consumerNo = (user.ConsumerNo ?? string.Empty).Trim().ToUpperInvariant();
        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo != null && x.ConsNo.ToUpper() == consumerNo, ct);

        if (consumer is null)
            throw new UnauthorizedAccessException("Linked consumer number was not found. Please contact support.");

        if (consumer.Status.HasValue && consumer.Status != 1)
            throw new UnauthorizedAccessException("This consumer account is not active. Please contact support.");

        user.FailedLoginCount = 0;
        user.LockoutUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var name = (consumer.ConsNm1 ?? string.Empty).Trim();

        var consumerRole = await _db.Approles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Name.ToLower() == AppConstants.Roles.Consumer.ToLower(), ct);

        return new ConsumerAccountLoginResult
        {
            Id = user.Id,
            ConsumerNo = user.ConsumerNo,
            ConsumerName = string.IsNullOrWhiteSpace(name) ? user.Username : name,
            Email = user.Email ?? consumer.EmailId,
            MobileNo = consumer.MobNo,
            Username = user.Username,
            ConsumerRoleId = consumerRole?.Id
        };
    }

    public async Task<ConsumerAccountLoginResult> LoginByConsumerNoAsync(string consumerNo, CancellationToken ct = default)
    {
        var normalizedConsumerNo = (consumerNo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedConsumerNo))
            throw new UnauthorizedAccessException("Consumer Number is required.");

        var consumer = await _db.ConsumerDetailsMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConsNo != null && x.ConsNo.ToUpper() == normalizedConsumerNo, ct);

        if (consumer is null)
            throw new UnauthorizedAccessException("Consumer not found.");

        if (consumer.DeleteDate != null || (consumer.Status.HasValue && consumer.Status != 1))
            throw new UnauthorizedAccessException("Consumer is not active.");

        var linkedConsumerUserId = await _db.ConsumerUsers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ConsumerNo != null && x.ConsumerNo.ToUpper() == normalizedConsumerNo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var consumerRole = await _db.Approles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Name.ToLower() == AppConstants.Roles.Consumer.ToLower(), ct);

        var name = (consumer.ConsNm1 ?? string.Empty).Trim();

        return new ConsumerAccountLoginResult
        {
            Id = linkedConsumerUserId ?? 0,
            ConsumerNo = normalizedConsumerNo,
            ConsumerName = string.IsNullOrWhiteSpace(name) ? normalizedConsumerNo : name,
            Email = consumer.EmailId,
            MobileNo = consumer.MobNo,
            Username = normalizedConsumerNo,
            ConsumerRoleId = consumerRole?.Id
        };
    }
}
