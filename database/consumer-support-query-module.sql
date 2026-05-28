-- Consumer Support / Query module for Water.Bill.
-- Run manually in the Water.Bill MySQL database.
-- Uses EF Core entity mappings; no stored procedures required.

CREATE TABLE IF NOT EXISTS `SupportQueryCategories` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `CategoryName` VARCHAR(100) NOT NULL,
  `Description` VARCHAR(250) NULL,
  `DisplayOrder` INT NOT NULL DEFAULT 0,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` DATETIME NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_SupportQueryCategories_CategoryName` (`CategoryName`)
);

CREATE TABLE IF NOT EXISTS `ConsumerSupportQueries` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `QueryNo` VARCHAR(30) NOT NULL,
  `ConsumerUserId` INT NULL,
  `ConsumerNo` VARCHAR(20) NOT NULL,
  `ConsumerName` VARCHAR(150) NOT NULL,
  `MobileNo` VARCHAR(15) NULL,
  `Email` VARCHAR(100) NULL,
  `CategoryId` INT NOT NULL,
  `CategoryName` VARCHAR(100) NOT NULL,
  `Subject` VARCHAR(150) NOT NULL,
  `Description` VARCHAR(2000) NOT NULL,
  `Priority` VARCHAR(20) NOT NULL DEFAULT 'Normal',
  `Status` VARCHAR(30) NOT NULL DEFAULT 'Open',
  `RelatedBillNo` VARCHAR(50) NULL,
  `RelatedApplicationNo` VARCHAR(50) NULL,
  `AdminRemarks` VARCHAR(1000) NULL,
  `AssignedToUserId` INT NULL,
  `ResolvedByUserId` INT NULL,
  `ResolvedAt` DATETIME NULL,
  `ClosedAt` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` DATETIME NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_ConsumerSupportQueries_QueryNo` (`QueryNo`),
  INDEX `IX_ConsumerSupportQueries_ConsumerNo` (`ConsumerNo`),
  INDEX `IX_ConsumerSupportQueries_Status` (`Status`),
  INDEX `IX_ConsumerSupportQueries_CategoryId` (`CategoryId`),
  INDEX `IX_ConsumerSupportQueries_CreatedAt` (`CreatedAt`),
  CONSTRAINT `FK_ConsumerSupportQueries_Category` FOREIGN KEY (`CategoryId`) REFERENCES `SupportQueryCategories` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `ConsumerSupportQueryDocuments` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `QueryId` BIGINT NOT NULL,
  `DocumentType` VARCHAR(50) NOT NULL DEFAULT 'Support Document',
  `FileName` VARCHAR(255) NOT NULL,
  `FilePath` VARCHAR(500) NOT NULL,
  `ContentType` VARCHAR(100) NULL,
  `FileSize` BIGINT NULL,
  `UploadedByConsumerUserId` INT NULL,
  `UploadedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_ConsumerSupportQueryDocuments_QueryId` (`QueryId`),
  CONSTRAINT `FK_ConsumerSupportQueryDocuments_Query` FOREIGN KEY (`QueryId`) REFERENCES `ConsumerSupportQueries` (`Id`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS `ConsumerSupportQueryHistories` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `QueryId` BIGINT NOT NULL,
  `FromStatus` VARCHAR(30) NULL,
  `ToStatus` VARCHAR(30) NOT NULL,
  `Action` VARCHAR(50) NOT NULL,
  `Remarks` VARCHAR(1000) NULL,
  `ActionByUserId` INT NULL,
  `ActionByName` VARCHAR(150) NULL,
  `ActionByRole` VARCHAR(50) NULL,
  `ActionAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_ConsumerSupportQueryHistories_QueryId` (`QueryId`),
  CONSTRAINT `FK_ConsumerSupportQueryHistories_Query` FOREIGN KEY (`QueryId`) REFERENCES `ConsumerSupportQueries` (`Id`) ON DELETE CASCADE
);

INSERT INTO `SupportQueryCategories`
(`CategoryName`, `Description`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
VALUES
('Billing Issue', 'General billing-related support request.', 10, 1, 0, NOW()),
('Payment Issue', 'Payment gateway, receipt, or payment posting issue.', 20, 1, 0, NOW()),
('Wrong Bill Amount', 'Consumer reports incorrect bill amount.', 30, 1, 0, NOW()),
('Payment Not Reflected', 'Paid amount is not reflected in bill account.', 40, 1, 0, NOW()),
('New Connection Issue', 'Issue related to new connection application.', 50, 1, 0, NOW()),
('Profile / Mobile / Email Update Issue', 'Issue in consumer profile or contact update.', 60, 1, 0, NOW()),
('Login / OTP Issue', 'Consumer login or OTP verification issue.', 70, 1, 0, NOW()),
('NDC / No Dues Issue', 'No dues certificate or NDC-related issue.', 80, 1, 0, NOW()),
('Water Supply Related', 'Water supply/service-related issue.', 90, 1, 0, NOW()),
('Other', 'Any other consumer support request.', 100, 1, 0, NOW())
ON DUPLICATE KEY UPDATE
  `Description` = VALUES(`Description`),
  `DisplayOrder` = VALUES(`DisplayOrder`),
  `IsActive` = 1,
  `IsDeleted` = 0,
  `UpdatedAt` = NOW();

-- Menu and permission seed.
SET @adminModuleName := 'Consumer Query Management';
SET @consumerModuleName := 'Consumer Support Queries';

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT @adminModuleName, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = @adminModuleName AND `IsDeleted` = 0);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT @consumerModuleName, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = @consumerModuleName AND `IsDeleted` = 0);

SET @adminModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = @adminModuleName AND `IsDeleted` = 0 LIMIT 1);
SET @consumerModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = @consumerModuleName AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, 'Consumer Query Management', 'Q', '/ConsumerQueryManagement', 'Operations', @adminModuleName, @adminModuleId, 80, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 1 AND `Label` = 'Consumer Query Management' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, NULL, 'Support & Queries', 'S', NULL, 'Support', NULL, NULL, 60, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Support & Queries' AND `IsDeleted` = 0);

SET @supportParentId := (SELECT `Id` FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Support & Queries' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @supportParentId, 'My Queries', 'Q', '/Consumer/SupportQueries', 'Support', @consumerModuleName, @consumerModuleId, 1, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'My Queries' AND `ParentId` = @supportParentId AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @supportParentId, 'Raise Query', '+', '/Consumer/SupportQueries/Create', 'Support', @consumerModuleName, @consumerModuleId, 2, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Raise Query' AND `ParentId` = @supportParentId AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @adminModuleName, @adminModuleId, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Admin'
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` rp WHERE rp.`RoleId` = r.`Id` AND rp.`ModuleId` = @adminModuleId AND rp.`IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @consumerModuleName, @consumerModuleId, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Consumer'
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` rp WHERE rp.`RoleId` = r.`Id` AND rp.`ModuleId` = @consumerModuleId AND rp.`IsDeleted` = 0);
