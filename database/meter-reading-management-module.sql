-- Meter Reading Management module.
-- New additive table. Existing old consumer/bill/challan tables are not altered.

CREATE TABLE IF NOT EXISTS `ConsumerMeterReadings` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ReadingNo` VARCHAR(30) NOT NULL,
    `ConsumerNo` VARCHAR(20) NOT NULL,
    `ReadingDate` DATETIME NOT NULL,
    `PeriodFrom` DATETIME NULL,
    `PeriodTo` DATETIME NULL,
    `PreviousReading` DECIMAL(18,2) NULL,
    `CurrentReading` DECIMAL(18,2) NOT NULL,
    `Consumption` DECIMAL(18,2) NOT NULL,
    `MeterStatus` VARCHAR(30) NOT NULL,
    `MeterNo` VARCHAR(50) NULL,
    `Remarks` VARCHAR(500) NULL,
    `Source` VARCHAR(30) NOT NULL DEFAULT 'Admin',
    `RecordedByUserId` INT NULL,
    `RecordedByName` VARCHAR(100) NULL,
    `RecordedAt` DATETIME NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ConsumerMeterReadings_ReadingNo` (`ReadingNo`),
    KEY `IX_ConsumerMeterReadings_Consumer_Date` (`ConsumerNo`, `ReadingDate`, `IsDeleted`),
    KEY `IX_ConsumerMeterReadings_Status` (`MeterStatus`, `IsDeleted`)
);

SET @moduleName := 'Meter Reading Management';

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
SELECT 1, NULL, @moduleName, 'MR', '/MeterReadingManagement', 'Billing', @moduleName, @moduleId, 69, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 1 AND `Label` = @moduleName AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, NOW(6), 0
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
    `CanDownload` = 1,
    `CanExport` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin')
  AND `IsDeleted` = 0;
