-- Masters module/menu/permission seed and sample Noida lookup data.
-- Run manually in the Water.Bill MySQL database.
-- This script does not create duplicate master tables and does not touch final consumer tables.

-- ---------------------------------------------------------------------
-- Optional key fixes for imported old lookup tables used by EF CRUD.
-- If a primary key already exists, these statements safely do nothing.
-- ---------------------------------------------------------------------
SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `sector_detail` ADD PRIMARY KEY (`S_no`)',
    'SELECT 1')
  FROM information_schema.table_constraints
  WHERE table_schema = DATABASE()
    AND table_name = 'sector_detail'
    AND constraint_type = 'PRIMARY KEY');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `pipe_size_master` ADD PRIMARY KEY (`PIPE_SIZE_ID`)',
    'SELECT 1')
  FROM information_schema.table_constraints
  WHERE table_schema = DATABASE()
    AND table_name = 'pipe_size_master'
    AND constraint_type = 'PRIMARY KEY');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `master_connection_type_details` ADD PRIMARY KEY (`CON_ID`)',
    'SELECT 1')
  FROM information_schema.table_constraints
  WHERE table_schema = DATABASE()
    AND table_name = 'master_connection_type_details'
    AND constraint_type = 'PRIMARY KEY');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `master_connection_type_details_trans` ADD PRIMARY KEY (`SUB_CON_ID`)',
    'SELECT 1')
  FROM information_schema.table_constraints
  WHERE table_schema = DATABASE()
    AND table_name = 'master_connection_type_details_trans'
    AND constraint_type = 'PRIMARY KEY');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `village_detail` ADD PRIMARY KEY (`Village_no`)',
    'SELECT 1')
  FROM information_schema.table_constraints
  WHERE table_schema = DATABASE()
    AND table_name = 'village_detail'
    AND constraint_type = 'PRIMARY KEY');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------
-- Permission modules.
-- ---------------------------------------------------------------------
INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
VALUES
  ('Sector Master', 1, 0),
  ('Block Master', 1, 0),
  ('Pipe Size Master', 1, 0),
  ('Connection Category Master', 1, 0),
  ('Connection Sub-Type Master', 1, 0),
  ('Connection Type Master', 1, 0),
  ('Village Master', 1, 0),
  ('Document Type Master', 1, 0)
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
  SELECT 'Sector Master' AS `Label`, 'SC' AS `Icon`, '/Masters/sectors' AS `Url`, 'Sector Master' AS `Module`, 10 AS `OrderNo`
  UNION ALL SELECT 'Block Master', 'BK', '/Masters/blocks', 'Block Master', 20
  UNION ALL SELECT 'Pipe Size Master', 'PS', '/Masters/pipe-sizes', 'Pipe Size Master', 30
  UNION ALL SELECT 'Connection Category Master', 'CC', '/Masters/connection-categories', 'Connection Category Master', 40
  UNION ALL SELECT 'Connection Sub-Type Master', 'ST', '/Masters/connection-sub-types', 'Connection Sub-Type Master', 50
  UNION ALL SELECT 'Connection Type Master', 'CT', '/Masters/connection-types', 'Connection Type Master', 60
  UNION ALL SELECT 'Village Master', 'VG', '/Masters/villages', 'Village Master', 70
  UNION ALL SELECT 'Document Type Master', 'DT', '/Masters/document-types', 'Document Type Master', 80
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
  mi.`ShowInSidebar` = 1
WHERE mi.`TenantId` = 1
  AND mi.`Label` IN (
    'Sector Master',
    'Block Master',
    'Pipe Size Master',
    'Connection Category Master',
    'Connection Sub-Type Master',
    'Connection Type Master',
    'Village Master',
    'Document Type Master'
  );

-- Give Admin full access to the new master modules.
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
  'Sector Master',
  'Block Master',
  'Pipe Size Master',
  'Connection Category Master',
  'Connection Sub-Type Master',
  'Connection Type Master',
  'Village Master',
  'Document Type Master'
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

