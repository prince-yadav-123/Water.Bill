-- Dynamic Communication / Notification system for Water.Bill
-- Safe to run multiple times. Adds only new communication tables plus menu/permission seed.

CREATE TABLE IF NOT EXISTS `CommunicationPurposes` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PurposeKey` VARCHAR(100) NOT NULL,
  `DisplayName` VARCHAR(150) NOT NULL,
  `Description` VARCHAR(500) NULL,
  `AllowedPlaceholders` JSON NOT NULL,
  `IsSystem` TINYINT(1) NOT NULL DEFAULT 1,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_CommunicationPurposes_PurposeKey` (`PurposeKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `CommunicationTemplates` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PurposeId` INT NOT NULL,
  `PurposeKey` VARCHAR(100) NOT NULL,
  `Channel` VARCHAR(20) NOT NULL,
  `TemplateName` VARCHAR(150) NOT NULL,
  `Subject` VARCHAR(300) NULL,
  `Body` TEXT NOT NULL,
  `ExternalTemplateId` VARCHAR(150) NULL,
  `Language` VARCHAR(10) NULL,
  `IsDefault` TINYINT(1) NOT NULL DEFAULT 1,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` DATETIME(6) NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CommunicationTemplates_PurposeId` (`PurposeId`),
  KEY `IX_CommunicationTemplates_DefaultLookup` (`PurposeKey`, `Channel`, `Language`, `IsDefault`, `IsActive`, `IsDeleted`),
  CONSTRAINT `FK_CommunicationTemplates_CommunicationPurposes_PurposeId`
    FOREIGN KEY (`PurposeId`) REFERENCES `CommunicationPurposes` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `CommunicationTemplates`
  MODIFY COLUMN `Language` VARCHAR(10) NULL;

UPDATE `CommunicationTemplates`
SET `Language` = NULL
WHERE `Language` = 'en';

CREATE TABLE IF NOT EXISTS `CommunicationLogs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `PurposeKey` VARCHAR(100) NOT NULL,
  `Channel` VARCHAR(20) NOT NULL,
  `RecipientName` VARCHAR(150) NULL,
  `RecipientEmail` VARCHAR(150) NULL,
  `RecipientMobile` VARCHAR(20) NULL,
  `Subject` VARCHAR(300) NULL,
  `MessageBody` TEXT NOT NULL,
  `TemplateId` INT NULL,
  `ExternalTemplateId` VARCHAR(150) NULL,
  `Status` VARCHAR(20) NOT NULL DEFAULT 'Pending',
  `ErrorMessage` VARCHAR(1000) NULL,
  `SentAt` DATETIME(6) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `ReferenceType` VARCHAR(100) NULL,
  `ReferenceId` VARCHAR(100) NULL,
  `ReferenceNo` VARCHAR(100) NULL,
  `RetryCount` INT NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_CommunicationLogs_Purpose_Channel_CreatedAt` (`PurposeKey`, `Channel`, `CreatedAt`),
  KEY `IX_CommunicationLogs_Reference` (`ReferenceType`, `ReferenceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `InAppNotifications` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `UserType` VARCHAR(20) NOT NULL,
  `UserId` BIGINT NOT NULL,
  `Title` VARCHAR(300) NOT NULL,
  `Message` TEXT NOT NULL,
  `PurposeKey` VARCHAR(100) NOT NULL,
  `ReferenceType` VARCHAR(100) NULL,
  `ReferenceId` VARCHAR(100) NULL,
  `ReferenceNo` VARCHAR(100) NULL,
  `IsRead` TINYINT(1) NOT NULL DEFAULT 0,
  `ReadAt` DATETIME(6) NULL,
  `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_InAppNotifications_User_Read` (`UserType`, `UserId`, `IsRead`, `IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'ConsumerOtp', 'Consumer OTP', 'OTP for existing consumer login.', JSON_ARRAY('ConsumerName','ConsumerNo','Otp','Date','ExpiryMinutes'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'ConsumerOtp');

UPDATE `CommunicationPurposes`
SET `AllowedPlaceholders` = JSON_ARRAY('ConsumerName','ConsumerNo','Otp','Date','ExpiryMinutes'),
    `UpdatedAt` = NOW(6)
WHERE `PurposeKey` = 'ConsumerOtp';

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'PublicNewConnectionOtp', 'Public New Connection OTP', 'OTP for public new connection verification.', JSON_ARRAY('ConsumerName','MobileNo','Otp','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'PublicNewConnectionOtp');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'NewConnectionSubmitted', 'New Connection Submitted', 'New connection application submitted.', JSON_ARRAY('ConsumerName','ApplicationNo','Amount','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'NewConnectionSubmitted');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'ApprovalStageAssigned', 'Approval Stage Assigned', 'Approval workflow stage assigned.', JSON_ARRAY('ConsumerName','ApplicationNo','StageName','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'ApprovalStageAssigned');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'QueryRaised', 'Consumer Query Raised', 'Consumer support query created.', JSON_ARRAY('ConsumerName','ConsumerNo','QueryNo','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'QueryRaised');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'QueryResolved', 'Consumer Query Resolved', 'Consumer query status updated/resolved.', JSON_ARRAY('ConsumerName','ConsumerNo','QueryNo','Status','Remarks','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'QueryResolved');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'ChallanGenerated', 'Challan Generated', 'Payment request/challan generated.', JSON_ARRAY('ConsumerName','ConsumerNo','ChallanNo','Purpose','Amount','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'ChallanGenerated');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'PaymentSuccess', 'Payment Success', 'Payment success confirmation.', JSON_ARRAY('ConsumerName','ConsumerNo','ChallanNo','BillNo','Amount','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'PaymentSuccess');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'PaymentFailed', 'Payment Failed', 'Payment failure alert.', JSON_ARRAY('ConsumerName','ConsumerNo','ChallanNo','BillNo','Amount','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'PaymentFailed');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'NewConnectionApproved', 'New Connection Approved', 'New connection approved.', JSON_ARRAY('ConsumerName','ApplicationNo','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'NewConnectionApproved');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'FinalConsumerCreated', 'Final Consumer Created', 'Final consumer number generated.', JSON_ARRAY('ConsumerName','ApplicationNo','ConsumerNo','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'FinalConsumerCreated');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'NdcSubmitted', 'NDC Submitted', 'NDC application submitted.', JSON_ARRAY('ConsumerName','ConsumerNo','ApplicationNo','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'NdcSubmitted');

INSERT INTO `CommunicationPurposes`
(`PurposeKey`, `DisplayName`, `Description`, `AllowedPlaceholders`, `IsSystem`, `IsActive`, `CreatedAt`)
SELECT 'NdcApproved', 'NDC Approved', 'NDC application approved.', JSON_ARRAY('ConsumerName','ConsumerNo','ApplicationNo','Status','Date'), 1, 1, NOW(6)
WHERE NOT EXISTS (SELECT 1 FROM `CommunicationPurposes` WHERE `PurposeKey` = 'NdcApproved');

-- Sample default templates for core purposes/channels.
INSERT INTO `CommunicationTemplates`
(`PurposeId`, `PurposeKey`, `Channel`, `TemplateName`, `Subject`, `Body`, `Language`, `IsDefault`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT p.`Id`, p.`PurposeKey`, v.`Channel`, v.`TemplateName`, v.`Subject`, v.`Body`, NULL, 1, 1, 0, NOW(6)
FROM `CommunicationPurposes` p
JOIN (
  SELECT 'ConsumerOtp' PurposeKey, 'SMS' Channel, 'Consumer Login OTP' TemplateName, NULL Subject,
         'Your OTP for Noida Jal consumer login is {{Otp}}. It is valid for a short time.' Body
  UNION ALL SELECT 'ConsumerOtp', 'Email', 'Consumer Login OTP Email', 'Your Noida Jal login OTP',
         'Dear {{ConsumerName}},<br>Your OTP for Noida Jal consumer login is <strong>{{Otp}}</strong>. It is valid for {{ExpiryMinutes}} minutes.'
  UNION ALL SELECT 'PublicNewConnectionOtp', 'SMS', 'Public New Connection OTP', NULL,
         'Your OTP to start or continue Noida water connection application is {{Otp}}.'
  UNION ALL SELECT 'NewConnectionSubmitted', 'Email', 'New Connection Submitted Email', 'Application {{ApplicationNo}} submitted',
         'Dear {{ConsumerName}},<br>Your new connection application {{ApplicationNo}} has been submitted successfully. Current status: {{Status}}. Amount: Rs. {{Amount}}.'
  UNION ALL SELECT 'NewConnectionSubmitted', 'InApp', 'New Connection Submitted In-App', 'Application submitted',
         'Your new connection application {{ApplicationNo}} has been submitted successfully.'
  UNION ALL SELECT 'ApprovalStageAssigned', 'InApp', 'Approval Stage Assigned In-App', 'Approval task assigned',
         'Application {{ApplicationNo}} is assigned at {{StageName}} for action.'
  UNION ALL SELECT 'QueryRaised', 'InApp', 'Query Raised In-App', 'Query submitted',
         'Your query {{QueryNo}} has been submitted successfully. Status: {{Status}}.'
  UNION ALL SELECT 'QueryRaised', 'Email', 'Query Raised Email', 'Query {{QueryNo}} submitted',
         'Dear {{ConsumerName}},<br>Your support query {{QueryNo}} has been submitted successfully on {{Date}}.'
  UNION ALL SELECT 'QueryResolved', 'InApp', 'Query Resolved In-App', 'Query status updated',
         'Your query {{QueryNo}} is now {{Status}}. Remarks: {{Remarks}}'
  UNION ALL SELECT 'QueryResolved', 'Email', 'Query Resolved Email', 'Query {{QueryNo}} status updated',
         'Dear {{ConsumerName}},<br>Your query {{QueryNo}} is now {{Status}}.<br>Remarks: {{Remarks}}'
  UNION ALL SELECT 'ChallanGenerated', 'Email', 'Challan Generated Email', 'Challan {{ChallanNo}} generated',
         'Dear {{ConsumerName}},<br>Challan {{ChallanNo}} for {{Purpose}} has been generated. Amount payable: Rs. {{Amount}}.'
  UNION ALL SELECT 'PaymentSuccess', 'Email', 'Payment Success Email', 'Payment successful',
         'Dear {{ConsumerName}},<br>Your payment of Rs. {{Amount}} has been received successfully on {{Date}}.'
) v ON v.`PurposeKey` = p.`PurposeKey`
WHERE NOT EXISTS (
  SELECT 1 FROM `CommunicationTemplates` t
  WHERE t.`PurposeKey` = v.`PurposeKey`
    AND t.`Channel` = v.`Channel`
    AND t.`Language` IS NULL
    AND t.`IsDeleted` = 0
);

-- Menu and permissions.
INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'Communication Templates', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'Communication Templates' AND `IsDeleted` = 0);

SET @commModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'Communication Templates' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, 'Communication Templates', 'CT', '/CommunicationTemplates', 'Administration', 'Communication Templates', @commModuleId, 170, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = 1 AND `Label` = 'Communication Templates' AND `IsDeleted` = 0);

UPDATE `menuitems`
SET `ModuleId` = @commModuleId,
    `Module` = 'Communication Templates',
    `Url` = '/CommunicationTemplates',
    `IsActive` = 1,
    `ShowInSidebar` = 1
WHERE `TenantId` = 1 AND `Label` = 'Communication Templates' AND `IsDeleted` = 0;

UPDATE `rolepermissions` rp
JOIN `approles` r ON r.`Id` = rp.`RoleId`
SET rp.`Module` = 'Communication Templates',
    rp.`ModuleId` = @commModuleId,
    rp.`CanSeeMenu` = 1,
    rp.`CanView` = 1,
    rp.`CanAdd` = 1,
    rp.`CanEdit` = 1,
    rp.`CanDelete` = 1,
    rp.`IsDeleted` = 0,
    rp.`UpdatedAt` = NOW(6)
WHERE r.`Name` = 'Admin'
  AND r.`IsDeleted` = 0
  AND rp.`ModuleId` = @commModuleId;

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, 'Communication Templates', @commModuleId, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Admin'
  AND r.`IsDeleted` = 0
  AND NOT EXISTS (
    SELECT 1 FROM `rolepermissions` rp
    WHERE rp.`RoleId` = r.`Id`
      AND rp.`ModuleId` = @commModuleId
  );

UPDATE `rolepermissions`
SET `Module` = 'Communication Templates',
    `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanAdd` = 1,
    `CanEdit` = 1,
    `CanDelete` = 1
WHERE `ModuleId` = @commModuleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin' AND `IsDeleted` = 0);
