USE water_bill;

SET @TenantId = '50000000-0000-0000-0000-000000000001';
SET @AdminRoleId = '50000000-0000-0000-0000-000000000101';
SET @SecurityMenuId = '50000000-0000-0000-0000-000000000306';

CREATE TABLE IF NOT EXISTS securitysettings (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    SessionTimeoutMinutes INT NOT NULL DEFAULT 480,
    IdleTimeoutMinutes INT NOT NULL DEFAULT 30,
    PasswordMinLength INT NOT NULL DEFAULT 8,
    PasswordRequireUppercase TINYINT(1) NOT NULL DEFAULT 1,
    PasswordRequireLowercase TINYINT(1) NOT NULL DEFAULT 1,
    PasswordRequireDigit TINYINT(1) NOT NULL DEFAULT 1,
    PasswordRequireSpecialChar TINYINT(1) NOT NULL DEFAULT 1,
    PasswordExpiryDays INT NOT NULL DEFAULT 90,
    PasswordHistoryCount INT NOT NULL DEFAULT 5,
    MaxFailedLoginAttempts INT NOT NULL DEFAULT 5,
    LockoutDurationMinutes INT NOT NULL DEFAULT 15,
    EnableCaptchaAfterFailures TINYINT(1) NOT NULL DEFAULT 0,
    CaptchaAfterAttempts INT NOT NULL DEFAULT 3,
    AllowMultipleSessions TINYINT(1) NOT NULL DEFAULT 1,
    BlockNewLoginOnConflict TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_securitysettings PRIMARY KEY (Id),
    CONSTRAINT UX_SecuritySettings_TenantId UNIQUE (TenantId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO approles (Id, Name, Description, CreatedAt, IsDeleted)
VALUES
(@AdminRoleId, 'Admin', 'System administrator with full Admin Portal access', UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000102', 'Staff', 'Water billing staff', UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000103', 'Consumer', 'Consumer portal user', UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    Description = VALUES(Description),
    IsDeleted = 0;

INSERT INTO menuitems (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, `Order`, IsActive, OpenInNewTab, CreatedAt, IsDeleted)
VALUES
('50000000-0000-0000-0000-000000000301', @TenantId, NULL, 'Dashboard', 'DB', '/Dashboard', 'Main', 'Dashboard', 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000302', @TenantId, NULL, 'Consumers', 'CS', '#', 'Operations', 'Consumers', 2, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000303', @TenantId, NULL, 'Billing', 'BL', '#', 'Operations', 'Billing', 3, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000304', @TenantId, NULL, 'Payments', 'PY', '#', 'Operations', 'Payments', 4, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000305', @TenantId, NULL, 'Reports', 'RP', '#', 'Reports', 'Reports', 5, 1, 0, UTC_TIMESTAMP(6), 0),
(@SecurityMenuId, @TenantId, NULL, 'Administration', 'AD', '#', 'Administration', NULL, 6, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000307', @TenantId, @SecurityMenuId, 'Roles & Users', 'RU', '/RolesUsers', 'Administration', 'Roles & Users', 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000308', @TenantId, @SecurityMenuId, 'Role Permission', 'PM', '/RolesUsers/Permissions', 'Administration', 'Roles & Users', 2, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000309', @TenantId, @SecurityMenuId, 'Menu Management', 'MN', '/Menu', 'Administration', 'Menu Management', 3, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000310', @TenantId, @SecurityMenuId, 'Security Settings', 'SS', '/SecuritySettings', 'Administration', 'Security Settings', 4, 1, 0, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    TenantId = VALUES(TenantId),
    ParentId = VALUES(ParentId),
    Label = VALUES(Label),
    Icon = VALUES(Icon),
    Url = VALUES(Url),
    SectionLabel = VALUES(SectionLabel),
    Module = VALUES(Module),
    `Order` = VALUES(`Order`),
    IsActive = VALUES(IsActive),
    OpenInNewTab = VALUES(OpenInNewTab),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);

INSERT INTO rolepermissions
(Id, RoleId, Module, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, IsDeleted)
VALUES
(UUID(), @AdminRoleId, 'Dashboard', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Consumers', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Billing', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Payments', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Reports', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Roles & Users', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Menu Management', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Security Settings', 1, 0, 1, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Profile', 1, 0, 1, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    CanView = VALUES(CanView),
    CanAdd = VALUES(CanAdd),
    CanEdit = VALUES(CanEdit),
    CanDelete = VALUES(CanDelete),
    CanDownload = VALUES(CanDownload),
    CanExport = VALUES(CanExport),
    CanApprove = VALUES(CanApprove),
    CanForward = VALUES(CanForward),
    CanPrint = VALUES(CanPrint),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);

INSERT INTO securitysettings
(Id, TenantId, SessionTimeoutMinutes, IdleTimeoutMinutes, PasswordMinLength, PasswordRequireUppercase,
 PasswordRequireLowercase, PasswordRequireDigit, PasswordRequireSpecialChar, PasswordExpiryDays,
 PasswordHistoryCount, MaxFailedLoginAttempts, LockoutDurationMinutes, EnableCaptchaAfterFailures,
 CaptchaAfterAttempts, AllowMultipleSessions, BlockNewLoginOnConflict, CreatedAt, IsDeleted)
VALUES
('50000000-0000-0000-0000-000000000401', @TenantId, 480, 30, 8, 1, 1, 1, 1, 90, 5, 5, 15, 0, 3, 1, 0, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    SessionTimeoutMinutes = VALUES(SessionTimeoutMinutes),
    IdleTimeoutMinutes = VALUES(IdleTimeoutMinutes),
    PasswordMinLength = VALUES(PasswordMinLength),
    MaxFailedLoginAttempts = VALUES(MaxFailedLoginAttempts),
    LockoutDurationMinutes = VALUES(LockoutDurationMinutes),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);
