-- Bill Search & Print module for Water.Bill Authority Portal.
-- Run manually in the Water.Bill MySQL database.
-- Uses existing imported tables: consumer_details_master and jal_print_bill_master.
-- No stored procedures and no new bill tables are required.

SET @moduleName := 'Bill Search & Print';

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
SELECT 1, NULL, @moduleName, 'BP', '/BillSearchPrint', 'Billing', @moduleName, @moduleId, 70, 1, 1, 0, NOW(6), 0
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
