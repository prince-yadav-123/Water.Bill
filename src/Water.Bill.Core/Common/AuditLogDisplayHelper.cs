using Water.Bill.Core.Enums;

namespace Water.Bill.Core.Common;

public static class AuditLogDisplayHelper
{
    public const string AuthorizationModule = "Authorization";
    public const string ConsumerAuthenticationModule = "Consumer Authentication";
    public const string AuthorityAuthenticationModule = "Authority Authentication";

    public static readonly string[] ConsumerModules =
    [
        AppConstants.Modules.ConsumerDashboard,
        AppConstants.Modules.ConsumerBills,
        AppConstants.Modules.ConsumerProfile,
        AppConstants.Modules.ConsumerNewConnection,
        AppConstants.Modules.ConsumerNdcApplications,
        AppConstants.Modules.ConsumerChallans,
        AppConstants.Modules.ConsumerServiceRequests,
        AppConstants.Modules.ConsumerSupportQueries,
        AppConstants.Modules.ConsumerComplaints,
        ConsumerAuthenticationModule
    ];

    public static readonly string[] AuthorityModules =
    [
        AppConstants.Modules.Dashboard,
        AppConstants.Modules.Consumers,
        AppConstants.Modules.Billing,
        AppConstants.Modules.Payments,
        AppConstants.Modules.Reports,
        AppConstants.Modules.RoleManagement,
        AppConstants.Modules.UserManagement,
        AppConstants.Modules.RolePermission,
        AppConstants.Modules.RolesUsers,
        AppConstants.Modules.ConsumerLoginManagement,
        AppConstants.Modules.MenuManagement,
        AppConstants.Modules.PermissionModules,
        AppConstants.Modules.SecuritySettings,
        AppConstants.Modules.Profile,
        AppConstants.Modules.ConsumerQueryManagement,
        AppConstants.Modules.ComplaintManagement,
        AppConstants.Modules.ConsumerMasterMaintenance,
        AppConstants.Modules.ConsumerAccountAdjustments,
        AppConstants.Modules.ConsumerLedger,
        AppConstants.Modules.MeterReadingManagement,
        AppConstants.Modules.DisconnectionManagement,
        AppConstants.Modules.NoticeManagement,
        AppConstants.Modules.NameTransferMutation,
        AppConstants.Modules.ConnectionTypeCategoryChange,
        AppConstants.Modules.BillSearchPrint,
        AppConstants.Modules.BulkBillGeneration,
        AppConstants.Modules.ChallanManagement,
        AppConstants.Modules.OnlinePaymentHistory,
        AppConstants.Modules.NdcApplications,
        AppConstants.Modules.NdcCertificateManagement,
        AppConstants.Modules.DepartmentMaster,
        AppConstants.Modules.WorkflowMaster,
        AppConstants.Modules.MyPendingApplications,
        AppConstants.Modules.NewConnectionFeeConfiguration,
        AppConstants.Modules.CommunicationTemplates,
        AppConstants.Modules.ErrorLogs,
        AppConstants.Modules.UserActivityLogs,
        AppConstants.Modules.ConsumerActivityLogs,
        AuthorityAuthenticationModule
    ];

    public static string GetActionLabel(int action, string? module = null, string? details = null)
    {
        var normalizedDetails = details?.Trim();

        return action switch
        {
            (int)AuditAction.LoginSuccess => IsOtpVerification(normalizedDetails) ? "OTP Verified" : "Login",
            (int)AuditAction.LoginFailed => "Login Failed",
            (int)AuditAction.Logout => "Logout",
            (int)AuditAction.AccountLocked => "Account Locked",
            (int)AuditAction.SessionRevoked => "Session Revoked",
            (int)AuditAction.PermissionChanged => InferMutationAction(normalizedDetails, "Permission Denied"),
            (int)AuditAction.ProfileViewed => HasKeyword(normalizedDetails, "updated") ? "Profile Updated" : "View",
            (int)AuditAction.SecuritySettingsChanged => InferMutationAction(normalizedDetails, "Update"),
            (int)AuditAction.UserChanged => InferMutationAction(normalizedDetails, "Update"),
            (int)AuditAction.RoleChanged => InferMutationAction(normalizedDetails, "Update"),
            (int)AuditAction.MenuChanged => InferMutationAction(normalizedDetails, "Update"),
            (int)AuditAction.Delete => "Delete",
            _ => $"Action {action}"
        };
    }

