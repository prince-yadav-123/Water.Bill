-- Water.Bill master key mapping updates
-- Run only on development/testing database after backup.
-- This script does not truncate or drop data.

USE water_bill;

SET @schemaName = DATABASE();

-- ============================================================
-- Helper pattern: Add Id AUTO_INCREMENT PK only if:
-- 1. Table exists
-- 2. Id column does not exist
-- 3. Table does not already have a primary key
-- ============================================================

-- master_dept_details
SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'master_dept_details') > 0
    AND
    (SELECT COUNT(*) FROM information_schema.columns 
     WHERE table_schema = @schemaName AND table_name = 'master_dept_details' AND column_name = 'Id') = 0
    AND
    (SELECT COUNT(*) FROM information_schema.table_constraints
     WHERE table_schema = @schemaName AND table_name = 'master_dept_details' AND constraint_type = 'PRIMARY KEY') = 0,
    'ALTER TABLE `master_dept_details` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
    'SELECT ''master_dept_details: Id already exists, table missing, or primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- master_phone_details
SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'master_phone_details') > 0
    AND
    (SELECT COUNT(*) FROM information_schema.columns 
     WHERE table_schema = @schemaName AND table_name = 'master_phone_details' AND column_name = 'Id') = 0
    AND
    (SELECT COUNT(*) FROM information_schema.table_constraints
     WHERE table_schema = @schemaName AND table_name = 'master_phone_details' AND constraint_type = 'PRIMARY KEY') = 0,
    'ALTER TABLE `master_phone_details` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
    'SELECT ''master_phone_details: Id already exists, table missing, or primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- jal_bank_master
SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'jal_bank_master') > 0
    AND
    (SELECT COUNT(*) FROM information_schema.columns 
     WHERE table_schema = @schemaName AND table_name = 'jal_bank_master' AND column_name = 'Id') = 0
    AND
    (SELECT COUNT(*) FROM information_schema.table_constraints
     WHERE table_schema = @schemaName AND table_name = 'jal_bank_master' AND constraint_type = 'PRIMARY KEY') = 0,
    'ALTER TABLE `jal_bank_master` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
    'SELECT ''jal_bank_master: Id already exists, table missing, or primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- bank_master
SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'bank_master') > 0
    AND
    (SELECT COUNT(*) FROM information_schema.columns 
     WHERE table_schema = @schemaName AND table_name = 'bank_master' AND column_name = 'Id') = 0
    AND
    (SELECT COUNT(*) FROM information_schema.table_constraints
     WHERE table_schema = @schemaName AND table_name = 'bank_master' AND constraint_type = 'PRIMARY KEY') = 0,
    'ALTER TABLE `bank_master` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
    'SELECT ''bank_master: Id already exists, table missing, or primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- receipt_head_link
SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'receipt_head_link') > 0
    AND
    (SELECT COUNT(*) FROM information_schema.columns 
     WHERE table_schema = @schemaName AND table_name = 'receipt_head_link' AND column_name = 'Id') = 0
    AND
    (SELECT COUNT(*) FROM information_schema.table_constraints
     WHERE table_schema = @schemaName AND table_name = 'receipt_head_link' AND constraint_type = 'PRIMARY KEY') = 0,
    'ALTER TABLE `receipt_head_link` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
    'SELECT ''receipt_head_link: Id already exists, table missing, or primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;


-- ============================================================
-- payment_mode_mst: AUTO_ID as primary key
-- ============================================================

SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'payment_mode_mst') > 0,
    'UPDATE `payment_mode_mst` SET `AUTO_ID` = NULL WHERE `AUTO_ID` = 0',
    'SELECT ''payment_mode_mst not found'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @pkExists = (
    SELECT COUNT(*) 
    FROM information_schema.table_constraints
    WHERE table_schema = @schemaName 
      AND table_name = 'payment_mode_mst'
      AND constraint_type = 'PRIMARY KEY'
);

SET @sql = IF(
    @pkExists = 0,
    'ALTER TABLE `payment_mode_mst` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
    'SELECT ''payment_mode_mst: primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;


-- ============================================================
-- payment_type_mst: AUTO_ID as primary key
-- ============================================================

SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'payment_type_mst') > 0,
    'UPDATE `payment_type_mst` SET `AUTO_ID` = NULL WHERE `AUTO_ID` = 0',
    'SELECT ''payment_type_mst not found'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @pkExists = (
    SELECT COUNT(*) 
    FROM information_schema.table_constraints
    WHERE table_schema = @schemaName 
      AND table_name = 'payment_type_mst'
      AND constraint_type = 'PRIMARY KEY'
);

SET @sql = IF(
    @pkExists = 0,
    'ALTER TABLE `payment_type_mst` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
    'SELECT ''payment_type_mst: primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;


-- ============================================================
-- application_status: AUTO_ID as primary key
-- ============================================================

SET @sql = IF(
    (SELECT COUNT(*) FROM information_schema.tables 
     WHERE table_schema = @schemaName AND table_name = 'application_status') > 0,
    'UPDATE `application_status` SET `AUTO_ID` = NULL WHERE `AUTO_ID` = 0',
    'SELECT ''application_status not found'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @pkExists = (
    SELECT COUNT(*) 
    FROM information_schema.table_constraints
    WHERE table_schema = @schemaName 
      AND table_name = 'application_status'
      AND constraint_type = 'PRIMARY KEY'
);

SET @sql = IF(
    @pkExists = 0,
    'ALTER TABLE `application_status` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
    'SELECT ''application_status: primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;


-- ============================================================
-- master_noc_amt: CON_ID as primary key
-- ============================================================

SET @pkExists = (
    SELECT COUNT(*) 
    FROM information_schema.table_constraints
    WHERE table_schema = @schemaName 
      AND table_name = 'master_noc_amt'
      AND constraint_type = 'PRIMARY KEY'
);

SET @sql = IF(
    @pkExists = 0,
    'ALTER TABLE `master_noc_amt` MODIFY COLUMN `CON_ID` VARCHAR(2) NOT NULL, ADD PRIMARY KEY (`CON_ID`)',
    'SELECT ''master_noc_amt: primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;


-- ============================================================
-- receipt_head_master: HEAD_ID as primary key
-- ============================================================

SET @pkExists = (
    SELECT COUNT(*) 
    FROM information_schema.table_constraints
    WHERE table_schema = @schemaName 
      AND table_name = 'receipt_head_master'
      AND constraint_type = 'PRIMARY KEY'
);

SET @sql = IF(
    @pkExists = 0,
    'ALTER TABLE `receipt_head_master` MODIFY COLUMN `HEAD_ID` INT NOT NULL, ADD PRIMARY KEY (`HEAD_ID`)',
    'SELECT ''receipt_head_master: primary key already exists'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;