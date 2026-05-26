-- Water.Bill auth/admin integer ID reset script
-- Purpose:
--   Recreate the new application auth/admin tables with INT AUTO_INCREMENT IDs.
--
-- IMPORTANT:
--   Use this only on a development/testing database, or after taking a full backup.
--   This script intentionally deletes data from the new auth/admin tables listed below.
--   It does NOT touch old water billing business tables such as CONSUMER_DETAILS_MASTER,
--   jal_print_bill_master, challan, bank payment tables, or old master lookup tables.

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `usersessions`;
DROP TABLE IF EXISTS `loginattempts`;
DROP TABLE IF EXISTS `rolepermissions`;
DROP TABLE IF EXISTS `menuitems`;
DROP TABLE IF EXISTS `PermissionModules`;
DROP TABLE IF EXISTS `securitysettings`;
DROP TABLE IF EXISTS `auditlogs`;
DROP TABLE IF EXISTS `ConsumerOtpVerifications`;
DROP TABLE IF EXISTS `ConsumerUsers`;
DROP TABLE IF EXISTS `appusers`;
DROP TABLE IF EXISTS `approles`;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE IF NOT EXISTS `approles` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  `Description` VARCHAR(500) NULL,
  `Permissions` JSON NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_AppRoles_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `appusers` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `FullName` VARCHAR(150) NOT NULL,
  `Email` VARCHAR(150) NOT NULL,
  `Username` VARCHAR(100) NOT NULL,
  `PasswordHash` VARCHAR(500) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `RoleId` INT NOT NULL,
  `PhoneNumber` VARCHAR(30) NULL,
  `FailedLoginCount` INT NOT NULL DEFAULT 0,
  `LockoutUntil` DATETIME(6) NULL,
  `PasswordChangedAt` DATETIME(6) NULL,
  `LastLoginAt` DATETIME(6) NULL,
  `LastLoginIp` VARCHAR(64) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_AppUsers_Email` (`Email`),
  UNIQUE KEY `UX_AppUsers_Username` (`Username`),
  KEY `FK_AppUsers_AppRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_AppUsers_AppRoles_RoleId`
    FOREIGN KEY (`RoleId`) REFERENCES `approles` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `PermissionModules` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_PermissionModules_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `menuitems` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `TenantId` INT NOT NULL DEFAULT 1,
  `ParentId` INT NULL,
  `Label` VARCHAR(100) NOT NULL,
  `Icon` VARCHAR(100) NULL,
  `Url` VARCHAR(300) NULL,
  `SectionLabel` VARCHAR(100) NULL,
  `Module` VARCHAR(100) NULL,
  `ModuleId` INT NULL,
  `Order` INT NOT NULL DEFAULT 0,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `ShowInSidebar` TINYINT(1) NOT NULL DEFAULT 1,
  `OpenInNewTab` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `FK_MenuItems_MenuItems_ParentId` (`ParentId`),
  KEY `IX_MenuItems_TenantId_ParentId_Order` (`TenantId`, `ParentId`, `Order`),
  KEY `IX_MenuItems_ModuleId` (`ModuleId`),
  CONSTRAINT `FK_MenuItems_MenuItems_ParentId`
    FOREIGN KEY (`ParentId`) REFERENCES `menuitems` (`Id`),
  CONSTRAINT `FK_MenuItems_PermissionModules_ModuleId`
    FOREIGN KEY (`ModuleId`) REFERENCES `PermissionModules` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `rolepermissions` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `RoleId` INT NOT NULL,
  `Module` VARCHAR(100) NOT NULL,
  `ModuleId` INT NULL,
  `CanSeeMenu` TINYINT(1) NOT NULL DEFAULT 0,
  `CanView` TINYINT(1) NOT NULL DEFAULT 0,
  `CanAdd` TINYINT(1) NOT NULL DEFAULT 0,
  `CanEdit` TINYINT(1) NOT NULL DEFAULT 0,
  `CanDelete` TINYINT(1) NOT NULL DEFAULT 0,
  `CanDownload` TINYINT(1) NOT NULL DEFAULT 0,
  `CanExport` TINYINT(1) NOT NULL DEFAULT 0,
  `CanApprove` TINYINT(1) NOT NULL DEFAULT 0,
  `CanForward` TINYINT(1) NOT NULL DEFAULT 0,
  `CanPrint` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_RolePermissions_Role_Module` (`RoleId`, `Module`),
  KEY `IX_RolePermissions_ModuleId` (`ModuleId`),
  CONSTRAINT `FK_RolePermissions_AppRoles_RoleId`
    FOREIGN KEY (`RoleId`) REFERENCES `approles` (`Id`),
  CONSTRAINT `FK_RolePermissions_PermissionModules_ModuleId`
    FOREIGN KEY (`ModuleId`) REFERENCES `PermissionModules` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `usersessions` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `SessionToken` VARCHAR(500) NOT NULL,
  `IpAddress` VARCHAR(64) NULL,
  `UserAgent` VARCHAR(500) NULL,
  `DeviceFingerprint` VARCHAR(200) NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `ExpiresAt` DATETIME(6) NOT NULL,
  `LastActivityAt` DATETIME(6) NULL,
  `RevokedAt` DATETIME(6) NULL,
  `RevokedReason` VARCHAR(300) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `FK_UserSessions_AppUsers_UserId` (`UserId`),
  KEY `IX_UserSessions_SessionToken` (`SessionToken`),
  CONSTRAINT `FK_UserSessions_AppUsers_UserId`
    FOREIGN KEY (`UserId`) REFERENCES `appusers` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `loginattempts` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(100) NOT NULL,
  `UserId` INT NULL,
  `IpAddress` VARCHAR(64) NULL,
  `UserAgent` VARCHAR(500) NULL,
  `Success` TINYINT(1) NOT NULL DEFAULT 0,
  `FailureReason` VARCHAR(200) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `FK_LoginAttempts_AppUsers_UserId` (`UserId`),
  KEY `IX_LoginAttempts_Username_CreatedAt` (`Username`, `CreatedAt`),
  CONSTRAINT `FK_LoginAttempts_AppUsers_UserId`
    FOREIGN KEY (`UserId`) REFERENCES `appusers` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `securitysettings` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `TenantId` INT NOT NULL DEFAULT 1,
  `SessionTimeoutMinutes` INT NOT NULL DEFAULT 480,
  `IdleTimeoutMinutes` INT NOT NULL DEFAULT 30,
  `PasswordMinLength` INT NOT NULL DEFAULT 8,
  `PasswordRequireUppercase` TINYINT(1) NOT NULL DEFAULT 1,
  `PasswordRequireLowercase` TINYINT(1) NOT NULL DEFAULT 1,
  `PasswordRequireDigit` TINYINT(1) NOT NULL DEFAULT 1,
  `PasswordRequireSpecialChar` TINYINT(1) NOT NULL DEFAULT 0,
  `PasswordExpiryDays` INT NOT NULL DEFAULT 90,
  `PasswordHistoryCount` INT NOT NULL DEFAULT 3,
  `MaxFailedLoginAttempts` INT NOT NULL DEFAULT 5,
  `LockoutDurationMinutes` INT NOT NULL DEFAULT 15,
  `EnableCaptchaAfterFailures` TINYINT(1) NOT NULL DEFAULT 0,
  `CaptchaAfterAttempts` INT NOT NULL DEFAULT 3,
  `AllowMultipleSessions` TINYINT(1) NOT NULL DEFAULT 1,
  `BlockNewLoginOnConflict` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_SecuritySettings_TenantId` (`TenantId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `auditlogs` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NULL,
  `Username` VARCHAR(100) NULL,
  `Action` INT NOT NULL,
  `Module` VARCHAR(100) NULL,
  `EntityId` VARCHAR(100) NULL,
  `Details` TEXT NULL,
  `IpAddress` VARCHAR(64) NULL,
  `UserAgent` VARCHAR(500) NULL,
  `Success` TINYINT(1) NOT NULL DEFAULT 1,
  `Timestamp` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_AuditLogs_UserId` (`UserId`),
  KEY `IX_AuditLogs_Timestamp` (`Timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `ConsumerUsers` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `ConsumerNo` VARCHAR(10) NOT NULL,
  `Username` VARCHAR(100) NOT NULL,
  `Email` VARCHAR(150) NULL,
  `PasswordHash` VARCHAR(500) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `FailedLoginCount` INT NOT NULL DEFAULT 0,
  `LockoutUntil` DATETIME(6) NULL,
  `LastLoginAt` DATETIME(6) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_ConsumerUsers_Username` (`Username`),
  KEY `IX_ConsumerUsers_ConsumerNo` (`ConsumerNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `ConsumerOtpVerifications` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ConsumerNo` VARCHAR(10) NOT NULL,
  `MobileNo` VARCHAR(12) NOT NULL,
  `OtpHash` VARCHAR(256) NOT NULL,
  `OtpSalt` VARCHAR(128) NOT NULL,
  `Purpose` VARCHAR(50) NOT NULL DEFAULT 'ConsumerLogin',
  `ExpiresAt` DATETIME(6) NOT NULL,
  `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
  `VerifiedAt` DATETIME(6) NULL,
  `AttemptCount` INT NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ConsumerOtp_ConsumerNo_Purpose` (`ConsumerNo`, `Purpose`),
  KEY `IX_ConsumerOtp_MobileNo` (`MobileNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Align New Connection audit columns with appusers.Id INT if those tables already exist.
