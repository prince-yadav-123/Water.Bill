/*
    Water.Bill consolidated SQL Server update script

    Purpose
    -------
    Add the database changes introduced in the new Water.Bill project on top of the
    older SQL Server database without disturbing old data.

    Safety notes
    ------------
    - Safe to rerun: all schema changes are guarded.
    - No old tables are dropped.
    - No old columns are renamed or truncated.
    - Seed data uses natural-key upserts (MERGE / IF NOT EXISTS patterns).
    - Dummy/test-data scripts are intentionally excluded.

    Included areas
    --------------
    - Auth / admin seed tables
    - PermissionModules / MenuItems / RolePermissions
    - Workflow engine tables and status code columns
    - Communication / notification system
    - New connection application tables
    - Consumer support query module
    - Consumer login / OTP tables
    - Consumer notices / meter readings
*/

SET NOCOUNT ON;

/* ============================================================
   1) AUTH / ADMIN BASE TABLES
   ============================================================ */

IF OBJECT_ID(N'dbo.AppRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppRoles
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppRoles PRIMARY KEY,
        Name          NVARCHAR(100) NOT NULL,
        Description   NVARCHAR(500) NULL,
        Permissions   NVARCHAR(MAX) NULL,
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_AppRoles_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt     DATETIME2(6) NULL,
        IsDeleted     BIT NOT NULL CONSTRAINT DF_AppRoles_IsDeleted DEFAULT (0),
        CONSTRAINT UX_AppRoles_Name UNIQUE (Name),
        CONSTRAINT CK_AppRoles_Permissions_IsJson CHECK (Permissions IS NULL OR ISJSON(Permissions) = 1)
    );
END;

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers
    (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
        FullName           NVARCHAR(150) NOT NULL,
        Email              NVARCHAR(150) NOT NULL,
        Username           NVARCHAR(100) NOT NULL,
        PasswordHash       NVARCHAR(500) NOT NULL,
        IsActive           BIT NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT (1),
        RoleId             INT NOT NULL,
        PhoneNumber        NVARCHAR(30) NULL,
        FailedLoginCount    INT NOT NULL CONSTRAINT DF_AppUsers_FailedLoginCount DEFAULT (0),
        LockoutUntil       DATETIME2(6) NULL,
        PasswordChangedAt  DATETIME2(6) NULL,
        LastLoginAt        DATETIME2(6) NULL,
        LastLoginIp        NVARCHAR(64) NULL,
        CreatedAt          DATETIME2(6) NOT NULL CONSTRAINT DF_AppUsers_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt          DATETIME2(6) NULL,
        IsDeleted          BIT NOT NULL CONSTRAINT DF_AppUsers_IsDeleted DEFAULT (0),
        CONSTRAINT UX_AppUsers_Email UNIQUE (Email),
        CONSTRAINT UX_AppUsers_Username UNIQUE (Username),
        CONSTRAINT FK_AppUsers_AppRoles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.AppRoles (Id)
    );
