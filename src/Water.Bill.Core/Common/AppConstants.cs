namespace Water.Bill.Core.Common;

public static class AppConstants
{
    public const string CookieScheme = "WaterBillCookie";
    public const int DefaultTenantId = 1;
    public const int ConsumerTenantId = 2;

    public static class Divisions
    {
        public static readonly DivisionOption Jal1 = new(1, "JAL1", "JAL-1", "Sector-5");
        public static readonly DivisionOption Jal2 = new(2, "JAL2", "JAL-2", "Sector-37");
        public static readonly DivisionOption Jal3 = new(3, "JAL3", "JAL-3", "Sector-39");
        public static readonly DivisionOption AllDivision = new(4, "ALL", "All Divisions", null);

        public static readonly IReadOnlyList<DivisionOption> Options =
        [
            Jal1,
            Jal2,
            Jal3,
            AllDivision
        ];

        public static DivisionOption? Find(int? devType)
            => Options.FirstOrDefault(x => x.DevType == devType);

        public static string FormatDisplay(int? devType)
            => Find(devType)?.DisplayText ?? devType?.ToString() ?? string.Empty;
    }

    public sealed record DivisionOption(int DevType, string Code, string Name, string? Office)
    {
        public string DisplayText => string.IsNullOrWhiteSpace(Office) ? Name : $"{Name} ({Office})";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string Consumer = "Consumer";
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string AuthenticatedUser = "AuthenticatedUser";
    }

    public static class Claims
    {
        public const string UserId = "user_id";
        public const string Username = "username";
        public const string Email = "email";
        public const string RoleName = "role_name";
    }

    public static class Modules
    {
        public const string Dashboard = "Dashboard";
        public const string Consumers = "Consumers";
        public const string Billing = "Billing";
        public const string Payments = "Payments";
        public const string Reports = "Reports";
        public const string RoleManagement = "Role Management";
        public const string UserManagement = "User Management";
        public const string RolePermission = "Role Permission";
        public const string RolesUsers = "Roles & Users";
        public const string ConsumerLoginManagement = "Consumer Login Management";
        public const string MenuManagement = "Menu Management";
        public const string PermissionModules = "Permission Modules";
        public const string SecuritySettings = "Security Settings";
        public const string Profile = "Profile";
        public const string ConsumerDashboard = "Consumer Dashboard";
        public const string ConsumerBills = "Consumer Bills";
        public const string ConsumerProfile = "Consumer Profile";
        public const string ConsumerNewConnection = "Consumer New Connection";
        public const string ConsumerSupportQueries = "Consumer Support Queries";
        public const string ConsumerQueryManagement = "Consumer Query Management";
        public const string BillSearchPrint = "Bill Search & Print";
        public const string DepartmentMaster = "Department Master";
        public const string WorkflowMaster = "Workflow Master";
        public const string MyPendingApplications = "My Pending Applications";
        public const string NewConnectionFeeConfiguration = "New Connection Fee Configuration";
    }
}
