-- New Connection Application module - EF Core matching schema
-- Safe to run once. Does not modify final consumer tables and does not call old stored procedures.

CREATE TABLE IF NOT EXISTS `new_connection_applications` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `ApplicationStatus` VARCHAR(30) NOT NULL DEFAULT 'Draft',
  `FinalConsumerNo` VARCHAR(10) NULL,
  `IsPublicApplication` TINYINT(1) NOT NULL DEFAULT 0,
  `ApplicantName` VARCHAR(100) NOT NULL,
  `FatherName` VARCHAR(100) NULL,
  `MobileNumber` VARCHAR(12) NOT NULL,
  `EmailId` VARCHAR(50) NULL,
  `Address` VARCHAR(150) NOT NULL,
  `Sector` VARCHAR(10) NOT NULL,
  `Block` VARCHAR(10) NOT NULL,
  `FlatNo` VARCHAR(15) NOT NULL,
  `PlotSize` DECIMAL(8,2) NOT NULL,
  `PipeSize` DECIMAL(8,2) NULL,
  `KhasraNo` VARCHAR(20) NULL,
  `VillageName` VARCHAR(100) NULL,
  `VillageId` INT NULL,
  `ConnectionCategory` VARCHAR(2) NOT NULL,
  `ConnectionType` VARCHAR(10) NOT NULL,
  `FlatType` VARCHAR(10) NOT NULL,
  `PurposeOfConnection` VARCHAR(50) NULL,
  `PreviousConnectionYesNo` VARCHAR(1) NULL,
  `OtherConnection` VARCHAR(150) NULL,
  `Rid` VARCHAR(15) NULL,
  `DevType` INT NULL,
  `RegNo` VARCHAR(10) NULL,
  `ConnectionDate` DATETIME NULL,
  `EstimationNo` VARCHAR(10) NULL,
  `EstimationAmount` DECIMAL(12,2) NULL,
  `SecurityAmount` DECIMAL(12,2) NULL,
  `EstimationDate` DATETIME NULL,
  `CessAmount` DECIMAL(12,2) NULL,
  `MonthlyCharges` DECIMAL(12,2) NULL,
  `IssueOfficer` VARCHAR(50) NULL,
  `AllotmentDate` DATE NULL,
  `PossessionDate` DATE NULL,
  `ComplianceDate` DATE NULL,
  `SsiDate` DATE NULL,
  `AffidavitYn` VARCHAR(2) NULL,
  `SubmittedByConsumerNo` VARCHAR(10) NULL,
  `SubmittedByConsumerUserId` CHAR(36) NULL,
  `SubmittedOn` DATETIME NULL,
  `CreatedBy` CHAR(36) NULL,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedBy` CHAR(36) NULL,
  `UpdatedOn` DATETIME NULL,
  `ApprovedBy` CHAR(36) NULL,
  `ApprovedOn` DATETIME NULL,
  `RejectedBy` CHAR(36) NULL,
  `RejectedOn` DATETIME NULL,
  `RejectionReason` VARCHAR(500) NULL,
  `Remarks` VARCHAR(500) NULL,
  `DeclarationAccepted` TINYINT(1) NOT NULL DEFAULT 0,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_NewConnectionApplications_ApplicationNo` (`ApplicationNo`),
  KEY `IX_NewConnectionApplications_MobileNumber` (`MobileNumber`),
  KEY `IX_NewConnectionApplications_ApplicationStatus` (`ApplicationStatus`),
  KEY `IX_NewConnectionApplications_SubmittedByConsumerNo` (`SubmittedByConsumerNo`),
  KEY `IX_NewConnectionApplications_SubmittedByConsumerUserId` (`SubmittedByConsumerUserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `new_connection_application_documents` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationId` BIGINT NOT NULL,
  `DocumentType` VARCHAR(50) NOT NULL,
  `DocumentNo` VARCHAR(100) NULL,
  `DocumentDate` DATE NULL,
  `FileName` VARCHAR(200) NULL,
  `FilePath` VARCHAR(500) NULL,
  `ContentType` VARCHAR(100) NULL,
  `FileSize` BIGINT NULL,
  `UploadedBy` CHAR(36) NULL,
  `UploadedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_NewConnectionApplicationDocuments_ApplicationId` (`ApplicationId`),
  KEY `IX_NewConnectionApplicationDocuments_DocumentType` (`DocumentType`),
  CONSTRAINT `FK_NewConnectionApplicationDocuments_Applications`
    FOREIGN KEY (`ApplicationId`) REFERENCES `new_connection_applications` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `NewConnectionApprovalHistory` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationId` BIGINT NOT NULL,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `FromStatus` VARCHAR(30) NULL,
  `ToStatus` VARCHAR(30) NOT NULL,
  `Action` VARCHAR(50) NOT NULL,
  `Remarks` VARCHAR(500) NULL,
  `ActionBy` CHAR(36) NULL,
  `ActionByName` VARCHAR(150) NULL,
  `ActionByRole` VARCHAR(50) NULL,
  `ActionOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IpAddress` VARCHAR(50) NULL,
  `UserAgent` VARCHAR(500) NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_NewConnectionApprovalHistory_ApplicationId` (`ApplicationId`),
  KEY `IX_NewConnectionApprovalHistory_ApplicationNo` (`ApplicationNo`),
  KEY `IX_NewConnectionApprovalHistory_ActionOn` (`ActionOn`),
  CONSTRAINT `FK_NewConnectionApprovalHistory_Applications`
    FOREIGN KEY (`ApplicationId`) REFERENCES `new_connection_applications` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Optional menu/permission seed for Consumer Portal visibility.
-- Uses fixed IDs so repeated execution updates existing rows instead of duplicating them.
SET @NewConnectionModuleId = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa31';
SET @NewConnectionMenuId = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa32';
SET @MyApplicationsMenuId = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa33';

INSERT INTO `PermissionModules` (`Id`, `Name`, `IsActive`, `IsDeleted`)
VALUES (@NewConnectionModuleId, 'New Connection', 1, 0)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `IsActive` = 1,
  `IsDeleted` = 0;

INSERT INTO `menuitems`
  (`Id`, `TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
VALUES
  (@NewConnectionMenuId, '00000000-0000-0000-0000-000000000000', NULL, 'New Connection', 'plus-circle', '/Consumer/NewConnection/Apply', 'MAIN', 'New Connection', @NewConnectionModuleId, 50, 1, 1, 0, CURRENT_TIMESTAMP(6), 0),
  (@MyApplicationsMenuId, '00000000-0000-0000-0000-000000000000', NULL, 'My Applications', 'clipboard-list', '/Consumer/NewConnection/MyApplications', 'MAIN', 'New Connection', @NewConnectionModuleId, 51, 1, 1, 0, CURRENT_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `Label` = VALUES(`Label`),
  `Icon` = VALUES(`Icon`),
  `Url` = VALUES(`Url`),
  `SectionLabel` = VALUES(`SectionLabel`),
  `Module` = VALUES(`Module`),
  `ModuleId` = VALUES(`ModuleId`),
  `Order` = VALUES(`Order`),
  `IsActive` = 1,
  `ShowInSidebar` = 1,
  `UpdatedAt` = CURRENT_TIMESTAMP(6),
  `IsDeleted` = 0;

INSERT INTO `rolepermissions`
  (`Id`, `RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa34',
  r.`Id`,
  'New Connection',
  @NewConnectionModuleId,
  1, 1, 1, 0, 0, 0, 0, 0, 0, 1,
  CURRENT_TIMESTAMP(6),
  0
FROM `approles` r
WHERE LOWER(r.`Name`) = 'consumer'
ON DUPLICATE KEY UPDATE
  `CanSeeMenu` = 1,
  `CanView` = 1,
  `CanAdd` = 1,
  `CanPrint` = 1,
  `UpdatedAt` = CURRENT_TIMESTAMP(6),
  `IsDeleted` = 0;
