-- Consumer Portal Service Requests: Name Transfer / Mutation and Connection Change.
-- Uses existing old-compatible table `master_application_detail`; no new request tables are created.
-- Run manually in the Water.Bill MySQL database.

SET @moduleName := 'Consumer Service Requests';

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
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, NULL, 'Service Requests', 'SR', NULL, 'Main', NULL, 47, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `ParentId` IS NULL AND `Label` = 'Service Requests' AND `IsDeleted` = 0
);

SET @parentMenuId := (
    SELECT `Id` FROM `menuitems`
    WHERE `TenantId` = 2 AND `ParentId` IS NULL AND `Label` = 'Service Requests' AND `IsDeleted` = 0
    LIMIT 1
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @parentMenuId, 'My Requests', 'MR', '/Consumer/ServiceRequests', 'Requests', @moduleId, 1, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `ParentId` = @parentMenuId AND `Label` = 'My Requests' AND `IsDeleted` = 0
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @parentMenuId, 'Name Transfer / Mutation', 'NT', '/Consumer/ServiceRequests/NameTransfer', 'Apply', @moduleId, 2, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `ParentId` = @parentMenuId AND `Label` = 'Name Transfer / Mutation' AND `IsDeleted` = 0
);

INSERT INTO `menuitems`
(`TenantId`, `ParentId`, `Label`, `Icon`, `Url`, `SectionLabel`, `ModuleId`, `Order`, `IsActive`, `ShowInSidebar`, `OpenInNewTab`, `CreatedAt`, `IsDeleted`)
SELECT 2, @parentMenuId, 'Connection Change', 'CC', '/Consumer/ServiceRequests/ConnectionChange', 'Apply', @moduleId, 3, 1, 1, 0, NOW(6), 0
WHERE NOT EXISTS (
    SELECT 1 FROM `menuitems`
    WHERE `TenantId` = 2 AND `ParentId` = @parentMenuId AND `Label` = 'Connection Change' AND `IsDeleted` = 0
);

INSERT INTO `rolepermissions`
(`RoleId`, `Module`, `ModuleId`, `CanSeeMenu`, `CanView`, `CanAdd`, `CanEdit`, `CanDelete`, `CanDownload`, `CanExport`, `CanApprove`, `CanForward`, `CanPrint`, `CreatedAt`, `IsDeleted`)
SELECT r.`Id`, @moduleName, @moduleId, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, NOW(6), 0
FROM `approles` r
WHERE r.`Name` = 'Consumer'
  AND NOT EXISTS (
      SELECT 1 FROM `rolepermissions`
      WHERE `RoleId` = r.`Id` AND `ModuleId` = @moduleId AND `IsDeleted` = 0
  );

UPDATE `rolepermissions`
SET `CanSeeMenu` = 1,
    `CanView` = 1,
    `CanAdd` = 1,
    `CanPrint` = 1,
    `UpdatedAt` = NOW(6)
WHERE `ModuleId` = @moduleId
  AND `RoleId` IN (SELECT `Id` FROM `approles` WHERE `Name` = 'Consumer')
  AND `IsDeleted` = 0;

-- Workflow application type seeds. Configure stages from Admin UI if you need custom department/role/user assignment.
INSERT INTO `WorkflowMasters` (`WorkflowName`, `ApplicationType`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT 'Name Transfer Approval', 'NameTransfer', 1, 0, NOW(6)
WHERE NOT EXISTS (
    SELECT 1 FROM `WorkflowMasters`
    WHERE `ApplicationType` = 'NameTransfer' AND `IsDeleted` = 0
);

INSERT INTO `WorkflowMasters` (`WorkflowName`, `ApplicationType`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT 'Connection Change Approval', 'ConnectionChange', 1, 0, NOW(6)
WHERE NOT EXISTS (
    SELECT 1 FROM `WorkflowMasters`
    WHERE `ApplicationType` = 'ConnectionChange' AND `IsDeleted` = 0
);

SET @staffRoleId := (
    SELECT `Id` FROM `approles`
    WHERE `Name` IN ('Staff', 'Operator', 'Admin')
    ORDER BY FIELD(`Name`, 'Staff', 'Operator', 'Admin')
    LIMIT 1
);

SET @nameTransferWorkflowId := (
    SELECT `Id` FROM `WorkflowMasters`
    WHERE `ApplicationType` = 'NameTransfer' AND `IsDeleted` = 0
    ORDER BY `Id` DESC
    LIMIT 1
);

SET @connectionChangeWorkflowId := (
    SELECT `Id` FROM `WorkflowMasters`
    WHERE `ApplicationType` = 'ConnectionChange' AND `IsDeleted` = 0
    ORDER BY `Id` DESC
    LIMIT 1
);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @nameTransferWorkflowId, 'Initial Review', 1, NULL, @staffRoleId, NULL, 'DepartmentRole', 1, 1, 0, 0, 1, 3, 1, 0, NOW(6)
WHERE @staffRoleId IS NOT NULL
  AND @nameTransferWorkflowId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM `WorkflowStages`
      WHERE `WorkflowId` = @nameTransferWorkflowId AND `IsDeleted` = 0
  );

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @connectionChangeWorkflowId, 'Initial Review', 1, NULL, @staffRoleId, NULL, 'DepartmentRole', 1, 1, 0, 0, 1, 3, 1, 0, NOW(6)
WHERE @staffRoleId IS NOT NULL
  AND @connectionChangeWorkflowId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM `WorkflowStages`
      WHERE `WorkflowId` = @connectionChangeWorkflowId AND `IsDeleted` = 0
  );
