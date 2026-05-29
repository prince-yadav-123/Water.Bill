-- Consumer Portal NDC / No Dues application menu and permission seed.
-- Safe to run multiple times.

SET @consumerModuleName := 'Consumer NDC Applications';

INSERT INTO `PermissionModules` (`Name`, `IsActive`, `IsDeleted`)
SELECT @consumerModuleName, 1, 0
WHERE NOT EXISTS (
    SELECT 1 FROM `PermissionModules`
    WHERE `Name` = @consumerModuleName AND `IsDeleted` = 0
);

SET @consumerModuleId := (
    SELECT `Id`
    FROM `PermissionModules`
    WHERE `Name` = @consumerModuleName AND `IsDeleted` = 0
    LIMIT 1
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `Module`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, NULL, 'NDC / No Dues', 'ND', '/Consumer/Ndc', 'Main', @consumerModuleName, @consumerModuleId, 61, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `Label` = 'NDC / No Dues' AND `IsDeleted` = 0
);

UPDATE `menuitems`
SET `Url` = '/Consumer/Ndc',
    `Module` = @consumerModuleName,
    `ModuleId` = @consumerModuleId,
    `IsActive` = 1,
    `ShowInSidebar` = 1,
    `UpdatedAt` = NOW(6)
WHERE `TenantId` = 2
  AND `Label` = 'NDC / No Dues'
  AND `IsDeleted` = 0;

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @consumerModuleName, @consumerModuleId, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Consumer'
  AND NOT EXISTS (
      SELECT 1
      FROM `rolepermissions` rp
      WHERE rp.`RoleId` = r.`Id`
        AND rp.`ModuleId` = @consumerModuleId
        AND rp.`IsDeleted` = 0
  );

UPDATE `rolepermissions` rp
JOIN `approles` r ON r.`Id` = rp.`RoleId`
SET rp.`CanSeeMenu` = 1,
    rp.`CanView` = 1,
    rp.`CanAdd` = 1,
    rp.`CanDownload` = 1,
    rp.`CanPrint` = 1,
    rp.`IsDeleted` = 0
WHERE r.`Name` = 'Consumer'
  AND rp.`ModuleId` = @consumerModuleId;

-- Optional safety seed: only inserts default NDC document requirements when the imported table has no ND rows.
SET @hasNdcDocs := (
    SELECT COUNT(*)
    FROM `master_document_upload`
    WHERE `Doc_for` = 'ND'
);

SET @nextDocId := (
    SELECT COALESCE(MAX(`Document_id`), 0) + 1
    FROM `master_document_upload`
);

INSERT INTO `master_document_upload` (`Document_id`, `Document_Name`, `status`, `Doc_for`)
SELECT @nextDocId, 'Applicant ID Proof', 1, 'ND'
WHERE @hasNdcDocs = 0;

INSERT INTO `master_document_upload` (`Document_id`, `Document_Name`, `status`, `Doc_for`)
SELECT @nextDocId + 1, 'Property Ownership Proof', 1, 'ND'
WHERE @hasNdcDocs = 0;

INSERT INTO `master_document_upload` (`Document_id`, `Document_Name`, `status`, `Doc_for`)
SELECT @nextDocId + 2, 'Latest Paid Bill / Challan', 1, 'ND'
WHERE @hasNdcDocs = 0;
