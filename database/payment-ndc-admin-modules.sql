-- Payment and NDC admin modules for Water.Bill Authority Portal.
-- Run manually in the Water.Bill MySQL database.
-- Uses existing/imported tables:
--   jalnoida_bankpay_master, jalnoida_bankpay_tran
--   consumer_apply_ndc, ndc_document
-- No stored procedures are used by the new implementation.

SET @tenantId := 1;
SET @adminRoleId := (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'Online Payment History', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'Online Payment History' AND `IsDeleted` = 0);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'NDC Applications', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'NDC Applications' AND `IsDeleted` = 0);

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT 'NDC Certificate Management', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `PermissionModules` WHERE `Name` = 'NDC Certificate Management' AND `IsDeleted` = 0);

SET @paymentModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'Online Payment History' AND `IsDeleted` = 0 LIMIT 1);
SET @ndcModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'NDC Applications' AND `IsDeleted` = 0 LIMIT 1);
SET @ndcCertificateModuleId := (SELECT `Id` FROM `PermissionModules` WHERE `Name` = 'NDC Certificate Management' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'Online Payment History', 'PH', '/OnlinePaymentHistory', 'Payments', 'Online Payment History', @paymentModuleId, 80, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'Online Payment History' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'NDC Applications', 'ND', '/NdcApplications', 'Approvals', 'NDC Applications', @ndcModuleId, 95, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'NDC Applications' AND `IsDeleted` = 0);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT @tenantId, NULL, 'NDC Certificate Management', 'NC', '/NdcCertificates', 'Approvals', 'NDC Certificate Management', @ndcCertificateModuleId, 96, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (SELECT 1 FROM `menuitems` WHERE `TenantId` = @tenantId AND `Label` = 'NDC Certificate Management' AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'Online Payment History', @paymentModuleId, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @paymentModuleId AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'NDC Applications', @ndcModuleId, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @ndcModuleId AND `IsDeleted` = 0);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT @adminRoleId, 'NDC Certificate Management', @ndcCertificateModuleId, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, NOW(6), 0
WHERE @adminRoleId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM `rolepermissions` WHERE `RoleId` = @adminRoleId AND `ModuleId` = @ndcCertificateModuleId AND `IsDeleted` = 0);

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanDownload` = 1,
    `CanExport` = 1,
    `CanPrint` = 1
WHERE `RoleId` = @adminRoleId
  AND `ModuleId` IN (@paymentModuleId, @ndcModuleId, @ndcCertificateModuleId)
  AND `IsDeleted` = 0;

UPDATE `rolepermissions`
SET `CanApprove` = 1,
    `CanForward` = 1
WHERE `RoleId` = @adminRoleId
  AND `ModuleId` = @ndcModuleId
  AND `IsDeleted` = 0;