-- These statements preserve applications/documents/history; incompatible old GUID values are cleared.
SET @schemaName = DATABASE();

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'new_connection_applications') > 0,
  'UPDATE `new_connection_applications` SET `SubmittedByConsumerUserId` = NULL, `CreatedBy` = NULL, `UpdatedBy` = NULL, `ApprovedBy` = NULL, `RejectedBy` = NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'new_connection_applications') > 0,
  'ALTER TABLE `new_connection_applications` MODIFY COLUMN `SubmittedByConsumerUserId` INT NULL, MODIFY COLUMN `CreatedBy` INT NULL, MODIFY COLUMN `UpdatedBy` INT NULL, MODIFY COLUMN `ApprovedBy` INT NULL, MODIFY COLUMN `RejectedBy` INT NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'new_connection_application_documents') > 0,
  'UPDATE `new_connection_application_documents` SET `UploadedBy` = NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'new_connection_application_documents') > 0,
  'ALTER TABLE `new_connection_application_documents` MODIFY COLUMN `UploadedBy` INT NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'NewConnectionApprovalHistory') > 0,
  'UPDATE `NewConnectionApprovalHistory` SET `ActionBy` = NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'NewConnectionApprovalHistory') > 0,
  'ALTER TABLE `NewConnectionApprovalHistory` MODIFY COLUMN `ActionBy` INT NULL',
  'SELECT 1'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Seed core roles.
