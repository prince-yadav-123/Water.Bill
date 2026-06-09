-- ═══════════════════════════════════════════════════════════════════════════
--  Water.Bill — Consolidated Database Changes (this work session)
--  Target: water_bill (MySQL 8.x / MariaDB)
--
--  SAFE TO RE-RUN: every statement is guarded so it will not error if the
--  object already exists. No existing columns are altered or dropped.
--  No existing table data is deleted or overwritten.
--
--  Covered changes:
--    A. New table  : notification_masters
--    B. New table  : notification_targets  (+ FK to notification_masters)
--    C. New columns: WorkflowStages.CanForwardToUser / CanSendBackToApplicant / CanSendBackToPrevious
--    D. Data backfill (additive, non-destructive) on the 3 new columns
--    E. Seed: permission module + menu items + role permission
--    F. Seed: communication purpose + email template (AdminNotification)
-- ═══════════════════════════════════════════════════════════════════════════

-- ───────────────────────────────────────────────────────────────────────────
-- A. notification_masters  (NEW TABLE)
-- ───────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS `notification_masters` (
    `Id`               BIGINT       NOT NULL AUTO_INCREMENT,
    `Title`            VARCHAR(200) NOT NULL,
    `Message`          TEXT         NOT NULL,
    `NotificationType` VARCHAR(50)  NOT NULL DEFAULT 'General',
    `TargetAudience`   VARCHAR(20)  NOT NULL DEFAULT 'Consumer',
    `Channels`         VARCHAR(100) NOT NULL DEFAULT 'InApp',
    `Priority`         VARCHAR(20)  NOT NULL DEFAULT 'Normal',
    `Status`           VARCHAR(20)  NOT NULL DEFAULT 'Draft',
    `ValidFrom`        DATETIME     NULL,
    `ValidTo`          DATETIME     NULL,
    `CreatedByUserId`  INT          NOT NULL DEFAULT 0,
    `CreatedByName`    VARCHAR(200) NULL,
    `CreatedAt`        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `SentAt`           DATETIME     NULL,
    `IsActive`         TINYINT(1)   NOT NULL DEFAULT 1,
    `IsDeleted`        TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    INDEX `IX_NotifMaster_Status`    (`Status`),
    INDEX `IX_NotifMaster_Audience`  (`TargetAudience`),
    INDEX `IX_NotifMaster_CreatedAt` (`CreatedAt`, `IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ───────────────────────────────────────────────────────────────────────────
-- B. notification_targets  (NEW TABLE + FK)
-- ───────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS `notification_targets` (
    `Id`             BIGINT       NOT NULL AUTO_INCREMENT,
    `NotificationId` BIGINT       NOT NULL,
    `TargetType`     VARCHAR(50)  NOT NULL,
    `TargetId`       VARCHAR(200) NULL,
    `TargetName`     VARCHAR(300) NULL,
    `IsDeleted`      TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    INDEX `IX_NotifTarget_NotifId`  (`NotificationId`),
    INDEX `IX_NotifTarget_Type_Id`  (`TargetType`, `TargetId`),
    CONSTRAINT `FK_NotifTarget_Master`
        FOREIGN KEY (`NotificationId`)
        REFERENCES `notification_masters`(`Id`)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ───────────────────────────────────────────────────────────────────────────
-- C. WorkflowStages — 3 NEW COLUMNS (additive only)
--    Guarded with INFORMATION_SCHEMA so it is safe on every MySQL 8 build
--    (older 8.0.x does not support ADD COLUMN IF NOT EXISTS).
-- ───────────────────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS `wb_add_col`;
DELIMITER //
CREATE PROCEDURE `wb_add_col`(
    IN p_table VARCHAR(64), IN p_col VARCHAR(64), IN p_ddl VARCHAR(512))
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME   = p_table
          AND COLUMN_NAME  = p_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` ADD COLUMN `', p_col, '` ', p_ddl);
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
END //
DELIMITER ;

CALL `wb_add_col`('WorkflowStages', 'CanForwardToUser',
    "TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Allows forwarding to a specific internal user'");
CALL `wb_add_col`('WorkflowStages', 'CanSendBackToApplicant',
    "TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Allows returning application to applicant for correction'");
CALL `wb_add_col`('WorkflowStages', 'CanSendBackToPrevious',
    "TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Allows sending application back to previous workflow stage'");

DROP PROCEDURE IF EXISTS `wb_add_col`;

-- ───────────────────────────────────────────────────────────────────────────
-- D. Data backfill (NON-DESTRUCTIVE) — only sets the NEW column.
--    Maps the legacy "CanSendCorrection" flag to the new SendBackToApplicant.
--    Touches only the new column; never modifies existing columns/data.
-- ───────────────────────────────────────────────────────────────────────────
UPDATE `WorkflowStages`
SET `CanSendBackToApplicant` = 1
WHERE `CanSendCorrection` = 1
  AND `CanSendBackToApplicant` = 0;

-- ───────────────────────────────────────────────────────────────────────────
-- E. Permission module + menu items + role permission (SEED)
--    INSERT IGNORE / existence checks prevent duplicates on re-run.
-- ───────────────────────────────────────────────────────────────────────────
INSERT IGNORE INTO `permissionmodules` (`Name`, `IsActive`, `IsDeleted`, `CreatedAt`)
VALUES ('NotificationManagement', 1, 0, NOW());

SET @notifModuleId = (SELECT `Id` FROM `permissionmodules`
                      WHERE `Name` = 'NotificationManagement' LIMIT 1);

-- Parent "Communication" menu (insert only if missing)
INSERT INTO `menuitems`
    (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`,
     `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`)
SELECT 1, NULL, 'Communication', '📢', NULL, NULL, @notifModuleId, 90, 1, 1, 0, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems` WHERE `Label` = 'Communication' AND `TenantId` = 1
);

SET @commParentId = (SELECT `Id` FROM `menuitems`
                     WHERE `Label` = 'Communication' AND `TenantId` = 1 LIMIT 1);

-- Child "Notification Management" menu (insert only if missing)
INSERT INTO `menuitems`
    (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`,
     `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`)
SELECT 1, @commParentId, 'Notification Management', '🔔', '/NotificationManagement',
       'Communication', @notifModuleId, 1, 1, 1, 0, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems` WHERE `Url` = '/NotificationManagement' AND `TenantId` = 1
);

-- Grant permission to Admin role (RoleId = 1; adjust if your admin role differs)
INSERT IGNORE INTO `rolepermissions`
    (`RoleId`, `ModuleId`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`,
     `CanApprove`, `CanForward`, `CanPrint`, `IsDeleted`)
SELECT 1, @notifModuleId, 1, 1, 1, 1, 1, 1, 1, 0;

-- ───────────────────────────────────────────────────────────────────────────
-- F. Communication purpose + email template (SEED)
-- ───────────────────────────────────────────────────────────────────────────
INSERT IGNORE INTO `communicationpurposes` (`PurposeKey`, `Description`, `IsActive`, `CreatedAt`)
VALUES ('AdminNotification', 'System-generated admin notifications to users', 1, NOW());

SET @purposeId = (SELECT `Id` FROM `communicationpurposes`
                  WHERE `PurposeKey` = 'AdminNotification' LIMIT 1);

-- Insert default email template only if one does not already exist for this purpose+channel
INSERT INTO `communicationtemplates`
    (`PurposeId`, `PurposeKey`, `Channel`, `TemplateName`, `Subject`, `Body`,
     `Language`, `IsDefault`, `IsActive`, `IsDeleted`, `CreatedAt`)
SELECT
    @purposeId, 'AdminNotification', 'Email', 'Admin Notification - Default',
    '{{NotificationTitle}} - Noida Jal Authority',
    '<!DOCTYPE html><html><head><meta charset="utf-8"></head>
<body style="font-family:Arial,sans-serif;background:#f4f7fb;margin:0;padding:0;">
  <div style="max-width:560px;margin:32px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">
    <div style="background:linear-gradient(135deg,#0f172a,#2563eb);padding:28px 32px;">
      <div style="color:#fff;font-size:1.1rem;font-weight:700;">Noida Jal Authority</div>
      <div style="color:rgba(255,255,255,.7);font-size:.85rem;margin-top:4px;">Official Communication</div>
    </div>
    <div style="padding:28px 32px;">
      <p style="font-size:1rem;color:#0f172a;margin:0 0 8px;">Dear {{UserName}},</p>
      <h2 style="font-size:1.15rem;color:#1e40af;margin:0 0 16px;">{{NotificationTitle}}</h2>
      <div style="background:#f8fafc;border-left:4px solid #2563eb;border-radius:0 8px 8px 0;padding:14px 16px;font-size:.93rem;color:#334155;line-height:1.6;">
        {{NotificationMessage}}
      </div>
      <div style="margin-top:20px;font-size:.82rem;color:#64748b;">
        <span style="background:#e0f2fe;color:#0284c7;padding:3px 8px;border-radius:999px;font-weight:700;">{{NotificationType}}</span>
        &nbsp; {{Date}}
      </div>
    </div>
    <div style="background:#f8fafc;padding:16px 32px;border-top:1px solid #e2e8f0;font-size:.78rem;color:#94a3b8;text-align:center;">
      This is an automated message from Noida Jal Authority. Please do not reply.
    </div>
  </div>
</body></html>',
    'en', 1, 1, 0, NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM `communicationtemplates`
    WHERE `PurposeKey` = 'AdminNotification' AND `Channel` = 'Email' AND `IsDeleted` = 0
);

-- ═══════════════════════════════════════════════════════════════════════════
--  VERIFICATION (optional — read-only)
-- ═══════════════════════════════════════════════════════════════════════════
-- SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
--  WHERE TABLE_SCHEMA = DATABASE()
--    AND TABLE_NAME IN ('notification_masters','notification_targets');
--
-- SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
--  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WorkflowStages'
--    AND COLUMN_NAME IN ('CanForwardToUser','CanSendBackToApplicant','CanSendBackToPrevious');
-- ═══════════════════════════════════════════════════════════════════════════
