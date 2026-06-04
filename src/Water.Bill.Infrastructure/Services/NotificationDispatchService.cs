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
                        RefType, notificationId.ToString(), notif.NotificationType, ct);

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
                    var byDept = await _db.AuthorityUserDepartments.AsNoTracking()
                        .Where(x => x.DepartmentId == deptId && x.IsActive && !x.IsDeleted)
                        .Select(x => (long)x.UserId)
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
            var consumerRows = await _db.ConsumerDetailsMasters.AsNoTracking()
                .Where(x => x.Status == 1 && x.DeleteDate == null)
                .Select(x => new
                {
                    ConsNo = x.ConsNo,
                    Name = x.ConsNm1 ?? x.ConsNo,
                    Email = x.EmailId ?? string.Empty,
                    Mobile = x.MobNo ?? string.Empty
                })
                .ToListAsync(ct);

            var consumerNos = consumerRows
                .Select(x => x.ConsNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpperInvariant())
                .ToList();

            var linkedUsers = await _db.ConsumerUsers.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted && x.ConsumerNo != null && consumerNos.Contains(x.ConsumerNo))
                .Select(x => new { x.Id, x.ConsumerNo })
                .ToListAsync(ct);

            var linkedByNo = linkedUsers
                .GroupBy(x => x.ConsumerNo!)
                .ToDictionary(g => g.Key, g => g.First().Id);

            return consumerRows
                .Select(x => new NotifRecipient(
                    linkedByNo.TryGetValue(x.ConsNo.Trim().ToUpperInvariant(), out var userId) ? userId : 0,
                    x.Name,
                    x.Email,
                    x.Mobile,
                    "Consumer"))
                .ToList();
        }

        var activeTargets = targets.Where(t => !t.IsDeleted).ToList();

        foreach (var target in activeTargets)
        {
            switch (target.TargetType)
            {
                case "ConsumerUser" when long.TryParse(target.TargetId, out var uid):
                    var consumerUserExists = await _db.ConsumerUsers.AsNoTracking()
                        .AnyAsync(x => x.Id == (int)uid && x.IsActive && !x.IsDeleted, ct);
                    if (!consumerUserExists)
                        break;
                    break;

                case "ConsumerNo" when !string.IsNullOrWhiteSpace(target.TargetId):
                    // ConsumerNo is stored uppercase (NormalizeConsumerNo)
                    var normalized = target.TargetId.Trim().ToUpperInvariant();
                    var consumerExists = await _db.ConsumerDetailsMasters.AsNoTracking()
                        .AnyAsync(x => x.ConsNo == normalized && x.Status == 1 && x.DeleteDate == null, ct);

                    if (!consumerExists)
                        break;
                    break;
            }
        }

        var normalizedTargets = activeTargets
            .Where(t => t.TargetType == "ConsumerNo" && !string.IsNullOrWhiteSpace(t.TargetId))
            .Select(t => t.TargetId!.Trim().ToUpperInvariant())
            .ToList();

        var recipients = new List<NotifRecipient>();

        if (normalizedTargets.Count > 0)
        {
            var masterMatches = await _db.ConsumerDetailsMasters.AsNoTracking()
                .Where(x => normalizedTargets.Contains(x.ConsNo) && x.Status == 1 && x.DeleteDate == null)
                .Select(x => new
                {
                    ConsNo = x.ConsNo,
                    Name = x.ConsNm1 ?? x.ConsNo,
                    Email = x.EmailId ?? string.Empty,
                    Mobile = x.MobNo ?? string.Empty
                })
                .ToListAsync(ct);

            var consumerNos = masterMatches.Select(x => x.ConsNo).ToList();

            var linkedUsers = await _db.ConsumerUsers.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted && x.ConsumerNo != null && consumerNos.Contains(x.ConsumerNo))
                .Select(x => new { x.Id, x.ConsumerNo, x.Username, Email = x.Email ?? string.Empty })
                .ToListAsync(ct);

            foreach (var master in masterMatches)
            {
                var linkedUser = linkedUsers.FirstOrDefault(x => string.Equals(x.ConsumerNo, master.ConsNo, StringComparison.OrdinalIgnoreCase));

                recipients.Add(new NotifRecipient(
                    linkedUser?.Id ?? 0,
                    linkedUser?.Username ?? master.Name,
                    string.IsNullOrWhiteSpace(linkedUser?.Email) ? master.Email : linkedUser!.Email,
                    master.Mobile,
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
