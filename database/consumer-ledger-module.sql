-- Consumer Ledger / Account Statement module.
-- Read-only module. No old bill/challan/consumer tables are altered.

SET @moduleName := 'Consumer Ledger';

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT @moduleName, 1, 0
WHERE NOT EXISTS (
    SELECT 1 FROM `PermissionModules`
    WHERE `Name` = @moduleName AND `IsDeleted` = 0
);

SET @moduleId := (
    SELECT `Id` FROM `PermissionModules`
    WHERE `Name` = @moduleName AND `IsDeleted` = 0
    LIMIT 1
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 1, NULL, @moduleName, 'CL', '/ConsumerLedger', 'Billing', @moduleName, @moduleId, 68, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 1 AND `Label` = @moduleName AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM `rolepermissions`
      WHERE `RoleId` = r.`Id` AND `ModuleId` = @moduleId AND `IsDeleted` = 0
  );

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanDownload` = 1,
    `CanExport` = 1,
    `CanPrint` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin')
  AND `IsDeleted` = 0;
