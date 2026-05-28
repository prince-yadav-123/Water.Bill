-- Public New Connection OTP, fee-ready submission, and workflow foundation.
-- Run manually in the Water.Bill MySQL database.
-- No imported old table is truncated or replaced.

CREATE TABLE IF NOT EXISTS `public_new_connection_otp_verifications` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `MobileNumber` VARCHAR(12) NOT NULL,
  `OtpHash` VARCHAR(128) NOT NULL,
  `OtpSalt` VARCHAR(64) NOT NULL,
  `Purpose` VARCHAR(50) NOT NULL,
  `ExpiresAt` DATETIME NOT NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `VerifiedAt` DATETIME NULL,
  `AttemptCount` INT NOT NULL DEFAULT 0,
  `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_PublicNewConnectionOtp_Mobile_Purpose_Active` (`MobileNumber`, `Purpose`, `IsActive`)
);

CREATE TABLE IF NOT EXISTS `NewConnectionFeeConfigurations` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `ConnectionCategory` VARCHAR(20) NULL,
  `ConnectionType` VARCHAR(50) NULL,
  `PipeSize` DECIMAL(8,2) NULL,
  `PlotSizeFrom` DECIMAL(12,2) NULL,
  `PlotSizeTo` DECIMAL(12,2) NULL,
  `ApplicationFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `ProcessingFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `SecurityAmount` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `MeterInstallationFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `OtherCharges` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `TotalAmount` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `EffectiveFrom` DATETIME NOT NULL,
  `EffectiveTo` DATETIME NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedOn` DATETIME NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_NewConnectionFeeConfigurations_Lookup` (`ConnectionCategory`, `ConnectionType`, `PipeSize`, `IsActive`)
);

CREATE TABLE IF NOT EXISTS `NewConnectionApplicationFees` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationId` BIGINT NOT NULL,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `FeeConfigurationId` INT NOT NULL,
  `ApplicationFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `ProcessingFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `SecurityAmount` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `MeterInstallationFee` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `OtherCharges` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `TotalAmount` DECIMAL(12,2) NOT NULL DEFAULT 0,
  `PaymentStatus` VARCHAR(30) NOT NULL DEFAULT 'Pending',
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedOn` DATETIME NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_NewConnectionApplicationFees_ApplicationId` (`ApplicationId`),
  INDEX `IX_NewConnectionApplicationFees_ApplicationNo` (`ApplicationNo`),
  CONSTRAINT `FK_NewConnectionApplicationFees_Applications` FOREIGN KEY (`ApplicationId`) REFERENCES `new_connection_applications` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_NewConnectionApplicationFees_FeeConfigurations` FOREIGN KEY (`FeeConfigurationId`) REFERENCES `NewConnectionFeeConfigurations` (`Id`) ON DELETE RESTRICT
);

-- Department Master is intentionally reused from the imported old table:
-- `master_dept_details` (`Id`, `DEPT_ID`, `DEPT_NAME`, `STATUS`, `DEV_TYPE`).
-- `DEPT_ID` is used as the visible code and `STATUS = '1'` is treated as Active.

CREATE TABLE IF NOT EXISTS `AuthorityUserDepartments` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `DepartmentId` INT NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_AuthorityUserDepartments_User_Department` (`UserId`, `DepartmentId`),
  CONSTRAINT `FK_AuthorityUserDepartments_MasterDeptDetails` FOREIGN KEY (`DepartmentId`) REFERENCES `master_dept_details` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `WorkflowMasters` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `WorkflowName` VARCHAR(100) NOT NULL,
  `ApplicationType` VARCHAR(50) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedOn` DATETIME NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowMasters_ApplicationType_Active` (`ApplicationType`, `IsActive`)
);

