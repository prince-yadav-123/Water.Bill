-- Seed Consumer Login Management module/menu/permissions.
-- This is safe to run once after the auto-increment auth schema is already present.

INSERT INTO `PermissionModules` (`Id`, `Name`, `IsActive`, `IsDeleted`)
VALUES (17, 'Consumer Login Management', 1, 0)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `IsActive` = 1,
  `IsDeleted` = 0;

INSERT INTO `menuitems`
  (`Id`, `TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `CreatedAt`, `IsDeleted`)
VALUES
  (13, 1, NULL, 'Consumer Login Management', 'CL', '/ConsumerLoginManagement', 'Administration', 'Consumer Login Management', 17, 150, 1, 1, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
  `TenantId` = VALUES(`TenantId`),
  `Label` = VALUES(`Label`),
  `Icon` = VALUES(`Icon`),
  `Url` = VALUES(`Url`),
  `SectionLabel` = VALUES(`SectionLabel`),
  `Module` = VALUES(`Module`),
  `ModuleId` = VALUES(`ModuleId`),
  `Order` = VALUES(`Order`),
  `IsActive` = 1,
  `ShowInSidebar` = 1,
  `IsDeleted` = 0;

-- Give Admin full access to this module.
INSERT INTO `rolepermissions`
  (`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT
  r.`Id`,
  pm.`Name`,
  pm.`Id`,
  1, 1, 1, 1, 1, 1, 1, 0, 0, 1,
  UTC_TIMESTAMP(6),
  0
FROM `approles` r
JOIN `PermissionModules` pm ON pm.`Name` = 'Consumer Login Management' AND pm.`IsDeleted` = 0
WHERE r.`Name` = 'Admin'
  AND r.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE
  `ModuleId` = VALUES(`ModuleId`),
  `CanSeeMenu` = 1,
  `CanView` = 1,
  `CanAdd` = 1,
  `CanEdit` = 1,
  `CanDelete` = 1,
  `CanDownload` = 1,
  `CanExport` = 1,
  `CanPrint` = 1,
  `IsDeleted` = 0;