-- ---------------------------------------------------------------------
-- Sample Noida master data for dropdown testing.
-- Uses old-compatible saved values/codes.
-- ---------------------------------------------------------------------
INSERT IGNORE INTO `sector_detail` (`S_no`, `Sector_id`, `sector_no`, `status`, `ORDER_BY`, `DEV_TYPE`)
VALUES
  (5001, 'SECTOR-50', '50', 1, 50, 1),
  (5002, 'SECTOR-51', '51', 1, 51, 1),
  (5003, 'SECTOR-62', '62', 1, 62, 1),
  (5004, 'SECTOR-18', '18', 1, 18, 1),
  (5005, 'SECTOR-137', '137', 1, 137, 1);

INSERT IGNORE INTO `block_detail` (`sector_id`, `block`, `status`, `DEV_TYPE`)
VALUES
  ('SECTOR-50', 'A', 1, 1),
  ('SECTOR-50', 'B', 1, 1),
  ('SECTOR-51', 'C', 1, 1),
  ('SECTOR-62', 'D', 1, 1),
  ('SECTOR-18', 'MARKET', 1, 1),
  ('SECTOR-137', 'TOWER-1', 1, 1);

INSERT IGNORE INTO `pipe_size_master` (`PIPE_SIZE_ID`, `PIPE_SIZE`, `STATUS`, `DEV_TYPE`)
VALUES
  (101, 15, 1, 1),
  (102, 20, 1, 1),
  (103, 25, 1, 1),
  (104, 40, 1, 1),
  (105, 50, 1, 1);

INSERT IGNORE INTO `master_connection_type_details` (`CON_ID`, `CON_NAME`, `CON_MAIN_ID`, `STATUS`, `DEV_TYPE`)
VALUES
  ('R', 'Residential', 'R', '1', 1),
  ('C', 'Commercial', 'C', '1', 1),
  ('I', 'Institutional', 'I', '1', 1),
  ('T', 'Industrial', 'T', '1', 1),
  ('G', 'Group Housing', 'G', '1', 1);

INSERT IGNORE INTO `master_connection_type_details_trans` (`SUB_CON_ID`, `CON_ID`, `SUB_CON_NAME`, `STATUS`, `DEV_TYPE`)
VALUES
  (1001, 'R', 'Domestic', '1', 1),
  (1002, 'R', 'Housing', '1', 1),
  (1003, 'R', 'Duplex', '1', 1),
  (1004, 'C', 'Shop / Office', '1', 1),
  (1005, 'I', 'School / Hospital', '1', 1),
  (1006, 'G', 'Group Housing Society', '1', 1);

INSERT IGNORE INTO `connection_type_mst` (`AUTO_ID`, `CONNECTION_NAME`, `CONNECTION_MAIN_ID`, `STATUS`, `CREATED_ON`)
VALUES
  (101, 'Regular', 'R', 1, NOW()),
  (102, 'Temporary', 'T', 1, NOW()),
  (103, 'Bulk', 'B', 1, NOW());

INSERT IGNORE INTO `village_detail` (`Village_no`, `Village_id`, `Village_Name`, `Village_str`, `status`, `DEV_TYPE`)
VALUES
  (501, 1, 'Pachrukha', 'PAC', 1, 1),
  (502, 2, 'Sorkha', 'SOR', 1, 1),
  (503, 3, 'Barola', 'BAR', 1, 1),
  (504, 4, 'Hajipur', 'HAJ', 1, 1),
  (505, 5, 'Gejha', 'GEJ', 1, 1);

INSERT IGNORE INTO `master_document_upload` (`Document_id`, `Document_Name`, `status`, `Doc_for`)
VALUES
  (101, 'Allotment Letter', 1, 'NCH'),
  (102, 'Possession Letter', 1, 'NCH'),
  (103, 'Compliance Letter', 1, 'NCH'),
  (104, 'SSI Letter', 1, 'NCH'),
  (105, 'Affidavit', 1, 'NCH'),
  (106, 'ID Proof', 1, 'NCH'),
  (107, 'Address Proof', 1, 'NCH'),
  (108, 'Property Document', 1, 'NCH'),
  (109, 'Other', 1, 'NCH');
