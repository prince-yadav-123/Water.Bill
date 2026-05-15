namespace Water.Bill.Core.Common;

public static class AppConstants
{
    public const string CookieScheme = "WaterBillCookie";
    public static readonly Guid DefaultTenantId = Guid.Parse("50000000-0000-0000-0000-000000000001");

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
        public const string UserManagement = "UserManagement";
        public const string RolesUsers = "Roles & Users";
        public const string MenuManagement = "Menu Management";
        public const string PermissionModules = "Permission Modules";
        public const string SecuritySettings = "Security Settings";
        public const string Profile = "Profile";
    }
}