CREATE TABLE IF NOT EXISTS `WorkflowStages` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `WorkflowId` INT NOT NULL,
  `StageName` VARCHAR(100) NOT NULL,
  `StageOrder` INT NOT NULL,
  `DepartmentId` INT NULL,
  `ApproverRoleId` INT NULL,
  `ApproverUserId` INT NULL,
  `ApprovalType` VARCHAR(30) NOT NULL DEFAULT 'DepartmentRole',
  `CanApprove` TINYINT(1) NOT NULL DEFAULT 1,
  `CanReject` TINYINT(1) NOT NULL DEFAULT 1,
  `CanSendCorrection` TINYINT(1) NOT NULL DEFAULT 1,
  `CanForward` TINYINT(1) NOT NULL DEFAULT 0,
  `IsFinalStage` TINYINT(1) NOT NULL DEFAULT 0,
  `SlaDays` INT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedOn` DATETIME NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowStages_Workflow_Order` (`WorkflowId`, `StageOrder`),
  CONSTRAINT `FK_WorkflowStages_WorkflowMasters` FOREIGN KEY (`WorkflowId`) REFERENCES `WorkflowMasters` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_WorkflowStages_MasterDeptDetails` FOREIGN KEY (`DepartmentId`) REFERENCES `master_dept_details` (`Id`) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS `ApplicationWorkflowInstances` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationId` BIGINT NOT NULL,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `ApplicationType` VARCHAR(50) NOT NULL,
  `WorkflowId` INT NOT NULL,
  `CurrentStageId` INT NULL,
  `CurrentStatus` VARCHAR(30) NOT NULL,
  `StartedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CompletedOn` DATETIME NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowInstances_Application` (`ApplicationType`, `ApplicationId`),
  INDEX `IX_WorkflowInstances_ApplicationNo` (`ApplicationNo`)
);

CREATE TABLE IF NOT EXISTS `ApplicationWorkflowTasks` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `WorkflowInstanceId` BIGINT NOT NULL,
  `ApplicationId` BIGINT NOT NULL,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `StageId` INT NOT NULL,
  `AssignedDepartmentId` INT NULL,
  `AssignedRoleId` INT NULL,
  `AssignedUserId` INT NULL,
  `Status` VARCHAR(30) NOT NULL DEFAULT 'Pending',
  `AssignedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ActionOn` DATETIME NULL,
  `Remarks` VARCHAR(500) NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowTasks_Instance` (`WorkflowInstanceId`),
  INDEX `IX_WorkflowTasks_Assignment` (`Status`, `AssignedRoleId`, `AssignedUserId`, `AssignedDepartmentId`)
);

CREATE TABLE IF NOT EXISTS `ApplicationWorkflowHistory` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `WorkflowInstanceId` BIGINT NOT NULL,
  `ApplicationId` BIGINT NOT NULL,
  `ApplicationNo` VARCHAR(30) NOT NULL,
  `StageId` INT NULL,
  `FromStatus` VARCHAR(30) NULL,
  `ToStatus` VARCHAR(30) NOT NULL,
  `Action` VARCHAR(50) NOT NULL,
  `Remarks` VARCHAR(500) NULL,
  `ActionBy` INT NULL,
  `ActionByName` VARCHAR(150) NULL,
  `ActionByRole` VARCHAR(50) NULL,
  `ActionOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowHistory_Instance` (`WorkflowInstanceId`),
  INDEX `IX_WorkflowHistory_ApplicationNo` (`ApplicationNo`)
);

