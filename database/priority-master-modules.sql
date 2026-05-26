-- Priority master modules for Water.Bill Authority Portal.
-- Run manually in the Water.Bill MySQL database.
-- Uses existing imported old tables; does not truncate or replace data.

-- ---------------------------------------------------------------------
-- Required key/schema adjustments for EF CRUD.
-- ---------------------------------------------------------------------
SET @schemaName := DATABASE();

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'payment_mode_mst') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'payment_mode_mst' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `payment_mode_mst` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
  'SELECT ''payment_mode_mst: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'payment_type_mst') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'payment_type_mst' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `payment_type_mst` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
  'SELECT ''payment_type_mst: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'jal_bank_master') > 0
  AND (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @schemaName AND table_name = 'jal_bank_master' AND column_name = 'Id') = 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'jal_bank_master' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `jal_bank_master` ADD COLUMN `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST',
  'SELECT ''jal_bank_master: Id already exists, primary key already exists, or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'master_noc_amt') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'master_noc_amt' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `master_noc_amt` MODIFY COLUMN `CON_ID` VARCHAR(2) NOT NULL, ADD PRIMARY KEY (`CON_ID`)',
  'SELECT ''master_noc_amt: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'application_status') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'application_status' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `application_status` MODIFY COLUMN `AUTO_ID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`AUTO_ID`)',
  'SELECT ''application_status: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- Permission modules.
-- ---------------------------------------------------------------------
INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
VALUES
  ('Payment Mode Master', 1, 0),
  ('Payment Type Master', 1, 0),
  ('Bank Master', 1, 0),
  ('NDC Amount Master', 1, 0),
  ('Application Status Master', 1, 0)
ON DUPLICATE KEY UPDATE
  `IsActive` = 1,
  `IsDeleted` = 0;

-- ---------------------------------------------------------------------
-- Masters menu parent and child menu items.
-- ---------------------------------------------------------------------
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
  ORDER BY `Id`
  LIMIT 1
);

INSERT INTO `menuitems`
  (`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
SELECT 1, @MastersParentId, v.`Label`, v.`Icon`, v.`Url`, 'Masters', v.`Module`, pm.`Id`, v.`OrderNo`, 1, 1, UTC_TIMESTAMP(6), 0
FROM (
  SELECT 'Payment Mode Master' AS `Label`, 'PM' AS `Icon`, '/Masters/payment-modes' AS `Url`, 'Payment Mode Master' AS `Module`, 90 AS `OrderNo`
  UNION ALL SELECT 'Payment Type Master', 'PT', '/Masters/payment-types', 'Payment Type Master', 100
  UNION ALL SELECT 'Bank Master', 'BK', '/Masters/banks', 'Bank Master', 110
  UNION ALL SELECT 'NDC Amount Master', 'NA', '/Masters/ndc-amounts', 'NDC Amount Master', 120
  UNION ALL SELECT 'Application Status Master', 'AS', '/Masters/application-statuses', 'Application Status Master', 130
) v
JOIN `PermissionModules` pm ON pm.`Name` = v.`Module` AND pm.`IsDeleted` = 0
WHERE NOT EXISTS (
  SELECT 1 FROM `menuitems` mi
  WHERE mi.`TenantId` = 1
    AND mi.`Label` = v.`Label`
    AND mi.`IsDeleted` = 0
);

UPDATE `menuitems` mi
JOIN `PermissionModules` pm ON pm.`Name` = mi.`Label` AND pm.`IsDeleted` = 0
SET
  mi.`ParentId` = @MastersParentId,
  mi.`SectionLabel` = 'Masters',
  mi.`Module` = pm.`Name`,
  mi.`ModuleId` = pm.`Id`,
  mi.`IsActive` = 1,
  mi.`ShowInSidebar` = 1,
  mi.`Url` = CASE mi.`Label`
    WHEN 'Payment Mode Master' THEN '/Masters/payment-modes'
    WHEN 'Payment Type Master' THEN '/Masters/payment-types'
    WHEN 'Bank Master' THEN '/Masters/banks'
    WHEN 'NDC Amount Master' THEN '/Masters/ndc-amounts'
    WHEN 'Application Status Master' THEN '/Masters/application-statuses'
    ELSE mi.`Url`
  END
WHERE mi.`TenantId` = 1
  AND mi.`Label` IN (
    'Payment Mode Master',
    'Payment Type Master',
    'Bank Master',
    'NDC Amount Master',
    'Application Status Master'
  );

-- Give Admin full access to the priority master modules.
INSERT INTO `rolepermissions`
  (`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  r.`Id`,
  pm.`Name`,
  pm.`Id`,
  1, 1, 1, 1, 1, 0, 1, 0, 0, 0,
  UTC_TIMESTAMP(6),
  0
FROM `approles` r
JOIN `PermissionModules` pm ON pm.`Name` IN (
  'Payment Mode Master',
  'Payment Type Master',
  'Bank Master',
  'NDC Amount Master',
  'Application Status Master'
) AND pm.`IsDeleted` = 0
WHERE r.`Name` = 'Admin'
  AND r.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE
  `ModuleId` = VALUES(`ModuleId`),
  `CanSeeMenu` = 1,
  `CanView` = 1,
  `CanAdd` = 1,
  `CanEdit` = 1,
  `CanDelete` = 1,
  `CanExport` = 1,
  `IsDeleted` = 0;
