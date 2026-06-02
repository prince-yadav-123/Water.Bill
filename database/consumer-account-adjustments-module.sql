-- Consumer Account Adjustments module.
-- New additive ledger tables. Existing old bill/challan/consumer tables are not altered.

CREATE TABLE IF NOT EXISTS `ConsumerAccountAdjustments` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `AdjustmentNo` VARCHAR(30) NOT NULL,
    `ConsumerNo` VARCHAR(20) NOT NULL,
    `AdjustmentType` VARCHAR(30) NOT NULL,
    `Amount` DECIMAL(18,2) NOT NULL,
    `EffectiveDate` DATETIME NOT NULL,
    `SourceBillNo` VARCHAR(30) NULL,
    `SourceChallanNo` VARCHAR(30) NULL,
    `Remarks` VARCHAR(500) NULL,
    `Status` VARCHAR(20) NOT NULL,
    `AppliedBillNo` VARCHAR(30) NULL,
    `AppliedOn` DATETIME NULL,
    `ReversalOfAdjustmentId` BIGINT NULL,
    `CreatedByUserId` INT NULL,
    `CreatedByName` VARCHAR(100) NULL,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedByUserId` INT NULL,
    `UpdatedByName` VARCHAR(100) NULL,
    `UpdatedAt` DATETIME NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ConsumerAccountAdjustments_AdjustmentNo` (`AdjustmentNo`),
    KEY `IX_ConsumerAccountAdjustments_Consumer_Status` (`ConsumerNo`, `Status`, `IsDeleted`),
    KEY `IX_ConsumerAccountAdjustments_Effective_Status` (`EffectiveDate`, `Status`, `IsDeleted`)
);

CREATE TABLE IF NOT EXISTS `ConsumerAccountAdjustmentHistories` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `AdjustmentId` BIGINT NOT NULL,
    `FromStatus` VARCHAR(20) NULL,
    `ToStatus` VARCHAR(20) NOT NULL,
    `Action` VARCHAR(50) NOT NULL,
    `Remarks` VARCHAR(500) NULL,
    `ActionByUserId` INT NULL,
    `ActionByName` VARCHAR(100) NULL,
    `ActionAt` DATETIME NOT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    KEY `IX_ConsumerAccountAdjustmentHistories_Adjustment` (`AdjustmentId`, `ActionAt`),
    CONSTRAINT `FK_ConsumerAccountAdjustmentHistories_Adjustments`
        FOREIGN KEY (`AdjustmentId`) REFERENCES `ConsumerAccountAdjustments` (`Id`)
);

SET @moduleName := 'Consumer Account Adjustments';

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
SELECT 1, NULL, @moduleName, 'AA', '/ConsumerAccountAdjustments', 'Billing', @moduleName, @moduleId, 67, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 1 AND `Label` = @moduleName AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, NOW(6), 0
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
    `CanDelete` = 1,
    `CanDownload` = 1,
    `CanExport` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin')
  AND `IsDeleted` = 0;
