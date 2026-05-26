-- Fix existing loginattempts table to match the EF Core Loginattempt entity.
-- Run this once if admin login fails with:
-- Unknown column 'IsDeleted' in 'field list'

SET @schema_name = DATABASE();

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'loginattempts'
       AND column_name = 'CreatedAt') = 0,
    'ALTER TABLE `loginattempts` ADD COLUMN `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)',
    'SELECT ''loginattempts.CreatedAt already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'loginattempts'
       AND column_name = 'UpdatedAt') = 0,
    'ALTER TABLE `loginattempts` ADD COLUMN `UpdatedAt` DATETIME(6) NULL',
    'SELECT ''loginattempts.UpdatedAt already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'loginattempts'
       AND column_name = 'IsDeleted') = 0,
    'ALTER TABLE `loginattempts` ADD COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0',
    'SELECT ''loginattempts.IsDeleted already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.columns
     WHERE table_schema = @schema_name
       AND table_name = 'loginattempts'
       AND column_name = 'AttemptedAt') > 0,
    'UPDATE `loginattempts` SET `CreatedAt` = `AttemptedAt` WHERE `AttemptedAt` IS NOT NULL',
    'SELECT ''loginattempts.AttemptedAt not present; no timestamp copy needed'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*)
     FROM information_schema.statistics
     WHERE table_schema = @schema_name
       AND table_name = 'loginattempts'
       AND index_name = 'IX_LoginAttempts_Username_CreatedAt') = 0,
    'ALTER TABLE `loginattempts` ADD INDEX `IX_LoginAttempts_Username_CreatedAt` (`Username`, `CreatedAt`)',
    'SELECT ''IX_LoginAttempts_Username_CreatedAt already exists'' AS Message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
