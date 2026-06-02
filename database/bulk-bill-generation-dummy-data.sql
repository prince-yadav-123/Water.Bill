-- Dummy data helpers for testing Bulk Bill Generation.
-- This does not truncate or replace old data. It only fills missing rate rows.

-- Ensure simple rate category rows exist for common connection types.
INSERT INTO `jal_rate_master` (`ID`, `PROPERTY_TYPE`, `ID_T`, `STATUS`, `DEV_TYPE`)
SELECT 9101, 'Residential', 'R', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_master` WHERE `ID` = 9101);

INSERT INTO `jal_rate_master` (`ID`, `PROPERTY_TYPE`, `ID_T`, `STATUS`, `DEV_TYPE`)
SELECT 9102, 'Commercial', 'C', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_master` WHERE `ID` = 9102);

INSERT INTO `jal_rate_master` (`ID`, `PROPERTY_TYPE`, `ID_T`, `STATUS`, `DEV_TYPE`)
SELECT 9201, 'Residential', 'R', '1', 2
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_master` WHERE `ID` = 9201);

INSERT INTO `jal_rate_master` (`ID`, `PROPERTY_TYPE`, `ID_T`, `STATUS`, `DEV_TYPE`)
SELECT 9301, 'Residential', 'R', '1', 3
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_master` WHERE `ID` = 9301);

-- Add broad slabs if no matching dummy slabs exist.
INSERT INTO `jal_rate_trans`
(`SID`, `ID`, `AREA_START`, `AREA_END`, `REGULAR`, `TEMPORARY`, `MAIN_RATE`, `EST_RATE_REG`, `EST_RATE_TEMP`, `PIPE_SIZE`, `CESS_RATE`, `EFF_FROM`, `EFF_TO`, `STATUS`, `DEV_TYPE`)
SELECT 99101, 9101, 0, 999999, 500, 650, 500, 0, 0, NULL, 5, '2026-04-01', NULL, '1', 1
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_trans` WHERE `SID` = 99101);

INSERT INTO `jal_rate_trans`
(`SID`, `ID`, `AREA_START`, `AREA_END`, `REGULAR`, `TEMPORARY`, `MAIN_RATE`, `EST_RATE_REG`, `EST_RATE_TEMP`, `PIPE_SIZE`, `CESS_RATE`, `EFF_FROM`, `EFF_TO`, `STATUS`, `DEV_TYPE`)
SELECT 99102, 9102, 0, 999999, 1200, 1500, 1200, 0, 0, NULL, 5, '2026-04-01', NULL, '1', 1
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_trans` WHERE `SID` = 99102);

INSERT INTO `jal_rate_trans`
(`SID`, `ID`, `AREA_START`, `AREA_END`, `REGULAR`, `TEMPORARY`, `MAIN_RATE`, `EST_RATE_REG`, `EST_RATE_TEMP`, `PIPE_SIZE`, `CESS_RATE`, `EFF_FROM`, `EFF_TO`, `STATUS`, `DEV_TYPE`)
SELECT 99201, 9201, 0, 999999, 550, 700, 550, 0, 0, NULL, 5, '2026-04-01', NULL, '1', 2
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_trans` WHERE `SID` = 99201);

INSERT INTO `jal_rate_trans`
(`SID`, `ID`, `AREA_START`, `AREA_END`, `REGULAR`, `TEMPORARY`, `MAIN_RATE`, `EST_RATE_REG`, `EST_RATE_TEMP`, `PIPE_SIZE`, `CESS_RATE`, `EFF_FROM`, `EFF_TO`, `STATUS`, `DEV_TYPE`)
SELECT 99301, 9301, 0, 999999, 575, 725, 575, 0, 0, NULL, 5, '2026-04-01', NULL, '1', 3
WHERE NOT EXISTS (SELECT 1 FROM `jal_rate_trans` WHERE `SID` = 99301);

-- Quick candidates for preview testing.
SELECT
    CONS_NO,
    CONS_NM1,
    DEV_TYPE,
    SECTOR,
    BLK_NO,
    FLAT_NO,
    CON_TP,
    CONS_CTG,
    PLOT_SIZE,
    PIPE_SIZE
FROM consumer_details_master
WHERE STATUS = 1
ORDER BY DEV_TYPE, SECTOR, BLK_NO, FLAT_NO
LIMIT 10;
