using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.DTOs.Communication;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

/// <summary>
/// Dispatches admin-created notifications to target users.
///
/// InApp channel: completely independent — inserts InAppNotification records
/// directly, no template required, no dependency on email success.
///
/// Email channel: uses existing CommunicationService + templates. Failures are
/// caught per-recipient and never block InApp delivery.
/// </summary>
public sealed class NotificationDispatchService : INotificationDispatchService
{
    private const string InAppPurposeKey = "AdminNotification";
    private const string InAppUserTypeConsumer = "Consumer";
    private const string InAppUserTypeInternal = "Internal";
    private const string RefType = "Notification";
    private const string EmailPurposeKey = "AdminNotification";

    private readonly ApplicationDbContext _db;
    private readonly ICommunicationService _comm;

    public NotificationDispatchService(ApplicationDbContext db, ICommunicationService comm)
    {
        _db = db;
        _comm = comm;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<NotificationDispatchResult> SendAsync(long notificationId, CancellationToken ct = default)
    {
        // 1. Load notification + targets
        var notif = await _db.NotificationMasters
            .Include(x => x.Targets)
            .FirstOrDefaultAsync(x => x.Id == notificationId && !x.IsDeleted, ct);

        if (notif is null)
            return NotificationDispatchResult.Failed("Notification not found.");

        if (notif.Status == "Sent")
            return NotificationDispatchResult.Failed("Notification has already been sent.");

        // 2. Parse channels
        var channels = notif.Channels
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var wantInApp = channels.Any(c => string.Equals(c, "InApp", StringComparison.OrdinalIgnoreCase));
        var wantEmail = channels.Any(c => string.Equals(c, "Email", StringComparison.OrdinalIgnoreCase));

        // 3. Resolve target recipients
        List<NotifRecipient> recipients;
        try
        {
            recipients = notif.TargetAudience == InAppUserTypeInternal
                ? await ResolveInternalRecipientsAsync(notif.Targets.ToList(), ct)
                : await ResolveConsumerRecipientsAsync(notif.Targets.ToList(), ct);
        }
        catch (Exception ex)
        {
            return NotificationDispatchResult.Failed($"Failed to resolve target users: {ex.Message}");
        }

        if (recipients.Count == 0)
            return NotificationDispatchResult.Failed("No users matched the selected target criteria.");

        var userType = notif.TargetAudience == InAppUserTypeInternal
            ? InAppUserTypeInternal
            : InAppUserTypeConsumer;

        int inAppSent = 0, emailSent = 0, emailFailed = 0;
        var now = DateTime.UtcNow;

        // ── STEP A: InApp — completely independent, no template needed ──────

        if (wantInApp)
        {
            var batch = new List<InAppNotification>(recipients.Count);
            foreach (var r in recipients)
            {
                if (r.UserId <= 0) continue;  // skip invalid recipients
                batch.Add(new InAppNotification
                {
                    UserType  = userType,
                    UserId    = r.UserId,
                    Title     = notif.Title,
                    Message   = notif.Message,
                    PurposeKey    = InAppPurposeKey,
                    ReferenceType = RefType,
                    ReferenceId   = notificationId.ToString(),
                    ReferenceNo   = notif.NotificationType,
                    RedirectUrl   = notif.RedirectUrl,
                    IsRead    = false,
                    CreatedAt = now,
                    IsDeleted = false
                });
            }

            if (batch.Count > 0)
            {
                try
                {
                    await _db.InAppNotifications.AddRangeAsync(batch, ct);
                    await _db.SaveChangesAsync(ct);
                    inAppSent = batch.Count;
                }
                catch (Exception ex)
                {
                    // Log but do not abort — email may still be attempted
                    Console.Error.WriteLine($"[NotificationDispatch] InApp batch insert failed for notification {notificationId}: {ex.Message}");
                }
            }
        }

        // ── STEP B: Email — independent of InApp, catches all failures ──────

        if (wantEmail)
        {
            var baseValues = BuildEmailValues(notif);

            foreach (var r in recipients)
            {
                if (string.IsNullOrWhiteSpace(r.Email)) continue;

                try
                {
                    var recipient = new NotificationRecipient
                    {
                        Name     = r.Name,
                        Email    = r.Email,
                        UserId   = r.UserId,
                        UserType = userType
                    };

                    var channelOptions = new NotificationChannelOptions { Email = true };
                    var values = new Dictionary<string, string?>(baseValues)
                    {
                        ["UserName"] = r.Name
                    };

                    await _comm.SendAsync(EmailPurposeKey, recipient, values, channelOptions,
                        RefType, notificationId.ToString(), notif.NotificationType, ct: ct);

                    emailSent++;
                }
                catch (Exception ex)
                {
                    emailFailed++;
                    Console.Error.WriteLine($"[NotificationDispatch] Email failed for user {r.UserId} ({r.Email}): {ex.Message}");
                }
            }
        }

        // ── STEP C: Mark notification as Sent ────────────────────────────────

        try
        {
            notif.Status = "Sent";
            notif.SentAt = now;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[NotificationDispatch] Failed to update notification status {notificationId}: {ex.Message}");
        }

        return NotificationDispatchResult.Ok(recipients.Count, inAppSent, emailSent, emailFailed);
    }

    public async Task<int> PreviewTargetCountAsync(
        string targetAudience,
        IReadOnlyList<NotificationTargetInput> targets,
        CancellationToken ct = default)
    {
        var tempTargets = targets.Select(t => new NotificationTarget
        {
            TargetType = t.TargetType,
            TargetId   = t.TargetId,
            TargetName = t.TargetName
        }).ToList();

        var recipients = targetAudience == InAppUserTypeInternal
            ? await ResolveInternalRecipientsAsync(tempTargets, ct)
            : await ResolveConsumerRecipientsAsync(tempTargets, ct);

        return recipients.Count;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal user resolution (Appuser)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<List<NotifRecipient>> ResolveInternalRecipientsAsync(
        List<NotificationTarget> targets, CancellationToken ct)
    {
        // AllInternalUsers overrides everything
        if (targets.Any(t => t.TargetType == "AllInternalUsers" && !t.IsDeleted))
        {
            return await _db.Appusers.AsNoTracking()
                .Where(x => x.IsActive == true && !x.IsDeleted)
                .Select(x => new NotifRecipient(x.Id, x.FullName, x.Email, null, null))
                .ToListAsync(ct);
        }

        var userIdSet = new HashSet<long>();
        var activeTargets = targets.Where(t => !t.IsDeleted).ToList();

        foreach (var target in activeTargets)
        {
            switch (target.TargetType)
            {
                case "InternalUser" when long.TryParse(target.TargetId, out var uid):
                    userIdSet.Add(uid);
                    break;

                case "Role" when int.TryParse(target.TargetId, out var roleId):
                    var byRole = await _db.Appusers.AsNoTracking()
                        .Where(x => x.RoleId == roleId && x.IsActive == true && !x.IsDeleted)
                        .Select(x => (long)x.Id)
                        .ToListAsync(ct);
                    foreach (var id in byRole) userIdSet.Add(id);
                    break;

                case "Department" when int.TryParse(target.TargetId, out var deptId):
                    var byDept = await _db.Appusers.AsNoTracking()
                        .Where(x => x.DeptId == deptId && x.IsActive == true && !x.IsDeleted)
                        .Select(x => (long)x.Id)
                        .ToListAsync(ct);
                    foreach (var id in byDept) userIdSet.Add(id);
                    break;
            }
        }

        if (userIdSet.Count == 0) return [];

        var ids = userIdSet.Select(x => (int)x).ToList();
        return await _db.Appusers.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive == true && !x.IsDeleted)
            .Select(x => new NotifRecipient(x.Id, x.FullName, x.Email, null, null))
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Consumer user resolution (ConsumerUser)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<List<NotifRecipient>> ResolveConsumerRecipientsAsync(
        List<NotificationTarget> targets, CancellationToken ct)
    {
        // AllConsumers overrides everything
        if (targets.Any(t => t.TargetType == "AllConsumers" && !t.IsDeleted))
        {
            var consumerRecipients = await
                (from consumerUser in _db.ConsumerUsers.AsNoTracking()
                 join consumer in _db.ConsumerDetailsMasters.AsNoTracking()
                     on consumerUser.ConsumerNo equals consumer.ConsNo
                 where consumerUser.IsActive && !consumerUser.IsDeleted
                       && consumer.Status == 1
                       && consumer.DeleteDate == null
                 select new
                 {
                     UserId = consumerUser.Id,
                     Name = !string.IsNullOrWhiteSpace(consumerUser.Username)
                         ? consumerUser.Username
                         : (consumer.ConsNm1 ?? consumer.ConsNo),
                     Email = !string.IsNullOrWhiteSpace(consumerUser.Email)
                         ? consumerUser.Email!
                         : (consumer.EmailId ?? string.Empty),
                     Mobile = consumer.MobNo ?? string.Empty
                 })
                .ToListAsync(ct);

            return consumerRecipients
                .Select(x => new NotifRecipient(
                    x.UserId,
                    x.Name,
                    x.Email,
                    x.Mobile,
                    "Consumer"))
                .ToList();
        }

        var activeTargets = targets.Where(t => !t.IsDeleted).ToList();

        var normalizedTargets = activeTargets
            .Where(t => t.TargetType == "ConsumerNo" && !string.IsNullOrWhiteSpace(t.TargetId))
            .Select(t => t.TargetId!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var recipients = new List<NotifRecipient>();

        if (normalizedTargets.Count > 0)
        {
            var matchedConsumers = await
                (from consumer in _db.ConsumerDetailsMasters.AsNoTracking()
                 join consumerUser in _db.ConsumerUsers.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
                     on consumer.ConsNo equals consumerUser.ConsumerNo into consumerUserGroup
                 from consumerUser in consumerUserGroup.DefaultIfEmpty()
                 where normalizedTargets.Contains(consumer.ConsNo)
                       && consumer.Status == 1
                       && consumer.DeleteDate == null
                 select new
                {
                    UserId = consumerUser != null ? consumerUser.Id : 0,
                    Name = consumerUser != null && !string.IsNullOrWhiteSpace(consumerUser.Username)
                        ? consumerUser.Username
                        : (consumer.ConsNm1 ?? consumer.ConsNo),
                    Email = consumerUser != null && !string.IsNullOrWhiteSpace(consumerUser.Email)
                        ? consumerUser.Email!
                        : (consumer.EmailId ?? string.Empty),
                    Mobile = consumer.MobNo ?? string.Empty
                })
                .ToListAsync(ct);

            foreach (var consumer in matchedConsumers)
            {
                recipients.Add(new NotifRecipient(
                    consumer.UserId,
                    consumer.Name,
                    consumer.Email,
                    consumer.Mobile,
                    "Consumer"));
            }
        }

        // If the selection also includes specific internal/consumer users, keep supporting those existing flows.
        if (targets.Any(t => t.TargetType == "ConsumerUser" && long.TryParse(t.TargetId, out var _)))
        {
            var ids = targets
                .Where(t => t.TargetType == "ConsumerUser" && long.TryParse(t.TargetId, out var _))
                .Select(t => int.Parse(t.TargetId!))
                .Distinct()
                .ToList();

            var linkedUsers = await _db.ConsumerUsers.AsNoTracking()
                .Where(x => ids.Contains(x.Id) && x.IsActive && !x.IsDeleted)
                .Select(x => new NotifRecipient(x.Id, x.Username, x.Email ?? string.Empty, null, "Consumer"))
                .ToListAsync(ct);
            recipients.AddRange(linkedUsers);
        }

        if (recipients.Count == 0)
            return [];

        return recipients
            .GroupBy(x => new { x.Name, x.Email, x.Mobile })
            .Select(g => g.First())
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, string?> BuildEmailValues(NotificationMaster n) => new()
    {
        ["NotificationTitle"]   = n.Title,
        ["NotificationMessage"] = n.Message,
        ["NotificationType"]    = n.NotificationType,
        ["Priority"]            = n.Priority,
        ["Date"]                = DateTime.Now.ToString("dd MMM yyyy"),
        ["UserName"]            = string.Empty      // overridden per-recipient
    };

    private sealed record NotifRecipient(long UserId, string Name, string Email, string? Mobile, string? UserType);
}
