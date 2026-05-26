-- Fix existing auditlogs table to match the EF Core Auditlog entity.
-- Run this once if admin login fails with:
-- Unknown column 'Timestamp' in 'field list'

SET @schema_name = DATABASE();

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'auditlogs'
       AND column_name = 'Timestamp') = 0,
    'ALTER TABLE `auditlogs` ADD COLUMN `Timestamp` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)',
    'SELECT ''auditlogs.Timestamp already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'auditlogs'
       AND column_name = 'CreatedAt') > 0,
    'UPDATE `auditlogs` SET `Timestamp` = `CreatedAt` WHERE `CreatedAt` IS NOT NULL',
    'SELECT ''auditlogs.CreatedAt not present; no timestamp copy needed'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.statistics
     WHERE table_schema = @schema_name
       AND table_name = 'auditlogs'
       AND index_name = 'IX_AuditLogs_Timestamp') = 0,
    'ALTER TABLE `auditlogs` ADD INDEX `IX_AuditLogs_Timestamp` (`Timestamp`)',
    'SELECT ''IX_AuditLogs_Timestamp already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
