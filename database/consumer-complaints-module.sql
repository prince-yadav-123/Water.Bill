-- Consumer Complaint / Service Request module.
-- Additive tables only. Existing old tables are not altered.

CREATE TABLE IF NOT EXISTS `ComplaintCategories` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `CategoryName` VARCHAR(100) NOT NULL,
  `Description` VARCHAR(300) NULL,
  `DisplayOrder` INT NOT NULL DEFAULT 0,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` DATETIME NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_ComplaintCategories_CategoryName` (`CategoryName`)
);

CREATE TABLE IF NOT EXISTS `ConsumerComplaints` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ComplaintNo` VARCHAR(30) NOT NULL,
  `ConsumerUserId` INT NULL,
  `ConsumerNo` VARCHAR(20) NOT NULL,
  `ConsumerName` VARCHAR(150) NOT NULL,
  `MobileNo` VARCHAR(15) NULL,
  `Email` VARCHAR(100) NULL,
  `CategoryId` INT NOT NULL,
  `CategoryName` VARCHAR(100) NOT NULL,
  `Subject` VARCHAR(150) NOT NULL,
  `Description` VARCHAR(2500) NOT NULL,
  `Priority` VARCHAR(20) NOT NULL DEFAULT 'Normal',
  `Status` VARCHAR(30) NOT NULL DEFAULT 'Open',
  `LocationDetails` VARCHAR(500) NULL,
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
  UNIQUE KEY `UX_ConsumerComplaints_ComplaintNo` (`ComplaintNo`),
  INDEX `IX_ConsumerComplaints_ConsumerNo` (`ConsumerNo`),
  INDEX `IX_ConsumerComplaints_Status` (`Status`),
  INDEX `IX_ConsumerComplaints_CategoryId` (`CategoryId`),
  INDEX `IX_ConsumerComplaints_CreatedAt` (`CreatedAt`),
  CONSTRAINT `FK_ConsumerComplaints_Category` FOREIGN KEY (`CategoryId`) REFERENCES `ComplaintCategories` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `ConsumerComplaintDocuments` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ComplaintId` BIGINT NOT NULL,
  `DocumentType` VARCHAR(100) NOT NULL DEFAULT 'Complaint Document',
  `FileName` VARCHAR(255) NOT NULL,
  `FilePath` VARCHAR(500) NOT NULL,
  `ContentType` VARCHAR(100) NULL,
  `FileSize` BIGINT NOT NULL DEFAULT 0,
  `UploadedByConsumerUserId` INT NULL,
  `UploadedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_ConsumerComplaintDocuments_ComplaintId` (`ComplaintId`),
  CONSTRAINT `FK_ConsumerComplaintDocuments_Complaint` FOREIGN KEY (`ComplaintId`) REFERENCES `ConsumerComplaints` (`Id`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS `ConsumerComplaintHistories` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ComplaintId` BIGINT NOT NULL,
  `FromStatus` VARCHAR(30) NULL,
  `ToStatus` VARCHAR(30) NOT NULL,
  `Action` VARCHAR(50) NOT NULL,
  `Remarks` VARCHAR(1000) NULL,
  `ActionByUserId` INT NULL,
  `ActionByName` VARCHAR(100) NULL,
  `ActionByRole` VARCHAR(50) NULL,
  `ActionAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_ConsumerComplaintHistories_ComplaintId` (`ComplaintId`),
  CONSTRAINT `FK_ConsumerComplaintHistories_Complaint` FOREIGN KEY (`ComplaintId`) REFERENCES `ConsumerComplaints` (`Id`) ON DELETE CASCADE
);

INSERT INTO `ComplaintCategories`
(`CategoryName`, `Description`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
VALUES
('Water Leakage', 'Leakage in supply line, meter chamber, or public line.', 10, 1, 0, NOW()),
('No Water Supply', 'No water supply or interrupted supply.', 20, 1, 0, NOW()),
('Low Pressure', 'Low water pressure complaint.', 30, 1, 0, NOW()),
('Meter Issue', 'Meter stopped, damaged, unreadable, or replacement related.', 40, 1, 0, NOW()),
('Sewer / Drainage Issue', 'Sewer overflow, blockage, or drainage-related issue.', 50, 1, 0, NOW()),
('Billing Service Request', 'Billing-related service request requiring field/admin action.', 60, 1, 0, NOW()),
('Disconnection / Reconnection', 'Service request related to disconnection or reconnection.', 70, 1, 0, NOW()),
('Other', 'Any other service complaint or request.', 100, 1, 0, NOW())
ON DUPLICATE KEY UPDATE
  `Description` = VALUES(`Description`),
  `DisplayOrder` = VALUES(`DisplayOrder`),
  `IsActive` = 1,
  `IsDeleted` = 0,
  `UpdatedAt` = NOW();

-- Menu and permission seed.
SET @adminModuleName := 'Complaint Management';
SET @consumerModuleName := 'Consumer Complaints';

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
SELECT 1, NULL, 'Complaint Management', 'CM', '/ComplaintManagement', 'Operations', @adminModuleName, @adminModuleId, 82, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 1 AND `Label` = 'Complaint Management' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, NULL, 'Complaints & Requests', 'CR', NULL, 'Support', NULL, NULL, 65, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Complaints & Requests' AND `IsDeleted` = 0);

SET @complaintParentId := (SELECT `Id` FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Complaints & Requests' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @complaintParentId, 'My Complaints', 'MC', '/Consumer/Complaints', 'Support', @consumerModuleName, @consumerModuleId, 1, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'My Complaints' AND `ParentId` = @complaintParentId AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @complaintParentId, 'Raise Complaint', '+', '/Consumer/Complaints/Create', 'Support', @consumerModuleName, @consumerModuleId, 2, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 2 AND `Label` = 'Raise Complaint' AND `ParentId` = @complaintParentId AND `IsDeleted` = 0);

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
