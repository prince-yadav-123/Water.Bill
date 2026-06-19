param(
    [string]$PermissionModulesCsv,
    [string]$MenuItemsCsv,
    [string]$OutputSql
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function SqlLiteral([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq 'NULL') { return 'NULL' }
    return "N'" + ($value -replace "'", "''") + "'"
}

function SqlIntOrNull([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq 'NULL') { return 'NULL' }
    return $value
}

function SqlBit([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq 'NULL') { return '0' }
    return ([int]$value).ToString()
}

function SqlDateOrNull([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq 'NULL') { return 'NULL' }
    return "CAST('$value' AS DATETIME2(6))"
}

$pmHeaders = 'Id','Name','IsActive','IsDeleted','Unused','PortalScope'
$menuHeaders = 'Id','TenantId','ParentId','Label','Icon','Url','SectionLabel','ModuleName','ModuleId','OrderNo','ShowInSidebar','IsActive','IsDeleted','CreatedAt','UpdatedAt','OpenInNewTab'

$pmRows = Import-Csv -Path $PermissionModulesCsv -Header $pmHeaders
$menuRows = Import-Csv -Path $MenuItemsCsv -Header $menuHeaders

$sb = [System.Text.StringBuilder]::new()

[void]$sb.AppendLine('/*')
[void]$sb.AppendLine('  UAT sync script generated from old DB CSV exports:')
[void]$sb.AppendLine('  - Results.csv -> PermissionModules')
[void]$sb.AppendLine('  - ResultsMenuiotems.csv -> MenuItems')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('  Behavior')
[void]$sb.AppendLine('  - Updates matching existing IDs')
[void]$sb.AppendLine('  - If ID is missing but natural key already exists, updates that row safely')
[void]$sb.AppendLine('  - Inserts only truly missing rows')
[void]$sb.AppendLine('  - Does not delete existing extra rows')
[void]$sb.AppendLine('*/')
[void]$sb.AppendLine('SET NOCOUNT ON;')
[void]$sb.AppendLine('SET XACT_ABORT ON;')
[void]$sb.AppendLine('')
[void]$sb.AppendLine("IF OBJECT_ID('tempdb..#PermissionModuleSeed') IS NOT NULL DROP TABLE #PermissionModuleSeed;")
[void]$sb.AppendLine('CREATE TABLE #PermissionModuleSeed (Id INT NOT NULL, Name NVARCHAR(100) NOT NULL, IsActive BIT NOT NULL, IsDeleted BIT NOT NULL, PortalScope NVARCHAR(20) NOT NULL);')
[void]$sb.AppendLine('INSERT INTO #PermissionModuleSeed (Id, Name, IsActive, IsDeleted, PortalScope) VALUES')

for ($i = 0; $i -lt $pmRows.Count; $i++) {
    $r = $pmRows[$i]
    $line = "    ({0}, {1}, {2}, {3}, {4})" -f $r.Id, (SqlLiteral $r.Name), (SqlBit $r.IsActive), (SqlBit $r.IsDeleted), (SqlLiteral $r.PortalScope)
    if ($i -lt $pmRows.Count - 1) { $line += ',' } else { $line += ';' }
    [void]$sb.AppendLine($line)
}

[void]$sb.AppendLine('')
[void]$sb.AppendLine("IF OBJECT_ID('tempdb..#MenuSeed') IS NOT NULL DROP TABLE #MenuSeed;")
[void]$sb.AppendLine('CREATE TABLE #MenuSeed (Id INT NOT NULL, TenantId INT NOT NULL, ParentId INT NULL, Label NVARCHAR(100) NOT NULL, Icon NVARCHAR(100) NULL, Url NVARCHAR(300) NULL, SectionLabel NVARCHAR(100) NULL, ModuleName NVARCHAR(100) NULL, ModuleId INT NULL, MenuOrder INT NOT NULL, ShowInSidebar BIT NOT NULL, IsActive BIT NOT NULL, IsDeleted BIT NOT NULL, CreatedAt DATETIME2(6) NULL, UpdatedAt DATETIME2(6) NULL, OpenInNewTab BIT NOT NULL);')
[void]$sb.AppendLine('INSERT INTO #MenuSeed (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, ModuleName, ModuleId, MenuOrder, ShowInSidebar, IsActive, IsDeleted, CreatedAt, UpdatedAt, OpenInNewTab) VALUES')

for ($i = 0; $i -lt $menuRows.Count; $i++) {
    $r = $menuRows[$i]
    $line = "    ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})" -f `
        $r.Id, `
        $r.TenantId, `
        (SqlIntOrNull $r.ParentId), `
        (SqlLiteral $r.Label), `
        (SqlLiteral $r.Icon), `
        (SqlLiteral $r.Url), `
        (SqlLiteral $r.SectionLabel), `
        (SqlLiteral $r.ModuleName), `
        (SqlIntOrNull $r.ModuleId), `
        $r.OrderNo, `
        (SqlBit $r.ShowInSidebar), `
        (SqlBit $r.IsActive), `
        (SqlBit $r.IsDeleted), `
        (SqlDateOrNull $r.CreatedAt), `
        (SqlDateOrNull $r.UpdatedAt), `
        (SqlBit $r.OpenInNewTab)
    if ($i -lt $menuRows.Count - 1) { $line += ',' } else { $line += ';' }
    [void]$sb.AppendLine($line)
}

$body = @"

BEGIN TRY
    BEGIN TRAN;

    UPDATE tgt
    SET tgt.Name = src.Name,
        tgt.IsActive = src.IsActive,
        tgt.IsDeleted = src.IsDeleted,
        tgt.PortalScope = src.PortalScope
    FROM dbo.PermissionModules tgt
    JOIN #PermissionModuleSeed src ON src.Id = tgt.Id;

    UPDATE tgt
    SET tgt.IsActive = src.IsActive,
        tgt.IsDeleted = src.IsDeleted,
        tgt.PortalScope = src.PortalScope
    FROM dbo.PermissionModules tgt
    JOIN #PermissionModuleSeed src
      ON tgt.Name COLLATE DATABASE_DEFAULT = src.Name COLLATE DATABASE_DEFAULT
    WHERE NOT EXISTS (SELECT 1 FROM dbo.PermissionModules x WHERE x.Id = src.Id);

    SET IDENTITY_INSERT dbo.PermissionModules ON;

    INSERT INTO dbo.PermissionModules (Id, Name, IsActive, IsDeleted, PortalScope)
    SELECT src.Id, src.Name, src.IsActive, src.IsDeleted, src.PortalScope
    FROM #PermissionModuleSeed src
    WHERE NOT EXISTS (SELECT 1 FROM dbo.PermissionModules tgt WHERE tgt.Id = src.Id)
      AND NOT EXISTS (SELECT 1 FROM dbo.PermissionModules tgt WHERE tgt.Name COLLATE DATABASE_DEFAULT = src.Name COLLATE DATABASE_DEFAULT);

    SET IDENTITY_INSERT dbo.PermissionModules OFF;

    IF COL_LENGTH('dbo.MenuItems', 'Module') IS NOT NULL
    BEGIN
        UPDATE tgt
        SET tgt.TenantId = src.TenantId,
            tgt.Label = src.Label,
            tgt.Icon = src.Icon,
            tgt.Url = src.Url,
            tgt.SectionLabel = src.SectionLabel,
            tgt.Module = src.ModuleName,
            tgt.ModuleId = src.ModuleId,
            tgt.[Order] = src.MenuOrder,
            tgt.ShowInSidebar = src.ShowInSidebar,
            tgt.IsActive = src.IsActive,
            tgt.IsDeleted = src.IsDeleted,
            tgt.CreatedAt = ISNULL(src.CreatedAt, tgt.CreatedAt),
            tgt.UpdatedAt = src.UpdatedAt,
            tgt.OpenInNewTab = src.OpenInNewTab
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src ON src.Id = tgt.Id;

        UPDATE tgt
        SET tgt.Icon = src.Icon,
            tgt.Url = src.Url,
            tgt.SectionLabel = src.SectionLabel,
            tgt.Module = src.ModuleName,
            tgt.ModuleId = src.ModuleId,
            tgt.[Order] = src.MenuOrder,
            tgt.ShowInSidebar = src.ShowInSidebar,
            tgt.IsActive = src.IsActive,
            tgt.IsDeleted = src.IsDeleted,
            tgt.UpdatedAt = src.UpdatedAt,
            tgt.OpenInNewTab = src.OpenInNewTab
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src
          ON tgt.TenantId = src.TenantId
         AND ISNULL(tgt.ParentId, -1) = ISNULL(src.ParentId, -1)
         AND tgt.Label COLLATE DATABASE_DEFAULT = src.Label COLLATE DATABASE_DEFAULT
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuItems x WHERE x.Id = src.Id);

        SET IDENTITY_INSERT dbo.MenuItems ON;

        INSERT INTO dbo.MenuItems (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, Module, ModuleId, [Order], ShowInSidebar, IsActive, IsDeleted, CreatedAt, UpdatedAt, OpenInNewTab)
        SELECT src.Id, src.TenantId, NULL, src.Label, src.Icon, src.Url, src.SectionLabel, src.ModuleName, src.ModuleId, src.MenuOrder, src.ShowInSidebar, src.IsActive, src.IsDeleted, ISNULL(src.CreatedAt, SYSDATETIME()), src.UpdatedAt, src.OpenInNewTab
        FROM #MenuSeed src
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuItems tgt WHERE tgt.Id = src.Id)
          AND NOT EXISTS (
              SELECT 1 FROM dbo.MenuItems tgt
              WHERE tgt.TenantId = src.TenantId
                AND ISNULL(tgt.ParentId, -1) = ISNULL(src.ParentId, -1)
                AND tgt.Label COLLATE DATABASE_DEFAULT = src.Label COLLATE DATABASE_DEFAULT
          )
        ORDER BY CASE WHEN src.ParentId IS NULL THEN 0 ELSE 1 END, src.ParentId, src.Id;

        SET IDENTITY_INSERT dbo.MenuItems OFF;

        UPDATE tgt
        SET tgt.ParentId = src.ParentId
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src ON src.Id = tgt.Id
        WHERE ISNULL(tgt.ParentId, -1) <> ISNULL(src.ParentId, -1)
          AND (src.ParentId IS NULL OR EXISTS (SELECT 1 FROM dbo.MenuItems p WHERE p.Id = src.ParentId));
    END
    ELSE
    BEGIN
        UPDATE tgt
        SET tgt.TenantId = src.TenantId,
            tgt.Label = src.Label,
            tgt.Icon = src.Icon,
            tgt.Url = src.Url,
            tgt.SectionLabel = src.SectionLabel,
            tgt.ModuleId = src.ModuleId,
            tgt.[Order] = src.MenuOrder,
            tgt.ShowInSidebar = src.ShowInSidebar,
            tgt.IsActive = src.IsActive,
            tgt.IsDeleted = src.IsDeleted,
            tgt.CreatedAt = ISNULL(src.CreatedAt, tgt.CreatedAt),
            tgt.UpdatedAt = src.UpdatedAt,
            tgt.OpenInNewTab = src.OpenInNewTab
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src ON src.Id = tgt.Id;

        UPDATE tgt
        SET tgt.Icon = src.Icon,
            tgt.Url = src.Url,
            tgt.SectionLabel = src.SectionLabel,
            tgt.ModuleId = src.ModuleId,
            tgt.[Order] = src.MenuOrder,
            tgt.ShowInSidebar = src.ShowInSidebar,
            tgt.IsActive = src.IsActive,
            tgt.IsDeleted = src.IsDeleted,
            tgt.UpdatedAt = src.UpdatedAt,
            tgt.OpenInNewTab = src.OpenInNewTab
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src
          ON tgt.TenantId = src.TenantId
         AND ISNULL(tgt.ParentId, -1) = ISNULL(src.ParentId, -1)
         AND tgt.Label COLLATE DATABASE_DEFAULT = src.Label COLLATE DATABASE_DEFAULT
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuItems x WHERE x.Id = src.Id);

        SET IDENTITY_INSERT dbo.MenuItems ON;

        INSERT INTO dbo.MenuItems (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, ModuleId, [Order], ShowInSidebar, IsActive, IsDeleted, CreatedAt, UpdatedAt, OpenInNewTab)
        SELECT src.Id, src.TenantId, NULL, src.Label, src.Icon, src.Url, src.SectionLabel, src.ModuleId, src.MenuOrder, src.ShowInSidebar, src.IsActive, src.IsDeleted, ISNULL(src.CreatedAt, SYSDATETIME()), src.UpdatedAt, src.OpenInNewTab
        FROM #MenuSeed src
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuItems tgt WHERE tgt.Id = src.Id)
          AND NOT EXISTS (
              SELECT 1 FROM dbo.MenuItems tgt
              WHERE tgt.TenantId = src.TenantId
                AND ISNULL(tgt.ParentId, -1) = ISNULL(src.ParentId, -1)
                AND tgt.Label COLLATE DATABASE_DEFAULT = src.Label COLLATE DATABASE_DEFAULT
          )
        ORDER BY CASE WHEN src.ParentId IS NULL THEN 0 ELSE 1 END, src.ParentId, src.Id;

        SET IDENTITY_INSERT dbo.MenuItems OFF;

        UPDATE tgt
        SET tgt.ParentId = src.ParentId
        FROM dbo.MenuItems tgt
        JOIN #MenuSeed src ON src.Id = tgt.Id
        WHERE ISNULL(tgt.ParentId, -1) <> ISNULL(src.ParentId, -1)
          AND (src.ParentId IS NULL OR EXISTS (SELECT 1 FROM dbo.MenuItems p WHERE p.Id = src.ParentId));
    END;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
"@

[void]$sb.AppendLine($body)

[System.IO.File]::WriteAllText($OutputSql, $sb.ToString(), [System.Text.Encoding]::UTF8)
Write-Output "Generated $OutputSql"
