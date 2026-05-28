-- Rate master modules for Water.Bill Authority Portal.
-- Run manually in the Water.Bill MySQL database.
-- Uses existing imported old tables: jal_rate_master and jal_rate_trans.
-- No data is truncated or replaced.

SET @schemaName := DATABASE();

-- ---------------------------------------------------------------------
-- Optional key safety checks for imported old tables.
-- ---------------------------------------------------------------------
SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'jal_rate_master') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'jal_rate_master' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `jal_rate_master` MODIFY COLUMN `ID` INT NOT NULL, ADD PRIMARY KEY (`ID`)',
  'SELECT ''jal_rate_master: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = 'jal_rate_trans') > 0
  AND (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = @schemaName AND table_name = 'jal_rate_trans' AND constraint_type = 'PRIMARY KEY') = 0,
  'ALTER TABLE `jal_rate_trans` MODIFY COLUMN `SID` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`SID`)',
  'SELECT ''jal_rate_trans: primary key already exists or table missing'' AS Message'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- Permission modules.
-- ---------------------------------------------------------------------
INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
VALUES
  ('Rate Category Master', 1, 0),
  ('Rate Master', 1, 0)
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
  SELECT 'Rate Category Master' AS `Label`, 'RC' AS `Icon`, '/Masters/rate-categories' AS `Url`, 'Rate Category Master' AS `Module`, 140 AS `OrderNo`
  UNION ALL SELECT 'Rate Master', 'RT', '/Masters/rates', 'Rate Master', 150
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
    WHEN 'Rate Category Master' THEN '/Masters/rate-categories'
    WHEN 'Rate Master' THEN '/Masters/rates'
    ELSE mi.`Url`
  END
WHERE mi.`TenantId` = 1
  AND mi.`Label` IN ('Rate Category Master', 'Rate Master');

-- Give Admin full access to the rate master modules.
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
JOIN `PermissionModules` pm ON pm.`Name` IN ('Rate Category Master', 'Rate Master') AND pm.`IsDeleted` = 0
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
