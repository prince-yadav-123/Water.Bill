/*
    Portal scope split for PermissionModules

    Purpose:
    Separate Consumer Portal modules from Authority/Admin modules so the
    Role Permission screen can show only the relevant modules for the
    selected role.
*/

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PermissionModules', N'PortalScope') IS NULL
BEGIN
    ALTER TABLE dbo.PermissionModules
        ADD PortalScope NVARCHAR(20) NOT NULL
            CONSTRAINT DF_PermissionModules_PortalScope DEFAULT (N'Authority');
END;
GO

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.PermissionModules
    SET PortalScope = CASE
        WHEN Name IN (
            N'Consumer Dashboard',
            N'Consumer Bills',
            N'Consumer Profile',
            N'Consumer New Connection',
            N'Consumer NDC Applications',
            N'Consumer Challans',
            N'Consumer Service Requests',
            N'Consumer Support Queries',
            N'Consumer Complaints'
        ) THEN N'Consumer'
        ELSE N'Authority'
    END
END;
GO

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'IX_PermissionModules_PortalScope_IsDeleted_IsActive'
         AND object_id = OBJECT_ID(N'dbo.PermissionModules')
   )
BEGIN
    CREATE INDEX IX_PermissionModules_PortalScope_IsDeleted_IsActive
        ON dbo.PermissionModules (PortalScope, IsDeleted, IsActive);
END;
GO
