/*
    Error Logs module
    -----------------
    Safe SQL Server script to add the Water.Bill Error Logs table and
    the related authority portal menu / permission seeds.
*/

SET NOCOUNT ON;

/* ============================================================
   1) ERROR LOGS TABLE
   ============================================================ */

IF OBJECT_ID(N'dbo.ErrorLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ErrorLogs
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ErrorLogs PRIMARY KEY,
        CreatedAt      DATETIME2(6) NOT NULL CONSTRAINT DF_ErrorLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ExceptionType  NVARCHAR(200) NOT NULL,
        Message        NVARCHAR(2000) NOT NULL,
        StackTrace     NVARCHAR(MAX) NULL,
        RequestPath    NVARCHAR(500) NULL,
        HttpMethod     NVARCHAR(10) NULL,
        QueryString    NVARCHAR(2000) NULL,
        StatusCode     INT NOT NULL CONSTRAINT DF_ErrorLogs_StatusCode DEFAULT (500),
        IpAddress      NVARCHAR(64) NULL,
        Username       NVARCHAR(150) NULL,
        UserId         NVARCHAR(100) NULL,
        PortalType     NVARCHAR(20) NOT NULL CONSTRAINT DF_ErrorLogs_PortalType DEFAULT (N'Unknown'),
        UserAgent      NVARCHAR(1000) NULL,
        ControllerName NVARCHAR(150) NULL,
        ActionName     NVARCHAR(150) NULL,
        TraceId        NVARCHAR(100) NULL,
        IsHandled      BIT NOT NULL CONSTRAINT DF_ErrorLogs_IsHandled DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ErrorLogs_CreatedAt' AND object_id = OBJECT_ID(N'dbo.ErrorLogs'))
BEGIN
    CREATE INDEX IX_ErrorLogs_CreatedAt ON dbo.ErrorLogs (CreatedAt DESC);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ErrorLogs_ExceptionType' AND object_id = OBJECT_ID(N'dbo.ErrorLogs'))
BEGIN
    CREATE INDEX IX_ErrorLogs_ExceptionType ON dbo.ErrorLogs (ExceptionType);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ErrorLogs_StatusCode' AND object_id = OBJECT_ID(N'dbo.ErrorLogs'))
BEGIN
    CREATE INDEX IX_ErrorLogs_StatusCode ON dbo.ErrorLogs (StatusCode);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ErrorLogs_PortalType' AND object_id = OBJECT_ID(N'dbo.ErrorLogs'))
BEGIN
    CREATE INDEX IX_ErrorLogs_PortalType ON dbo.ErrorLogs (PortalType);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ErrorLogs_CreatedAt_Portal_Status' AND object_id = OBJECT_ID(N'dbo.ErrorLogs'))
BEGIN
    CREATE INDEX IX_ErrorLogs_CreatedAt_Portal_Status ON dbo.ErrorLogs (CreatedAt DESC, PortalType, StatusCode);
END;

/* ============================================================
   2) PERMISSION MODULE
   ============================================================ */

MERGE dbo.PermissionModules AS tgt
USING
(
    SELECT N'Error Logs' AS Name, N'Authority' AS PortalScope
) AS src
ON tgt.Name = src.Name
WHEN MATCHED THEN
    UPDATE SET
        tgt.IsActive = 1,
        tgt.IsDeleted = 0,
        tgt.PortalScope = src.PortalScope
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Name, PortalScope, IsActive, IsDeleted)
    VALUES (src.Name, src.PortalScope, 1, 0);

/* ============================================================
   3) ADMIN ROLE PERMISSION
   ============================================================ */

MERGE dbo.RolePermissions AS tgt
USING
(
    SELECT r.Id AS RoleId, pm.Id AS ModuleId, pm.Name AS ModuleName
    FROM dbo.AppRoles r
    JOIN dbo.PermissionModules pm
      ON pm.Name = N'Error Logs'
     AND pm.IsDeleted = 0
    WHERE r.Name = N'Admin'
      AND r.IsDeleted = 0
) AS src
ON tgt.RoleId = src.RoleId AND tgt.ModuleId = src.ModuleId
WHEN MATCHED THEN
    UPDATE SET
        tgt.Module = src.ModuleName,
        tgt.CanSeeMenu = 1,
        tgt.CanView = 1,
        tgt.CanAdd = 0,
        tgt.CanEdit = 0,
        tgt.CanDelete = 1,
        tgt.CanDownload = 1,
        tgt.CanExport = 1,
        tgt.CanApprove = 0,
        tgt.CanForward = 0,
        tgt.CanPrint = 0,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, Module, ModuleId, CanSeeMenu, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.RoleId, src.ModuleName, src.ModuleId, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, SYSDATETIME(), NULL, 0);

