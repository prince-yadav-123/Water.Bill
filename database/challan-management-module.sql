-- Challan Management module for Water.Bill Authority Portal.
-- Run manually in the Water.Bill MySQL database.
-- Uses existing imported tables: challan, consumer_details_master, bank_master,
-- jal_print_bill_master, and master_noc_amt. No stored procedures are used.

CREATE TABLE IF NOT EXISTS `ChallanHistories` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ChallanId` BIGINT NOT NULL,
  `ChallanNo` VARCHAR(30) NULL,
  `ConsumerNo` VARCHAR(15) NULL,
  `FromStatus` VARCHAR(30) NULL,
  `ToStatus` VARCHAR(30) NULL,
  `Action` VARCHAR(50) NOT NULL,
  `Remarks` VARCHAR(500) NULL,
  `ActionByUserId` INT NULL,
  `ActionByName` VARCHAR(150) NULL,
  `ActionOn` DATETIME NOT NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ChallanHistories_ChallanId` (`ChallanId`),
  KEY `IX_ChallanHistories_ChallanNo` (`ChallanNo`)
);

CREATE TABLE IF NOT EXISTS `ChallanPaymentHistories` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ChallanId` BIGINT NOT NULL,
  `ChallanNo` VARCHAR(30) NULL,
  `ConsumerNo` VARCHAR(15) NULL,
  `SourceBillNo` VARCHAR(30) NULL,
  `Amount` DOUBLE NOT NULL,
  `PaymentDate` DATETIME NOT NULL,
  `PaymentMode` VARCHAR(50) NULL,
  `BankCode` VARCHAR(100) NULL,
  `BankName` VARCHAR(150) NULL,
  `TransactionReferenceNo` VARCHAR(100) NULL,
  `Remarks` VARCHAR(500) NULL,
  `PostedByUserId` INT NULL,
  `PostedByName` VARCHAR(150) NULL,
  `PostedOn` DATETIME NOT NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ChallanPaymentHistories_ChallanId` (`ChallanId`),
  KEY `IX_ChallanPaymentHistories_ChallanNo` (`ChallanNo`),
  KEY `IX_ChallanPaymentHistories_ConsumerNo` (`ConsumerNo`),
  KEY `IX_ChallanPaymentHistories_PaymentDate` (`PaymentDate`)
);

SET @moduleName := 'Challan Management';

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
SELECT 1, NULL, @moduleName, 'CH', '/ChallanManagement', 'Billing', @moduleName, @moduleId, 72, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 1 AND `Label` = @moduleName AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, NOW(6), 0
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
    `CanDownload` = 1,
    `CanExport` = 1,
    `CanPrint` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin')
  AND `IsDeleted` = 0;
