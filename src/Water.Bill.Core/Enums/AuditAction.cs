namespace Water.Bill.Core.Enums;

public enum AuditAction
{
    LoginSuccess = 1,
    LoginFailed = 2,
    Logout = 3,
    AccountLocked = 4,
    SessionRevoked = 5,
    PermissionChanged = 6,
    ProfileViewed = 7,
    SecuritySettingsChanged = 8,
    UserChanged = 9,
    RoleChanged = 10,
    MenuChanged = 11
}
