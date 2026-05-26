USE water_bill;

SET @TenantId = '50000000-0000-0000-0000-000000000001';
SET @AdminRoleId = COALESCE((SELECT Id FROM approles WHERE Name = 'Admin' AND IsDeleted = 0 LIMIT 1), '50000000-0000-0000-0000-000000000101');
SET @AdministrationMenuId = COALESCE(
    (SELECT Id FROM menuitems WHERE Label = 'Administration' AND IsDeleted = 0 LIMIT 1),
    '50000000-0000-0000-0000-000000000306'
);

SET @RoleManagementModuleId = '50000000-0000-0000-0000-000000000521';
SET @UserManagementModuleId = '50000000-0000-0000-0000-000000000522';
SET @RolePermissionModuleId = '50000000-0000-0000-0000-000000000523';
SET @MenuManagementModuleId = COALESCE((SELECT Id FROM PermissionModules WHERE Name = 'Menu Management' AND IsDeleted = 0 LIMIT 1), '50000000-0000-0000-0000-000000000507');
SET @PermissionModulesModuleId = COALESCE((SELECT Id FROM PermissionModules WHERE Name = 'Permission Modules' AND IsDeleted = 0 LIMIT 1), '50000000-0000-0000-0000-000000000510');
SET @SecuritySettingsModuleId = COALESCE((SELECT Id FROM PermissionModules WHERE Name = 'Security Settings' AND IsDeleted = 0 LIMIT 1), '50000000-0000-0000-0000-000000000508');

INSERT INTO PermissionModules (Id, Name, IsActive, IsDeleted)
VALUES
(@RoleManagementModuleId, 'Role Management', 1, 0),
(@UserManagementModuleId, 'User Management', 1, 0),
(@RolePermissionModuleId, 'Role Permission', 1, 0),
(@MenuManagementModuleId, 'Menu Management', 1, 0),
(@PermissionModulesModuleId, 'Permission Modules', 1, 0),
(@SecuritySettingsModuleId, 'Security Settings', 1, 0)
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    IsActive = 1,
    IsDeleted = 0;

SET @RoleManagementModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'Role Management' AND IsDeleted = 0 LIMIT 1);
SET @UserManagementModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'User Management' AND IsDeleted = 0 LIMIT 1);
SET @RolePermissionModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'Role Permission' AND IsDeleted = 0 LIMIT 1);
SET @MenuManagementModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'Menu Management' AND IsDeleted = 0 LIMIT 1);
SET @PermissionModulesModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'Permission Modules' AND IsDeleted = 0 LIMIT 1);
SET @SecuritySettingsModuleId = (SELECT Id FROM PermissionModules WHERE Name = 'Security Settings' AND IsDeleted = 0 LIMIT 1);

UPDATE PermissionModules
SET IsActive = 0
WHERE Name = 'Roles & Users'
  AND IsDeleted = 0;

INSERT INTO menuitems
(Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, `Order`, IsActive, ShowInSidebar, OpenInNewTab, CreatedAt, IsDeleted)
VALUES
('50000000-0000-0000-0000-000000000307', @TenantId, @AdministrationMenuId, 'Role Management', 'RM', '/Roles', 'Administration', 'Role Management', @RoleManagementModuleId, 1, 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000311', @TenantId, @AdministrationMenuId, 'User Management', 'UM', '/Users', 'Administration', 'User Management', @UserManagementModuleId, 2, 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000308', @TenantId, @AdministrationMenuId, 'Role Permission', 'RP', '/RolePermissions', 'Administration', 'Role Permission', @RolePermissionModuleId, 3, 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000309', @TenantId, @AdministrationMenuId, 'Menu Management', 'MN', '/Menu', 'Administration', 'Menu Management', @MenuManagementModuleId, 4, 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000312', @TenantId, @AdministrationMenuId, 'Permission Modules', 'PM', '/PermissionModules', 'Administration', 'Permission Modules', @PermissionModulesModuleId, 5, 1, 1, 0, UTC_TIMESTAMP(6), 0),
('50000000-0000-0000-0000-000000000310', @TenantId, @AdministrationMenuId, 'Security Settings', 'SS', '/SecuritySettings', 'Administration', 'Security Settings', @SecuritySettingsModuleId, 6, 1, 1, 0, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    TenantId = VALUES(TenantId),
    ParentId = VALUES(ParentId),
    Label = VALUES(Label),
    Icon = VALUES(Icon),
    Url = VALUES(Url),
    SectionLabel = VALUES(SectionLabel),
    Module = VALUES(Module),
    ModuleId = VALUES(ModuleId),
    `Order` = VALUES(`Order`),
    IsActive = VALUES(IsActive),
    ShowInSidebar = VALUES(ShowInSidebar),
    OpenInNewTab = VALUES(OpenInNewTab),
    IsDeleted = 0,
    UpdatedAt = UTC_TIMESTAMP(6);

INSERT INTO rolepermissions
(Id, RoleId, Module, ModuleId, CanSeeMenu, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, IsDeleted)
VALUES
(UUID(), @AdminRoleId, 'Role Management', @RoleManagementModuleId, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'User Management', @UserManagementModuleId, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Role Permission', @RolePermissionModuleId, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Menu Management', @MenuManagementModuleId, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Permission Modules', @PermissionModulesModuleId, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0),
(UUID(), @AdminRoleId, 'Security Settings', @SecuritySettingsModuleId, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, UTC_TIMESTAMP(6), 0)
ON DUPLICATE KEY UPDATE
    Module = VALUES(Module),
    CanSeeMenu = VALUES(CanSeeMenu),
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