    public static string GetEntityLabel(string? module)
    {
        if (string.IsNullOrWhiteSpace(module))
            return "General";

        return module.Trim() switch
        {
            AppConstants.Modules.RoleManagement or AppConstants.Modules.RolesUsers => "Role",
            AppConstants.Modules.UserManagement => "User",
            AppConstants.Modules.PermissionModules => "Permission",
            AppConstants.Modules.MenuManagement => "Menu",
            AppConstants.Modules.SecuritySettings => "Security Settings",
            AppConstants.Modules.Profile or AppConstants.Modules.ConsumerProfile => "Profile",
            AppConstants.Modules.ConsumerBills or AppConstants.Modules.BillSearchPrint => "Bill",
            AppConstants.Modules.ChallanManagement or AppConstants.Modules.ConsumerChallans => "Challan",
            AppConstants.Modules.OnlinePaymentHistory => "Payment",
            AppConstants.Modules.ConsumerNewConnection => "New Connection",
            AppConstants.Modules.ConsumerNdcApplications or AppConstants.Modules.NdcApplications or AppConstants.Modules.NdcCertificateManagement => "NDC",
            AppConstants.Modules.ComplaintManagement or AppConstants.Modules.ConsumerComplaints => "Complaint",
            AppConstants.Modules.ConsumerSupportQueries or AppConstants.Modules.ConsumerQueryManagement => "Support Query",
            AppConstants.Modules.WorkflowMaster or AppConstants.Modules.MyPendingApplications => "Workflow",
            AuthorizationModule => "Authorization",
            ConsumerAuthenticationModule or AuthorityAuthenticationModule => "Authentication",
            _ => module.Trim()
        };
    }

    public static string GetModuleLabel(string? module)
        => string.IsNullOrWhiteSpace(module) ? "General" : module.Trim();

    private static string InferMutationAction(string? details, string fallback)
    {
        if (HasKeyword(details, "permission denied") || HasKeyword(details, "blocked direct access"))
            return "Permission Denied";
        if (HasKeyword(details, "otp sent"))
            return "OTP Sent";
        if (HasKeyword(details, "otp verified"))
            return "OTP Verified";
        if (HasKeyword(details, "payment initiated"))
            return "Payment Initiated";
        if (HasKeyword(details, "payment success") || HasKeyword(details, "payment completed"))
            return "Payment Success";
        if (HasKeyword(details, "payment failed"))
            return "Payment Failed";
        if (HasKeyword(details, "mobile updated"))
            return "Mobile Updated";
        if (HasKeyword(details, "created"))
            return "Create";
        if (HasKeyword(details, "updated"))
            return "Update";
        if (HasKeyword(details, "deleted") || HasKeyword(details, "cleared"))
            return "Delete";
        if (HasKeyword(details, "viewed"))
            return "View";

        return fallback;
    }

    private static bool IsOtpVerification(string? details)
        => HasKeyword(details, "otp login verified") || HasKeyword(details, "otp verified");

    private static bool HasKeyword(string? text, string keyword)
        => !string.IsNullOrWhiteSpace(text)
           && text.Contains(keyword, StringComparison.OrdinalIgnoreCase);

    public static string InferModuleFromPath(string? path, bool isConsumer)
    {
        if (string.IsNullOrWhiteSpace(path))
            return isConsumer ? ConsumerAuthenticationModule : AuthorityAuthenticationModule;

        var normalized = path.Trim();

        if (normalized.StartsWith("/Consumer/Dashboard", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerDashboard;
        if (normalized.StartsWith("/Consumer/Bills", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerBills;
        if (normalized.StartsWith("/Consumer/Profile", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerProfile;
        if (normalized.StartsWith("/Consumer/NewConnection", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerNewConnection;
        if (normalized.StartsWith("/Consumer/Ndc", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerNdcApplications;
        if (normalized.StartsWith("/Consumer/Challans", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerChallans;
        if (normalized.StartsWith("/Consumer/ServiceRequests", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerServiceRequests;
        if (normalized.StartsWith("/Consumer/SupportQueries", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerSupportQueries;
        if (normalized.StartsWith("/Consumer/Complaints", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerComplaints;
        if (normalized.StartsWith("/Consumer/Login", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("/Consumer/VerifyOtp", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("/Account/VerifyOtp", StringComparison.OrdinalIgnoreCase))
            return isConsumer ? ConsumerAuthenticationModule : AuthorityAuthenticationModule;

        if (normalized.StartsWith("/Dashboard", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.Dashboard;
        if (normalized.StartsWith("/Roles", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.RoleManagement;
        if (normalized.StartsWith("/Users", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.UserManagement;
        if (normalized.StartsWith("/RolePermissions", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.RolePermission;
        if (normalized.StartsWith("/Menu", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.MenuManagement;
        if (normalized.StartsWith("/PermissionModules", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.PermissionModules;
        if (normalized.StartsWith("/SecuritySettings", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.SecuritySettings;
        if (normalized.StartsWith("/Profile", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.Profile;
        if (normalized.StartsWith("/ErrorLogs", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ErrorLogs;
        if (normalized.StartsWith("/UserActivityLogs", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("/OperatorAudit", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.UserActivityLogs;
        if (normalized.StartsWith("/ConsumerActivityLogs", StringComparison.OrdinalIgnoreCase))
            return AppConstants.Modules.ConsumerActivityLogs;

        return isConsumer ? ConsumerAuthenticationModule : AuthorityAuthenticationModule;
    }
}
