/*
  UAT sync script generated from old DB CSV exports:
  - Results.csv -> PermissionModules
  - ResultsMenuiotems.csv -> MenuItems

  Behavior
  - Updates matching existing IDs
  - If ID is missing but natural key already exists, updates that row safely
  - Inserts only truly missing rows
  - Does not delete existing extra rows
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('tempdb..#PermissionModuleSeed') IS NOT NULL DROP TABLE #PermissionModuleSeed;
CREATE TABLE #PermissionModuleSeed (Id INT NOT NULL, Name NVARCHAR(100) NOT NULL, IsActive BIT NOT NULL, IsDeleted BIT NOT NULL, PortalScope NVARCHAR(20) NOT NULL);
INSERT INTO #PermissionModuleSeed (Id, Name, IsActive, IsDeleted, PortalScope) VALUES
    (1, N'Dashboard', 1, 0, N'Authority'),
    (2, N'Consumers', 1, 0, N'Authority'),
    (3, N'Billing', 1, 0, N'Authority'),
    (4, N'Payments', 1, 0, N'Authority'),
    (5, N'Reports', 1, 0, N'Authority'),
    (6, N'Role Management', 1, 0, N'Authority'),
    (7, N'User Management', 1, 0, N'Authority'),
    (8, N'Role Permission', 1, 0, N'Authority'),
    (9, N'Menu Management', 1, 0, N'Authority'),
    (10, N'Permission Modules', 1, 0, N'Authority'),
    (11, N'Security Settings', 1, 0, N'Authority'),
    (12, N'Profile', 1, 0, N'Authority'),
    (13, N'Consumer Dashboard', 1, 0, N'Consumer'),
    (14, N'Consumer Bills', 1, 0, N'Consumer'),
    (15, N'Consumer Profile', 1, 0, N'Consumer'),
    (16, N'Consumer New Connection', 1, 0, N'Consumer'),
    (17, N'Consumer Login Management', 1, 0, N'Authority'),
    (18, N'Sector Master', 1, 0, N'Authority'),
    (19, N'Block Master', 1, 0, N'Authority'),
    (20, N'Pipe Size Master', 1, 0, N'Authority'),
    (21, N'Connection Category Master', 1, 0, N'Authority'),
    (22, N'Connection Sub-Type Master', 1, 0, N'Authority'),
    (23, N'Connection Type Master', 1, 0, N'Authority'),
    (24, N'Village Master', 1, 0, N'Authority'),
    (25, N'Document Type Master', 1, 0, N'Authority'),
    (26, N'Masters', 1, 0, N'Authority'),
    (27, N'Payment Mode Master - Duplicate Old Id 27', 0, 1, N'Authority'),
    (28, N'Payment Type Master - Duplicate Old Id 28', 0, 1, N'Authority'),
    (29, N'Bank Master - Duplicate Old Id 29', 0, 1, N'Authority'),
    (30, N'NDC Amount Master - Duplicate Old Id 30', 0, 1, N'Authority'),
    (31, N'Application Status Master - Duplicate Old Id 31', 0, 1, N'Authority'),
    (32, N'Rate Category Master - Duplicate Old Id 32', 0, 1, N'Authority'),
    (33, N'Rate Master - Duplicate Old Id 33', 0, 1, N'Authority'),
    (34, N'Department Master - Duplicate Old Id 34', 0, 1, N'Authority'),
    (35, N'Payment Mode Master', 1, 0, N'Authority'),
    (36, N'Payment Type Master', 1, 0, N'Authority'),
    (37, N'Bank Master', 1, 0, N'Authority'),
    (38, N'NDC Amount Master', 1, 0, N'Authority'),
    (39, N'Application Status Master', 1, 0, N'Authority'),
    (40, N'Rate Category Master', 1, 0, N'Authority'),
    (41, N'Rate Master', 1, 0, N'Authority'),
    (42, N'Department Master', 1, 0, N'Authority'),
    (43, N'Workflow Master', 1, 0, N'Authority'),
    (44, N'My Pending Applications', 1, 0, N'Authority'),
    (45, N'New Connection Fee Configuration', 1, 0, N'Authority'),
    (46, N'Consumer Challans - Duplicate Old Id 46', 0, 1, N'Authority'),
    (47, N'Bulk Bill Generation - Duplicate Old Id 47', 0, 1, N'Authority'),
    (48, N'Consumer Master Maintenance - Duplicate Old Id 48', 0, 1, N'Authority'),
    (49, N'Consumer Account Adjustments - Duplicate Old Id 49', 0, 1, N'Authority'),
    (50, N'Consumer Query Management', 1, 0, N'Authority'),
    (51, N'Consumer Support Queries', 1, 0, N'Consumer'),
    (52, N'Bill Search & Print', 1, 0, N'Authority'),
    (53, N'Online Payment History', 1, 0, N'Authority'),
    (54, N'NDC Applications', 1, 0, N'Authority'),
    (55, N'NDC Certificate Management', 1, 0, N'Authority'),
    (56, N'Consumer NDC Applications', 1, 0, N'Consumer'),
    (57, N'Challan Management', 1, 0, N'Authority'),
    (58, N'Consumer Challans', 1, 0, N'Consumer'),
    (59, N'Bulk Bill Generation', 1, 0, N'Authority'),
    (60, N'Consumer Master Maintenance', 1, 0, N'Authority'),
    (61, N'Consumer Account Adjustments', 1, 0, N'Authority'),
    (62, N'Consumer Ledger', 1, 0, N'Authority'),
    (63, N'Meter Reading Management', 1, 0, N'Authority'),
    (64, N'Disconnection / Reconnection Management', 1, 0, N'Authority'),
    (65, N'Notice Management', 1, 0, N'Authority'),
    (66, N'Complaint Management', 1, 0, N'Authority'),
    (67, N'Consumer Complaints', 1, 0, N'Consumer'),
    (68, N'Connection Type / Category Change', 1, 0, N'Authority'),
    (69, N'Name Transfer / Mutation', 1, 0, N'Authority'),
    (70, N'Reports / MIS', 1, 0, N'Authority'),
    (71, N'Advanced Bill Revision / Reversal', 1, 0, N'Authority'),
    (72, N'User Activity Logs', 1, 0, N'Authority'),
    (73, N'Communication Templates', 1, 0, N'Authority'),
    (74, N'Consumer Service Requests', 1, 0, N'Consumer'),
    (75, N'NotificationManagement', 1, 0, N'Authority'),
    (76, N'Test', 0, 1, N'Consumer'),
    (77, N'Error Logs', 1, 0, N'Authority'),
    (78, N'Consumer Activity Logs', 1, 0, N'Authority');

IF OBJECT_ID('tempdb..#MenuSeed') IS NOT NULL DROP TABLE #MenuSeed;
CREATE TABLE #MenuSeed (Id INT NOT NULL, TenantId INT NOT NULL, ParentId INT NULL, Label NVARCHAR(100) NOT NULL, Icon NVARCHAR(100) NULL, Url NVARCHAR(300) NULL, SectionLabel NVARCHAR(100) NULL, ModuleName NVARCHAR(100) NULL, ModuleId INT NULL, MenuOrder INT NOT NULL, ShowInSidebar BIT NOT NULL, IsActive BIT NOT NULL, IsDeleted BIT NOT NULL, CreatedAt DATETIME2(6) NULL, UpdatedAt DATETIME2(6) NULL, OpenInNewTab BIT NOT NULL);
INSERT INTO #MenuSeed (Id, TenantId, ParentId, Label, Icon, Url, SectionLabel, ModuleName, ModuleId, MenuOrder, ShowInSidebar, IsActive, IsDeleted, CreatedAt, UpdatedAt, OpenInNewTab) VALUES
    (1, 1, NULL, N'Dashboard', N'bi-grid-1x2', N'/Dashboard', N'Main', N'Dashboard', 1, 2, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702709' AS DATETIME2(6)), 0),
    (2, 1, NULL, N'Consumer Management - Duplicate Old Id 2', N'fa fa-users', N'#?duplicateOldId=2', N'Operations', N'Consumer Management', NULL, 2, 0, 0, 0, CAST('2026-06-11 18:09:58.665986' AS DATETIME2(6)), CAST('2026-06-11 13:47:00.078381' AS DATETIME2(6)), 1),
    (3, 1, NULL, N'Billing & Metering - Duplicate Old Id 3', N'fa fa-file-invoice', N'#?duplicateOldId=3', N'Operations', N'Billing & Metering', NULL, 3, 0, 0, 0, CAST('2026-06-11 18:09:58.665986' AS DATETIME2(6)), CAST('2026-06-11 13:47:00.078381' AS DATETIME2(6)), 1),
    (4, 1, NULL, N'Challan & Payments - Duplicate Old Id 4', N'fa fa-credit-card', N'#?duplicateOldId=4', N'Operations', N'Challan & Payments', NULL, 4, 0, 0, 0, CAST('2026-06-11 18:09:58.665986' AS DATETIME2(6)), CAST('2026-06-11 13:47:00.078381' AS DATETIME2(6)), 1),
    (5, 1, NULL, N'Reports / MIS - Duplicate Old Id 5', N'fa fa-chart-line', N'#?duplicateOldId=5', N'Reports', N'Reports / MIS', 58, 5, 0, 0, 0, CAST('2026-06-11 18:09:58.665986' AS DATETIME2(6)), CAST('2026-06-11 13:47:00.078381' AS DATETIME2(6)), 1),
    (6, 1, 167, N'Role Management', N'bi-person-badge', N'/Roles', N'Administration', NULL, 6, 1, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 18:13:51.844786' AS DATETIME2(6)), 0),
    (7, 1, 167, N'User Management', N'bi-people', N'/Users', N'Administration', NULL, 7, 2, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702715' AS DATETIME2(6)), 0),
    (8, 1, 167, N'Role Permission', N'bi-shield-lock', N'/RolePermissions', N'Administration', NULL, 8, 3, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 18:14:14.178893' AS DATETIME2(6)), 0),
    (9, 1, 167, N'Menu Management', N'bi-list', N'/Menu', N'Administration', NULL, 9, 4, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 18:14:26.082104' AS DATETIME2(6)), 0),
    (10, 1, 167, N'Permission Modules', N'bi-shield-lock', N'/PermissionModules', N'Administration', NULL, 10, 5, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 18:14:42.370445' AS DATETIME2(6)), 0),
    (11, 1, 167, N'Security Settings', N'bi-gear-wide-connected', N'/SecuritySettings', N'Administration', NULL, 11, 6, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 18:15:22.733193' AS DATETIME2(6)), 0),
    (12, 1, NULL, N'Profile', N'PR', N'/Profile', N'Account', NULL, 12, 14, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702781' AS DATETIME2(6)), 0),
    (13, 1, 170, N'Consumer Login Management', N'CL', N'/ConsumerLoginManagement', N'Consumer Management', NULL, 13, 1, 1, 1, 0, CAST('2026-05-21 14:17:53.186797' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702737' AS DATETIME2(6)), 0),
    (101, 2, NULL, N'Dashboard', N'bi-grid-1x2', N'/Consumer/Dashboard', N'Main', NULL, 13, 1, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702708' AS DATETIME2(6)), 0),
    (102, 2, NULL, N'Pay Bill', N'PB', N'/Consumer/Bills/Pay', N'Main', NULL, 14, 6, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702742' AS DATETIME2(6)), 0),
    (103, 2, NULL, N'Current Bill', N'CB', N'/Consumer/Bills/Current', N'Main', NULL, 14, 7, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702743' AS DATETIME2(6)), 0),
    (104, 2, NULL, N'Bill History', N'BH', N'/Consumer/Bills/History', N'Main', NULL, 14, 8, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702744' AS DATETIME2(6)), 0),
    (105, 2, NULL, N'New Connection', N'NC', N'/Consumer/NewConnection/Apply', N'Main', NULL, 16, 9, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702745' AS DATETIME2(6)), 0),
    (106, 2, NULL, N'My Applications', N'MA', N'/Consumer/NewConnection/MyApplications', N'Main', NULL, 16, 10, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702746' AS DATETIME2(6)), 0),
    (107, 2, NULL, N'Profile & Connections', N'PC', N'/Consumer/Profile', N'Account', NULL, 15, 11, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702747' AS DATETIME2(6)), 0),
    (108, 2, NULL, N'Update Mobile/Email', N'UE', N'/Consumer/Profile/UpdateContact', N'Account', NULL, 15, 13, 1, 1, 0, CAST('2026-05-21 14:17:53.223781' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702780' AS DATETIME2(6)), 0),
    (109, 1, NULL, N'Masters', N'MS', N'#', N'Masters', NULL, NULL, 12, 1, 1, 0, CAST('2026-05-21 15:03:20.068473' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702748' AS DATETIME2(6)), 0),
    (110, 1, 109, N'Sector Master', N'SC', N'/Masters/sectors', N'Location Masters', NULL, 18, 1, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702749' AS DATETIME2(6)), 0),
    (111, 1, 109, N'Block Master', N'BK', N'/Masters/blocks', N'Location Masters', NULL, 19, 2, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702750' AS DATETIME2(6)), 0),
    (112, 1, 109, N'Pipe Size Master', N'PS', N'/Masters/pipe-sizes', N'Connection Masters', NULL, 20, 4, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702752' AS DATETIME2(6)), 0),
    (113, 1, 109, N'Connection Category Master', N'CC', N'/Masters/connection-categories', N'Connection Masters', NULL, 21, 5, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702754' AS DATETIME2(6)), 0),
    (114, 1, 109, N'Connection Sub-Type Master', N'ST', N'/Masters/connection-sub-types', N'Connection Masters', NULL, 22, 7, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702758' AS DATETIME2(6)), 0),
    (115, 1, 109, N'Connection Type Master', N'CT', N'/Masters/connection-types', N'Connection Masters', NULL, 23, 6, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702757' AS DATETIME2(6)), 0),
    (116, 1, 109, N'Village Master', N'VG', N'/Masters/villages', N'Location Masters', NULL, 24, 3, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702751' AS DATETIME2(6)), 0),
    (117, 1, 109, N'Document Type Master', N'DT', N'/Masters/document-types', N'Application Masters', NULL, 25, 14, 1, 1, 0, CAST('2026-05-21 15:39:55.285202' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702769' AS DATETIME2(6)), 0),
    (125, 1, 109, N'Payment Mode Master', N'PM', N'/Masters/payment-modes', N'Payment Masters', NULL, 35, 11, 1, 1, 0, CAST('2026-05-22 10:21:10.829348' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702765' AS DATETIME2(6)), 0),
    (126, 1, 109, N'Payment Type Master', N'PT', N'/Masters/payment-types', N'Payment Masters', NULL, 36, 12, 1, 1, 0, CAST('2026-05-22 10:21:10.829348' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702766' AS DATETIME2(6)), 0),
    (127, 1, 109, N'Bank Master', N'BK', N'/Masters/banks', N'Payment Masters', NULL, 37, 13, 1, 1, 0, CAST('2026-05-22 10:21:10.829348' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702768' AS DATETIME2(6)), 0),
    (128, 1, 109, N'NDC Amount Master', N'NA', N'/Masters/ndc-amounts', N'Application Masters', NULL, 38, 16, 1, 1, 0, CAST('2026-05-22 10:21:10.829348' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702773' AS DATETIME2(6)), 0),
    (129, 1, 109, N'Application Status Master', N'AS', N'/Masters/application-statuses', N'Application Masters', NULL, 39, 15, 1, 1, 0, CAST('2026-05-22 10:21:10.829348' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702771' AS DATETIME2(6)), 0),
    (132, 1, 109, N'Rate Category Master', N'RC', N'/Masters/rate-categories', N'Connection Masters', NULL, 40, 8, 1, 1, 0, CAST('2026-05-27 08:31:54.836710' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702760' AS DATETIME2(6)), 0),
    (133, 1, 109, N'Rate Master', N'RT', N'/Masters/rates', N'Connection Masters', NULL, 41, 9, 1, 1, 0, CAST('2026-05-27 08:31:54.836710' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702762' AS DATETIME2(6)), 0),
    (135, 1, 109, N'Department Master', N'DP', N'/Departments', N'Application Masters', NULL, 42, 17, 1, 1, 0, CAST('2026-05-27 10:41:12.333118' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702774' AS DATETIME2(6)), 0),
    (136, 1, 109, N'Workflow Master', N'WF', N'/Workflows', N'Workflow Masters', NULL, 43, 18, 1, 1, 0, CAST('2026-05-27 10:41:12.333118' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702776' AS DATETIME2(6)), 0),
    (137, 1, 109, N'New Connection Fee Configuration', N'FE', N'/NewConnectionFeeConfigurations', N'Connection Masters', NULL, 45, 10, 1, 1, 0, CAST('2026-05-27 10:41:12.333118' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702763' AS DATETIME2(6)), 0),
    (138, 1, NULL, N'Pending Applications', N'AP', N'/Approvals/Pending', N'Applications & Approvals', NULL, 44, 24, 1, 1, 0, CAST('2026-05-27 10:41:12.355297' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702824' AS DATETIME2(6)), 0),
    (139, 1, NULL, N'Consumer Queries', N'Q', N'/ConsumerQueryManagement', N'Operations', NULL, 50, 20, 1, 1, 0, CAST('2026-05-28 18:09:59.254349' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702816' AS DATETIME2(6)), 0),
    (140, 2, NULL, N'Support & Queries', N'S', NULL, N'Support', NULL, NULL, 17, 1, 1, 0, CAST('2026-05-28 18:09:59.276937' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702797' AS DATETIME2(6)), 0),
    (141, 2, 140, N'My Queries', N'Q', N'/Consumer/SupportQueries', N'Support', NULL, 51, 1, 1, 1, 0, CAST('2026-05-28 18:09:59.300120' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702799' AS DATETIME2(6)), 0),
    (142, 2, 140, N'Raise Query', N'+', N'/Consumer/SupportQueries/Create', N'Support', NULL, 51, 2, 1, 1, 0, CAST('2026-05-28 18:09:59.316126' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702801' AS DATETIME2(6)), 0),
    (143, 1, 168, N'Bill Search & Print', N'BP', N'/BillSearchPrint', N'Billing & Metering', NULL, 52, 4, 1, 1, 0, CAST('2026-05-28 20:25:49.245782' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702836' AS DATETIME2(6)), 0),
    (144, 1, 169, N'Online Payment History', N'PH', N'/OnlinePaymentHistory', N'Challan & Payments', NULL, 53, 2, 1, 1, 0, CAST('2026-05-29 13:03:09.833503' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702733' AS DATETIME2(6)), 0),
    (145, 1, NULL, N'NDC Applications', N'ND', N'/NdcApplications', N'Applications & Approvals', NULL, 54, 21, 1, 1, 0, CAST('2026-05-29 13:03:09.858835' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702818' AS DATETIME2(6)), 0),
    (146, 1, NULL, N'NDC Certificate Management', N'NC', N'/NdcCertificates', N'Applications & Approvals', NULL, 55, 22, 1, 1, 0, CAST('2026-05-29 13:03:09.921366' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702820' AS DATETIME2(6)), 0),
    (147, 2, NULL, N'NDC / No Dues', N'ND', N'/Consumer/Ndc', N'Main', NULL, 56, 18, 1, 1, 0, CAST('2026-05-29 20:02:24.381495' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702804' AS DATETIME2(6)), 0),
    (148, 1, 169, N'Challan Management', N'CH', N'/ChallanManagement', N'Challan & Payments', NULL, 57, 1, 1, 1, 0, CAST('2026-06-01 12:57:18.196457' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702730' AS DATETIME2(6)), 0),
    (149, 2, NULL, N'My Challans', N'CH', N'/Consumer/Challans', N'Main', NULL, 58, 15, 1, 1, 0, CAST('2026-06-01 15:10:53.949197' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702783' AS DATETIME2(6)), 0),
    (150, 1, 168, N'Bulk Bill Generation', N'BG', N'/BulkBillGeneration', N'Billing & Metering', NULL, 59, 1, 1, 1, 0, CAST('2026-06-01 16:17:41.935710' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702830' AS DATETIME2(6)), 0),
    (151, 1, 170, N'Consumer Master Maintenance', N'CM', N'/ConsumerMasterMaintenance', N'Consumer Management', NULL, 60, 2, 1, 1, 0, CAST('2026-06-01 16:53:39.984991' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702739' AS DATETIME2(6)), 0),
    (152, 1, 168, N'Consumer Account Adjustments', N'AA', N'/ConsumerAccountAdjustments', N'Billing & Metering', NULL, 61, 2, 1, 1, 0, CAST('2026-06-01 17:19:24.259774' AS DATETIME2(6)), CAST('2026-06-03 21:13:04.088130' AS DATETIME2(6)), 1),
    (153, 1, 168, N'Consumer Ledger', N'CL', N'/ConsumerLedger', N'Billing & Metering', NULL, 62, 2, 1, 1, 0, CAST('2026-06-01 17:39:34.405676' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702832' AS DATETIME2(6)), 0),
    (154, 1, 168, N'Meter Reading Management', N'MR', N'/MeterReadingManagement', N'Billing & Metering', NULL, 63, 3, 1, 1, 0, CAST('2026-06-01 18:35:49.329169' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702834' AS DATETIME2(6)), 0),
    (155, 1, NULL, N'Disconnection / Reconnection Management', N'DR', N'/DisconnectionManagement', N'Operations', NULL, 64, 21, 1, 1, 0, CAST('2026-06-01 20:57:31.938516' AS DATETIME2(6)), CAST('2026-06-03 21:06:47.355417' AS DATETIME2(6)), 1),
    (156, 1, NULL, N'Notice Management', N'NM', N'/NoticeManagement', N'Operations', NULL, 65, 22, 1, 1, 0, CAST('2026-06-01 21:33:54.157009' AS DATETIME2(6)), CAST('2026-06-03 21:06:32.318567' AS DATETIME2(6)), 1),
    (157, 1, NULL, N'Complaint Management', N'CM', N'/ComplaintManagement', N'Operations', NULL, 66, 24, 1, 1, 0, CAST('2026-06-01 22:02:35.316954' AS DATETIME2(6)), CAST('2026-06-03 21:05:52.165224' AS DATETIME2(6)), 1),
    (158, 2, NULL, N'Complaints & Requests', N'CR', NULL, N'Support', NULL, NULL, 19, 1, 1, 0, CAST('2026-06-01 22:02:35.337934' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702808' AS DATETIME2(6)), 0),
    (159, 2, 158, N'My Complaints', N'MC', N'/Consumer/Complaints', N'Support', NULL, 67, 1, 1, 1, 0, CAST('2026-06-01 22:02:35.374978' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702811' AS DATETIME2(6)), 0),
    (160, 2, 158, N'Raise Complaint', N'+', N'/Consumer/Complaints/Create', N'Support', NULL, 67, 2, 1, 1, 0, CAST('2026-06-01 22:02:35.414937' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702813' AS DATETIME2(6)), 0),
    (161, 1, 170, N'Connection Type / Category Change', N'CT', N'/ConnectionTypeCategoryChange', N'Consumer Management', NULL, 68, 4, 1, 1, 0, CAST('2026-06-01 22:38:03.977728' AS DATETIME2(6)), CAST('2026-06-03 21:08:32.661227' AS DATETIME2(6)), 1),
    (162, 1, 170, N'Name Transfer / Mutation', N'NT', N'/NameTransferMutation', N'Consumer Management', NULL, 69, 3, 1, 1, 0, CAST('2026-06-01 22:39:18.879748' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702742' AS DATETIME2(6)), 0),
    (163, 1, NULL, N'Reports / MIS', N'RP', N'/ReportsMis', N'Reports', NULL, 70, 23, 1, 1, 0, CAST('2026-06-02 10:52:47.224889' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702823' AS DATETIME2(6)), 0),
    (164, 1, 168, N'Advanced Bill Revision / Reversal', N'BR', N'/BillRevision', N'Billing & Metering', NULL, 71, 6, 1, 1, 0, CAST('2026-06-02 10:52:47.286796' AS DATETIME2(6)), CAST('2026-06-03 21:09:19.585844' AS DATETIME2(6)), 1),
    (165, 1, 167, N'User Activity Logs', N'AL', N'/UserActivityLogs', N'Administration', N'User Activity Logs', 72, 12, 1, 1, 0, CAST('2026-06-02 10:52:47.358003' AS DATETIME2(6)), CAST('2026-06-16 13:56:48.614350' AS DATETIME2(6)), 0),
    (166, 1, 167, N'Communication Templates', N'CT', N'/CommunicationTemplates', N'Administration', NULL, 73, 8, 1, 1, 0, CAST('2026-06-02 14:51:09.825644' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702724' AS DATETIME2(6)), 0),
    (167, 1, NULL, N'Administration', N'bi-person-badge', NULL, NULL, NULL, NULL, 3, 1, 1, 0, CAST('2026-06-02 12:59:54.657079' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702715' AS DATETIME2(6)), 0),
    (168, 1, NULL, N'Billing & Metering', N'BM', NULL, NULL, NULL, NULL, 25, 1, 1, 0, CAST('2026-06-02 13:02:38.163524' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702827' AS DATETIME2(6)), 0),
    (169, 1, NULL, N'Challan & Payments', N'CP', NULL, NULL, NULL, NULL, 4, 1, 1, 0, CAST('2026-06-02 13:06:58.207238' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702728' AS DATETIME2(6)), 0),
    (170, 1, NULL, N'Consumer Management', N'CM', NULL, NULL, NULL, NULL, 5, 1, 1, 0, CAST('2026-06-02 13:08:35.048981' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702736' AS DATETIME2(6)), 0),
    (171, 2, NULL, N'Service Requests', N'SR', NULL, N'Main', NULL, NULL, 16, 1, 1, 0, CAST('2026-06-02 19:20:05.420572' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702786' AS DATETIME2(6)), 0),
    (172, 2, 171, N'My Requests', N'MR', N'/Consumer/ServiceRequests', N'Requests', NULL, 74, 1, 1, 1, 0, CAST('2026-06-02 19:20:05.457713' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702789' AS DATETIME2(6)), 0),
    (173, 2, 171, N'Name Transfer / Mutation', N'NT', N'/Consumer/ServiceRequests/NameTransfer', N'Apply', NULL, 74, 2, 1, 1, 0, CAST('2026-06-02 19:20:05.539559' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702792' AS DATETIME2(6)), 0),
    (174, 2, 171, N'Connection Change', N'CC', N'/Consumer/ServiceRequests/ConnectionChange', N'Apply', NULL, 74, 3, 1, 1, 0, CAST('2026-06-02 19:20:05.560660' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702796' AS DATETIME2(6)), 0),
    (175, 1, NULL, N'Communication', N'📢', NULL, NULL, NULL, 75, 25, 1, 1, 0, CAST('2026-06-03 19:56:30.000000' AS DATETIME2(6)), CAST('2026-06-03 21:04:29.733057' AS DATETIME2(6)), 1),
    (176, 1, 109, N'Notification Management', N'NM', N'/NotificationManagement', N'Notification', NULL, 75, 19, 1, 1, 0, CAST('2026-06-03 19:56:30.000000' AS DATETIME2(6)), CAST('2026-06-12 10:39:24.702779' AS DATETIME2(6)), 0),
    (177, 1, 167, N'Error Logs', N'bi-exclamation-triangle', N'/ErrorLogs', NULL, NULL, 77, 9, 1, 1, 0, CAST('2026-06-15 06:56:01.522989' AS DATETIME2(6)), CAST('2026-06-15 15:37:40.924461' AS DATETIME2(6)), 0),
    (179, 1, 167, N'Consumer Activity Logs', N'CL', N'/ConsumerActivityLogs', N'Administration', N'Consumer Activity Logs', 78, 13, 1, 1, 0, CAST('2026-06-16 13:56:48.614350' AS DATETIME2(6)), NULL, 0);

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