INSERT INTO `approles` (`Id`, `Name`, `Description`, `CreatedAt`, `IsDeleted`)
VALUES
  (1, 'Admin', 'Full authority portal access', UTC_TIMESTAMP(6), 0),
  (2, 'Staff', 'Limited authority portal access', UTC_TIMESTAMP(6), 0),
  (3, 'Consumer', 'Consumer portal role', UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `Description` = VALUES(`Description`),
  `IsDeleted` = 0;

-- Seed default admin user.
-- Username: admin
-- Password: admin123
INSERT INTO `appusers`
  (`Id`, `FullName`, `Email`, `Username`, `PasswordHash`, `IsActive`, `RoleId`, `PasswordChangedAt`, `CreatedAt`, `IsDeleted`)
VALUES
  (1, 'System Administrator', 'admin@noidajal.local', 'admin', 'FhIf2waQb0aDz8oj+R8lsEx2idj6SwaFEd8bBtmvW1M=', 1, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `FullName` = VALUES(`FullName`),
  `Email` = VALUES(`Email`),
  `PasswordHash` = VALUES(`PasswordHash`),
  `IsActive` = 1,
  `RoleId` = 1,
  `IsDeleted` = 0;

-- Seed permission modules.
INSERT INTO `PermissionModules` (`Id`, `Name`, `IsActive`, `IsDeleted`)
VALUES
  (1, 'Dashboard', 1, 0),
  (2, 'Consumers', 1, 0),
  (3, 'Billing', 1, 0),
  (4, 'Payments', 1, 0),
  (5, 'Reports', 1, 0),
  (6, 'Role Management', 1, 0),
  (7, 'User Management', 1, 0),
  (8, 'Role Permission', 1, 0),
  (9, 'Menu Management', 1, 0),
  (10, 'Permission Modules', 1, 0),
  (11, 'Security Settings', 1, 0),
  (12, 'Profile', 1, 0),
  (17, 'Consumer Login Management', 1, 0),
  (13, 'Consumer Dashboard', 1, 0),
  (14, 'Consumer Bills', 1, 0),
  (15, 'Consumer Profile', 1, 0),
  (16, 'Consumer New Connection', 1, 0)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `IsActive` = 1,
  `IsDeleted` = 0;

-- Seed authority sidebar menus.
INSERT INTO `menuitems`
  (`Id`, `TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
VALUES
  (1, 1, NULL, 'Dashboard', 'DB', '/Dashboard', 'Main', 'Dashboard', 1, 10, 1, 1, UTC_TIMESTAMP(6), 0),
  (2, 1, NULL, 'Consumers', 'CS', '#', 'Main', 'Consumers', 2, 20, 1, 1, UTC_TIMESTAMP(6), 0),
  (3, 1, NULL, 'Billing', 'BL', '#', 'Main', 'Billing', 3, 30, 1, 1, UTC_TIMESTAMP(6), 0),
  (4, 1, NULL, 'Payments', 'PY', '#', 'Main', 'Payments', 4, 40, 1, 1, UTC_TIMESTAMP(6), 0),
  (5, 1, NULL, 'Reports', 'RP', '#', 'Main', 'Reports', 5, 50, 1, 1, UTC_TIMESTAMP(6), 0),
  (6, 1, NULL, 'Role Management', 'RM', '/Roles', 'Administration', 'Role Management', 6, 100, 1, 1, UTC_TIMESTAMP(6), 0),
  (7, 1, NULL, 'User Management', 'UM', '/Users', 'Administration', 'User Management', 7, 110, 1, 1, UTC_TIMESTAMP(6), 0),
  (8, 1, NULL, 'Role Permission', 'RP', '/RolePermissions', 'Administration', 'Role Permission', 8, 120, 1, 1, UTC_TIMESTAMP(6), 0),
  (9, 1, NULL, 'Menu Management', 'MM', '/Menu', 'Administration', 'Menu Management', 9, 130, 1, 1, UTC_TIMESTAMP(6), 0),
  (10, 1, NULL, 'Permission Modules', 'PM', '/PermissionModules', 'Administration', 'Permission Modules', 10, 140, 1, 1, UTC_TIMESTAMP(6), 0),
  (13, 1, NULL, 'Consumer Login Management', 'CL', '/ConsumerLoginManagement', 'Administration', 'Consumer Login Management', 17, 150, 1, 1, UTC_TIMESTAMP(6), 0),
  (11, 1, NULL, 'Security Settings', 'SS', '/SecuritySettings', 'Administration', 'Security Settings', 11, 160, 1, 1, UTC_TIMESTAMP(6), 0),
  (12, 1, NULL, 'Profile', 'PR', '/Profile', 'Account', 'Profile', 12, 200, 1, 1, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `TenantId` = VALUES(`TenantId`),
  `Label` = VALUES(`Label`),
  `Icon` = VALUES(`Icon`),
  `Url` = VALUES(`Url`),
  `SectionLabel` = VALUES(`SectionLabel`),
  `Module` = VALUES(`Module`),
  `ModuleId` = VALUES(`ModuleId`),
  `Order` = VALUES(`Order`),
  `IsActive` = 1,
  `ShowInSidebar` = 1,
  `IsDeleted` = 0;

-- Seed consumer sidebar menus under tenant 2.
INSERT INTO `menuitems`
  (`Id`, `TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
VALUES
  (101, 2, NULL, 'Dashboard', 'DB', '/Consumer/Dashboard', 'Main', 'Consumer Dashboard', 13, 10, 1, 1, UTC_TIMESTAMP(6), 0),
  (102, 2, NULL, 'Pay Bill', 'PB', '/Consumer/Bills/Pay', 'Main', 'Consumer Bills', 14, 20, 1, 1, UTC_TIMESTAMP(6), 0),
  (103, 2, NULL, 'Current Bill', 'CB', '/Consumer/Bills/Current', 'Main', 'Consumer Bills', 14, 30, 1, 1, UTC_TIMESTAMP(6), 0),
  (104, 2, NULL, 'Bill History', 'BH', '/Consumer/Bills/History', 'Main', 'Consumer Bills', 14, 40, 1, 1, UTC_TIMESTAMP(6), 0),
  (105, 2, NULL, 'New Connection', 'NC', '/Consumer/NewConnection/Apply', 'Main', 'Consumer New Connection', 16, 50, 1, 1, UTC_TIMESTAMP(6), 0),
  (106, 2, NULL, 'My Applications', 'MA', '/Consumer/NewConnection/MyApplications', 'Main', 'Consumer New Connection', 16, 60, 1, 1, UTC_TIMESTAMP(6), 0),
  (107, 2, NULL, 'Profile & Connections', 'PC', '/Consumer/Profile', 'Account', 'Consumer Profile', 15, 100, 1, 1, UTC_TIMESTAMP(6), 0),
  (108, 2, NULL, 'Update Mobile/Email', 'UE', '/Consumer/Profile/UpdateContact', 'Account', 'Consumer Profile', 15, 110, 1, 1, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `TenantId` = VALUES(`TenantId`),
  `Label` = VALUES(`Label`),
  `Icon` = VALUES(`Icon`),
  `Url` = VALUES(`Url`),
  `SectionLabel` = VALUES(`SectionLabel`),
  `Module` = VALUES(`Module`),
  `ModuleId` = VALUES(`ModuleId`),
  `Order` = VALUES(`Order`),
  `IsActive` = 1,
  `ShowInSidebar` = 1,
  `IsDeleted` = 0;

-- Give Admin full permissions to all seeded modules.
INSERT INTO `rolepermissions`
  (`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  1, pm.`Name`, pm.`Id`, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, UTC_TIMESTAMP(6), 0
FROM `PermissionModules` pm
WHERE pm.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE
  `ModuleId` = VALUES(`ModuleId`),
  `CanSeeMenu` = 1,
  `CanView` = 1,
  `CanAdd` = 1,
  `CanEdit` = 1,
  `CanDelete` = 1,
  `CanDownload` = 1,
  `CanExport` = 1,
  `CanApprove` = 1,
  `CanForward` = 1,
  `CanPrint` = 1,
  `IsDeleted` = 0;

-- Give Consumer portal role access to consumer modules only.
INSERT INTO `rolepermissions`
  (`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  3,
  pm.`Name`,
  pm.`Id`,
  1,
  1,
  CASE WHEN pm.`Name` = 'Consumer New Connection' THEN 1 ELSE 0 END,
  CASE WHEN pm.`Name` = 'Consumer Profile' THEN 1 ELSE 0 END,
  0,
  CASE WHEN pm.`Name` = 'Consumer Bills' THEN 1 ELSE 0 END,
  CASE WHEN pm.`Name` = 'Consumer Bills' THEN 1 ELSE 0 END,
  0,
  0,
  CASE WHEN pm.`Name` = 'Consumer Bills' THEN 1 ELSE 0 END,
  UTC_TIMESTAMP(6),
  0
FROM `PermissionModules` pm
WHERE pm.`Name` IN ('Consumer Dashboard', 'Consumer Bills', 'Consumer Profile', 'Consumer New Connection')
ON DUPLICATE KEY UPDATE
  `ModuleId` = VALUES(`ModuleId`),
  `CanSeeMenu` = VALUES(`CanSeeMenu`),
  `CanView` = VALUES(`CanView`),
  `CanAdd` = VALUES(`CanAdd`),
  `CanEdit` = VALUES(`CanEdit`),
  `CanDownload` = VALUES(`CanDownload`),
  `CanExport` = VALUES(`CanExport`),
  `CanPrint` = VALUES(`CanPrint`),
  `IsDeleted` = 0;

INSERT INTO `securitysettings` (`TenantId`, `CreatedAt`, `IsDeleted`)
VALUES (1, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE `IsDeleted` = 0;
