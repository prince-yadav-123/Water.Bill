-- Notice Management module.
-- New additive notice/template tables. Existing old tables are not altered.

CREATE TABLE IF NOT EXISTS `NoticeTemplates` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `TemplateName` VARCHAR(100) NOT NULL,
    `NoticeType` VARCHAR(50) NOT NULL,
    `Subject` VARCHAR(200) NOT NULL,
    `Body` TEXT NOT NULL,
    `DisplayOrder` INT NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_NoticeTemplates_Type` (`NoticeType`, `IsDeleted`),
    KEY `IX_NoticeTemplates_Name` (`TemplateName`, `IsDeleted`)
);

CREATE TABLE IF NOT EXISTS `ConsumerNotices` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `NoticeNo` VARCHAR(30) NOT NULL,
    `ConsumerNo` VARCHAR(20) NOT NULL,
    `TemplateId` INT NULL,
    `NoticeType` VARCHAR(50) NOT NULL,
    `Subject` VARCHAR(200) NOT NULL,
    `Body` TEXT NOT NULL,
    `NoticeDate` DATETIME NOT NULL,
    `DueDate` DATETIME NULL,
    `Status` VARCHAR(30) NOT NULL,
    `RelatedBillNo` VARCHAR(30) NULL,
    `RelatedChallanNo` VARCHAR(30) NULL,
    `RelatedDisconnectionCaseId` BIGINT NULL,
    `AmountDue` DECIMAL(18,2) NOT NULL DEFAULT 0,
    `Remarks` VARCHAR(500) NULL,
    `CreatedByUserId` INT NULL,
    `CreatedByName` VARCHAR(100) NULL,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedByUserId` INT NULL,
    `UpdatedByName` VARCHAR(100) NULL,
    `UpdatedAt` DATETIME NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ConsumerNotices_NoticeNo` (`NoticeNo`),
    KEY `IX_ConsumerNotices_Consumer_Status` (`ConsumerNo`, `Status`, `IsDeleted`),
    KEY `IX_ConsumerNotices_Date_Type` (`NoticeDate`, `NoticeType`, `IsDeleted`),
    KEY `IX_ConsumerNotices_TemplateId` (`TemplateId`),
    CONSTRAINT `FK_ConsumerNotices_NoticeTemplates`
        FOREIGN KEY (`TemplateId`) REFERENCES `NoticeTemplates` (`Id`)
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS `ConsumerNoticeHistories` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `NoticeId` BIGINT NOT NULL,
    `FromStatus` VARCHAR(30) NULL,
    `ToStatus` VARCHAR(30) NOT NULL,
    `Action` VARCHAR(50) NOT NULL,
    `Remarks` VARCHAR(500) NULL,
    `ActionByUserId` INT NULL,
    `ActionByName` VARCHAR(100) NULL,
    `ActionAt` DATETIME NOT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    KEY `IX_ConsumerNoticeHistories_Notice_ActionAt` (`NoticeId`, `ActionAt`),
    CONSTRAINT `FK_ConsumerNoticeHistories_Notices`
        FOREIGN KEY (`NoticeId`) REFERENCES `ConsumerNotices` (`Id`)
);

INSERT INTO `NoticeTemplates`
(`TemplateName`, `NoticeType`, `Subject`, `Body`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT 'Default Due Notice', 'DueNotice', 'Pending water bill dues',
       'Dear {ConsumerName}, dues of Rs. {AmountDue} are pending against consumer no {ConsumerNo}, property {PropertyNo}. Please pay before {DueDate}.',
       1, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `NoticeTemplates` WHERE `TemplateName` = 'Default Due Notice' AND `IsDeleted` = 0);

INSERT INTO `NoticeTemplates`
(`TemplateName`, `NoticeType`, `Subject`, `Body`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT 'Default Disconnection Notice', 'DisconnectionNotice', 'Disconnection notice for pending dues',
       'Dear {ConsumerName}, your water connection {ConsumerNo} is liable for disconnection due to pending dues of Rs. {AmountDue}. Please clear before {DueDate}.',
       2, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `NoticeTemplates` WHERE `TemplateName` = 'Default Disconnection Notice' AND `IsDeleted` = 0);

INSERT INTO `NoticeTemplates`
(`TemplateName`, `NoticeType`, `Subject`, `Body`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT 'Default Demand Notice', 'DemandNotice', 'Demand notice',
       'Demand notice is issued for consumer no {ConsumerNo}, property {PropertyNo}, for amount Rs. {AmountDue}. Reference bill no: {BillNo}.',
       3, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `NoticeTemplates` WHERE `TemplateName` = 'Default Demand Notice' AND `IsDeleted` = 0);

INSERT INTO `NoticeTemplates`
(`TemplateName`, `NoticeType`, `Subject`, `Body`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT 'Default Reconnection Order', 'ReconnectionOrder', 'Reconnection order',
       'Reconnection order is issued for consumer no {ConsumerNo}, property {PropertyNo}. Reference challan no: {ChallanNo}.',
       4, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `NoticeTemplates` WHERE `TemplateName` = 'Default Reconnection Order' AND `IsDeleted` = 0);

INSERT INTO `NoticeTemplates`
(`TemplateName`, `NoticeType`, `Subject`, `Body`, `DisplayOrder`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT 'Default General Notice', 'GeneralNotice', 'General notice',
       'General notice is issued for consumer no {ConsumerNo}, {ConsumerName}, property {PropertyNo}.',
       5, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `NoticeTemplates` WHERE `TemplateName` = 'Default General Notice' AND `IsDeleted` = 0);

SET @moduleName := 'Notice Management';

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
SELECT 1, NULL, @moduleName, 'NM', '/NoticeManagement', 'Billing', @moduleName, @moduleId, 71, 1, 1, 0, NOW(6), 0
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