CREATE TABLE IF NOT EXISTS `WorkflowStageNotifications` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `WorkflowStageId` INT NOT NULL,
  `EventType` VARCHAR(50) NOT NULL,
  `SendEmail` TINYINT(1) NOT NULL DEFAULT 0,
  `SendSms` TINYINT(1) NOT NULL DEFAULT 0,
  `SendWhatsApp` TINYINT(1) NOT NULL DEFAULT 0,
  `SendInAppNotification` TINYINT(1) NOT NULL DEFAULT 0,
  `TemplateId` INT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkflowStageNotifications_Stage_Event` (`WorkflowStageId`, `EventType`)
);

CREATE TABLE IF NOT EXISTS `NotificationLogs` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `ApplicationId` BIGINT NULL,
  `ApplicationNo` VARCHAR(30) NULL,
  `WorkflowInstanceId` BIGINT NULL,
  `StageId` INT NULL,
  `Channel` VARCHAR(30) NOT NULL,
  `Recipient` VARCHAR(150) NOT NULL,
  `Message` VARCHAR(1000) NULL,
  `Status` VARCHAR(30) NOT NULL,
  `SentOn` DATETIME NULL,
  `ErrorMessage` VARCHAR(1000) NULL,
  `RetryCount` INT NOT NULL DEFAULT 0,
  `CreatedOn` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `IX_NotificationLogs_ApplicationNo` (`ApplicationNo`)
);

-- Permission modules and menu.
INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
VALUES
  ('Department Master', 1, 0),
  ('Workflow Master', 1, 0),
  ('My Pending Applications', 1, 0),
  ('New Connection Fee Configuration', 1, 0)
ON DUPLICATE KEY UPDATE `IsActive` = 1, `IsDeleted` = 0;

INSERT INTO `menuitems`
  (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, 'Masters', 'MS', '#', 'Masters', NULL, NULL, 300, 1, 1, UTC_TIMESTAMP(6), 0
WHERE NOT EXISTS (
  SELECT 1 FROM `menuitems`
  WHERE `TenantId` = 1 AND `ParentId` IS NULL AND `Label` = 'Masters' AND `IsDeleted` = 0
);

SET @MastersParentId := (
  SELECT `Id` FROM `menuitems`
  WHERE `TenantId` = 1 AND `ParentId` IS NULL AND `Label` = 'Masters' AND `IsDeleted` = 0
  ORDER BY `Id` LIMIT 1
);

INSERT INTO `menuitems`
  (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
SELECT 1, @MastersParentId, v.`Label`, v.`Icon`, v.`Url`, 'Masters', v.`Module`, pm.`Id`, v.`OrderNo`, 1, 1, UTC_TIMESTAMP(6), 0
FROM (
  SELECT 'Department Master' AS `Label`, 'DP' AS `Icon`, '/Departments' AS `Url`, 'Department Master' AS `Module`, 160 AS `OrderNo`
  UNION ALL SELECT 'Workflow Master', 'WF', '/Workflows', 'Workflow Master', 170
  UNION ALL SELECT 'New Connection Fee Configuration', 'FE', '/NewConnectionFeeConfigurations', 'New Connection Fee Configuration', 180
) v
JOIN `PermissionModules` pm ON pm.`Name` = v.`Module` AND pm.`IsDeleted` = 0
WHERE NOT EXISTS (
  SELECT 1 FROM `menuitems` mi
  WHERE mi.`TenantId` = 1 AND mi.`Label` = v.`Label` AND mi.`IsDeleted` = 0
);

INSERT INTO `menuitems`
  (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, 'My Pending Applications', 'AP', '/Approvals/Pending', 'Approvals', 'My Pending Applications', pm.`Id`, 410, 1, 1, UTC_TIMESTAMP(6), 0
FROM `PermissionModules` pm
WHERE pm.`Name` = 'My Pending Applications'
  AND pm.`IsDeleted` = 0
  AND NOT EXISTS (
    SELECT 1 FROM `menuitems` mi
    WHERE mi.`TenantId` = 1 AND mi.`Label` = 'My Pending Applications' AND mi.`IsDeleted` = 0
  );

INSERT INTO `rolepermissions`
  (`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  r.`Id`, pm.`Name`, pm.`Id`,
  1, 1, 1, 1, 1, 0, 1, 1, 1, 0,
  UTC_TIMESTAMP(6), 0
FROM `approles` r
JOIN `PermissionModules` pm ON pm.`Name` IN ('Department Master', 'Workflow Master', 'My Pending Applications', 'New Connection Fee Configuration') AND pm.`IsDeleted` = 0
WHERE r.`Name` = 'Admin' AND r.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE
  `ModuleId` = VALUES(`ModuleId`),
  `CanSeeMenu` = 1,
  `CanView` = 1,
  `CanAdd` = 1,
  `CanEdit` = 1,
  `CanDelete` = 1,
  `CanApprove` = 1,
  `CanForward` = 1,
  `IsDeleted` = 0;
