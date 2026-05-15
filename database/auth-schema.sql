CREATE DATABASE IF NOT EXISTS water_bill CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE water_bill;

CREATE TABLE IF NOT EXISTS AppRoles (
    Id CHAR(36) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    Permissions JSON NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_AppRoles PRIMARY KEY (Id),
    CONSTRAINT UX_AppRoles_Name UNIQUE (Name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AppUsers (
    Id CHAR(36) NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Username VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    RoleId CHAR(36) NOT NULL,
    PhoneNumber VARCHAR(30) NULL,
    FailedLoginCount INT NOT NULL DEFAULT 0,
    LockoutUntil DATETIME(6) NULL,
    PasswordChangedAt DATETIME(6) NULL,
    LastLoginAt DATETIME(6) NULL,
    LastLoginIp VARCHAR(64) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_AppUsers PRIMARY KEY (Id),
    CONSTRAINT UX_AppUsers_Email UNIQUE (Email),
    CONSTRAINT UX_AppUsers_Username UNIQUE (Username),
    CONSTRAINT FK_AppUsers_AppRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AppRoles(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS MenuItems (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    ParentId CHAR(36) NULL,
    Label VARCHAR(100) NOT NULL,
    Icon VARCHAR(100) NULL,
    Url VARCHAR(300) NULL,
    SectionLabel VARCHAR(100) NULL,
    Module VARCHAR(100) NULL,
    `Order` INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    OpenInNewTab TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_MenuItems PRIMARY KEY (Id),
    CONSTRAINT FK_MenuItems_MenuItems_ParentId FOREIGN KEY (ParentId) REFERENCES MenuItems(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_MenuItems_TenantId_ParentId_Order ON MenuItems (TenantId, ParentId, `Order`);

CREATE TABLE IF NOT EXISTS RolePermissions (
    Id CHAR(36) NOT NULL,
    RoleId CHAR(36) NOT NULL,
    Module VARCHAR(100) NOT NULL,
    CanView TINYINT(1) NOT NULL DEFAULT 0,
    CanAdd TINYINT(1) NOT NULL DEFAULT 0,
    CanEdit TINYINT(1) NOT NULL DEFAULT 0,
    CanDelete TINYINT(1) NOT NULL DEFAULT 0,
    CanDownload TINYINT(1) NOT NULL DEFAULT 0,
    CanExport TINYINT(1) NOT NULL DEFAULT 0,
    CanApprove TINYINT(1) NOT NULL DEFAULT 0,
    CanForward TINYINT(1) NOT NULL DEFAULT 0,
    CanPrint TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (Id),
    CONSTRAINT UX_RolePermissions_RoleId_Module UNIQUE (RoleId, Module),
    CONSTRAINT FK_RolePermissions_AppRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AppRoles(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS UserSessions (
    Id CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    SessionToken VARCHAR(200) NOT NULL,
    IpAddress VARCHAR(64) NULL,
    UserAgent VARCHAR(500) NULL,
    DeviceFingerprint VARCHAR(200) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    ExpiresAt DATETIME(6) NOT NULL,
    LastActivityAt DATETIME(6) NULL,
    RevokedAt DATETIME(6) NULL,
    RevokedReason VARCHAR(100) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_UserSessions PRIMARY KEY (Id),
    CONSTRAINT UX_UserSessions_SessionToken UNIQUE (SessionToken),
    CONSTRAINT FK_UserSessions_AppUsers_UserId FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_UserSessions_UserId_IsActive ON UserSessions (UserId, IsActive);

CREATE TABLE IF NOT EXISTS LoginAttempts (
    Id CHAR(36) NOT NULL,
    Username VARCHAR(100) NULL,
    IpAddress VARCHAR(64) NULL,
    Success TINYINT(1) NOT NULL DEFAULT 0,
    FailureReason VARCHAR(100) NULL,
    UserAgent VARCHAR(500) NULL,
    UserId CHAR(36) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_LoginAttempts PRIMARY KEY (Id),
    CONSTRAINT FK_LoginAttempts_AppUsers_UserId FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_LoginAttempts_Username_CreatedAt ON LoginAttempts (Username, CreatedAt);

CREATE TABLE IF NOT EXISTS AuditLogs (
    Id CHAR(36) NOT NULL,
    Timestamp DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UserId CHAR(36) NULL,
    Username VARCHAR(100) NULL,
    Action INT NOT NULL,
    Module VARCHAR(100) NULL,
    EntityId VARCHAR(100) NULL,
    IpAddress VARCHAR(64) NULL,
    UserAgent VARCHAR(500) NULL,
    Details TEXT NULL,
    Success TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs (Timestamp);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs (UserId);

SET @TenantId = '50000000-0000-0000-0000-000000000001';
SET @AdminRoleId = '50000000-0000-0000-0000-000000000101';
SET @AdminUserId = '50000000-0000-0000-0000-000000000201';

INSERT INTO AppRoles (Id, Name, Description, CreatedAt, IsDeleted)
VALUES
(@AdminRoleId, 'Admin', 'System administrator', UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000102', 'Staff', 'Water billing staff', UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000103', 'Consumer', 'Consumer portal user', UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name), Description = VALUES(Description), IsDeleted = 0;

INSERT INTO AppUsers (Id, FullName, Email, Username, PasswordHash, IsActive, RoleId, PasswordChangedAt, CreatedAt, IsDeleted)
VALUES
(@AdminUserId, 'System Administrator', 'admin@waterbill.local', 'admin', '7KoxPJvp8WR5itphuA1oNlQ1X530kLs/uxbAtSXpz10=', 1, @AdminRoleId, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE PasswordHash = VALUES(PasswordHash), IsActive = 1, RoleId = @AdminRoleId, IsDeleted = 0;

INSERT INTO MenuItems (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, `Order`, IsActive, CreatedAt, IsDeleted)
VALUES
('50000000-0000-0000-0000-000000000301', @TenantId, NULL, 'Dashboard', 'fa fa-gauge', '/Dashboard', 'Main', 'Dashboard', 1, 1, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000302', @TenantId, NULL, 'Consumers', 'fa fa-users', '#', 'Operations', 'Consumers', 2, 1, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000303', @TenantId, NULL, 'Billing', 'fa fa-file-invoice', '#', 'Operations', 'Billing', 3, 1, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000304', @TenantId, NULL, 'Payments', 'fa fa-credit-card', '#', 'Operations', 'Payments', 4, 1, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000305', @TenantId, NULL, 'Reports', 'fa fa-chart-line', '#', 'Reports', 'Reports', 5, 1, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000306', @TenantId, NULL, 'User Management', 'fa fa-user-shield', '#', 'Administration', 'UserManagement', 6, 1, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE Label = VALUES(Label), Url = VALUES(Url), Module = VALUES(Module), IsActive = 1, IsDeleted = 0;

INSERT INTO RolePermissions (Id, RoleId, Module, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, IsDeleted)
VALUES
(UUID(), @AdminRoleId, 'Dashboard', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Consumers', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Billing', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Payments', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Reports', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'UserManagement', 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0)
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
    IsDeleted = 0;