END;

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PermissionModules
    (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PermissionModules PRIMARY KEY,
        Name       NVARCHAR(100) NOT NULL,
        IsActive   BIT NOT NULL CONSTRAINT DF_PermissionModules_IsActive DEFAULT (1),
        IsDeleted  BIT NOT NULL CONSTRAINT DF_PermissionModules_IsDeleted DEFAULT (0),
        CreatedAt  DATETIME2(6) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PermissionModules_Name_IsDeleted' AND object_id = OBJECT_ID(N'dbo.PermissionModules'))
BEGIN
    CREATE UNIQUE INDEX UX_PermissionModules_Name_IsDeleted
    ON dbo.PermissionModules (Name, IsDeleted);
END;

IF OBJECT_ID(N'dbo.PermissionModules', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PermissionModules', N'PortalScope') IS NULL
BEGIN
    ALTER TABLE dbo.PermissionModules
        ADD PortalScope NVARCHAR(20) NOT NULL
            CONSTRAINT DF_PermissionModules_PortalScope DEFAULT (N'Authority');
END;

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

IF OBJECT_ID(N'dbo.MenuItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuItems
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY,
        TenantId      INT NOT NULL,
        ParentId      INT NULL,
        Label         NVARCHAR(100) NOT NULL,
        Icon          NVARCHAR(100) NULL,
        Url           NVARCHAR(300) NULL,
        SectionLabel  NVARCHAR(100) NULL,
        Module        NVARCHAR(100) NULL,
        ModuleId      INT NULL,
        [Order]       INT NOT NULL CONSTRAINT DF_MenuItems_Order DEFAULT (0),
        IsActive      BIT NULL CONSTRAINT DF_MenuItems_IsActive DEFAULT (1),
        ShowInSidebar BIT NOT NULL CONSTRAINT DF_MenuItems_ShowInSidebar DEFAULT (1),
        OpenInNewTab  BIT NOT NULL CONSTRAINT DF_MenuItems_OpenInNewTab DEFAULT (0),
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_MenuItems_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt     DATETIME2(6) NULL,
        IsDeleted     BIT NOT NULL CONSTRAINT DF_MenuItems_IsDeleted DEFAULT (0),
        CONSTRAINT FK_MenuItems_MenuItems_ParentId FOREIGN KEY (ParentId) REFERENCES dbo.MenuItems (Id),
        CONSTRAINT FK_MenuItems_PermissionModules_ModuleId FOREIGN KEY (ModuleId) REFERENCES dbo.PermissionModules (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MenuItems_TenantId_ParentId_Order' AND object_id = OBJECT_ID(N'dbo.MenuItems'))
BEGIN
    CREATE INDEX IX_MenuItems_TenantId_ParentId_Order ON dbo.MenuItems (TenantId, ParentId, [Order]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MenuItems_ModuleId' AND object_id = OBJECT_ID(N'dbo.MenuItems'))
BEGIN
    CREATE INDEX IX_MenuItems_ModuleId ON dbo.MenuItems (ModuleId);
END;

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId      INT NOT NULL,
        Module      NVARCHAR(100) NOT NULL,
        ModuleId    INT NULL,
        CanSeeMenu  BIT NOT NULL CONSTRAINT DF_RolePermissions_CanSeeMenu DEFAULT (0),
        CanView     BIT NOT NULL CONSTRAINT DF_RolePermissions_CanView DEFAULT (0),
        CanAdd      BIT NOT NULL CONSTRAINT DF_RolePermissions_CanAdd DEFAULT (0),
        CanEdit     BIT NOT NULL CONSTRAINT DF_RolePermissions_CanEdit DEFAULT (0),
        CanDelete   BIT NOT NULL CONSTRAINT DF_RolePermissions_CanDelete DEFAULT (0),
        CanDownload BIT NOT NULL CONSTRAINT DF_RolePermissions_CanDownload DEFAULT (0),
        CanExport   BIT NOT NULL CONSTRAINT DF_RolePermissions_CanExport DEFAULT (0),
        CanApprove  BIT NOT NULL CONSTRAINT DF_RolePermissions_CanApprove DEFAULT (0),
        CanForward  BIT NOT NULL CONSTRAINT DF_RolePermissions_CanForward DEFAULT (0),
        CanPrint    BIT NOT NULL CONSTRAINT DF_RolePermissions_CanPrint DEFAULT (0),
        CreatedAt   DATETIME2(6) NOT NULL CONSTRAINT DF_RolePermissions_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt   DATETIME2(6) NULL,
        IsDeleted   BIT NOT NULL CONSTRAINT DF_RolePermissions_IsDeleted DEFAULT (0),
        CONSTRAINT FK_RolePermissions_AppRoles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.AppRoles (Id),
        CONSTRAINT FK_RolePermissions_PermissionModules_ModuleId FOREIGN KEY (ModuleId) REFERENCES dbo.PermissionModules (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RolePermissions_RoleId_Module' AND object_id = OBJECT_ID(N'dbo.RolePermissions'))
BEGIN
    CREATE INDEX UX_RolePermissions_RoleId_Module ON dbo.RolePermissions (RoleId, Module);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RolePermissions_RoleId_ModuleId' AND object_id = OBJECT_ID(N'dbo.RolePermissions'))
BEGIN
    CREATE UNIQUE INDEX UX_RolePermissions_RoleId_ModuleId ON dbo.RolePermissions (RoleId, ModuleId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RolePermissions_ModuleId' AND object_id = OBJECT_ID(N'dbo.RolePermissions'))
BEGIN
    CREATE INDEX IX_RolePermissions_ModuleId ON dbo.RolePermissions (ModuleId);
END;

/* ============================================================
   1A) CHALLAN MANAGEMENT SUPPORT TABLES
   Source: database/challan-management-module.sql
   ============================================================ */

IF OBJECT_ID(N'dbo.ChallanHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChallanHistories
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChallanHistories PRIMARY KEY,
        ChallanId      BIGINT NOT NULL,
        ChallanNo      NVARCHAR(30) NULL,
        ConsumerNo     NVARCHAR(15) NULL,
        FromStatus     NVARCHAR(30) NULL,
        ToStatus       NVARCHAR(30) NULL,
        [Action]       NVARCHAR(50) NOT NULL,
        Remarks        NVARCHAR(500) NULL,
        ActionByUserId INT NULL,
        ActionByName   NVARCHAR(150) NULL,
        ActionOn       DATETIME NOT NULL,
        IsDeleted      BIT NOT NULL CONSTRAINT DF_ChallanHistories_IsDeleted DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanHistories_ChallanId' AND object_id = OBJECT_ID(N'dbo.ChallanHistories'))
BEGIN
    CREATE INDEX IX_ChallanHistories_ChallanId ON dbo.ChallanHistories (ChallanId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanHistories_ChallanNo' AND object_id = OBJECT_ID(N'dbo.ChallanHistories'))
BEGIN
    CREATE INDEX IX_ChallanHistories_ChallanNo ON dbo.ChallanHistories (ChallanNo);
END;

IF OBJECT_ID(N'dbo.ChallanPaymentHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChallanPaymentHistories
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChallanPaymentHistories PRIMARY KEY,
        ChallanId              BIGINT NOT NULL,
        ChallanNo              NVARCHAR(30) NULL,
        ConsumerNo             NVARCHAR(15) NULL,
        SourceBillNo           NVARCHAR(30) NULL,
        Amount                 FLOAT NOT NULL,
        PaymentDate            DATETIME NOT NULL,
        PaymentMode            NVARCHAR(50) NULL,
        BankCode               NVARCHAR(100) NULL,
        BankName               NVARCHAR(150) NULL,
        TransactionReferenceNo NVARCHAR(100) NULL,
        Remarks                NVARCHAR(500) NULL,
        PostedByUserId         INT NULL,
        PostedByName           NVARCHAR(150) NULL,
        PostedOn               DATETIME NOT NULL,
        IsDeleted              BIT NOT NULL CONSTRAINT DF_ChallanPaymentHistories_IsDeleted DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanPaymentHistories_ChallanId' AND object_id = OBJECT_ID(N'dbo.ChallanPaymentHistories'))
BEGIN
    CREATE INDEX IX_ChallanPaymentHistories_ChallanId ON dbo.ChallanPaymentHistories (ChallanId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanPaymentHistories_ChallanNo' AND object_id = OBJECT_ID(N'dbo.ChallanPaymentHistories'))
BEGIN
    CREATE INDEX IX_ChallanPaymentHistories_ChallanNo ON dbo.ChallanPaymentHistories (ChallanNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanPaymentHistories_ConsumerNo' AND object_id = OBJECT_ID(N'dbo.ChallanPaymentHistories'))
BEGIN
    CREATE INDEX IX_ChallanPaymentHistories_ConsumerNo ON dbo.ChallanPaymentHistories (ConsumerNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChallanPaymentHistories_PaymentDate' AND object_id = OBJECT_ID(N'dbo.ChallanPaymentHistories'))
BEGIN
    CREATE INDEX IX_ChallanPaymentHistories_PaymentDate ON dbo.ChallanPaymentHistories (PaymentDate);
END;

IF OBJECT_ID(N'dbo.AuthorityUserDepartments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuthorityUserDepartments
    (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuthorityUserDepartments PRIMARY KEY,
        UserId       INT NOT NULL,
        DepartmentId INT NOT NULL,
        IsActive     BIT NOT NULL CONSTRAINT DF_AuthorityUserDepartments_IsActive DEFAULT (1),
        IsDeleted    BIT NOT NULL CONSTRAINT DF_AuthorityUserDepartments_IsDeleted DEFAULT (0),
        CreatedOn    DATETIME2(6) NOT NULL CONSTRAINT DF_AuthorityUserDepartments_CreatedOn DEFAULT (SYSDATETIME())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_AuthorityUserDepartments_User_Department'
      AND object_id = OBJECT_ID(N'dbo.AuthorityUserDepartments')
)
BEGIN
    CREATE UNIQUE INDEX UX_AuthorityUserDepartments_User_Department
        ON dbo.AuthorityUserDepartments (UserId, DepartmentId);
END;

IF OBJECT_ID(N'dbo.UserSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSessions
    (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserSessions PRIMARY KEY,
        UserId             INT NOT NULL,
        SessionToken       NVARCHAR(200) NOT NULL,
        IpAddress          NVARCHAR(64) NULL,
        UserAgent          NVARCHAR(500) NULL,
        DeviceFingerprint  NVARCHAR(200) NULL,
        IsActive           BIT NOT NULL CONSTRAINT DF_UserSessions_IsActive DEFAULT (1),
        ExpiresAt          DATETIME2(6) NOT NULL,
        LastActivityAt     DATETIME2(6) NULL,
        RevokedAt          DATETIME2(6) NULL,
        RevokedReason      NVARCHAR(100) NULL,
        CreatedAt          DATETIME2(6) NOT NULL CONSTRAINT DF_UserSessions_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt          DATETIME2(6) NULL,
        IsDeleted          BIT NOT NULL CONSTRAINT DF_UserSessions_IsDeleted DEFAULT (0),
        CONSTRAINT UX_UserSessions_SessionToken UNIQUE (SessionToken),
        CONSTRAINT FK_UserSessions_AppUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AppUsers (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserSessions_UserId_IsActive' AND object_id = OBJECT_ID(N'dbo.UserSessions'))
BEGIN
    CREATE INDEX IX_UserSessions_UserId_IsActive ON dbo.UserSessions (UserId, IsActive);
END;

IF OBJECT_ID(N'dbo.LoginAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAttempts
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginAttempts PRIMARY KEY,
        Username      NVARCHAR(100) NULL,
        IpAddress     NVARCHAR(64) NULL,
        Success       BIT NOT NULL CONSTRAINT DF_LoginAttempts_Success DEFAULT (0),
        FailureReason NVARCHAR(100) NULL,
        UserAgent     NVARCHAR(500) NULL,
        UserId        INT NULL,
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_LoginAttempts_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt     DATETIME2(6) NULL,
        IsDeleted     BIT NOT NULL CONSTRAINT DF_LoginAttempts_IsDeleted DEFAULT (0),
        CONSTRAINT FK_LoginAttempts_AppUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AppUsers (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LoginAttempts_Username_CreatedAt' AND object_id = OBJECT_ID(N'dbo.LoginAttempts'))
BEGIN
    CREATE INDEX IX_LoginAttempts_Username_CreatedAt ON dbo.LoginAttempts (Username, CreatedAt);
END;

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        [Timestamp] DATETIME2(6) NOT NULL CONSTRAINT DF_AuditLogs_Timestamp DEFAULT (SYSDATETIME()),
        UserId     INT NULL,
        Username   NVARCHAR(100) NULL,
        [Action]   INT NOT NULL,
        Module     NVARCHAR(100) NULL,
        EntityId   NVARCHAR(100) NULL,
        IpAddress  NVARCHAR(64) NULL,
        UserAgent  NVARCHAR(500) NULL,
        Details    NVARCHAR(MAX) NULL,
        Success    BIT NOT NULL CONSTRAINT DF_AuditLogs_Success DEFAULT (1)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Timestamp' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
BEGIN
    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs ([Timestamp]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_UserId' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
BEGIN
    CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs (UserId);
END;

IF OBJECT_ID(N'dbo.securitysettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.securitysettings
    (
        Id                           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_securitysettings PRIMARY KEY,
        TenantId                     INT NOT NULL,
        SessionTimeoutMinutes        INT NOT NULL CONSTRAINT DF_securitysettings_SessionTimeoutMinutes DEFAULT (480),
        IdleTimeoutMinutes           INT NOT NULL CONSTRAINT DF_securitysettings_IdleTimeoutMinutes DEFAULT (30),
        PasswordMinLength            INT NOT NULL CONSTRAINT DF_securitysettings_PasswordMinLength DEFAULT (8),
        PasswordRequireUppercase     BIT NOT NULL CONSTRAINT DF_securitysettings_PasswordRequireUppercase DEFAULT (1),
        PasswordRequireLowercase     BIT NOT NULL CONSTRAINT DF_securitysettings_PasswordRequireLowercase DEFAULT (1),
        PasswordRequireDigit         BIT NOT NULL CONSTRAINT DF_securitysettings_PasswordRequireDigit DEFAULT (1),
        PasswordRequireSpecialChar   BIT NOT NULL CONSTRAINT DF_securitysettings_PasswordRequireSpecialChar DEFAULT (1),
        PasswordExpiryDays           INT NOT NULL CONSTRAINT DF_securitysettings_PasswordExpiryDays DEFAULT (90),
        PasswordHistoryCount         INT NOT NULL CONSTRAINT DF_securitysettings_PasswordHistoryCount DEFAULT (5),
        MaxFailedLoginAttempts       INT NOT NULL CONSTRAINT DF_securitysettings_MaxFailedLoginAttempts DEFAULT (5),
        LockoutDurationMinutes       INT NOT NULL CONSTRAINT DF_securitysettings_LockoutDurationMinutes DEFAULT (15),
        EnableCaptchaAfterFailures   BIT NOT NULL CONSTRAINT DF_securitysettings_EnableCaptchaAfterFailures DEFAULT (0),
        CaptchaAfterAttempts         INT NOT NULL CONSTRAINT DF_securitysettings_CaptchaAfterAttempts DEFAULT (3),
        AllowMultipleSessions        BIT NOT NULL CONSTRAINT DF_securitysettings_AllowMultipleSessions DEFAULT (1),
        BlockNewLoginOnConflict      BIT NOT NULL CONSTRAINT DF_securitysettings_BlockNewLoginOnConflict DEFAULT (0),
        CreatedAt                    DATETIME2(6) NOT NULL CONSTRAINT DF_securitysettings_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt                    DATETIME2(6) NULL,
        IsDeleted                    BIT NOT NULL CONSTRAINT DF_securitysettings_IsDeleted DEFAULT (0),
        CONSTRAINT UX_SecuritySettings_TenantId UNIQUE (TenantId)
    );
END;

IF OBJECT_ID(N'dbo.securitysettings', N'U') IS NOT NULL
   AND EXISTS
   (
       SELECT 1
       FROM sys.columns c
       JOIN sys.types t ON c.user_type_id = t.user_type_id
       WHERE c.object_id = OBJECT_ID(N'dbo.securitysettings')
         AND c.name = N'TenantId'
         AND t.name = N'uniqueidentifier'
   )
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM sys.key_constraints
        WHERE [type] = 'PK'
          AND parent_object_id = OBJECT_ID(N'dbo.securitysettings')
          AND name = N'PK_securitysettings'
    )
    BEGIN
        ALTER TABLE dbo.securitysettings DROP CONSTRAINT PK_securitysettings;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_SecuritySettings_TenantId'
          AND object_id = OBJECT_ID(N'dbo.securitysettings')
    )
    BEGIN
        DROP INDEX UX_SecuritySettings_TenantId ON dbo.securitysettings;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM sys.key_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.securitysettings')
          AND name = N'UX_SecuritySettings_TenantId'
    )
    BEGIN
        ALTER TABLE dbo.securitysettings DROP CONSTRAINT UX_SecuritySettings_TenantId;
    END;

    IF COL_LENGTH(N'dbo.securitysettings', N'LegacyGuidId') IS NULL
       AND COL_LENGTH(N'dbo.securitysettings', N'Id') IS NOT NULL
    BEGIN
        EXEC sp_rename N'dbo.securitysettings.Id', N'LegacyGuidId', N'COLUMN';
    END;

    IF COL_LENGTH(N'dbo.securitysettings', N'LegacyTenantGuid') IS NULL
       AND COL_LENGTH(N'dbo.securitysettings', N'TenantId') IS NOT NULL
    BEGIN
        EXEC sp_rename N'dbo.securitysettings.TenantId', N'LegacyTenantGuid', N'COLUMN';
    END;

    IF COL_LENGTH(N'dbo.securitysettings', N'Id') IS NULL
    BEGIN
        ALTER TABLE dbo.securitysettings
            ADD Id INT IDENTITY(1,1) NOT NULL;
    END;

    IF COL_LENGTH(N'dbo.securitysettings', N'TenantId') IS NULL
    BEGIN
        ALTER TABLE dbo.securitysettings
            ADD TenantId INT NULL;
    END;

    UPDATE dbo.securitysettings
    SET TenantId = 1
    WHERE TenantId IS NULL;

    ALTER TABLE dbo.securitysettings
        ALTER COLUMN TenantId INT NOT NULL;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.key_constraints
        WHERE [type] = 'PK'
          AND parent_object_id = OBJECT_ID(N'dbo.securitysettings')
    )
    BEGIN
        ALTER TABLE dbo.securitysettings
            ADD CONSTRAINT PK_securitysettings_int PRIMARY KEY (Id);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes i
        INNER JOIN sys.index_columns ic
            ON i.object_id = ic.object_id
           AND i.index_id = ic.index_id
        INNER JOIN sys.columns c
            ON c.object_id = ic.object_id
           AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.securitysettings')
          AND i.is_unique = 1
          AND c.name = N'TenantId'
    )
    BEGIN
        CREATE UNIQUE INDEX UX_SecuritySettings_TenantId
            ON dbo.securitysettings (TenantId);
    END;
END;

/* ============================================================
   2) LEGACY AUTH / CONSUMER LOGIN TABLES
   ============================================================ */

IF OBJECT_ID(N'dbo.ConsumerUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerUsers
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerUsers PRIMARY KEY,
        ConsumerNo        NVARCHAR(10) NOT NULL,
        Username          NVARCHAR(100) NOT NULL,
        Email             NVARCHAR(150) NULL,
        PasswordHash      NVARCHAR(512) NOT NULL,
        IsActive          BIT NOT NULL CONSTRAINT DF_ConsumerUsers_IsActive DEFAULT (1),
        FailedLoginCount   INT NOT NULL CONSTRAINT DF_ConsumerUsers_FailedLoginCount DEFAULT (0),
        LockoutUntil      DATETIME2(6) NULL,
        LastLoginAt       DATETIME2(6) NULL,
        CreatedAt         DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerUsers_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt         DATETIME2(6) NULL,
        IsDeleted         BIT NOT NULL CONSTRAINT DF_ConsumerUsers_IsDeleted DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerUsers_ConsumerNo' AND object_id = OBJECT_ID(N'dbo.ConsumerUsers'))
BEGIN
    CREATE INDEX IX_ConsumerUsers_ConsumerNo ON dbo.ConsumerUsers (ConsumerNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ConsumerUsers_Username' AND object_id = OBJECT_ID(N'dbo.ConsumerUsers'))
BEGIN
    CREATE UNIQUE INDEX UX_ConsumerUsers_Username ON dbo.ConsumerUsers (Username);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ConsumerUsers_Email' AND object_id = OBJECT_ID(N'dbo.ConsumerUsers'))
BEGIN
    CREATE UNIQUE INDEX UX_ConsumerUsers_Email ON dbo.ConsumerUsers (Email) WHERE Email IS NOT NULL;
END;

IF OBJECT_ID(N'dbo.ConsumerOtpVerifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerOtpVerifications
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerOtpVerifications PRIMARY KEY,
        ConsumerNo    NVARCHAR(10) NOT NULL,
        MobileNo      NVARCHAR(12) NOT NULL,
        OtpHash       NVARCHAR(128) NOT NULL,
        OtpSalt       NVARCHAR(64) NOT NULL,
        Purpose       NVARCHAR(50) NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_Purpose DEFAULT ('ConsumerLogin'),
        ExpiresAt     DATETIME2(6) NOT NULL,
        IsVerified    BIT NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_IsVerified DEFAULT (0),
        VerifiedAt    DATETIME2(6) NULL,
        AttemptCount  INT NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_AttemptCount DEFAULT (0),
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_CreatedAt DEFAULT (SYSDATETIME()),
        IsActive      BIT NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_IsActive DEFAULT (1),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_ConsumerOtpVerifications_IsDeleted DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerOtpVerifications_Consumer_Purpose_CreatedAt' AND object_id = OBJECT_ID(N'dbo.ConsumerOtpVerifications'))
BEGIN
    CREATE INDEX IX_ConsumerOtpVerifications_Consumer_Purpose_CreatedAt
    ON dbo.ConsumerOtpVerifications (ConsumerNo, Purpose, CreatedAt);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerOtpVerifications_ActiveLookup' AND object_id = OBJECT_ID(N'dbo.ConsumerOtpVerifications'))
BEGIN
    CREATE INDEX IX_ConsumerOtpVerifications_ActiveLookup
    ON dbo.ConsumerOtpVerifications (ConsumerNo, Purpose, IsActive, IsVerified);
END;

/* ============================================================
   3) COMMUNICATION / NOTIFICATION SYSTEM
   ============================================================ */

IF OBJECT_ID(N'dbo.CommunicationPurposes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommunicationPurposes
    (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommunicationPurposes PRIMARY KEY,
        PurposeKey          NVARCHAR(100) NOT NULL,
        DisplayName         NVARCHAR(150) NOT NULL,
        Description         NVARCHAR(500) NULL,
        AllowedPlaceholders NVARCHAR(MAX) NOT NULL CONSTRAINT DF_CommunicationPurposes_AllowedPlaceholders DEFAULT (N'[]'),
        IsSystem            BIT NOT NULL CONSTRAINT DF_CommunicationPurposes_IsSystem DEFAULT (1),
        IsActive            BIT NOT NULL CONSTRAINT DF_CommunicationPurposes_IsActive DEFAULT (1),
        CreatedAt           DATETIME2(6) NOT NULL CONSTRAINT DF_CommunicationPurposes_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt           DATETIME2(6) NULL,
        CONSTRAINT UX_CommunicationPurposes_PurposeKey UNIQUE (PurposeKey),
        CONSTRAINT CK_CommunicationPurposes_AllowedPlaceholders_IsJson CHECK (ISJSON(AllowedPlaceholders) = 1)
    );
END;

IF OBJECT_ID(N'dbo.CommunicationTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommunicationTemplates
    (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommunicationTemplates PRIMARY KEY,
        PurposeId           INT NOT NULL,
        PurposeKey          NVARCHAR(100) NOT NULL,
        Channel             NVARCHAR(20) NOT NULL,
        TemplateName        NVARCHAR(150) NOT NULL,
        Subject             NVARCHAR(300) NULL,
        Body                NVARCHAR(MAX) NOT NULL,
        ExternalTemplateId   NVARCHAR(150) NULL,
        Language            NVARCHAR(10) NULL,
        IsDefault           BIT NOT NULL CONSTRAINT DF_CommunicationTemplates_IsDefault DEFAULT (1),
        IsActive            BIT NOT NULL CONSTRAINT DF_CommunicationTemplates_IsActive DEFAULT (1),
        IsDeleted           BIT NOT NULL CONSTRAINT DF_CommunicationTemplates_IsDeleted DEFAULT (0),
        CreatedAt           DATETIME2(6) NOT NULL CONSTRAINT DF_CommunicationTemplates_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt           DATETIME2(6) NULL,
        CONSTRAINT FK_CommunicationTemplates_CommunicationPurposes_PurposeId FOREIGN KEY (PurposeId) REFERENCES dbo.CommunicationPurposes (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationTemplates_PurposeId' AND object_id = OBJECT_ID(N'dbo.CommunicationTemplates'))
BEGIN
    CREATE INDEX IX_CommunicationTemplates_PurposeId ON dbo.CommunicationTemplates (PurposeId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationTemplates_DefaultLookup' AND object_id = OBJECT_ID(N'dbo.CommunicationTemplates'))
BEGIN
    CREATE INDEX IX_CommunicationTemplates_DefaultLookup
    ON dbo.CommunicationTemplates (PurposeKey, Channel, Language, IsDefault, IsActive, IsDeleted);
END;

IF OBJECT_ID(N'dbo.CommunicationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommunicationLogs
    (
        Id                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommunicationLogs PRIMARY KEY,
        PurposeKey         NVARCHAR(100) NOT NULL,
        Channel            NVARCHAR(20) NOT NULL,
        RecipientName      NVARCHAR(150) NULL,
        RecipientEmail     NVARCHAR(150) NULL,
        RecipientMobile    NVARCHAR(20) NULL,
        Subject            NVARCHAR(300) NULL,
        MessageBody        NVARCHAR(MAX) NOT NULL,
        TemplateId         INT NULL,
        ExternalTemplateId NVARCHAR(150) NULL,
        Status             NVARCHAR(20) NOT NULL CONSTRAINT DF_CommunicationLogs_Status DEFAULT ('Pending'),
        ErrorMessage       NVARCHAR(1000) NULL,
        SentAt             DATETIME2(6) NULL,
        CreatedAt          DATETIME2(6) NOT NULL CONSTRAINT DF_CommunicationLogs_CreatedAt DEFAULT (SYSDATETIME()),
        ReferenceType      NVARCHAR(100) NULL,
        ReferenceId        NVARCHAR(100) NULL,
        ReferenceNo        NVARCHAR(100) NULL,
        RetryCount         INT NOT NULL CONSTRAINT DF_CommunicationLogs_RetryCount DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationLogs_Purpose_Channel_CreatedAt' AND object_id = OBJECT_ID(N'dbo.CommunicationLogs'))
BEGIN
    CREATE INDEX IX_CommunicationLogs_Purpose_Channel_CreatedAt
    ON dbo.CommunicationLogs (PurposeKey, Channel, CreatedAt);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommunicationLogs_Reference' AND object_id = OBJECT_ID(N'dbo.CommunicationLogs'))
BEGIN
    CREATE INDEX IX_CommunicationLogs_Reference
    ON dbo.CommunicationLogs (ReferenceType, ReferenceId);
END;

IF OBJECT_ID(N'dbo.InAppNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InAppNotifications
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InAppNotifications PRIMARY KEY,
        UserType      NVARCHAR(20) NOT NULL,
        UserId        BIGINT NOT NULL,
        Title         NVARCHAR(300) NOT NULL,
        Message       NVARCHAR(MAX) NOT NULL,
        PurposeKey    NVARCHAR(100) NOT NULL,
        ReferenceType NVARCHAR(100) NULL,
        ReferenceId   NVARCHAR(100) NULL,
        ReferenceNo   NVARCHAR(100) NULL,
        RedirectUrl   NVARCHAR(1000) NULL,
        IsRead        BIT NOT NULL CONSTRAINT DF_InAppNotifications_IsRead DEFAULT (0),
        ReadAt        DATETIME2(6) NULL,
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_InAppNotifications_CreatedAt DEFAULT (SYSDATETIME()),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_InAppNotifications_IsDeleted DEFAULT (0)
    );
END;

IF OBJECT_ID(N'dbo.InAppNotifications', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.InAppNotifications', N'RedirectUrl') IS NULL
BEGIN
    ALTER TABLE dbo.InAppNotifications
        ADD RedirectUrl NVARCHAR(1000) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InAppNotifications_User_Read' AND object_id = OBJECT_ID(N'dbo.InAppNotifications'))
BEGIN
    CREATE INDEX IX_InAppNotifications_User_Read
    ON dbo.InAppNotifications (UserType, UserId, IsRead, IsDeleted);
END;

IF OBJECT_ID(N'dbo.NotificationMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationMasters
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationMasters PRIMARY KEY,
        Title            NVARCHAR(200) NOT NULL,
        Message          NVARCHAR(MAX) NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL CONSTRAINT DF_NotificationMasters_Type DEFAULT ('General'),
        TargetAudience   NVARCHAR(20) NOT NULL CONSTRAINT DF_NotificationMasters_TargetAudience DEFAULT ('Consumer'),
        Channels         NVARCHAR(100) NOT NULL CONSTRAINT DF_NotificationMasters_Channels DEFAULT ('InApp'),
        Priority         NVARCHAR(20) NOT NULL CONSTRAINT DF_NotificationMasters_Priority DEFAULT ('Normal'),
        Status           NVARCHAR(20) NOT NULL CONSTRAINT DF_NotificationMasters_Status DEFAULT ('Draft'),
        ValidFrom        DATETIME2(6) NULL,
        ValidTo          DATETIME2(6) NULL,
        RedirectUrl      NVARCHAR(1000) NULL,
        CreatedByUserId  INT NOT NULL CONSTRAINT DF_NotificationMasters_CreatedByUserId DEFAULT (0),
        CreatedByName    NVARCHAR(200) NULL,
        CreatedAt        DATETIME2(6) NOT NULL CONSTRAINT DF_NotificationMasters_CreatedAt DEFAULT (SYSDATETIME()),
        SentAt           DATETIME2(6) NULL,
        IsActive         BIT NOT NULL CONSTRAINT DF_NotificationMasters_IsActive DEFAULT (1),
        IsDeleted        BIT NOT NULL CONSTRAINT DF_NotificationMasters_IsDeleted DEFAULT (0)
    );
END;

IF OBJECT_ID(N'dbo.NotificationMasters', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.NotificationMasters', N'RedirectUrl') IS NULL
BEGIN
    ALTER TABLE dbo.NotificationMasters
        ADD RedirectUrl NVARCHAR(1000) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotifMaster_Status' AND object_id = OBJECT_ID(N'dbo.NotificationMasters'))
BEGIN
    CREATE INDEX IX_NotifMaster_Status ON dbo.NotificationMasters (Status);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotifMaster_Audience' AND object_id = OBJECT_ID(N'dbo.NotificationMasters'))
BEGIN
    CREATE INDEX IX_NotifMaster_Audience ON dbo.NotificationMasters (TargetAudience);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotifMaster_CreatedAt' AND object_id = OBJECT_ID(N'dbo.NotificationMasters'))
BEGIN
    CREATE INDEX IX_NotifMaster_CreatedAt ON dbo.NotificationMasters (CreatedAt, IsDeleted);
END;

IF OBJECT_ID(N'dbo.NotificationTargets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationTargets
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationTargets PRIMARY KEY,
        NotificationId BIGINT NOT NULL,
        TargetType     NVARCHAR(50) NOT NULL,
        TargetId       NVARCHAR(200) NULL,
        TargetName     NVARCHAR(300) NULL,
        IsDeleted      BIT NOT NULL CONSTRAINT DF_NotificationTargets_IsDeleted DEFAULT (0),
        CONSTRAINT FK_NotificationTargets_NotificationMasters FOREIGN KEY (NotificationId)
            REFERENCES dbo.NotificationMasters (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotifTarget_NotifId' AND object_id = OBJECT_ID(N'dbo.NotificationTargets'))
BEGIN
    CREATE INDEX IX_NotifTarget_NotifId ON dbo.NotificationTargets (NotificationId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotifTarget_Type_Id' AND object_id = OBJECT_ID(N'dbo.NotificationTargets'))
BEGIN
    CREATE INDEX IX_NotifTarget_Type_Id ON dbo.NotificationTargets (TargetType, TargetId);
END;

IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationLogs
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationLogs PRIMARY KEY,
        ApplicationId BIGINT NULL,
        ApplicationNo NVARCHAR(30) NULL,
        WorkflowInstanceId BIGINT NULL,
        StageId INT NULL,
        Channel NVARCHAR(30) NOT NULL,
        Recipient NVARCHAR(150) NOT NULL,
        Message NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL,
        SentOn DATETIME NULL,
        ErrorMessage NVARCHAR(1000) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_NotificationLogs_RetryCount DEFAULT(0),
        CreatedOn DATETIME NOT NULL CONSTRAINT DF_NotificationLogs_CreatedOn DEFAULT(GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NotificationLogs_ApplicationNo' AND object_id = OBJECT_ID(N'dbo.NotificationLogs'))
BEGIN
    CREATE INDEX IX_NotificationLogs_ApplicationNo ON dbo.NotificationLogs (ApplicationNo);
END;

/* ============================================================
   4) SUPPORT / QUERY MODULE
   ============================================================ */

IF OBJECT_ID(N'dbo.SupportQueryCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupportQueryCategories
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportQueryCategories PRIMARY KEY,
        CategoryName  NVARCHAR(100) NOT NULL,
        Description   NVARCHAR(250) NULL,
        DisplayOrder  INT NOT NULL CONSTRAINT DF_SupportQueryCategories_DisplayOrder DEFAULT (0),
        IsActive      BIT NOT NULL CONSTRAINT DF_SupportQueryCategories_IsActive DEFAULT (1),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_SupportQueryCategories_IsDeleted DEFAULT (0),
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_SupportQueryCategories_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt     DATETIME2(6) NULL,
        CONSTRAINT UX_SupportQueryCategories_CategoryName UNIQUE (CategoryName)
    );
END;

IF OBJECT_ID(N'dbo.ConsumerSupportQueries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerSupportQueries
    (
        Id                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerSupportQueries PRIMARY KEY,
        QueryNo            NVARCHAR(30) NOT NULL,
        ConsumerUserId     INT NULL,
        ConsumerNo         NVARCHAR(20) NOT NULL,
        ConsumerName       NVARCHAR(150) NOT NULL,
        MobileNo           NVARCHAR(20) NULL,
        Email              NVARCHAR(150) NULL,
        CategoryId         INT NOT NULL,
        CategoryName       NVARCHAR(100) NOT NULL,
        Subject            NVARCHAR(200) NOT NULL,
        Description        NVARCHAR(MAX) NOT NULL,
        Priority           NVARCHAR(20) NOT NULL CONSTRAINT DF_ConsumerSupportQueries_Priority DEFAULT ('Normal'),
        Status             NVARCHAR(20) NOT NULL CONSTRAINT DF_ConsumerSupportQueries_Status DEFAULT ('Open'),
        RelatedBillNo      NVARCHAR(30) NULL,
        RelatedApplicationNo NVARCHAR(30) NULL,
        AdminRemarks       NVARCHAR(MAX) NULL,
        AssignedToUserId   INT NULL,
        ResolvedByUserId   INT NULL,
        ResolvedAt         DATETIME2(6) NULL,
        ClosedAt           DATETIME2(6) NULL,
        CreatedAt          DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerSupportQueries_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt          DATETIME2(6) NULL,
        IsActive           BIT NOT NULL CONSTRAINT DF_ConsumerSupportQueries_IsActive DEFAULT (1),
        IsDeleted          BIT NOT NULL CONSTRAINT DF_ConsumerSupportQueries_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerSupportQueries_QueryNo UNIQUE (QueryNo),
        CONSTRAINT FK_ConsumerSupportQueries_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.SupportQueryCategories (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueries_ConsumerNo' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueries'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueries_ConsumerNo ON dbo.ConsumerSupportQueries (ConsumerNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueries_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueries'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueries_Status ON dbo.ConsumerSupportQueries (Status);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueries_Category_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueries'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueries_Category_Status ON dbo.ConsumerSupportQueries (CategoryId, Status);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueries_CreatedAt' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueries'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueries_CreatedAt ON dbo.ConsumerSupportQueries (CreatedAt);
END;

IF OBJECT_ID(N'dbo.ConsumerSupportQueryDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerSupportQueryDocuments
    (
        Id                        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerSupportQueryDocuments PRIMARY KEY,
        QueryId                   BIGINT NOT NULL,
        DocumentType              NVARCHAR(50) NOT NULL CONSTRAINT DF_ConsumerSupportQueryDocuments_DocumentType DEFAULT ('Support Document'),
        FileName                  NVARCHAR(200) NOT NULL,
        FilePath                  NVARCHAR(500) NOT NULL,
        ContentType               NVARCHAR(100) NULL,
        FileSize                  BIGINT NULL,
        UploadedByConsumerUserId  INT NULL,
        UploadedAt                DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerSupportQueryDocuments_UploadedAt DEFAULT (SYSDATETIME()),
        IsDeleted                 BIT NOT NULL CONSTRAINT DF_ConsumerSupportQueryDocuments_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerSupportQueryDocuments_Query FOREIGN KEY (QueryId) REFERENCES dbo.ConsumerSupportQueries (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueryDocuments_QueryId' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueryDocuments'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueryDocuments_QueryId ON dbo.ConsumerSupportQueryDocuments (QueryId);
END;

IF OBJECT_ID(N'dbo.ConsumerSupportQueryHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerSupportQueryHistories
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerSupportQueryHistories PRIMARY KEY,
        QueryId         BIGINT NOT NULL,
        FromStatus      NVARCHAR(20) NULL,
        ToStatus        NVARCHAR(20) NOT NULL,
        [Action]        NVARCHAR(50) NOT NULL,
        Remarks         NVARCHAR(MAX) NULL,
        ActionByUserId  INT NULL,
        ActionByName    NVARCHAR(100) NULL,
        ActionByRole    NVARCHAR(100) NULL,
        ActionAt        DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerSupportQueryHistories_ActionAt DEFAULT (SYSDATETIME()),
        IsDeleted       BIT NOT NULL CONSTRAINT DF_ConsumerSupportQueryHistories_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerSupportQueryHistories_Query FOREIGN KEY (QueryId) REFERENCES dbo.ConsumerSupportQueries (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerSupportQueryHistories_Query_ActionAt' AND object_id = OBJECT_ID(N'dbo.ConsumerSupportQueryHistories'))
BEGIN
    CREATE INDEX IX_ConsumerSupportQueryHistories_Query_ActionAt ON dbo.ConsumerSupportQueryHistories (QueryId, ActionAt);
END;

/* ============================================================
   5) ADJUSTMENTS / COMPLAINTS / DISCONNECTION / NOTICE / METER READING MODULES
   ============================================================ */

IF OBJECT_ID(N'dbo.ConsumerAccountAdjustments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerAccountAdjustments
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerAccountAdjustments PRIMARY KEY,
        AdjustmentNo           NVARCHAR(30) NOT NULL,
        ConsumerNo             NVARCHAR(20) NOT NULL,
        AdjustmentType         NVARCHAR(30) NOT NULL,
        Amount                 DECIMAL(18,2) NOT NULL,
        EffectiveDate          DATETIME2(6) NOT NULL,
        SourceBillNo           NVARCHAR(30) NULL,
        SourceChallanNo        NVARCHAR(30) NULL,
        Remarks                NVARCHAR(500) NULL,
        Status                 NVARCHAR(20) NOT NULL,
        AppliedBillNo          NVARCHAR(30) NULL,
        AppliedOn              DATETIME2(6) NULL,
        ReversalOfAdjustmentId BIGINT NULL,
        CreatedByUserId        INT NULL,
        CreatedByName          NVARCHAR(100) NULL,
        CreatedAt              DATETIME2(6) NOT NULL,
        UpdatedByUserId        INT NULL,
        UpdatedByName          NVARCHAR(100) NULL,
        UpdatedAt              DATETIME2(6) NULL,
        IsActive               BIT NOT NULL CONSTRAINT DF_ConsumerAccountAdjustments_IsActive DEFAULT (1),
        IsDeleted              BIT NOT NULL CONSTRAINT DF_ConsumerAccountAdjustments_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerAccountAdjustments_AdjustmentNo UNIQUE (AdjustmentNo)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerAccountAdjustments_Consumer_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerAccountAdjustments'))
BEGIN
    CREATE INDEX IX_ConsumerAccountAdjustments_Consumer_Status
        ON dbo.ConsumerAccountAdjustments (ConsumerNo, Status, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerAccountAdjustments_Effective_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerAccountAdjustments'))
BEGIN
    CREATE INDEX IX_ConsumerAccountAdjustments_Effective_Status
        ON dbo.ConsumerAccountAdjustments (EffectiveDate, Status, IsDeleted);
END;

IF OBJECT_ID(N'dbo.ConsumerAccountAdjustmentHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerAccountAdjustmentHistories
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerAccountAdjustmentHistories PRIMARY KEY,
        AdjustmentId   BIGINT NOT NULL,
        FromStatus     NVARCHAR(20) NULL,
        ToStatus       NVARCHAR(20) NOT NULL,
        [Action]       NVARCHAR(50) NOT NULL,
        Remarks        NVARCHAR(500) NULL,
        ActionByUserId INT NULL,
        ActionByName   NVARCHAR(100) NULL,
        ActionAt       DATETIME2(6) NOT NULL,
        IsDeleted      BIT NOT NULL CONSTRAINT DF_ConsumerAccountAdjustmentHistories_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerAccountAdjustmentHistories_Adjustments
            FOREIGN KEY (AdjustmentId) REFERENCES dbo.ConsumerAccountAdjustments (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerAccountAdjustmentHistories_Adjustment' AND object_id = OBJECT_ID(N'dbo.ConsumerAccountAdjustmentHistories'))
BEGIN
    CREATE INDEX IX_ConsumerAccountAdjustmentHistories_Adjustment
        ON dbo.ConsumerAccountAdjustmentHistories (AdjustmentId, ActionAt);
END;

IF OBJECT_ID(N'dbo.ComplaintCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComplaintCategories
    (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComplaintCategories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        Description  NVARCHAR(300) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_ComplaintCategories_DisplayOrder DEFAULT (0),
        IsActive     BIT NOT NULL CONSTRAINT DF_ComplaintCategories_IsActive DEFAULT (1),
        IsDeleted    BIT NOT NULL CONSTRAINT DF_ComplaintCategories_IsDeleted DEFAULT (0),
        CreatedAt    DATETIME2(6) NOT NULL CONSTRAINT DF_ComplaintCategories_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt    DATETIME2(6) NULL,
        CONSTRAINT UX_ComplaintCategories_CategoryName UNIQUE (CategoryName)
    );
END;

IF OBJECT_ID(N'dbo.ConsumerComplaints', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerComplaints
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerComplaints PRIMARY KEY,
        ComplaintNo          NVARCHAR(30) NOT NULL,
        ConsumerUserId       INT NULL,
        ConsumerNo           NVARCHAR(20) NOT NULL,
        ConsumerName         NVARCHAR(150) NOT NULL,
        MobileNo             NVARCHAR(15) NULL,
        Email                NVARCHAR(100) NULL,
        CategoryId           INT NOT NULL,
        CategoryName         NVARCHAR(100) NOT NULL,
        Subject              NVARCHAR(150) NOT NULL,
        Description          NVARCHAR(2500) NOT NULL,
        Priority             NVARCHAR(20) NOT NULL CONSTRAINT DF_ConsumerComplaints_Priority DEFAULT ('Normal'),
        Status               NVARCHAR(30) NOT NULL CONSTRAINT DF_ConsumerComplaints_Status DEFAULT ('Open'),
        LocationDetails      NVARCHAR(500) NULL,
        RelatedBillNo        NVARCHAR(50) NULL,
        RelatedApplicationNo NVARCHAR(50) NULL,
        AdminRemarks         NVARCHAR(1000) NULL,
        AssignedToUserId     INT NULL,
        ResolvedByUserId     INT NULL,
        ResolvedAt           DATETIME2(6) NULL,
        ClosedAt             DATETIME2(6) NULL,
        CreatedAt            DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerComplaints_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt            DATETIME2(6) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_ConsumerComplaints_IsActive DEFAULT (1),
        IsDeleted            BIT NOT NULL CONSTRAINT DF_ConsumerComplaints_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerComplaints_ComplaintNo UNIQUE (ComplaintNo),
        CONSTRAINT FK_ConsumerComplaints_Category FOREIGN KEY (CategoryId) REFERENCES dbo.ComplaintCategories (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaints_ConsumerNo' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaints'))
BEGIN
    CREATE INDEX IX_ConsumerComplaints_ConsumerNo ON dbo.ConsumerComplaints (ConsumerNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaints_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaints'))
BEGIN
    CREATE INDEX IX_ConsumerComplaints_Status ON dbo.ConsumerComplaints (Status);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaints_CategoryId' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaints'))
BEGIN
    CREATE INDEX IX_ConsumerComplaints_CategoryId ON dbo.ConsumerComplaints (CategoryId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaints_CreatedAt' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaints'))
BEGIN
    CREATE INDEX IX_ConsumerComplaints_CreatedAt ON dbo.ConsumerComplaints (CreatedAt);
END;

IF OBJECT_ID(N'dbo.ConsumerComplaintDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerComplaintDocuments
    (
        Id                       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerComplaintDocuments PRIMARY KEY,
        ComplaintId              BIGINT NOT NULL,
        DocumentType             NVARCHAR(100) NOT NULL CONSTRAINT DF_ConsumerComplaintDocuments_DocumentType DEFAULT ('Complaint Document'),
        FileName                 NVARCHAR(255) NOT NULL,
        FilePath                 NVARCHAR(500) NOT NULL,
        ContentType              NVARCHAR(100) NULL,
        FileSize                 BIGINT NOT NULL CONSTRAINT DF_ConsumerComplaintDocuments_FileSize DEFAULT (0),
        UploadedByConsumerUserId INT NULL,
        UploadedAt               DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerComplaintDocuments_UploadedAt DEFAULT (SYSDATETIME()),
        IsDeleted                BIT NOT NULL CONSTRAINT DF_ConsumerComplaintDocuments_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerComplaintDocuments_Complaint
            FOREIGN KEY (ComplaintId) REFERENCES dbo.ConsumerComplaints (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaintDocuments_ComplaintId' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaintDocuments'))
BEGIN
    CREATE INDEX IX_ConsumerComplaintDocuments_ComplaintId ON dbo.ConsumerComplaintDocuments (ComplaintId);
END;

IF OBJECT_ID(N'dbo.ConsumerComplaintHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerComplaintHistories
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerComplaintHistories PRIMARY KEY,
        ComplaintId    BIGINT NOT NULL,
        FromStatus     NVARCHAR(30) NULL,
        ToStatus       NVARCHAR(30) NOT NULL,
        [Action]       NVARCHAR(50) NOT NULL,
        Remarks        NVARCHAR(1000) NULL,
        ActionByUserId INT NULL,
        ActionByName   NVARCHAR(100) NULL,
        ActionByRole   NVARCHAR(50) NULL,
        ActionAt       DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerComplaintHistories_ActionAt DEFAULT (SYSDATETIME()),
        IsDeleted      BIT NOT NULL CONSTRAINT DF_ConsumerComplaintHistories_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerComplaintHistories_Complaint
            FOREIGN KEY (ComplaintId) REFERENCES dbo.ConsumerComplaints (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerComplaintHistories_ComplaintId' AND object_id = OBJECT_ID(N'dbo.ConsumerComplaintHistories'))
BEGIN
    CREATE INDEX IX_ConsumerComplaintHistories_ComplaintId ON dbo.ConsumerComplaintHistories (ComplaintId);
END;

IF OBJECT_ID(N'dbo.ConsumerDisconnectionCases', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerDisconnectionCases
    (
        Id                         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerDisconnectionCases PRIMARY KEY,
        CaseNo                     NVARCHAR(30) NOT NULL,
        ConsumerNo                 NVARCHAR(20) NOT NULL,
        CaseType                   NVARCHAR(30) NOT NULL,
        Reason                     NVARCHAR(100) NOT NULL,
        Status                     NVARCHAR(30) NOT NULL,
        NoticeDate                 DATETIME2(6) NOT NULL,
        DueDate                    DATETIME2(6) NULL,
        OutstandingAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsumerDisconnectionCases_OutstandingAmount DEFAULT (0),
        DisconnectionFee           DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsumerDisconnectionCases_DisconnectionFee DEFAULT (0),
        ReconnectionFee            DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsumerDisconnectionCases_ReconnectionFee DEFAULT (0),
        DisconnectedOn             DATETIME2(6) NULL,
        ReconnectionRequestedOn    DATETIME2(6) NULL,
        ReconnectedOn              DATETIME2(6) NULL,
        ChallanNo                  NVARCHAR(30) NULL,
        FieldOfficerName           NVARCHAR(100) NULL,
        Remarks                    NVARCHAR(500) NULL,
        PreviousConsumerCategory   NVARCHAR(20) NULL,
        PreviousStatus             INT NULL,
        PreviousNewStatus          INT NULL,
        CreatedByUserId            INT NULL,
        CreatedByName              NVARCHAR(100) NULL,
        CreatedAt                  DATETIME2(6) NOT NULL,
        UpdatedByUserId            INT NULL,
        UpdatedByName              NVARCHAR(100) NULL,
        UpdatedAt                  DATETIME2(6) NULL,
        IsActive                   BIT NOT NULL CONSTRAINT DF_ConsumerDisconnectionCases_IsActive DEFAULT (1),
        IsDeleted                  BIT NOT NULL CONSTRAINT DF_ConsumerDisconnectionCases_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerDisconnectionCases_CaseNo UNIQUE (CaseNo)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerDisconnectionCases_Consumer_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerDisconnectionCases'))
BEGIN
    CREATE INDEX IX_ConsumerDisconnectionCases_Consumer_Status
        ON dbo.ConsumerDisconnectionCases (ConsumerNo, Status, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerDisconnectionCases_Notice_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerDisconnectionCases'))
BEGIN
    CREATE INDEX IX_ConsumerDisconnectionCases_Notice_Status
        ON dbo.ConsumerDisconnectionCases (NoticeDate, Status, IsDeleted);
END;

IF OBJECT_ID(N'dbo.ConsumerDisconnectionCaseHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerDisconnectionCaseHistories
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerDisconnectionCaseHistories PRIMARY KEY,
        CaseId         BIGINT NOT NULL,
        FromStatus     NVARCHAR(30) NULL,
        ToStatus       NVARCHAR(30) NOT NULL,
        [Action]       NVARCHAR(50) NOT NULL,
        Remarks        NVARCHAR(500) NULL,
        ActionByUserId INT NULL,
        ActionByName   NVARCHAR(100) NULL,
        ActionAt       DATETIME2(6) NOT NULL,
        IsDeleted      BIT NOT NULL CONSTRAINT DF_ConsumerDisconnectionCaseHistories_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerDisconnectionCaseHistories_Cases
            FOREIGN KEY (CaseId) REFERENCES dbo.ConsumerDisconnectionCases (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerDisconnectionCaseHistories_Case_ActionAt' AND object_id = OBJECT_ID(N'dbo.ConsumerDisconnectionCaseHistories'))
BEGIN
    CREATE INDEX IX_ConsumerDisconnectionCaseHistories_Case_ActionAt
        ON dbo.ConsumerDisconnectionCaseHistories (CaseId, ActionAt);
END;

IF OBJECT_ID(N'dbo.NoticeTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NoticeTemplates
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NoticeTemplates PRIMARY KEY,
        TemplateName  NVARCHAR(100) NOT NULL,
        NoticeType    NVARCHAR(50) NOT NULL,
        Subject       NVARCHAR(200) NOT NULL,
        Body          NVARCHAR(MAX) NOT NULL,
        DisplayOrder  INT NOT NULL CONSTRAINT DF_NoticeTemplates_DisplayOrder DEFAULT (0),
        IsActive      BIT NOT NULL CONSTRAINT DF_NoticeTemplates_IsActive DEFAULT (1),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_NoticeTemplates_IsDeleted DEFAULT (0),
        CreatedAt     DATETIME2(6) NOT NULL CONSTRAINT DF_NoticeTemplates_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt     DATETIME2(6) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NoticeTemplates_Type' AND object_id = OBJECT_ID(N'dbo.NoticeTemplates'))
BEGIN
    CREATE INDEX IX_NoticeTemplates_Type ON dbo.NoticeTemplates (NoticeType, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NoticeTemplates_Name' AND object_id = OBJECT_ID(N'dbo.NoticeTemplates'))
BEGIN
    CREATE INDEX IX_NoticeTemplates_Name ON dbo.NoticeTemplates (TemplateName, IsDeleted);
END;

IF OBJECT_ID(N'dbo.ConsumerNotices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerNotices
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerNotices PRIMARY KEY,
        NoticeNo             NVARCHAR(30) NOT NULL,
        ConsumerNo           NVARCHAR(20) NOT NULL,
        TemplateId           INT NULL,
        NoticeType           NVARCHAR(50) NOT NULL,
        Subject              NVARCHAR(200) NOT NULL,
        Body                 NVARCHAR(MAX) NOT NULL,
        NoticeDate           DATETIME2(6) NOT NULL,
        DueDate              DATETIME2(6) NULL,
        Status               NVARCHAR(30) NOT NULL,
        RelatedBillNo        NVARCHAR(30) NULL,
        RelatedChallanNo     NVARCHAR(30) NULL,
        RelatedDisconnectionCaseId BIGINT NULL,
        AmountDue            DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsumerNotices_AmountDue DEFAULT (0),
        Remarks              NVARCHAR(500) NULL,
        CreatedByUserId      INT NULL,
        CreatedByName        NVARCHAR(100) NULL,
        CreatedAt            DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerNotices_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedByUserId      INT NULL,
        UpdatedByName        NVARCHAR(100) NULL,
        UpdatedAt            DATETIME2(6) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_ConsumerNotices_IsActive DEFAULT (1),
        IsDeleted            BIT NOT NULL CONSTRAINT DF_ConsumerNotices_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerNotices_NoticeNo UNIQUE (NoticeNo),
        CONSTRAINT FK_ConsumerNotices_NoticeTemplates_TemplateId FOREIGN KEY (TemplateId) REFERENCES dbo.NoticeTemplates (Id) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerNotices_Consumer_Status' AND object_id = OBJECT_ID(N'dbo.ConsumerNotices'))
BEGIN
    CREATE INDEX IX_ConsumerNotices_Consumer_Status ON dbo.ConsumerNotices (ConsumerNo, Status, IsDeleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerNotices_Date_Type' AND object_id = OBJECT_ID(N'dbo.ConsumerNotices'))
BEGIN
    CREATE INDEX IX_ConsumerNotices_Date_Type ON dbo.ConsumerNotices (NoticeDate, NoticeType, IsDeleted);
END;

IF OBJECT_ID(N'dbo.ConsumerNoticeHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerNoticeHistories
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerNoticeHistories PRIMARY KEY,
        NoticeId        BIGINT NOT NULL,
        FromStatus      NVARCHAR(30) NULL,
        ToStatus        NVARCHAR(30) NOT NULL,
        [Action]        NVARCHAR(50) NOT NULL,
        Remarks         NVARCHAR(500) NULL,
        ActionByUserId  INT NULL,
        ActionByName    NVARCHAR(100) NULL,
        ActionAt        DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerNoticeHistories_ActionAt DEFAULT (SYSDATETIME()),
        IsDeleted       BIT NOT NULL CONSTRAINT DF_ConsumerNoticeHistories_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ConsumerNoticeHistories_Notice FOREIGN KEY (NoticeId) REFERENCES dbo.ConsumerNotices (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerNoticeHistories_Notice_ActionAt' AND object_id = OBJECT_ID(N'dbo.ConsumerNoticeHistories'))
BEGIN
    CREATE INDEX IX_ConsumerNoticeHistories_Notice_ActionAt ON dbo.ConsumerNoticeHistories (NoticeId, ActionAt);
END;

IF OBJECT_ID(N'dbo.ConsumerMeterReadings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumerMeterReadings
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsumerMeterReadings PRIMARY KEY,
        ReadingNo       NVARCHAR(30) NOT NULL,
        ConsumerNo      NVARCHAR(20) NOT NULL,
        ReadingDate     DATETIME2(6) NOT NULL,
        PeriodFrom      DATETIME2(6) NULL,
        PeriodTo        DATETIME2(6) NULL,
        PreviousReading DECIMAL(18,2) NULL,
        CurrentReading  DECIMAL(18,2) NOT NULL,
        Consumption     DECIMAL(18,2) NOT NULL,
        MeterStatus     NVARCHAR(50) NOT NULL,
        MeterNo         NVARCHAR(50) NULL,
        Remarks         NVARCHAR(MAX) NULL,
        Source          NVARCHAR(50) NOT NULL,
        RecordedByUserId INT NULL,
        RecordedByName   NVARCHAR(150) NULL,
        RecordedAt      DATETIME2(6) NOT NULL CONSTRAINT DF_ConsumerMeterReadings_RecordedAt DEFAULT (SYSDATETIME()),
        IsActive        BIT NOT NULL CONSTRAINT DF_ConsumerMeterReadings_IsActive DEFAULT (1),
        IsDeleted       BIT NOT NULL CONSTRAINT DF_ConsumerMeterReadings_IsDeleted DEFAULT (0),
        CONSTRAINT UX_ConsumerMeterReadings_ReadingNo UNIQUE (ReadingNo)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConsumerMeterReadings_ConsumerNo' AND object_id = OBJECT_ID(N'dbo.ConsumerMeterReadings'))
BEGIN
    CREATE INDEX IX_ConsumerMeterReadings_ConsumerNo ON dbo.ConsumerMeterReadings (ConsumerNo, ReadingDate);
END;

/* ============================================================
   6) WORKFLOW ENGINE TABLES
   ============================================================ */

IF OBJECT_ID(N'dbo.WorkflowMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowMasters
    (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowMasters PRIMARY KEY,
        WorkflowName   NVARCHAR(100) NOT NULL,
        ApplicationType NVARCHAR(50) NOT NULL,
        IsActive       BIT NOT NULL CONSTRAINT DF_WorkflowMasters_IsActive DEFAULT (1),
        IsDeleted      BIT NOT NULL CONSTRAINT DF_WorkflowMasters_IsDeleted DEFAULT (0),
        CreatedOn      DATETIME2(6) NOT NULL CONSTRAINT DF_WorkflowMasters_CreatedOn DEFAULT (SYSDATETIME()),
        UpdatedOn      DATETIME2(6) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowMasters_ApplicationType_Active' AND object_id = OBJECT_ID(N'dbo.WorkflowMasters'))
BEGIN
    CREATE INDEX IX_WorkflowMasters_ApplicationType_Active ON dbo.WorkflowMasters (ApplicationType, IsActive);
END;

IF OBJECT_ID(N'dbo.WorkflowStages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowStages
    (
        Id                        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowStages PRIMARY KEY,
        WorkflowId                INT NOT NULL,
        StageName                 NVARCHAR(100) NOT NULL,
        StageOrder                INT NOT NULL,
        DepartmentId              INT NULL,
        ApproverRoleId            INT NULL,
        ApproverUserId            INT NULL,
        ApprovalType              NVARCHAR(30) NOT NULL CONSTRAINT DF_WorkflowStages_ApprovalType DEFAULT ('DepartmentRole'),
        CanApprove                BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanApprove DEFAULT (1),
        CanReject                 BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanReject DEFAULT (1),
        CanSendCorrection         BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanSendCorrection DEFAULT (1),
        CanForward                BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanForward DEFAULT (0),
        IsFinalStage              BIT NOT NULL CONSTRAINT DF_WorkflowStages_IsFinalStage DEFAULT (0),
        SlaDays                   INT NULL,
        IsActive                  BIT NOT NULL CONSTRAINT DF_WorkflowStages_IsActive DEFAULT (1),
        IsDeleted                 BIT NOT NULL CONSTRAINT DF_WorkflowStages_IsDeleted DEFAULT (0),
        CreatedOn                 DATETIME2(6) NOT NULL CONSTRAINT DF_WorkflowStages_CreatedOn DEFAULT (SYSDATETIME()),
        UpdatedOn                 DATETIME2(6) NULL,
        CanForwardToUser          BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanForwardToUser DEFAULT (0),
        CanSendBackToApplicant    BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanSendBackToApplicant DEFAULT (0),
        CanSendBackToPrevious     BIT NOT NULL CONSTRAINT DF_WorkflowStages_CanSendBackToPrevious DEFAULT (0),
        CONSTRAINT FK_WorkflowStages_WorkflowMasters_WorkflowId FOREIGN KEY (WorkflowId) REFERENCES dbo.WorkflowMasters (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowStages_Workflow_Order' AND object_id = OBJECT_ID(N'dbo.WorkflowStages'))
BEGIN
    CREATE INDEX IX_WorkflowStages_Workflow_Order ON dbo.WorkflowStages (WorkflowId, StageOrder);
END;

IF OBJECT_ID(N'dbo.WorkflowStageNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkflowStageNotifications
    (
        Id                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkflowStageNotifications PRIMARY KEY,
        WorkflowStageId        INT NOT NULL,
        EventType              NVARCHAR(50) NOT NULL,
        SendEmail              BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_SendEmail DEFAULT (0),
        SendSms                BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_SendSms DEFAULT (0),
        SendWhatsApp           BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_SendWhatsApp DEFAULT (0),
        SendInAppNotification  BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_SendInAppNotification DEFAULT (0),
        TemplateId             INT NULL,
        IsActive               BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_IsActive DEFAULT (1),
        IsDeleted              BIT NOT NULL CONSTRAINT DF_WorkflowStageNotifications_IsDeleted DEFAULT (0),
        CONSTRAINT FK_WorkflowStageNotifications_WorkflowStages_WorkflowStageId
            FOREIGN KEY (WorkflowStageId) REFERENCES dbo.WorkflowStages (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowStageNotifications_Stage_Event' AND object_id = OBJECT_ID(N'dbo.WorkflowStageNotifications'))
BEGIN
    CREATE INDEX IX_WorkflowStageNotifications_Stage_Event ON dbo.WorkflowStageNotifications (WorkflowStageId, EventType);
END;

IF OBJECT_ID(N'dbo.ApplicationWorkflowInstances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationWorkflowInstances
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationWorkflowInstances PRIMARY KEY,
        ApplicationId    BIGINT NOT NULL,
        ApplicationNo    NVARCHAR(30) NOT NULL,
        ApplicationType  NVARCHAR(50) NOT NULL,
        WorkflowId       INT NOT NULL,
        CurrentStageId   INT NULL,
        CurrentStatusCode INT NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_CurrentStatusCode DEFAULT (1),
        CurrentStatus    NVARCHAR(50) NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_CurrentStatus DEFAULT ('Pending'),
        StartedOn        DATETIME2(6) NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_StartedOn DEFAULT (SYSDATETIME()),
        CompletedOn      DATETIME2(6) NULL,
        IsActive         BIT NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_IsActive DEFAULT (1),
        IsDeleted        BIT NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ApplicationWorkflowInstances_WorkflowMasters_WorkflowId FOREIGN KEY (WorkflowId) REFERENCES dbo.WorkflowMasters (Id),
        CONSTRAINT FK_ApplicationWorkflowInstances_WorkflowStages_CurrentStageId FOREIGN KEY (CurrentStageId) REFERENCES dbo.WorkflowStages (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowInstances_Application' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowInstances'))
BEGIN
    CREATE INDEX IX_WorkflowInstances_Application ON dbo.ApplicationWorkflowInstances (ApplicationType, ApplicationId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowInstances_ApplicationNo' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowInstances'))
BEGIN
    CREATE INDEX IX_WorkflowInstances_ApplicationNo ON dbo.ApplicationWorkflowInstances (ApplicationNo);
END;

IF OBJECT_ID(N'dbo.ApplicationWorkflowTasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationWorkflowTasks
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationWorkflowTasks PRIMARY KEY,
        WorkflowInstanceId    BIGINT NOT NULL,
        ApplicationId         BIGINT NOT NULL,
        ApplicationNo         NVARCHAR(30) NOT NULL,
        StageId               INT NOT NULL,
        AssignedDepartmentId  INT NULL,
        AssignedRoleId        INT NULL,
        AssignedUserId        INT NULL,
        StatusCode            INT NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_StatusCode DEFAULT (1),
        Status                NVARCHAR(30) NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_Status DEFAULT ('Pending'),
        AssignedOn            DATETIME2(6) NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_AssignedOn DEFAULT (SYSDATETIME()),
        ActionOn              DATETIME2(6) NULL,
        Remarks               NVARCHAR(500) NULL,
        IsActive              BIT NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_IsActive DEFAULT (1),
        IsDeleted             BIT NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ApplicationWorkflowTasks_WorkflowInstances_WorkflowInstanceId FOREIGN KEY (WorkflowInstanceId) REFERENCES dbo.ApplicationWorkflowInstances (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ApplicationWorkflowTasks_WorkflowStages_StageId FOREIGN KEY (StageId) REFERENCES dbo.WorkflowStages (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowTasks_Instance' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowTasks'))
BEGIN
    CREATE INDEX IX_WorkflowTasks_Instance ON dbo.ApplicationWorkflowTasks (WorkflowInstanceId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowTasks_AssignmentCode' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowTasks'))
BEGIN
    CREATE INDEX IX_WorkflowTasks_AssignmentCode
    ON dbo.ApplicationWorkflowTasks (StatusCode, AssignedRoleId, AssignedUserId, AssignedDepartmentId);
END;

IF OBJECT_ID(N'dbo.ApplicationWorkflowHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationWorkflowHistory
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationWorkflowHistory PRIMARY KEY,
        WorkflowInstanceId BIGINT NOT NULL,
        ApplicationId    BIGINT NOT NULL,
        ApplicationNo    NVARCHAR(30) NOT NULL,
        StageId          INT NULL,
        FromStatusCode   INT NULL,
        FromStatus       NVARCHAR(30) NULL,
        ToStatusCode     INT NOT NULL CONSTRAINT DF_ApplicationWorkflowHistory_ToStatusCode DEFAULT (1),
        ToStatus         NVARCHAR(30) NOT NULL,
        ActionCode       INT NOT NULL CONSTRAINT DF_ApplicationWorkflowHistory_ActionCode DEFAULT (1),
        Action           NVARCHAR(50) NOT NULL,
        Remarks          NVARCHAR(500) NULL,
        ActionBy         INT NULL,
        ActionByName     NVARCHAR(150) NULL,
        ActionByRole     NVARCHAR(50) NULL,
        ActionOn         DATETIME2(6) NOT NULL CONSTRAINT DF_ApplicationWorkflowHistory_ActionOn DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_ApplicationWorkflowHistory_WorkflowInstances_WorkflowInstanceId FOREIGN KEY (WorkflowInstanceId) REFERENCES dbo.ApplicationWorkflowInstances (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ApplicationWorkflowHistory_WorkflowStages_StageId FOREIGN KEY (StageId) REFERENCES dbo.WorkflowStages (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowHistory_Instance' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowHistory'))
BEGIN
    CREATE INDEX IX_WorkflowHistory_Instance ON dbo.ApplicationWorkflowHistory (WorkflowInstanceId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WorkflowHistory_ApplicationNo' AND object_id = OBJECT_ID(N'dbo.ApplicationWorkflowHistory'))
BEGIN
    CREATE INDEX IX_WorkflowHistory_ApplicationNo ON dbo.ApplicationWorkflowHistory (ApplicationNo);
END;

/* Additive workflow columns for older tables */
IF OBJECT_ID(N'dbo.WorkflowStages', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.WorkflowStages', N'CanForwardToUser') IS NULL
    ALTER TABLE dbo.WorkflowStages ADD CanForwardToUser BIT NOT NULL CONSTRAINT DF_WorkflowStages_Add_CanForwardToUser DEFAULT (0);

IF OBJECT_ID(N'dbo.WorkflowStages', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.WorkflowStages', N'CanSendBackToApplicant') IS NULL
    ALTER TABLE dbo.WorkflowStages ADD CanSendBackToApplicant BIT NOT NULL CONSTRAINT DF_WorkflowStages_Add_CanSendBackToApplicant DEFAULT (0);

IF OBJECT_ID(N'dbo.WorkflowStages', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.WorkflowStages', N'CanSendBackToPrevious') IS NULL
    ALTER TABLE dbo.WorkflowStages ADD CanSendBackToPrevious BIT NOT NULL CONSTRAINT DF_WorkflowStages_Add_CanSendBackToPrevious DEFAULT (0);

IF OBJECT_ID(N'dbo.ApplicationWorkflowTasks', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationWorkflowTasks', N'StatusCode') IS NULL
    ALTER TABLE dbo.ApplicationWorkflowTasks ADD StatusCode INT NOT NULL CONSTRAINT DF_ApplicationWorkflowTasks_Add_StatusCode DEFAULT (1);

IF OBJECT_ID(N'dbo.ApplicationWorkflowInstances', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationWorkflowInstances', N'CurrentStatusCode') IS NULL
    ALTER TABLE dbo.ApplicationWorkflowInstances ADD CurrentStatusCode INT NOT NULL CONSTRAINT DF_ApplicationWorkflowInstances_Add_CurrentStatusCode DEFAULT (1);

IF OBJECT_ID(N'dbo.ApplicationWorkflowHistory', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationWorkflowHistory', N'FromStatusCode') IS NULL
    ALTER TABLE dbo.ApplicationWorkflowHistory ADD FromStatusCode INT NULL;

IF OBJECT_ID(N'dbo.ApplicationWorkflowHistory', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationWorkflowHistory', N'ToStatusCode') IS NULL
    ALTER TABLE dbo.ApplicationWorkflowHistory ADD ToStatusCode INT NOT NULL CONSTRAINT DF_ApplicationWorkflowHistory_Add_ToStatusCode DEFAULT (1);

IF OBJECT_ID(N'dbo.ApplicationWorkflowHistory', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.ApplicationWorkflowHistory', N'ActionCode') IS NULL
    ALTER TABLE dbo.ApplicationWorkflowHistory ADD ActionCode INT NOT NULL CONSTRAINT DF_ApplicationWorkflowHistory_Add_ActionCode DEFAULT (1);

/* ============================================================
   7) NEW CONNECTION APPLICATION
   ============================================================ */

IF OBJECT_ID(N'dbo.public_new_connection_otp_verifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.public_new_connection_otp_verifications
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_public_new_connection_otp_verifications PRIMARY KEY,
        MobileNumber NVARCHAR(12) NOT NULL,
        OtpHash      NVARCHAR(128) NOT NULL,
        OtpSalt      NVARCHAR(64) NOT NULL,
        Purpose      NVARCHAR(50) NOT NULL,
        ExpiresAt    DATETIME NOT NULL,
        CreatedAt    DATETIME NOT NULL CONSTRAINT DF_public_new_connection_otp_verifications_CreatedAt DEFAULT (GETDATE()),
        VerifiedAt   DATETIME NULL,
        AttemptCount INT NOT NULL CONSTRAINT DF_public_new_connection_otp_verifications_AttemptCount DEFAULT (0),
        IsVerified   BIT NOT NULL CONSTRAINT DF_public_new_connection_otp_verifications_IsVerified DEFAULT (0),
        IsActive     BIT NOT NULL CONSTRAINT DF_public_new_connection_otp_verifications_IsActive DEFAULT (1),
        IsDeleted    BIT NOT NULL CONSTRAINT DF_public_new_connection_otp_verifications_IsDeleted DEFAULT (0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PublicNewConnectionOtp_Mobile_Purpose_Active' AND object_id = OBJECT_ID(N'dbo.public_new_connection_otp_verifications'))
BEGIN
    CREATE INDEX IX_PublicNewConnectionOtp_Mobile_Purpose_Active
        ON dbo.public_new_connection_otp_verifications (MobileNumber, Purpose, IsActive);
END;

IF OBJECT_ID(N'dbo.NewConnectionFeeConfigurations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NewConnectionFeeConfigurations
    (
        Id                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewConnectionFeeConfigurations PRIMARY KEY,
        ConnectionCategory   NVARCHAR(20) NULL,
        ConnectionType       NVARCHAR(50) NULL,
        PipeSize             DECIMAL(8,2) NULL,
        PlotSizeFrom         DECIMAL(12,2) NULL,
        PlotSizeTo           DECIMAL(12,2) NULL,
        ApplicationFee       DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_ApplicationFee DEFAULT (0),
        ProcessingFee        DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_ProcessingFee DEFAULT (0),
        SecurityAmount       DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_SecurityAmount DEFAULT (0),
        MeterInstallationFee DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_MeterInstallationFee DEFAULT (0),
        OtherCharges         DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_OtherCharges DEFAULT (0),
        TotalAmount          DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_TotalAmount DEFAULT (0),
        EffectiveFrom        DATETIME NOT NULL,
        EffectiveTo          DATETIME NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_IsActive DEFAULT (1),
        IsDeleted            BIT NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_IsDeleted DEFAULT (0),
        CreatedOn            DATETIME NOT NULL CONSTRAINT DF_NewConnectionFeeConfigurations_CreatedOn DEFAULT (GETDATE()),
        UpdatedOn            DATETIME NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionFeeConfigurations_Lookup' AND object_id = OBJECT_ID(N'dbo.NewConnectionFeeConfigurations'))
BEGIN
    CREATE INDEX IX_NewConnectionFeeConfigurations_Lookup
        ON dbo.NewConnectionFeeConfigurations (ConnectionCategory, ConnectionType, PipeSize, IsActive);
END;

IF OBJECT_ID(N'dbo.new_connection_applications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.new_connection_applications
    (
        Id                       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_new_connection_applications PRIMARY KEY,
        ApplicationNo            NVARCHAR(30) NOT NULL,
        ApplicationStatus        NVARCHAR(30) NOT NULL CONSTRAINT DF_new_connection_applications_ApplicationStatus DEFAULT ('Draft'),
        FinalConsumerNo          NVARCHAR(10) NULL,
        IsPublicApplication      BIT NOT NULL CONSTRAINT DF_new_connection_applications_IsPublicApplication DEFAULT (0),
        ApplicantName            NVARCHAR(100) NOT NULL,
        FatherName               NVARCHAR(100) NULL,
        MobileNumber             NVARCHAR(12) NOT NULL,
        EmailId                  NVARCHAR(50) NULL,
        [Address]                NVARCHAR(150) NOT NULL,
        Sector                   NVARCHAR(10) NOT NULL,
        Block                    NVARCHAR(10) NOT NULL,
        FlatNo                   NVARCHAR(15) NOT NULL,
        PlotSize                 DECIMAL(8,2) NOT NULL,
        PipeSize                 DECIMAL(8,2) NULL,
        KhasraNo                 NVARCHAR(20) NULL,
        VillageName              NVARCHAR(100) NULL,
        VillageId                INT NULL,
        ConnectionCategory       NVARCHAR(4) NOT NULL,
        ConnectionType           NVARCHAR(10) NOT NULL,
        FlatType                 NVARCHAR(50) NOT NULL,
        PurposeOfConnection      NVARCHAR(50) NULL,
        PreviousConnectionYesNo  NVARCHAR(1) NULL,
        OtherConnection          NVARCHAR(150) NULL,
        Rid                      NVARCHAR(15) NULL,
        DevType                  INT NULL,
        RegNo                    NVARCHAR(10) NULL,
        ConnectionDate           DATETIME2(6) NULL,
        EstimationNo             NVARCHAR(10) NULL,
        EstimationAmount         DECIMAL(12,2) NULL,
        SecurityAmount           DECIMAL(12,2) NULL,
        EstimationDate           DATETIME2(6) NULL,
        CessAmount               DECIMAL(12,2) NULL,
        MonthlyCharges           DECIMAL(12,2) NULL,
        IssueOfficer             NVARCHAR(50) NULL,
        AllotmentDate            DATE NULL,
        PossessionDate           DATE NULL,
        ComplianceDate           DATE NULL,
        SsiDate                  DATE NULL,
        AffidavitYn              NVARCHAR(2) NULL,
        SubmittedByConsumerNo    NVARCHAR(10) NULL,
        SubmittedByConsumerUserId INT NULL,
        SubmittedOn              DATETIME2(6) NULL,
        CreatedBy                INT NULL,
        CreatedOn                DATETIME2(6) NOT NULL CONSTRAINT DF_new_connection_applications_CreatedOn DEFAULT (SYSDATETIME()),
        UpdatedBy                INT NULL,
        UpdatedOn                DATETIME2(6) NULL,
        ApprovedBy               INT NULL,
        ApprovedOn               DATETIME2(6) NULL,
        RejectedBy               INT NULL,
        RejectedOn               DATETIME2(6) NULL,
        RejectionReason          NVARCHAR(500) NULL,
        Remarks                  NVARCHAR(500) NULL,
        DeclarationAccepted      BIT NOT NULL CONSTRAINT DF_new_connection_applications_DeclarationAccepted DEFAULT (0),
        IsActive                 BIT NOT NULL CONSTRAINT DF_new_connection_applications_IsActive DEFAULT (1),
        IsDeleted                BIT NOT NULL CONSTRAINT DF_new_connection_applications_IsDeleted DEFAULT (0),
        CONSTRAINT UX_new_connection_applications_ApplicationNo UNIQUE (ApplicationNo)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_applications_MobileNumber' AND object_id = OBJECT_ID(N'dbo.new_connection_applications'))
BEGIN
    CREATE INDEX IX_new_connection_applications_MobileNumber ON dbo.new_connection_applications (MobileNumber);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_applications_ApplicationStatus' AND object_id = OBJECT_ID(N'dbo.new_connection_applications'))
BEGIN
    CREATE INDEX IX_new_connection_applications_ApplicationStatus ON dbo.new_connection_applications (ApplicationStatus);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_applications_SubmittedByConsumerNo' AND object_id = OBJECT_ID(N'dbo.new_connection_applications'))
BEGIN
    CREATE INDEX IX_new_connection_applications_SubmittedByConsumerNo ON dbo.new_connection_applications (SubmittedByConsumerNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_applications_SubmittedByConsumerUserId' AND object_id = OBJECT_ID(N'dbo.new_connection_applications'))
BEGIN
    CREATE INDEX IX_new_connection_applications_SubmittedByConsumerUserId ON dbo.new_connection_applications (SubmittedByConsumerUserId);
END;

IF OBJECT_ID(N'dbo.new_connection_applications', N'U') IS NOT NULL
AND EXISTS
(
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'new_connection_applications'
      AND COLUMN_NAME = 'ConnectionCategory'
      AND CHARACTER_MAXIMUM_LENGTH IS NOT NULL
      AND CHARACTER_MAXIMUM_LENGTH < 4
)
BEGIN
    ALTER TABLE dbo.new_connection_applications
    ALTER COLUMN ConnectionCategory NVARCHAR(4) NOT NULL;
END;

IF OBJECT_ID(N'dbo.new_connection_applications', N'U') IS NOT NULL
AND EXISTS
(
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'new_connection_applications'
      AND COLUMN_NAME = 'FlatType'
      AND CHARACTER_MAXIMUM_LENGTH IS NOT NULL
      AND CHARACTER_MAXIMUM_LENGTH < 50
)
BEGIN
    ALTER TABLE dbo.new_connection_applications
    ALTER COLUMN FlatType NVARCHAR(50) NOT NULL;
END;

IF OBJECT_ID(N'dbo.NewConnectionApplicationFees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NewConnectionApplicationFees
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewConnectionApplicationFees PRIMARY KEY,
        ApplicationId        BIGINT NOT NULL,
        ApplicationNo        NVARCHAR(30) NOT NULL,
        FeeConfigurationId   INT NOT NULL,
        ApplicationFee       DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_ApplicationFee DEFAULT (0),
        ProcessingFee        DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_ProcessingFee DEFAULT (0),
        SecurityAmount       DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_SecurityAmount DEFAULT (0),
        MeterInstallationFee DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_MeterInstallationFee DEFAULT (0),
        OtherCharges         DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_OtherCharges DEFAULT (0),
        TotalAmount          DECIMAL(12,2) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_TotalAmount DEFAULT (0),
        PaymentStatus        NVARCHAR(30) NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_PaymentStatus DEFAULT ('Pending'),
        CreatedOn            DATETIME NOT NULL CONSTRAINT DF_NewConnectionApplicationFees_CreatedOn DEFAULT (GETDATE()),
        UpdatedOn            DATETIME NULL,
        CONSTRAINT FK_NewConnectionApplicationFees_Applications
            FOREIGN KEY (ApplicationId) REFERENCES dbo.new_connection_applications (Id) ON DELETE CASCADE,
        CONSTRAINT FK_NewConnectionApplicationFees_FeeConfigurations
            FOREIGN KEY (FeeConfigurationId) REFERENCES dbo.NewConnectionFeeConfigurations (Id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionApplicationFees_ApplicationId' AND object_id = OBJECT_ID(N'dbo.NewConnectionApplicationFees'))
BEGIN
    CREATE UNIQUE INDEX IX_NewConnectionApplicationFees_ApplicationId
        ON dbo.NewConnectionApplicationFees (ApplicationId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionApplicationFees_ApplicationNo' AND object_id = OBJECT_ID(N'dbo.NewConnectionApplicationFees'))
BEGIN
    CREATE INDEX IX_NewConnectionApplicationFees_ApplicationNo
        ON dbo.NewConnectionApplicationFees (ApplicationNo);
END;

IF OBJECT_ID(N'dbo.new_connection_application_documents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.new_connection_application_documents
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_new_connection_application_documents PRIMARY KEY,
        ApplicationId BIGINT NOT NULL,
        DocumentType  NVARCHAR(50) NOT NULL,
        DocumentNo    NVARCHAR(100) NULL,
        DocumentDate  DATE NULL,
        FileName      NVARCHAR(200) NULL,
        FilePath      NVARCHAR(500) NULL,
        ContentType   NVARCHAR(100) NULL,
        FileSize      BIGINT NULL,
        UploadedBy    INT NULL,
        UploadedOn    DATETIME2(6) NOT NULL CONSTRAINT DF_new_connection_application_documents_UploadedOn DEFAULT (SYSDATETIME()),
        IsActive      BIT NOT NULL CONSTRAINT DF_new_connection_application_documents_IsActive DEFAULT (1),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_new_connection_application_documents_IsDeleted DEFAULT (0),
        CONSTRAINT FK_new_connection_application_documents_Applications
            FOREIGN KEY (ApplicationId) REFERENCES dbo.new_connection_applications (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_application_documents_ApplicationId' AND object_id = OBJECT_ID(N'dbo.new_connection_application_documents'))
BEGIN
    CREATE INDEX IX_new_connection_application_documents_ApplicationId ON dbo.new_connection_application_documents (ApplicationId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_new_connection_application_documents_DocumentType' AND object_id = OBJECT_ID(N'dbo.new_connection_application_documents'))
BEGIN
    CREATE INDEX IX_new_connection_application_documents_DocumentType ON dbo.new_connection_application_documents (DocumentType);
END;

/* ============================================================
   7A) LEGACY MASTER TABLE KEY-COMPATIBILITY
   Source: database/master-key-mapping-updates.sql
   ============================================================ */

IF OBJECT_ID(N'dbo.master_dept_details', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.master_dept_details', N'Id') IS NULL
BEGIN
    ALTER TABLE dbo.master_dept_details
        ADD Id INT IDENTITY(1,1) NOT NULL;
END;

IF OBJECT_ID(N'dbo.master_dept_details', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.master_dept_details', N'Id') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_master_dept_details_Id'
          AND object_id = OBJECT_ID(N'dbo.master_dept_details')
   )
BEGIN
    CREATE UNIQUE INDEX UX_master_dept_details_Id
        ON dbo.master_dept_details (Id);
END;

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUsers', N'DeptId') IS NULL
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD DeptId INT NULL;
END;

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUsers', N'DeptId') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_AppUsers_DeptId'
          AND object_id = OBJECT_ID(N'dbo.AppUsers')
   )
BEGIN
    CREATE INDEX IX_AppUsers_DeptId
        ON dbo.AppUsers (DeptId);
END;

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.master_dept_details', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUsers', N'DeptId') IS NOT NULL
   AND COL_LENGTH(N'dbo.master_dept_details', N'Id') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_AppUsers_MasterDeptDetails_DeptId'
   )
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD CONSTRAINT FK_AppUsers_MasterDeptDetails_DeptId
        FOREIGN KEY (DeptId) REFERENCES dbo.master_dept_details (Id);
END;

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AuthorityUserDepartments', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.AppUsers', N'DeptId') IS NOT NULL
BEGIN
    ;WITH SingleDept AS
    (
        SELECT
            aud.UserId,
            MIN(aud.DepartmentId) AS DepartmentId
        FROM dbo.AuthorityUserDepartments aud
        WHERE aud.IsActive = 1
          AND aud.IsDeleted = 0
        GROUP BY aud.UserId
        HAVING COUNT(*) = 1
    )
    UPDATE au
    SET au.DeptId = sd.DepartmentId
    FROM dbo.AppUsers au
    INNER JOIN SingleDept sd
        ON sd.UserId = au.Id
    WHERE au.DeptId IS NULL;
END;

IF OBJECT_ID(N'dbo.jal_bank_master', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.jal_bank_master', N'Id') IS NULL
BEGIN
    EXEC sp_executesql N'ALTER TABLE [dbo].[jal_bank_master] ADD [Id] INT IDENTITY(1,1) NOT NULL;';
END;

IF OBJECT_ID(N'dbo.jal_bank_master', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.jal_bank_master', N'Id') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_jal_bank_master_Id'
          AND object_id = OBJECT_ID(N'dbo.jal_bank_master')
   )
BEGIN
    EXEC sp_executesql N'CREATE UNIQUE INDEX [UX_jal_bank_master_Id] ON [dbo].[jal_bank_master] ([Id]);';
END;

IF OBJECT_ID(N'dbo.bank_master', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.bank_master', N'Id') IS NULL
BEGIN
    EXEC sp_executesql N'ALTER TABLE [dbo].[bank_master] ADD [Id] INT IDENTITY(1,1) NOT NULL;';
END;

IF OBJECT_ID(N'dbo.bank_master', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.bank_master', N'Id') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_bank_master_Id'
          AND object_id = OBJECT_ID(N'dbo.bank_master')
   )
BEGIN
    EXEC sp_executesql N'CREATE UNIQUE INDEX [UX_bank_master_Id] ON [dbo].[bank_master] ([Id]);';
END;

IF OBJECT_ID(N'dbo.NewConnectionApprovalHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NewConnectionApprovalHistory
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewConnectionApprovalHistory PRIMARY KEY,
        ApplicationId BIGINT NOT NULL,
        ApplicationNo NVARCHAR(30) NOT NULL,
        FromStatus    NVARCHAR(30) NULL,
        ToStatus      NVARCHAR(30) NOT NULL,
        [Action]      NVARCHAR(50) NOT NULL,
        Remarks       NVARCHAR(500) NULL,
        ActionBy      INT NULL,
        ActionByName  NVARCHAR(150) NULL,
        ActionByRole  NVARCHAR(50) NULL,
        ActionOn      DATETIME2(6) NOT NULL CONSTRAINT DF_NewConnectionApprovalHistory_ActionOn DEFAULT (SYSDATETIME()),
        IpAddress     NVARCHAR(50) NULL,
        UserAgent     NVARCHAR(500) NULL,
        IsActive      BIT NOT NULL CONSTRAINT DF_NewConnectionApprovalHistory_IsActive DEFAULT (1),
        IsDeleted     BIT NOT NULL CONSTRAINT DF_NewConnectionApprovalHistory_IsDeleted DEFAULT (0),
        CONSTRAINT FK_NewConnectionApprovalHistory_Applications
            FOREIGN KEY (ApplicationId) REFERENCES dbo.new_connection_applications (Id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionApprovalHistory_ApplicationId' AND object_id = OBJECT_ID(N'dbo.NewConnectionApprovalHistory'))
BEGIN
    CREATE INDEX IX_NewConnectionApprovalHistory_ApplicationId ON dbo.NewConnectionApprovalHistory (ApplicationId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionApprovalHistory_ApplicationNo' AND object_id = OBJECT_ID(N'dbo.NewConnectionApprovalHistory'))
BEGIN
    CREATE INDEX IX_NewConnectionApprovalHistory_ApplicationNo ON dbo.NewConnectionApprovalHistory (ApplicationNo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NewConnectionApprovalHistory_ActionOn' AND object_id = OBJECT_ID(N'dbo.NewConnectionApprovalHistory'))
BEGIN
    CREATE INDEX IX_NewConnectionApprovalHistory_ActionOn ON dbo.NewConnectionApprovalHistory (ActionOn);
END;

/* ============================================================
   8) WORKFLOW / STATUS SEED DATA
   ============================================================ */

MERGE dbo.AppRoles AS tgt
USING
(
    SELECT *
    FROM (VALUES
        (1, N'Admin', N'Full authority portal access', NULL, 0),
        (2, N'Junior Engineer (JE)', N'Limited authority portal access', NULL, 0),
        (3, N'Consumer', N'Consumer portal role', NULL, 0),
        (4, N'Operator', N'Operator / workflow processing user', N'{}', 0),
        (5, N'Assistant Engineer (AE)', NULL, NULL, 0),
        (6, N'Executive Engineer (EE)', NULL, NULL, 0),
        (7, N'Project Engineer (PE)', NULL, NULL, 0),
        (8, N'Test', NULL, NULL, 1)
    ) v (Id, Name, Description, Permissions, IsDeleted)
) AS src
ON tgt.Name = src.Name
WHEN MATCHED THEN
    UPDATE SET
        tgt.Description = src.Description,
        tgt.Permissions = src.Permissions,
        tgt.IsDeleted = src.IsDeleted,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Name, Description, Permissions, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.Name, src.Description, src.Permissions, SYSDATETIME(), NULL, src.IsDeleted);

MERGE dbo.AppUsers AS tgt
USING
(
    SELECT
        N'System Administrator' AS FullName,
        N'admin@waterbill.local' AS Email,
        N'admin' AS Username,
        N'7KoxPJvp8WR5itphuA1oNlQ1X530kLs/uxbAtSXpz10=' AS PasswordHash,
        CAST(1 AS BIT) AS IsActive,
        r.Id AS RoleId,
        CAST(NULL AS NVARCHAR(30)) AS PhoneNumber,
        CAST(NULL AS DATETIME2(6)) AS PasswordChangedAt,
        CAST(NULL AS DATETIME2(6)) AS LastLoginAt,
        CAST(NULL AS NVARCHAR(64)) AS LastLoginIp,
        CAST(NULL AS DATETIME2(6)) AS UpdatedAt,
        CAST(0 AS BIT) AS IsDeleted
    FROM dbo.AppRoles r
    WHERE r.Name = N'Admin'
) AS src
ON tgt.Username = src.Username
WHEN MATCHED THEN
    UPDATE SET
        tgt.FullName = src.FullName,
        tgt.Email = src.Email,
        tgt.PasswordHash = src.PasswordHash,
        tgt.IsActive = src.IsActive,
        tgt.RoleId = src.RoleId,
        tgt.PhoneNumber = src.PhoneNumber,
        tgt.IsDeleted = src.IsDeleted,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (FullName, Email, Username, PasswordHash, IsActive, RoleId, PhoneNumber, PasswordChangedAt, LastLoginAt, LastLoginIp, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.FullName, src.Email, src.Username, src.PasswordHash, src.IsActive, src.RoleId, src.PhoneNumber, src.PasswordChangedAt, src.LastLoginAt, src.LastLoginIp, SYSDATETIME(), NULL, src.IsDeleted);

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
END;

;WITH module_seed (Name, PortalScope) AS
(
    SELECT *
    FROM (VALUES
        (N'Dashboard', N'Authority'),
        (N'Consumers', N'Authority'),
        (N'Billing', N'Authority'),
        (N'Payments', N'Authority'),
        (N'Reports', N'Authority'),
        (N'Role Management', N'Authority'),
        (N'User Management', N'Authority'),
        (N'Role Permission', N'Authority'),
        (N'Menu Management', N'Authority'),
        (N'Permission Modules', N'Authority'),
        (N'Security Settings', N'Authority'),
        (N'Profile', N'Authority'),
        (N'Consumer Dashboard', N'Consumer'),
        (N'Consumer Bills', N'Consumer'),
        (N'Consumer Profile', N'Consumer'),
        (N'Consumer New Connection', N'Consumer'),
        (N'Consumer Login Management', N'Authority'),
        (N'Sector Master', N'Authority'),
        (N'Block Master', N'Authority'),
        (N'Pipe Size Master', N'Authority'),
        (N'Connection Category Master', N'Authority'),
        (N'Connection Sub-Type Master', N'Authority'),
        (N'Connection Type Master', N'Authority'),
        (N'Village Master', N'Authority'),
        (N'Document Type Master', N'Authority'),
        (N'Masters', N'Authority'),
        (N'Payment Mode Master', N'Authority'),
        (N'Payment Type Master', N'Authority'),
        (N'Bank Master', N'Authority'),
        (N'NDC Amount Master', N'Authority'),
        (N'Application Status Master', N'Authority'),
        (N'Rate Category Master', N'Authority'),
        (N'Rate Master', N'Authority'),
        (N'Department Master', N'Authority'),
        (N'Workflow Master', N'Authority'),
        (N'My Pending Applications', N'Authority'),
        (N'New Connection Fee Configuration', N'Authority'),
        (N'Consumer Query Management', N'Authority'),
        (N'Consumer Support Queries', N'Consumer'),
        (N'Bill Search & Print', N'Authority'),
        (N'Online Payment History', N'Authority'),
        (N'NDC Applications', N'Authority'),
        (N'NDC Certificate Management', N'Authority'),
        (N'Consumer NDC Applications', N'Consumer'),
        (N'Challan Management', N'Authority'),
        (N'Consumer Challans', N'Consumer'),
        (N'Bulk Bill Generation', N'Authority'),
        (N'Consumer Master Maintenance', N'Authority'),
        (N'Consumer Account Adjustments', N'Authority'),
        (N'Consumer Ledger', N'Authority'),
        (N'Meter Reading Management', N'Authority'),
        (N'Disconnection / Reconnection Management', N'Authority'),
        (N'Notice Management', N'Authority'),
        (N'Complaint Management', N'Authority'),
        (N'Consumer Complaints', N'Consumer'),
        (N'Connection Type / Category Change', N'Authority'),
        (N'Name Transfer / Mutation', N'Authority'),
        (N'Reports / MIS', N'Authority'),
        (N'Advanced Bill Revision / Reversal', N'Authority'),
        (N'User Activity Logs', N'Authority'),
        (N'Consumer Activity Logs', N'Authority'),
        (N'Error Logs', N'Authority'),
        (N'Communication Templates', N'Authority'),
        (N'Consumer Service Requests', N'Consumer'),
        (N'NotificationManagement', N'Authority')
    ) x(Name, PortalScope)
)
MERGE dbo.PermissionModules AS tgt
USING module_seed AS src
ON tgt.Name = src.Name
WHEN MATCHED THEN
    UPDATE SET tgt.IsActive = 1, tgt.IsDeleted = 0, tgt.PortalScope = src.PortalScope
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Name, PortalScope, IsActive, IsDeleted)
    VALUES (src.Name, src.PortalScope, 1, 0);

/* Admin gets full access to every module present in PermissionModules. */
MERGE dbo.RolePermissions AS tgt
USING
(
    SELECT r.Id AS RoleId, pm.Id AS ModuleId, pm.Name AS ModuleName
    FROM dbo.AppRoles r
    CROSS JOIN dbo.PermissionModules pm
    WHERE r.Name = N'Admin'
      AND r.IsDeleted = 0
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

/* Consumer role gets consumer-facing access only. */
MERGE dbo.RolePermissions AS tgt
USING
(
    SELECT r.Id AS RoleId, pm.Id AS ModuleId, pm.Name AS ModuleName
    FROM dbo.AppRoles r
    JOIN dbo.PermissionModules pm
      ON pm.Name IN
      (
        N'Consumer Dashboard', N'Consumer Bills', N'Consumer Profile', N'Consumer New Connection',
        N'Consumer Login Management', N'Consumer Challans', N'Consumer NDC Applications',
        N'Consumer Service Requests', N'Consumer Support Queries', N'Consumer Complaints'
      )
    WHERE r.Name = N'Consumer'
      AND r.IsDeleted = 0
      AND pm.IsDeleted = 0
) AS src
ON tgt.RoleId = src.RoleId AND tgt.ModuleId = src.ModuleId
WHEN MATCHED THEN
    UPDATE SET
        tgt.Module = src.ModuleName,
        tgt.CanSeeMenu = 1,
        tgt.CanView = 1,
        tgt.CanAdd = 1,
        tgt.CanEdit = 0,
        tgt.CanDelete = 0,
        tgt.CanDownload = 0,
        tgt.CanExport = 0,
        tgt.CanApprove = 0,
        tgt.CanForward = 0,
        tgt.CanPrint = 1,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, Module, ModuleId, CanSeeMenu, CanView, CanAdd, CanEdit, CanDelete, CanDownload, CanExport, CanApprove, CanForward, CanPrint, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (src.RoleId, src.ModuleName, src.ModuleId, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, SYSDATETIME(), NULL, 0);

/* ============================================================
   9) MENU SEED DATA
   ============================================================ */

;WITH parent_menu_seed AS
(
    SELECT *
    FROM (VALUES
        (1, N'Dashboard', N'fa fa-gauge', N'/Dashboard', N'Main', N'Dashboard', 1, 1, 1, 0),
        (1, N'Consumer Management', N'fa fa-users', N'#', N'Operations', N'Consumer Management', 2, 1, 1, 0),
        (1, N'Billing & Metering', N'fa fa-file-invoice', N'#', N'Operations', N'Billing & Metering', 3, 1, 1, 0),
        (1, N'Challan & Payments', N'fa fa-credit-card', N'#', N'Operations', N'Challan & Payments', 4, 1, 1, 0),
        (1, N'Reports / MIS', N'fa fa-chart-line', N'#', N'Reports', N'Reports / MIS', 5, 1, 1, 0),
        (1, N'Administration', N'fa fa-user-shield', N'#', N'Administration', NULL, 6, 1, 1, 0),
        (1, N'Masters', N'fa fa-cubes', N'#', N'Masters', NULL, 7, 1, 1, 0)
    ) v (TenantId, Label, Icon, Url, SectionLabel, ModuleName, MenuOrder, IsActive, ShowInSidebar, OpenInNewTab)
),
child_menu_seed AS
(
    SELECT *
    FROM (VALUES
        (1, N'Administration', N'Role Management', N'RM', N'/Roles', N'Administration', N'Role Management', 1, 1, 1, 0),
        (1, N'Administration', N'User Management', N'UM', N'/Users', N'Administration', N'User Management', 2, 1, 1, 0),
        (1, N'Administration', N'Role Permission', N'RP', N'/RolePermissions', N'Administration', N'Role Permission', 3, 1, 1, 0),
        (1, N'Administration', N'Menu Management', N'MN', N'/Menu', N'Administration', N'Menu Management', 4, 1, 1, 0),
        (1, N'Administration', N'Permission Modules', N'PM', N'/PermissionModules', N'Administration', N'Permission Modules', 5, 1, 1, 0),
        (1, N'Administration', N'Security Settings', N'SS', N'/SecuritySettings', N'Administration', N'Security Settings', 6, 1, 1, 0),
        (1, N'Administration', N'Workflow Master', N'WF', N'/Workflows', N'Administration', N'Workflow Master', 7, 1, 1, 0),
        (1, N'Administration', N'My Pending Applications', N'AP', N'/Approvals/Pending', N'Administration', N'My Pending Applications', 8, 1, 1, 0),
        (1, N'Administration', N'Consumer Queries', N'QY', N'/ConsumerQueryManagement', N'Administration', N'Consumer Queries', 9, 1, 1, 0),
        (1, N'Administration', N'Communication Templates', N'CT', N'/CommunicationTemplates', N'Administration', N'Communication Templates', 10, 1, 1, 0),
        (1, N'Administration', N'Communication', N'NT', N'/NotificationManagement', N'Administration', N'Communication', 11, 1, 1, 0),
        (1, N'Administration', N'User Activity Logs', N'AL', N'/UserActivityLogs', N'Administration', N'User Activity Logs', 12, 1, 1, 0),
        (1, N'Administration', N'Consumer Activity Logs', N'CL', N'/ConsumerActivityLogs', N'Administration', N'Consumer Activity Logs', 13, 1, 1, 0),
        (1, N'Administration', N'Error Logs', N'EL', N'/ErrorLogs', N'Administration', N'Error Logs', 14, 1, 1, 0),
        (1, N'Billing & Metering', N'Bill Search & Print', N'BS', N'/BillSearchPrint', N'Billing & Metering', N'Bill Search & Print', 1, 1, 1, 0),
        (1, N'Billing & Metering', N'Challan Management', N'CH', N'/ChallanManagement', N'Billing & Metering', N'Challan Management', 2, 1, 1, 0),
        (1, N'Billing & Metering', N'Bulk Bill Generation', N'BB', N'/BulkBillGeneration', N'Billing & Metering', N'Bulk Bill Generation', 3, 1, 1, 0),
        (1, N'Billing & Metering', N'Advanced Bill Revision / Reversal', N'BR', N'/BillRevision', N'Billing & Metering', N'Advanced Bill Revision / Reversal', 4, 1, 1, 0),
        (1, N'Billing & Metering', N'Consumer Ledger', N'LG', N'/ConsumerLedger', N'Billing & Metering', N'Consumer Ledger', 5, 1, 1, 0),
        (1, N'Billing & Metering', N'Consumer Account Adjustments', N'AD', N'/ConsumerAccountAdjustments', N'Billing & Metering', N'Consumer Account Adjustments', 6, 1, 1, 0),
        (1, N'Billing & Metering', N'Consumer Master Maintenance', N'CM', N'/ConsumerMasterMaintenance', N'Billing & Metering', N'Consumer Master Maintenance', 7, 1, 1, 0),
        (1, N'Billing & Metering', N'Meter Reading Management', N'MR', N'/MeterReadingManagement', N'Billing & Metering', N'Meter Reading Management', 8, 1, 1, 0),
        (1, N'Billing & Metering', N'Disconnection / Reconnection Management', N'DR', N'/DisconnectionReconnectionManagement', N'Billing & Metering', N'Disconnection / Reconnection Management', 9, 1, 1, 0),
        (1, N'Billing & Metering', N'Connection Type / Category Change', N'CC', N'/ConnectionTypeCategoryChange', N'Billing & Metering', N'Connection Type / Category Change', 10, 1, 1, 0),
        (1, N'Billing & Metering', N'Name Transfer / Mutation', N'NM', N'/NameTransferMutation', N'Billing & Metering', N'Name Transfer / Mutation', 11, 1, 1, 0),
        (1, N'Billing & Metering', N'NDC Applications', N'ND', N'/NDCApplications', N'Billing & Metering', N'NDC Applications', 12, 1, 1, 0),
        (1, N'Billing & Metering', N'NDC Certificate Management', N'NC', N'/NdcCertificateManagement', N'Billing & Metering', N'NDC Certificate Management', 13, 1, 1, 0),
        (1, N'Billing & Metering', N'Consumer NDC Applications', N'CN', N'/ConsumerNdcApplications', N'Billing & Metering', N'Consumer NDC Applications', 14, 1, 1, 0),
        (1, N'Billing & Metering', N'My Challans', N'CC', N'/ConsumerChallans', N'Billing & Metering', N'My Challans', 15, 1, 1, 0),
        (1, N'Challan & Payments', N'Online Payment History', N'PH', N'/OnlinePaymentHistory', N'Challan & Payments', N'Online Payment History', 1, 1, 1, 0),
        (1, N'Masters', N'Payment Mode Master', N'PM', N'/Masters/payment-modes', N'Masters', N'Payment Mode Master', 1, 1, 1, 0),
        (1, N'Masters', N'Payment Type Master', N'PT', N'/Masters/payment-types', N'Masters', N'Payment Type Master', 2, 1, 1, 0),
        (1, N'Masters', N'Sector Master', N'SE', N'/Masters/sectors', N'Masters', N'Sector Master', 3, 1, 1, 0),
        (1, N'Masters', N'Block Master', N'BL', N'/Masters/blocks', N'Masters', N'Block Master', 4, 1, 1, 0),
        (1, N'Masters', N'Pipe Size Master', N'PS', N'/Masters/pipe-sizes', N'Masters', N'Pipe Size Master', 5, 1, 1, 0),
        (1, N'Masters', N'Connection Category Master', N'CM', N'/Masters/connection-categories', N'Masters', N'Connection Category Master', 6, 1, 1, 0),
        (1, N'Masters', N'Connection Sub-Type Master', N'CS', N'/Masters/connection-subtypes', N'Masters', N'Connection Sub-Type Master', 7, 1, 1, 0),
        (1, N'Masters', N'Connection Type Master', N'CT', N'/Masters/connection-types', N'Masters', N'Connection Type Master', 8, 1, 1, 0),
        (1, N'Masters', N'Village Master', N'VI', N'/Masters/villages', N'Masters', N'Village Master', 9, 1, 1, 0),
        (1, N'Masters', N'Document Type Master', N'DO', N'/Masters/document-types', N'Masters', N'Document Type Master', 10, 1, 1, 0),
        (1, N'Masters', N'Bank Master', N'BK', N'/Masters/banks', N'Masters', N'Bank Master', 11, 1, 1, 0),
        (1, N'Masters', N'NDC Amount Master', N'ND', N'/Masters/ndc-amounts', N'Masters', N'NDC Amount Master', 12, 1, 1, 0),
        (1, N'Masters', N'Application Status Master', N'AS', N'/Masters/application-statuses', N'Masters', N'Application Status Master', 13, 1, 1, 0),
        (1, N'Masters', N'Rate Category Master', N'RC', N'/Masters/rate-categories', N'Masters', N'Rate Category Master', 14, 1, 1, 0),
        (1, N'Masters', N'Rate Master', N'RT', N'/Masters/rates', N'Masters', N'Rate Master', 15, 1, 1, 0),
        (1, N'Masters', N'Department Master', N'DP', N'/Masters/departments', N'Masters', N'Department Master', 16, 1, 1, 0),
        (1, N'Masters', N'New Connection Fee Configuration', N'NC', N'/Masters/new-connection-fees', N'Masters', N'New Connection Fee Configuration', 17, 1, 1, 0),
        (1, N'Consumer Management', N'Consumer Dashboard', N'CD', N'/Consumer/Dashboard', N'Consumer Management', N'Consumer Dashboard', 1, 1, 1, 0),
        (1, N'Consumer Management', N'Consumer Bills', N'CB', N'/Consumer/Bills', N'Consumer Management', N'Consumer Bills', 2, 1, 1, 0),
        (1, N'Consumer Management', N'Consumer Profile', N'CP', N'/Consumer/Profile', N'Consumer Management', N'Consumer Profile', 3, 1, 1, 0),
        (1, N'Consumer Management', N'Consumer New Connection', N'CN', N'/Consumer/NewConnection', N'Consumer Management', N'Consumer New Connection', 4, 1, 1, 0),
        (1, N'Consumer Management', N'Consumer Login Management', N'CL', N'/ConsumerLoginManagement', N'Consumer Management', N'Consumer Login Management', 5, 1, 1, 0),
        (1, N'Consumer Management', N'Support & Queries', N'SQ', N'/Consumer/SupportQueries', N'Consumer Management', N'Support & Queries', 6, 1, 1, 0),
        (1, N'Consumer Management', N'Complaints & Requests', N'CO', N'/Consumer/Complaints', N'Consumer Management', N'Complaints & Requests', 7, 1, 1, 0),
        (1, N'Consumer Management', N'Service Requests', N'SR', N'/Consumer/ServiceRequests', N'Consumer Management', N'Service Requests', 8, 1, 1, 0),
        (1, N'Consumer Management', N'My Challans', N'CC', N'/Consumer/Challans', N'Consumer Management', N'My Challans', 9, 1, 1, 0),
        (1, N'Consumer Management', N'NDC / No Dues', N'ND', N'/Consumer/NdcApplications', N'Consumer Management', N'NDC / No Dues', 10, 1, 1, 0)
    ) v (TenantId, ParentLabel, Label, Icon, Url, SectionLabel, ModuleName, MenuOrder, IsActive, ShowInSidebar, OpenInNewTab)
)
MERGE dbo.MenuItems AS tgt
USING
(
    SELECT
        s.TenantId,
        CAST(NULL AS INT) AS ParentId,
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
    FROM parent_menu_seed s
    LEFT JOIN dbo.PermissionModules pm
        ON pm.Name = s.ModuleName
       AND pm.IsDeleted = 0
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

/* ============================================================
   10) COMMUNICATION PURPOSES + DEFAULT TEMPLATES
   ============================================================ */

MERGE dbo.CommunicationPurposes AS tgt
USING
(
    SELECT *
    FROM (VALUES
        (N'ConsumerOtp', N'Consumer OTP', N'OTP for existing consumer login.', N'["ConsumerName","ConsumerNo","Otp","Date","ExpiryMinutes"]', 1, 1),
        (N'PublicNewConnectionOtp', N'Public New Connection OTP', N'OTP for public new connection verification.', N'["ConsumerName","MobileNo","Otp","Date"]', 1, 1),
        (N'NewConnectionSubmitted', N'New Connection Submitted', N'New connection application submitted.', N'["ConsumerName","ApplicationNo","Amount","Status","Date"]', 1, 1),
        (N'ApprovalStageAssigned', N'Approval Stage Assigned', N'Approval workflow stage assigned.', N'["ConsumerName","ApplicationNo","StageName","Status","Date"]', 1, 1),
        (N'QueryRaised', N'Consumer Query Raised', N'Consumer support query created.', N'["ConsumerName","ConsumerNo","QueryNo","Status","Date"]', 1, 1),
        (N'QueryResolved', N'Consumer Query Resolved', N'Consumer query status updated/resolved.', N'["ConsumerName","ConsumerNo","QueryNo","Status","Remarks","Date"]', 1, 1),
        (N'ChallanGenerated', N'Challan Generated', N'Payment request/challan generated.', N'["ConsumerName","ConsumerNo","ChallanNo","Purpose","Amount","Status","Date"]', 1, 1),
        (N'PaymentSuccess', N'Payment Success', N'Payment success confirmation.', N'["ConsumerName","ConsumerNo","ChallanNo","BillNo","Amount","Status","Date"]', 1, 1),
        (N'PaymentFailed', N'Payment Failed', N'Payment failure alert.', N'["ConsumerName","ConsumerNo","ChallanNo","BillNo","Amount","Status","Date"]', 1, 1),
        (N'NewConnectionApproved', N'New Connection Approved', N'New connection approved.', N'["ConsumerName","ApplicationNo","Status","Date"]', 1, 1),
        (N'FinalConsumerCreated', N'Final Consumer Created', N'Final consumer number generated.', N'["ConsumerName","ApplicationNo","ConsumerNo","Status","Date"]', 1, 1),
        (N'NdcSubmitted', N'NDC Submitted', N'NDC application submitted.', N'["ConsumerName","ConsumerNo","ApplicationNo","Status","Date"]', 1, 1),
        (N'NdcApproved', N'NDC Approved', N'NDC application approved.', N'["ConsumerName","ConsumerNo","ApplicationNo","Status","Date"]', 1, 1),
        (N'AdminNotification', N'Admin Notification', N'System-generated admin notifications.', N'["NotificationTitle","NotificationMessage","NotificationType","Date","UserName"]', 1, 1)
    ) v (PurposeKey, DisplayName, Description, AllowedPlaceholders, IsSystem, IsActive)
) AS src
ON tgt.PurposeKey = src.PurposeKey
WHEN MATCHED THEN
    UPDATE SET
        tgt.DisplayName = src.DisplayName,
        tgt.Description = src.Description,
        tgt.AllowedPlaceholders = src.AllowedPlaceholders,
        tgt.IsSystem = src.IsSystem,
        tgt.IsActive = src.IsActive,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PurposeKey, DisplayName, Description, AllowedPlaceholders, IsSystem, IsActive, CreatedAt, UpdatedAt)
    VALUES (src.PurposeKey, src.DisplayName, src.Description, src.AllowedPlaceholders, src.IsSystem, src.IsActive, SYSDATETIME(), NULL);

MERGE dbo.CommunicationTemplates AS tgt
USING
(
    SELECT p.Id AS PurposeId, p.PurposeKey, v.Channel, v.TemplateName, v.Subject, v.Body, NULL AS ExternalTemplateId, NULL AS Language, 1 AS IsDefault, 1 AS IsActive, 0 AS IsDeleted
    FROM dbo.CommunicationPurposes p
    JOIN (VALUES
        (N'ConsumerOtp', N'SMS',  N'Consumer Login OTP SMS', NULL, N'Your OTP for Noida Jal consumer login is {{Otp}}.'),
        (N'ConsumerOtp', N'Email', N'Consumer Login OTP Email', N'Your Noida Jal login OTP', N'Dear {{ConsumerName}},<br>Your OTP for Noida Jal consumer login is <strong>{{Otp}}</strong>. It is valid for {{ExpiryMinutes}} minutes.'),
        (N'PublicNewConnectionOtp', N'SMS', N'Public New Connection OTP', NULL, N'Your OTP to start or continue Noida water connection application is {{Otp}}.'),
        (N'NewConnectionSubmitted', N'Email', N'New Connection Submitted Email', N'Application {{ApplicationNo}} submitted', N'Dear {{ConsumerName}},<br>Your new connection application {{ApplicationNo}} has been submitted successfully. Current status: {{Status}}. Amount: Rs. {{Amount}}.'),
        (N'NewConnectionSubmitted', N'InApp', N'New Connection Submitted In-App', N'Application submitted', N'Your new connection application {{ApplicationNo}} has been submitted successfully.'),
        (N'ApprovalStageAssigned', N'InApp', N'Approval Stage Assigned In-App', N'Approval task assigned', N'Application {{ApplicationNo}} is assigned at {{StageName}} for action.'),
        (N'QueryRaised', N'InApp', N'Query Raised In-App', N'Query submitted', N'Your query {{QueryNo}} has been submitted successfully. Status: {{Status}}.'),
        (N'QueryRaised', N'Email', N'Query Raised Email', N'Query {{QueryNo}} submitted', N'Dear {{ConsumerName}},<br>Your support query {{QueryNo}} has been submitted successfully on {{Date}}.'),
        (N'QueryResolved', N'InApp', N'Query Resolved In-App', N'Query status updated', N'Your query {{QueryNo}} is now {{Status}}. Remarks: {{Remarks}}'),
        (N'QueryResolved', N'Email', N'Query Resolved Email', N'Query {{QueryNo}} status updated', N'Dear {{ConsumerName}},<br>Your query {{QueryNo}} is now {{Status}}.<br>Remarks: {{Remarks}}'),
        (N'ChallanGenerated', N'Email', N'Challan Generated Email', N'Challan {{ChallanNo}} generated', N'Dear {{ConsumerName}},<br>Challan {{ChallanNo}} for {{Purpose}} has been generated. Amount payable: Rs. {{Amount}}.'),
        (N'PaymentSuccess', N'Email', N'Payment Success Email', N'Payment successful', N'Dear {{ConsumerName}},<br>Your payment of Rs. {{Amount}} has been received successfully on {{Date}}.'),
        (N'PaymentFailed', N'Email', N'Payment Failed Email', N'Payment failed', N'Dear {{ConsumerName}},<br>Your payment of Rs. {{Amount}} failed on {{Date}}. Please try again.'),
        (N'NewConnectionApproved', N'InApp', N'New Connection Approved In-App', N'Application approved', N'Your new connection application {{ApplicationNo}} has been approved.'),
        (N'FinalConsumerCreated', N'InApp', N'Final Consumer Created In-App', N'Consumer number generated', N'Your consumer number {{ConsumerNo}} has been generated successfully.'),
        (N'NdcSubmitted', N'InApp', N'NDC Submitted In-App', N'NDC submitted', N'Your NDC application {{ApplicationNo}} has been submitted successfully.'),
        (N'NdcApproved', N'InApp', N'NDC Approved In-App', N'NDC approved', N'Your NDC application {{ApplicationNo}} has been approved.')
    ) v (PurposeKey, Channel, TemplateName, Subject, Body) ON v.PurposeKey = p.PurposeKey
) AS src
ON tgt.PurposeKey = src.PurposeKey
   AND tgt.Channel = src.Channel
   AND ISNULL(tgt.Language, N'') = ISNULL(src.Language, N'')
   AND tgt.IsDeleted = 0
WHEN MATCHED THEN
    UPDATE SET
        tgt.PurposeId = src.PurposeId,
        tgt.TemplateName = src.TemplateName,
        tgt.Subject = src.Subject,
        tgt.Body = src.Body,
        tgt.ExternalTemplateId = src.ExternalTemplateId,
        tgt.IsDefault = src.IsDefault,
        tgt.IsActive = src.IsActive,
        tgt.UpdatedAt = SYSDATETIME(),
        tgt.IsDeleted = src.IsDeleted
WHEN NOT MATCHED BY TARGET THEN
    INSERT (PurposeId, PurposeKey, Channel, TemplateName, Subject, Body, ExternalTemplateId, Language, IsDefault, IsActive, IsDeleted, CreatedAt, UpdatedAt)
    VALUES (src.PurposeId, src.PurposeKey, src.Channel, src.TemplateName, src.Subject, src.Body, src.ExternalTemplateId, src.Language, src.IsDefault, src.IsActive, src.IsDeleted, SYSDATETIME(), NULL);

/* ============================================================
   11) CONNECTION TYPE MASTER SEED
   Source aligned with MySQL new-connection master seed
   ============================================================ */

DECLARE @ConnectionTypeSchemaName SYSNAME;
DECLARE @ConnectionTypeTableName SYSNAME;
DECLARE @ConnectionTypeFullName NVARCHAR(400);

SELECT TOP (1)
    @ConnectionTypeSchemaName = s.name,
    @ConnectionTypeTableName = t.name
FROM sys.tables t
INNER JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE LOWER(t.name) = N'connection_type_mst';

IF @ConnectionTypeTableName IS NOT NULL
BEGIN
    SET @ConnectionTypeFullName = QUOTENAME(@ConnectionTypeSchemaName) + N'.' + QUOTENAME(@ConnectionTypeTableName);

    IF OBJECT_ID('tempdb..#ConnectionTypeSeed') IS NOT NULL
        DROP TABLE #ConnectionTypeSeed;

    CREATE TABLE #ConnectionTypeSeed
    (
        AUTO_ID INT NOT NULL,
        CONNECTION_NAME NVARCHAR(100) NOT NULL,
        CONNECTION_MAIN_ID NVARCHAR(50) NOT NULL,
        STATUS BIT NOT NULL,
        CREATED_ON DATETIME NULL,
        CREATED_BY INT NULL,
        LAST_UPDATED_ON DATETIME NULL,
        LAST_UPDATED_BY INT NULL
    );

    INSERT INTO #ConnectionTypeSeed
    (
        AUTO_ID,
        CONNECTION_NAME,
        CONNECTION_MAIN_ID,
        STATUS,
        CREATED_ON,
        CREATED_BY,
        LAST_UPDATED_ON,
        LAST_UPDATED_BY
    )
    VALUES
        (1, N'Regular',   N'R', 1, CAST('2026-05-20T19:05:26' AS DATETIME), NULL, NULL, NULL),
        (2, N'Temporary', N'T', 1, CAST('2026-05-20T19:05:26' AS DATETIME), NULL, NULL, NULL),
        (3, N'RMC',       N'M', 1, CAST('2026-05-20T19:05:26' AS DATETIME), NULL, NULL, NULL),
        (4, N'Staff',     N'S', 1, CAST('2026-05-20T19:05:26' AS DATETIME), NULL, CAST('2026-05-22T16:04:27' AS DATETIME), NULL);

    DECLARE @ConnectionTypeUpdateSql NVARCHAR(MAX) =
        N'UPDATE tgt
          SET
              tgt.CONNECTION_NAME = src.CONNECTION_NAME,
              tgt.CONNECTION_MAIN_ID = src.CONNECTION_MAIN_ID,
              tgt.STATUS = src.STATUS,
              tgt.CREATED_ON = COALESCE(tgt.CREATED_ON, src.CREATED_ON),
              tgt.CREATED_BY = COALESCE(tgt.CREATED_BY, src.CREATED_BY),
              tgt.LAST_UPDATED_ON = src.LAST_UPDATED_ON,
              tgt.LAST_UPDATED_BY = src.LAST_UPDATED_BY
          FROM ' + @ConnectionTypeFullName + N' AS tgt
          INNER JOIN #ConnectionTypeSeed AS src
              ON tgt.AUTO_ID = src.AUTO_ID
              OR tgt.CONNECTION_MAIN_ID = src.CONNECTION_MAIN_ID;';

    EXEC sp_executesql @ConnectionTypeUpdateSql;

    DECLARE @ConnectionTypeIdentityOnSql NVARCHAR(200) = N'';
    DECLARE @ConnectionTypeIdentityOffSql NVARCHAR(200) = N'';

    IF EXISTS
    (
        SELECT 1
        FROM sys.identity_columns ic
        INNER JOIN sys.tables t
            ON t.object_id = ic.object_id
        INNER JOIN sys.schemas s
            ON s.schema_id = t.schema_id
        WHERE LOWER(t.name) = N'connection_type_mst'
          AND s.name = @ConnectionTypeSchemaName
    )
    BEGIN
        SET @ConnectionTypeIdentityOnSql = N'SET IDENTITY_INSERT ' + @ConnectionTypeFullName + N' ON;';
        SET @ConnectionTypeIdentityOffSql = N'SET IDENTITY_INSERT ' + @ConnectionTypeFullName + N' OFF;';
        EXEC sp_executesql @ConnectionTypeIdentityOnSql;
    END;

    DECLARE @ConnectionTypeInsertSql NVARCHAR(MAX) =
        N'INSERT INTO ' + @ConnectionTypeFullName + N'
        (
            AUTO_ID,
            CONNECTION_NAME,
            CONNECTION_MAIN_ID,
            STATUS,
            CREATED_ON,
            CREATED_BY,
            LAST_UPDATED_ON,
            LAST_UPDATED_BY
        )
        SELECT
            src.AUTO_ID,
            src.CONNECTION_NAME,
            src.CONNECTION_MAIN_ID,
            src.STATUS,
            src.CREATED_ON,
            src.CREATED_BY,
            src.LAST_UPDATED_ON,
            src.LAST_UPDATED_BY
        FROM #ConnectionTypeSeed AS src
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM ' + @ConnectionTypeFullName + N' AS tgt
            WHERE tgt.AUTO_ID = src.AUTO_ID
               OR tgt.CONNECTION_MAIN_ID = src.CONNECTION_MAIN_ID
        );';

    EXEC sp_executesql @ConnectionTypeInsertSql;

    IF @ConnectionTypeIdentityOffSql <> N''
    BEGIN
        EXEC sp_executesql @ConnectionTypeIdentityOffSql;
    END;

    DROP TABLE #ConnectionTypeSeed;
END;

/* ============================================================
   12) SUPPORT QUERY CATEGORIES SEED
   ============================================================ */

MERGE dbo.SupportQueryCategories AS tgt
USING
(
    SELECT *
    FROM (VALUES
        (N'Billing Issue', N'Issue in billing or invoice amount', 1),
        (N'Payment Issue', N'Payment not reflected or payment problem', 2),
        (N'Wrong Bill Amount', N'Bill amount is incorrect', 3),
        (N'Payment Not Reflected', N'Payment received but not updated', 4),
        (N'New Connection Issue', N'Issue related to new connection application', 5),
        (N'Profile / Mobile / Email Update Issue', N'Update issue for profile fields', 6),
        (N'Login / OTP Issue', N'Login or OTP problem', 7),
        (N'NDC / No Dues Issue', N'NDC or No Due certificate related issue', 8),
        (N'Water Supply Related', N'Water supply complaint or issue', 9),
        (N'Other', N'Any other issue', 10)
    ) v (CategoryName, Description, DisplayOrder)
) AS src
ON tgt.CategoryName = src.CategoryName
WHEN MATCHED THEN
    UPDATE SET
        tgt.Description = src.Description,
        tgt.DisplayOrder = src.DisplayOrder,
        tgt.IsActive = 1,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (CategoryName, Description, DisplayOrder, IsActive, IsDeleted, CreatedAt, UpdatedAt)
    VALUES (src.CategoryName, src.Description, src.DisplayOrder, 1, 0, SYSDATETIME(), NULL);

/* ============================================================
   13) WORKFLOW MASTER / STAGE SEED
   ============================================================ */

MERGE dbo.WorkflowMasters AS tgt
USING
(
    SELECT *
    FROM (VALUES
        (N'New Connection', N'NewConnection'),
        (N'Demo NDC Approval Workflow', N'NDC'),
        (N'Name Transfer Approval', N'NameTransfer'),
        (N'Connection Change Approval', N'ConnectionChange')
    ) v (WorkflowName, ApplicationType)
) AS src
ON tgt.ApplicationType = src.ApplicationType
WHEN MATCHED THEN
    UPDATE SET
        tgt.WorkflowName = src.WorkflowName,
        tgt.IsActive = 1,
        tgt.IsDeleted = 0,
        tgt.UpdatedOn = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (WorkflowName, ApplicationType, IsActive, IsDeleted, CreatedOn, UpdatedOn)
    VALUES (src.WorkflowName, src.ApplicationType, 1, 0, SYSDATETIME(), NULL);

;WITH stage_seed AS
(
    SELECT *
    FROM (VALUES
        (N'NewConnection',  N'First Stage',      1, 2, 4, 4, N'SpecificUser', 1, 1, 0, 0, 1, 30, 1, 0, 1, 1, 1),
        (N'NewConnection',  N'Second Stage',     2, 3, 2, 5, N'SpecificUser', 1, 1, 0, 0, 1, 30, 1, 0, 1, 1, 1),
        (N'NewConnection',  N'Stage3',           3, 4, 2, 6, N'SpecificUser', 1, 1, 0, 0, 1, 30, 1, 0, 1, 1, 1),
        (N'NDC',            N'Initial Scrutiny', 1, NULL, 2, NULL, N'AnyOne',    1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1),
        (N'NDC',            N'Revenue Verification',2, NULL, 1, NULL, N'AnyOne', 1, 1, 0, 1, 0, 2, 1, 0, 1, 0, 0),
        (N'NDC',            N'Document Verification',3, NULL, 1, NULL, N'AnyOne',1, 1, 0, 1, 0, 2, 1, 0, 0, 0, 0),
        (N'NDC',            N'Senior Review',    4, NULL, 1, NULL, N'AnyOne',    1, 1, 0, 1, 0, 2, 1, 0, 0, 0, 0),
        (N'NDC',            N'Final Approval',    5, NULL, 1, NULL, N'AnyOne',    1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0),
        (N'NameTransfer',   N'Initial Review',    1, NULL, 2, NULL, N'DepartmentRole',1,1,0,0,1,3,1,0,0,0,0),
        (N'ConnectionChange',N'Initial Review',   1, NULL, 2, NULL, N'DepartmentRole',1,1,0,0,1,3,1,0,0,0,0)
    ) v (ApplicationType, StageName, StageOrder, DepartmentId, ApproverRoleId, ApproverUserId, ApprovalType,
         CanApprove, CanReject, CanSendCorrection, CanForward, IsFinalStage, SlaDays, IsActive, IsDeleted,
         CanForwardToUser, CanSendBackToApplicant, CanSendBackToPrevious)
)
MERGE dbo.WorkflowStages AS tgt
USING
(
    SELECT wm.Id AS WorkflowId, s.*
    FROM stage_seed s
    JOIN dbo.WorkflowMasters wm ON wm.ApplicationType = s.ApplicationType
) AS src
ON tgt.WorkflowId = src.WorkflowId
   AND tgt.StageOrder = src.StageOrder
   AND tgt.StageName = src.StageName
WHEN MATCHED THEN
    UPDATE SET
        tgt.DepartmentId = src.DepartmentId,
        tgt.ApproverRoleId = src.ApproverRoleId,
        tgt.ApproverUserId = src.ApproverUserId,
        tgt.ApprovalType = src.ApprovalType,
        tgt.CanApprove = src.CanApprove,
        tgt.CanReject = src.CanReject,
        tgt.CanSendCorrection = src.CanSendCorrection,
        tgt.CanForward = src.CanForward,
        tgt.IsFinalStage = src.IsFinalStage,
        tgt.SlaDays = src.SlaDays,
        tgt.IsActive = src.IsActive,
        tgt.IsDeleted = src.IsDeleted,
        tgt.CanForwardToUser = src.CanForwardToUser,
        tgt.CanSendBackToApplicant = src.CanSendBackToApplicant,
        tgt.CanSendBackToPrevious = src.CanSendBackToPrevious,
        tgt.UpdatedOn = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (WorkflowId, StageName, StageOrder, DepartmentId, ApproverRoleId, ApproverUserId, ApprovalType,
            CanApprove, CanReject, CanSendCorrection, CanForward, IsFinalStage, SlaDays, IsActive, IsDeleted,
            CreatedOn, UpdatedOn, CanForwardToUser, CanSendBackToApplicant, CanSendBackToPrevious)
    VALUES (src.WorkflowId, src.StageName, src.StageOrder, src.DepartmentId, src.ApproverRoleId, src.ApproverUserId,
            src.ApprovalType, src.CanApprove, src.CanReject, src.CanSendCorrection, src.CanForward, src.IsFinalStage,
            src.SlaDays, src.IsActive, src.IsDeleted, SYSDATETIME(), NULL, src.CanForwardToUser, src.CanSendBackToApplicant, src.CanSendBackToPrevious);

/* ============================================================
   14) ADDITIONAL SEEDS / DEFAULT SETTINGS
   ============================================================ */

MERGE dbo.securitysettings AS tgt
USING
(
    SELECT
        1 AS TenantId
) AS src
ON tgt.TenantId = src.TenantId
WHEN MATCHED THEN
    UPDATE SET
        tgt.SessionTimeoutMinutes = 480,
        tgt.IdleTimeoutMinutes = 30,
        tgt.PasswordMinLength = 8,
        tgt.PasswordRequireUppercase = 1,
        tgt.PasswordRequireLowercase = 1,
        tgt.PasswordRequireDigit = 1,
        tgt.PasswordRequireSpecialChar = 1,
        tgt.PasswordExpiryDays = 90,
        tgt.PasswordHistoryCount = 5,
        tgt.MaxFailedLoginAttempts = 5,
        tgt.LockoutDurationMinutes = 15,
        tgt.EnableCaptchaAfterFailures = 0,
        tgt.CaptchaAfterAttempts = 3,
        tgt.AllowMultipleSessions = 1,
        tgt.BlockNewLoginOnConflict = 0,
        tgt.IsDeleted = 0,
        tgt.UpdatedAt = SYSDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, TenantId, SessionTimeoutMinutes, IdleTimeoutMinutes, PasswordMinLength,
            PasswordRequireUppercase, PasswordRequireLowercase, PasswordRequireDigit, PasswordRequireSpecialChar,
            PasswordExpiryDays, PasswordHistoryCount, MaxFailedLoginAttempts, LockoutDurationMinutes,
            EnableCaptchaAfterFailures, CaptchaAfterAttempts, AllowMultipleSessions, BlockNewLoginOnConflict,
            CreatedAt, UpdatedAt, IsDeleted)
    VALUES (CONVERT(UNIQUEIDENTIFIER, '50000000-0000-0000-0000-000000000401'),
            src.TenantId, 480, 30, 8, 1, 1, 1, 1, 90, 5, 5, 15, 0, 3, 1, 0, SYSDATETIME(), NULL, 0);

/* ============================================================
   15) ERROR LOGS MODULE
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
   END
   ============================================================ */
