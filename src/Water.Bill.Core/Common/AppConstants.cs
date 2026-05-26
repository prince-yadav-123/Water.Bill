namespace Water.Bill.Core.Common;

public static class AppConstants
{
    public const string CookieScheme = "WaterBillCookie";
    public const int DefaultTenantId = 1;
    public const int ConsumerTenantId = 2;

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
    }
}
