USE water_bill;

SET @TenantId = '50000000-0000-0000-0000-000000000001';
SET @AdminRoleId = '50000000-0000-0000-0000-000000000101';
SET @AdministrationMenuId = '50000000-0000-0000-0000-000000000306';

CREATE TABLE IF NOT EXISTS PermissionModules (
    Id CHAR(36) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT PK_PermissionModules PRIMARY KEY (Id),
    CONSTRAINT UX_PermissionModules_Name_IsDeleted UNIQUE (Name, IsDeleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

DROP PROCEDURE IF EXISTS AddColumnIfMissing;
DELIMITER //
CREATE PROCEDURE AddColumnIfMissing(
    IN tableName VARCHAR(128),
    IN columnName VARCHAR(128),
    IN columnDefinition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = tableName
          AND COLUMN_NAME = columnName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE ', tableName, ' ADD COLUMN ', columnDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

DROP PROCEDURE IF EXISTS AddIndexIfMissing;
DELIMITER //
CREATE PROCEDURE AddIndexIfMissing(
    IN tableName VARCHAR(128),
    IN indexName VARCHAR(128),
    IN indexDefinition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = tableName
          AND INDEX_NAME = indexName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE ', tableName, ' ADD ', indexDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

DROP PROCEDURE IF EXISTS AddForeignKeyIfMissing;
DELIMITER //
CREATE PROCEDURE AddForeignKeyIfMissing(
    IN tableName VARCHAR(128),
    IN constraintName VARCHAR(128),
    IN constraintDefinition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = tableName
          AND CONSTRAINT_NAME = constraintName
    ) THEN
        SET @sql = CONCAT('ALTER TABLE ', tableName, ' ADD CONSTRAINT ', constraintDefinition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//
DELIMITER ;

CALL AddColumnIfMissing('menuitems', 'ModuleId', 'ModuleId CHAR(36) NULL AFTER Module');
CALL AddColumnIfMissing('menuitems', 'ShowInSidebar', 'ShowInSidebar TINYINT(1) NOT NULL DEFAULT 1 AFTER ModuleId');
CALL AddColumnIfMissing('rolepermissions', 'ModuleId', 'ModuleId CHAR(36) NULL AFTER Module');
CALL AddColumnIfMissing('rolepermissions', 'CanSeeMenu', 'CanSeeMenu TINYINT(1) NOT NULL DEFAULT 0 AFTER ModuleId');

INSERT INTO PermissionModules (Id, Name, IsActive, IsDeleted)
VALUES
('50000000-0000-0000-0000-000000000501', 'Dashboard', 1, 0),
('50000000-0000-0000-0000-000000000502', 'Consumers', 1, 0),
('50000000-0000-0000-0000-000000000503', 'Billing', 1, 0),
('50000000-0000-0000-0000-000000000504', 'Payments', 1, 0),
('50000000-0000-0000-0000-000000000505', 'Reports', 1, 0),
('50000000-0000-0000-0000-000000000506', 'Roles & Users', 1, 0),
('50000000-0000-0000-0000-000000000507', 'Menu Management', 1, 0),
('50000000-0000-0000-0000-000000000508', 'Security Settings', 1, 0),
('50000000-0000-0000-0000-000000000509', 'Profile', 1, 0),
('50000000-0000-0000-0000-000000000510', 'Permission Modules', 1, 0)
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    IsActive = 1,
    IsDeleted = 0;

INSERT INTO PermissionModules (Id, Name, IsActive, IsDeleted)
SELECT UUID(), TRIM(sourceModules.ModuleName), 1, 0
FROM (
    SELECT Module AS ModuleName FROM menuitems WHERE Module IS NOT NULL AND TRIM(Module) <> ''
    UNION
    SELECT Module AS ModuleName FROM rolepermissions WHERE Module IS NOT NULL AND TRIM(Module) <> ''
) AS sourceModules
WHERE NOT EXISTS (
    SELECT 1
    FROM PermissionModules pm
    WHERE pm.IsDeleted = 0
      AND LOWER(TRIM(pm.Name)) = LOWER(TRIM(sourceModules.ModuleName))
);

UPDATE menuitems mi
JOIN PermissionModules pm
    ON pm.IsDeleted = 0
   AND LOWER(TRIM(pm.Name)) = LOWER(TRIM(mi.Module))
SET mi.ModuleId = pm.Id,
    mi.ShowInSidebar = 1
WHERE mi.Module IS NOT NULL
  AND TRIM(mi.Module) <> '';

UPDATE rolepermissions rp
JOIN PermissionModules pm
    ON pm.IsDeleted = 0
   AND LOWER(TRIM(pm.Name)) = LOWER(TRIM(rp.Module))
SET rp.ModuleId = pm.Id,
    rp.CanSeeMenu = CASE WHEN rp.CanSeeMenu = 1 THEN 1 ELSE rp.CanView END
WHERE rp.Module IS NOT NULL
  AND TRIM(rp.Module) <> '';

INSERT INTO menuitems
(Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, ShowInSidebar, `Order`, IsActive, OpenInNewTab, CreatedAt, IsDeleted)
SELECT
    '50000000-0000-0000-0000-000000000311',
    @TenantId,
    @AdministrationMenuId,
    'Permission Modules',
    'PM',
    '/PermissionModules',
    'Administration',
    'Permission Modules',
    pm.Id,
    1,
    5,
    1,
    0,
    UTC_TIMESTAMP(6),
    0
FROM PermissionModules pm
WHERE pm.Name = 'Permission Modules'
ON DUPLICATE KEY UPDATE
    TenantId = VALUES(TenantId),
    ParentId = VALUES(ParentId),
    Label = VALUES(Label),
    Icon = VALUES(Icon),
    Url = VALUES(Url),
    SectionLabel = VALUES(SectionLabel),
    Module = VALUES(Module),
    ModuleId = VALUES(ModuleId),
    ShowInSidebar = 1,
    `Order` = VALUES(`Order`),
    IsActive = 1,
    OpenInNewTab = VALUES(OpenInNewTab),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);

INSERT INTO rolepermissions
(Id, RoleId, Module, ModuleId, CanSeeMenu, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, IsDeleted)
SELECT
    UUID(),
    @AdminRoleId,
    pm.Name,
    pm.Id,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    UTC_TIMESTAMP(6),
    0
FROM PermissionModules pm
WHERE pm.IsDeleted = 0
  AND pm.Name IN ('Dashboard', 'Consumers', 'Billing', 'Payments', 'Reports', 'Roles & Users', 'Menu Management', 'Security Settings', 'Profile', 'Permission Modules')
ON DUPLICATE KEY UPDATE
    ModuleId = VALUES(ModuleId),
    CanSeeMenu = 1,
    CanView = VALUES(CanView),
    CanAdd = VALUES(CanAdd),
    CanEdit = VALUES(CanEdit),
    CanDelete = VALUES(CanDelete),
    CanDownload = VALUES(CanDownload),
    CanExport = VALUES(CanExport),
    CanApprove = VALUES(CanApprove),
    CanForward = VALUES(CanForward),
    CanPrint = VALUES(CanPrint),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);

CALL AddIndexIfMissing('menuitems', 'IX_MenuItems_ModuleId', 'INDEX IX_MenuItems_ModuleId (ModuleId)');
CALL AddIndexIfMissing('rolepermissions', 'IX_RolePermissions_ModuleId', 'INDEX IX_RolePermissions_ModuleId (ModuleId)');
CALL AddIndexIfMissing('rolepermissions', 'UX_RolePermissions_RoleId_ModuleId', 'UNIQUE INDEX UX_RolePermissions_RoleId_ModuleId (RoleId, ModuleId)');

CALL AddForeignKeyIfMissing(
    'menuitems',
    'FK_MenuItems_PermissionModules_ModuleId',
    'FK_MenuItems_PermissionModules_ModuleId FOREIGN KEY (ModuleId) REFERENCES PermissionModules(Id)'
);

CALL AddForeignKeyIfMissing(
    'rolepermissions',
    'FK_RolePermissions_PermissionModules_ModuleId',
    'FK_RolePermissions_PermissionModules_ModuleId FOREIGN KEY (ModuleId) REFERENCES PermissionModules(Id)'
);

DROP PROCEDURE IF EXISTS AddForeignKeyIfMissing;
DROP PROCEDURE IF EXISTS AddIndexIfMissing;
DROP PROCEDURE IF EXISTS AddColumnIfMissing;