/* ============================================================
   4) MENU ITEMS
   ============================================================ */

MERGE dbo.MenuItems AS tgt
USING
(
    SELECT
        1 AS TenantId,
        CAST(NULL AS INT) AS ParentId,
        N'Administration' AS Label,
        N'A' AS Icon,
        CAST(NULL AS NVARCHAR(300)) AS Url,
        CAST(NULL AS NVARCHAR(100)) AS SectionLabel,
        CAST(NULL AS NVARCHAR(100)) AS ModuleName,
        CAST(NULL AS INT) AS ModuleId,
        2 AS MenuOrder,
        CAST(1 AS BIT) AS IsActive,
        CAST(1 AS BIT) AS ShowInSidebar,
        CAST(0 AS BIT) AS OpenInNewTab
) AS src
ON tgt.TenantId = src.TenantId
   AND tgt.Label = src.Label
   AND tgt.ParentId IS NULL
WHEN MATCHED THEN
    UPDATE SET
        tgt.Icon = src.Icon,
        tgt.Url = src.Url,
        tgt.SectionLabel = src.SectionLabel,
        tgt.Module = src.ModuleName,
        tgt.ModuleId = src.ModuleId,
        tgt.[Order] = src.MenuOrder,
        tgt.IsActive = src.IsActive,
        tgt.ShowInSidebar = src.ShowInSidebar,
        tgt.OpenInNewTab = src.OpenInNewTab,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, [Order], IsActive, ShowInSidebar, OpenInNewTab, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.TenantId, src.ParentId, src.Label, src.Icon, src.Url, src.SectionLabel, src.ModuleName, src.ModuleId, src.MenuOrder, src.IsActive, src.ShowInSidebar, src.OpenInNewTab, SYSDATETIME(), NULL, 0);

;WITH menu_seed AS
(
    SELECT
        1 AS TenantId,
        N'Administration' AS ParentLabel,
        N'Error Logs' AS Label,
        N'EL' AS Icon,
        N'/ErrorLogs' AS Url,
        N'Administration' AS SectionLabel,
        N'Error Logs' AS ModuleName,
        13 AS MenuOrder,
        CAST(1 AS BIT) AS IsActive,
        CAST(1 AS BIT) AS ShowInSidebar,
        CAST(0 AS BIT) AS OpenInNewTab
)
MERGE dbo.MenuItems AS tgt
USING
(
    SELECT
        s.TenantId,
        p.Id AS ParentId,
        s.Label,
        s.Icon,
        s.Url,
        s.SectionLabel,
        s.ModuleName,
        pm.Id AS ModuleId,
        s.MenuOrder,
        s.IsActive,
        s.ShowInSidebar,
        s.OpenInNewTab
    FROM menu_seed s
    JOIN dbo.MenuItems p
      ON p.TenantId = s.TenantId
     AND p.ParentId IS NULL
     AND p.Label = s.ParentLabel
     AND p.IsDeleted = 0
    LEFT JOIN dbo.PermissionModules pm
      ON pm.Name = s.ModuleName
     AND pm.IsDeleted = 0
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
        tgt.IsActive = src.IsActive,
        tgt.ShowInSidebar = src.ShowInSidebar,
        tgt.OpenInNewTab = src.OpenInNewTab,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, [Order], IsActive, ShowInSidebar, OpenInNewTab, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.TenantId, src.ParentId, src.Label, src.Icon, src.Url, src.SectionLabel, src.ModuleName, src.ModuleId, src.MenuOrder, src.IsActive, src.ShowInSidebar, src.OpenInNewTab, SYSDATETIME(), NULL, 0);
