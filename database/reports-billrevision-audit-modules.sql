-- Reports / MIS, Advanced Bill Revision / Reversal, and Operator Activity Logs.
-- Uses existing tables only:
-- jal_print_bill_master, jal_print_bill_master_log, challan, jalnoida_bankpay_master,
-- jalnoida_bankpay_tran, consumer_details_master, auditlogs.

SET @tenantId := 1;
SET @adminRoleId := (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'Reports / MIS', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'Reports / MIS' AND `IsDeleted` = 0);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'Advanced Bill Revision / Reversal', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'Advanced Bill Revision / Reversal' AND `IsDeleted` = 0);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'Operator Activity Logs', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'Operator Activity Logs' AND `IsDeleted` = 0);

SET @reportsModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'Reports / MIS' AND `IsDeleted` = 0 LIMIT 1);
SET @revisionModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'Advanced Bill Revision / Reversal' AND `IsDeleted` = 0 LIMIT 1);
SET @auditModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'Operator Activity Logs' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'Reports / MIS', 'RP', '/ReportsMis', 'Reports', 'Reports / MIS', @reportsModuleId, 120, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'Reports / MIS' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'Advanced Bill Revision / Reversal', 'BR', '/BillRevision', 'Billing', 'Advanced Bill Revision / Reversal', @revisionModuleId, 121, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'Advanced Bill Revision / Reversal' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'Operator Activity Logs', 'AL', '/OperatorAudit', 'Administration', 'Operator Activity Logs', @auditModuleId, 122, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'Operator Activity Logs' AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'Reports / MIS', @reportsModuleId, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @reportsModuleId AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'Advanced Bill Revision / Reversal', @revisionModuleId, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @revisionModuleId AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'Operator Activity Logs', @auditModuleId, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @auditModuleId AND `IsDeleted` = 0);

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanEdit` = CASE WHEN `ModuleId` = @revisionModuleId THEN 1 ELSE `CanEdit` END,
    `CanDownload` = 1,
    `CanExport` = 1,
    `CanPrint` = 1
WHERE `RoleId` = @adminRoleId
  AND `ModuleId` IN (@reportsModuleId, @revisionModuleId, @auditModuleId)
  AND `IsDeleted` = 0;
