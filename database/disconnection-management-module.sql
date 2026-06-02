-- Disconnection / Reconnection Management module.
-- New additive case/history tables. Existing old consumer/challan/bill tables are not altered by this script.

CREATE TABLE IF NOT EXISTS `ConsumerDisconnectionCases` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `CaseNo` VARCHAR(30) NOT NULL,
    `ConsumerNo` VARCHAR(20) NOT NULL,
    `CaseType` VARCHAR(30) NOT NULL,
    `Reason` VARCHAR(100) NOT NULL,
    `Status` VARCHAR(30) NOT NULL,
    `NoticeDate` DATETIME NOT NULL,
    `DueDate` DATETIME NULL,
    `OutstandingAmount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `DisconnectionFee` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `ReconnectionFee` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `DisconnectedOn` DATETIME NULL,
    `ReconnectionRequestedOn` DATETIME NULL,
    `ReconnectedOn` DATETIME NULL,
    `ChallanNo` VARCHAR(30) NULL,
    `FieldOfficerName` VARCHAR(100) NULL,
    `Remarks` VARCHAR(500) NULL,
    `PreviousConsumerCategory` VARCHAR(20) NULL,
    `PreviousStatus` INT NULL,
    `PreviousNewStatus` INT NULL,
    `CreatedByUserId` INT NULL,
    `CreatedByName` VARCHAR(100) NULL,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedByUserId` INT NULL,
    `UpdatedByName` VARCHAR(100) NULL,
    `UpdatedAt` DATETIME NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ConsumerDisconnectionCases_CaseNo` (`CaseNo`),
    KEY `IX_ConsumerDisconnectionCases_Consumer_Status` (`ConsumerNo`, `Status`, `IsDeleted`),
    KEY `IX_ConsumerDisconnectionCases_Notice_Status` (`NoticeDate`, `Status`, `IsDeleted`)
);

CREATE TABLE IF NOT EXISTS `ConsumerDisconnectionCaseHistories` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `CaseId` BIGINT NOT NULL,
    `FromStatus` VARCHAR(30) NULL,
    `ToStatus` VARCHAR(30) NOT NULL,
    `Action` VARCHAR(50) NOT NULL,
    `Remarks` VARCHAR(500) NULL,
    `ActionByUserId` INT NULL,
    `ActionByName` VARCHAR(100) NULL,
    `ActionAt` DATETIME NOT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    KEY `IX_ConsumerDisconnectionCaseHistories_Case_ActionAt` (`CaseId`, `ActionAt`),
    CONSTRAINT `FK_ConsumerDisconnectionCaseHistories_Cases`
        FOREIGN KEY (`CaseId`) REFERENCES `ConsumerDisconnectionCases` (`Id`)
);

SET @moduleName := 'Disconnection / Reconnection Management';

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT @moduleName, 1, 0
WHERE NOT EXISTS (
    SELECT 1 FROM `PermissionModules`
    WHERE `Name` = @moduleName AND `IsDeleted` = 0
);

SET @moduleId := (
    SELECT `Id` FROM `PermissionModules`
    WHERE `Name` = @moduleName AND `IsDeleted` = 0
    LIMIT 1
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, @moduleName, 'DR', '/DisconnectionManagement', 'Billing', @moduleName, @moduleId, 70, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 1 AND `Label` = @moduleName AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM `rolepermissions`
      WHERE `RoleId` = r.`Id` AND `ModuleId` = @moduleId AND `IsDeleted` = 0
  );

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanAdd` = 1,
    `CanEdit` = 1,
    `CanDelete` = 1,
    `CanDownload` = 1,
    `CanExport` = 1,
    `CanPrint` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin')
  AND `IsDeleted` = 0;
