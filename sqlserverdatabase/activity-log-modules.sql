/*
    Activity Logs split seed for SQL Server
    - Renames Operator Activity Logs -> User Activity Logs when safe
    - Adds Consumer Activity Logs permission module
    - Updates Administration menu entries
*/

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NOT NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.PermissionModules
        WHERE Name = N'Operator Activity Logs'
          AND IsDeleted = 0
    )
    AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.PermissionModules
        WHERE Name = N'User Activity Logs'
          AND IsDeleted = 0
    )
    BEGIN
        UPDATE dbo.PermissionModules
        SET Name = N'User Activity Logs',
            PortalScope = N'Authority',
            IsActive = 1,
            IsDeleted = 0
        WHERE Name = N'Operator Activity Logs'
          AND IsDeleted = 0;
    END;

    MERGE dbo.PermissionModules AS tgt
    USING
    (
        SELECT N'User Activity Logs' AS Name, N'Authority' AS PortalScope
        UNION ALL
        SELECT N'Consumer Activity Logs', N'Authority'
    ) AS src
    ON tgt.Name = src.Name
    WHEN MATCHED THEN
        UPDATE SET
            tgt.PortalScope = src.PortalScope,
            tgt.IsActive = 1,
            tgt.IsDeleted = 0
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Name, PortalScope, IsActive, IsDeleted)
        VALUES (src.Name, src.PortalScope, 1, 0);
END;

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NOT NULL
BEGIN
    UPDATE rp
    SET rp.Module = pm.Name,
        rp.IsDeleted = 0,
        rp.UpdatedAt = SYSDATETIME()
    FROM dbo.RolePermissions rp
    INNER JOIN dbo.PermissionModules pm ON pm.Id = rp.ModuleId
    WHERE pm.Name IN (N'User Activity Logs', N'Consumer Activity Logs')
      AND (rp.Module <> pm.Name OR rp.IsDeleted = 1);

    MERGE dbo.RolePermissions AS tgt
    USING
    (
        SELECT r.Id AS RoleId, pm.Id AS ModuleId, pm.Name AS ModuleName
        FROM dbo.AppRoles r
        CROSS JOIN dbo.PermissionModules pm
        WHERE r.Name = N'Admin'
          AND r.IsDeleted = 0
          AND pm.Name IN (N'User Activity Logs', N'Consumer Activity Logs')
          AND pm.IsDeleted = 0
    ) AS src
    ON tgt.RoleId = src.RoleId AND tgt.ModuleId = src.ModuleId
    WHEN MATCHED THEN
        UPDATE SET
            tgt.Module = src.ModuleName,
            tgt.CanSeeMenu = 1,
            tgt.CanView = 1,
            tgt.CanAdd = 1,
            tgt.CanEdit = 1,
            tgt.CanDelete = 1,
            tgt.CanDownload = 1,
            tgt.CanExport = 1,
            tgt.CanApprove = 1,
            tgt.CanForward = 1,
            tgt.CanPrint = 1,
            tgt.IsDeleted = 0,
            tgt.UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (RoleId, Module, ModuleId, CanSeeMenu, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, UpdatedAt, IsDeleted)
        VALUES (src.RoleId, src.ModuleName, src.ModuleId, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, SYSDATETIME(), NULL, 0);
END;

IF OBJECT_ID(N'dbo.MenuItems', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.MenuItems
    SET Label = N'User Activity Logs',
        Url = N'/UserActivityLogs',
        Module = N'User Activity Logs',
        IsActive = 1,
        IsDeleted = 0,
        UpdatedAt = SYSDATETIME()
    WHERE Label = N'Operator Activity Logs'
      AND IsDeleted = 0;

    DECLARE @AdministrationParentId INT;
    DECLARE @UserActivityModuleId INT;
    DECLARE @ConsumerActivityModuleId INT;

    SELECT @AdministrationParentId = Id
    FROM dbo.MenuItems
    WHERE TenantId = 1
      AND ParentId IS NULL
      AND Label = N'Administration'
      AND IsDeleted = 0;

    SELECT @UserActivityModuleId = Id
    FROM dbo.PermissionModules
    WHERE Name = N'User Activity Logs'
      AND IsDeleted = 0;

    SELECT @ConsumerActivityModuleId = Id
    FROM dbo.PermissionModules
    WHERE Name = N'Consumer Activity Logs'
      AND IsDeleted = 0;

    IF @AdministrationParentId IS NOT NULL
    BEGIN
        MERGE dbo.MenuItems AS tgt
        USING
        (
            SELECT 1 AS TenantId, @AdministrationParentId AS ParentId, N'User Activity Logs' AS Label, N'AL' AS Icon, N'/UserActivityLogs' AS Url, N'Administration' AS SectionLabel, N'User Activity Logs' AS ModuleName, @UserActivityModuleId AS ModuleId, 12 AS MenuOrder
            UNION ALL
            SELECT 1, @AdministrationParentId, N'Consumer Activity Logs', N'CL', N'/ConsumerActivityLogs', N'Administration', N'Consumer Activity Logs', @ConsumerActivityModuleId, 13
        ) AS src
        ON tgt.TenantId = src.TenantId
           AND tgt.ParentId = src.ParentId
           AND tgt.Label = src.Label
        WHEN MATCHED THEN
            UPDATE SET
                tgt.Icon = src.Icon,
                tgt.Url = src.Url,
                tgt.SectionLabel = src.SectionLabel,
                tgt.Module = src.ModuleName,
                tgt.ModuleId = src.ModuleId,
                tgt.[Order] = src.MenuOrder,
                tgt.IsActive = 1,
                tgt.ShowInSidebar = 1,
                tgt.OpenInNewTab = 0,
                tgt.IsDeleted = 0,
                tgt.UpdatedAt = SYSDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, [Order], IsActive, ShowInSidebar, OpenInNewTab, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (src.TenantId, src.ParentId, src.Label, src.Icon, src.Url, src.SectionLabel, src.ModuleName, src.ModuleId, src.MenuOrder, 1, 1, 0, SYSDATETIME(), NULL, 0);
    END;
END;
