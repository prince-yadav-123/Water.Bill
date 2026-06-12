namespace Water.Bill.Core.Common;

public static class NotificationNavigationHelper
{
    public const string UserTypeConsumer = "Consumer";
    public const string UserTypeInternal = "Internal";

    public sealed record Target(bool HasTarget, string? Url, bool OpenInNewTab);

    public static bool TryValidateForAudience(string? rawUrl, string audience, out string? normalizedUrl, out bool openInNewTab)
    {
        if (string.Equals(audience, UserTypeInternal, StringComparison.OrdinalIgnoreCase))
            return TryValidateInternal(rawUrl, out normalizedUrl, out openInNewTab);

        return TryValidateConsumer(rawUrl, out normalizedUrl, out openInNewTab);
    }

    public static Target ResolveForInternal(string? rawUrl, string? referenceType, string? referenceId, string? referenceNo = null)
    {
        if (TryValidateInternal(rawUrl, out var normalizedUrl, out var openInNewTab))
            return new Target(true, normalizedUrl, openInNewTab);

        var fallback = BuildInternalFallback(referenceType, referenceId, referenceNo);
        if (TryValidateInternal(fallback, out normalizedUrl, out openInNewTab))
            return new Target(true, normalizedUrl, openInNewTab);

        return new Target(false, null, false);
    }

    public static Target ResolveForConsumer(string? rawUrl, string? referenceType, string? referenceId, string? referenceNo = null)
    {
        if (TryValidateConsumer(rawUrl, out var normalizedUrl, out var openInNewTab))
            return new Target(true, normalizedUrl, openInNewTab);

        var fallback = BuildConsumerFallback(referenceType, referenceId, referenceNo);
        if (TryValidateConsumer(fallback, out normalizedUrl, out openInNewTab))
            return new Target(true, normalizedUrl, openInNewTab);

        return new Target(false, null, false);
    }

    private static bool TryValidateInternal(string? rawUrl, out string? normalizedUrl, out bool openInNewTab)
    {
        return TryValidate(rawUrl, IsAllowedInternalPath, out normalizedUrl, out openInNewTab);
    }

    private static bool TryValidateConsumer(string? rawUrl, out string? normalizedUrl, out bool openInNewTab)
    {
        return TryValidate(rawUrl, IsAllowedConsumerPath, out normalizedUrl, out openInNewTab);
    }

    private static bool TryValidate(string? rawUrl, Func<string, bool> internalPathRule, out string? normalizedUrl, out bool openInNewTab)
    {
        normalizedUrl = null;
        openInNewTab = false;

        if (string.IsNullOrWhiteSpace(rawUrl))
            return false;

        var candidate = rawUrl.Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteUri))
        {
            if (string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                normalizedUrl = absoluteUri.ToString();
                openInNewTab = true;
                return true;
            }

            return false;
        }

        if (!candidate.StartsWith("/", StringComparison.Ordinal) || candidate.StartsWith("//", StringComparison.Ordinal))
            return false;

        if (!internalPathRule(candidate))
            return false;

        normalizedUrl = candidate;
        return true;
    }

    private static bool IsAllowedInternalPath(string path)
        => !StartsWithAny(path,
            "/Consumer",
            "/Notifications",
            "/NewConnection/Start",
            "/NewConnection/VerifyOtp",
            "/TrackApplication",
            "/Account/Login");

    private static bool IsAllowedConsumerPath(string path)
        => !StartsWithAny(path,
            "/Approvals",
            "/NotificationManagement",
            "/Roles",
            "/Users",
            "/RolePermissions",
            "/Menu",
            "/PermissionModules",
            "/SecuritySettings",
            "/Workflows",
            "/Departments",
            "/Masters",
            "/Dashboard");

    private static string? BuildInternalFallback(string? referenceType, string? referenceId, string? referenceNo)
    {
        if (string.Equals(referenceType, "WorkflowTask", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var taskId))
            return $"/Approvals/Details/{taskId}";

        if (string.Equals(referenceType, "WorkflowInstance", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var workflowInstanceId))
            return $"/Approvals/OpenCurrent/{workflowInstanceId}";

        if (string.Equals(referenceType, "Notification", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var notificationId))
            return $"/NotificationManagement/Details/{notificationId}";

        if (!string.IsNullOrWhiteSpace(referenceNo))
            return $"/Approvals/Pending?tab=All&search={Uri.EscapeDataString(referenceNo)}";

        return null;
    }

    private static string? BuildConsumerFallback(string? referenceType, string? referenceId, string? referenceNo)
    {
        if (string.Equals(referenceType, "NewConnectionApplication", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var applicationId))
            return $"/Consumer/NewConnection/Details/{applicationId}";

        if (string.Equals(referenceType, "ConsumerQuery", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var queryId))
            return $"/Consumer/SupportQueries/Details/{queryId}";

        if (string.Equals(referenceType, "ConsumerComplaint", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var complaintId))
            return $"/Consumer/Complaints/Details/{complaintId}";

        if (string.Equals(referenceType, "ConsumerChallan", StringComparison.OrdinalIgnoreCase) && long.TryParse(referenceId, out var challanId))
            return $"/Consumer/Challans/Details/{challanId}";

        if (string.Equals(referenceType, "ConsumerNdc", StringComparison.OrdinalIgnoreCase) && int.TryParse(referenceId, out var ndcId))
            return $"/Consumer/Ndc/Details/{ndcId}";

        if (!string.IsNullOrWhiteSpace(referenceNo))
            return "/Notifications";

        return null;
    }

    private static bool StartsWithAny(string value, params string[] prefixes)
        => prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
