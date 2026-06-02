-- Consumer Portal My Challans / Payment Requests menu.
-- Run manually in the Water.Bill MySQL database after the Challan Management module script.

SET @moduleName := 'Consumer Challans';

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
SELECT 2, NULL, 'My Challans', 'CH', '/Consumer/Challans', 'Main', @moduleName, @moduleId, 45, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `Label` = 'My Challans' AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Consumer'
  AND NOT EXISTS (
      SELECT 1 FROM `rolepermissions`
      WHERE `RoleId` = r.`Id` AND `ModuleId` = @moduleId AND `IsDeleted` = 0
  );

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanDownload` = 1,
    `CanPrint` = 1
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Consumer')
  AND `IsDeleted` = 0;
