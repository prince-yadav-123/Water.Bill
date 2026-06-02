-- Dummy requests for testing Name Transfer / Mutation and Connection Type / Category Change.
-- Uses existing live consumers. Run module seed scripts first.
-- These rows are safe test requests in master_application_detail only; they do not update consumer_details_master.

SET @cons1 := (
    SELECT CAST(`CONS_NO` AS CHAR) COLLATE utf8mb4_unicode_ci
    FROM `consumer_details_master`
    WHERE `CONS_NO` IS NOT NULL
    ORDER BY `CONS_NO`
    LIMIT 1
);
SET @cons2 := (
    SELECT CAST(`CONS_NO` AS CHAR) COLLATE utf8mb4_unicode_ci
    FROM `consumer_details_master`
    WHERE `CONS_NO` IS NOT NULL
    ORDER BY `CONS_NO`
    LIMIT 1 OFFSET 1
);

SET @trnApp := '30090001';
SET @ctcApp := '40090001';

INSERT INTO `master_application_detail`
(`Application_id`, `cons_no`, `con_Name`, `con_Address`, `con_Phone_mobile`, `Sector_vill`, `Block`, `Plot_no`, `plot_Area`, `Pipe_size`, `prev_con_detail`, `status`, `enter_date`, `div_name`, `application_status`, `applcation_status_detail`, `reg`, `current_holding_per`, `app_type`)
SELECT
    @trnApp,
    c.`CONS_NO`,
    CONCAT('Mutation Demo ', LEFT(COALESCE(c.`CONS_NM1`, c.`CONS_NO`), 30)),
    c.`CONS_ADDRESS`,
    c.`MOB_NO`,
    c.`SECTOR`,
    c.`BLK_NO`,
    c.`FLAT_NO`,
    c.`PLOT_SIZE`,
    c.`PIPE_SIZE`,
    CONCAT('Old: ', LEFT(COALESCE(c.`CONS_NM1`, ''), 35), '; New: Mutation Demo'),
    1,
    CURDATE(),
    CASE c.`DEV_TYPE` WHEN 1 THEN 'JAL1' WHEN 2 THEN 'JAL2' WHEN 3 THEN 'JAL3' ELSE 'JAL1' END,
    'Pending',
    CONCAT('OldName=', COALESCE(c.`CONS_NM1`, ''), ';OldFather=', COALESCE(c.`CONS_NM2`, ''), ';NewName=Mutation Demo ', c.`CONS_NO`, ';NewFather=Demo Father;Mobile=', COALESCE(c.`MOB_NO`, ''), ';TransferFee=500;SecurityAmount=0;ChallanNo=TRN-DEMO-001;ChallanDate=', CURDATE(), ';Remarks=Demo name transfer request'),
    0,
    CASE c.`DEV_TYPE` WHEN 1 THEN 'JAL1' WHEN 2 THEN 'JAL2' WHEN 3 THEN 'JAL3' ELSE 'JAL1' END,
    'TRN'
FROM `consumer_details_master` c
WHERE CAST(c.`CONS_NO` AS CHAR) COLLATE utf8mb4_unicode_ci = @cons1
  AND NOT EXISTS (
      SELECT 1 FROM `master_application_detail`
      WHERE CAST(`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @trnApp
  );

INSERT INTO `master_application_detail_history`
(`Application_id`, `serial_number`, `division`, `current_holding_per`, `forward_date`, `remark`, `flag`, `curent_status`, `status`)
SELECT
    @trnApp, 1, m.`div_name`, m.`current_holding_per`, CURDATE(), 'Demo name transfer request created.', 'N', '1', '1'
FROM `master_application_detail` m
WHERE CAST(m.`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @trnApp
  AND NOT EXISTS (
      SELECT 1 FROM `master_application_detail_history`
      WHERE CAST(`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @trnApp
        AND `serial_number` = 1
  );

INSERT INTO `master_application_detail`
(`Application_id`, `cons_no`, `con_Name`, `con_Address`, `con_Phone_mobile`, `Sector_vill`, `Block`, `Plot_no`, `plot_Area`, `Pipe_size`, `conn_type`, `property_type`, `prev_con_detail`, `status`, `enter_date`, `div_name`, `application_status`, `applcation_status_detail`, `reg`, `current_holding_per`, `app_type`)
SELECT
    @ctcApp,
    c.`CONS_NO`,
    c.`CONS_NM1`,
    c.`CONS_ADDRESS`,
    c.`MOB_NO`,
    c.`SECTOR`,
    c.`BLK_NO`,
    c.`FLAT_NO`,
    c.`PLOT_SIZE`,
    c.`PIPE_SIZE`,
    'C',
    'R',
    CONCAT('Old type: ', COALESCE(c.`CON_TP`, ''), '; Old category: ', COALESCE(c.`CONS_CTG`, '')),
    1,
    CURDATE(),
    CASE c.`DEV_TYPE` WHEN 1 THEN 'JAL1' WHEN 2 THEN 'JAL2' WHEN 3 THEN 'JAL3' ELSE 'JAL1' END,
    'Pending',
    CONCAT('OldConnectionType=', COALESCE(c.`CON_TP`, ''), ';NewConnectionType=C;OldCategory=', COALESCE(c.`CONS_CTG`, ''), ';NewCategory=R;TypeChangeDate=', CURDATE(), ';EstimationNo=DEMO001;EstimationAmount=1000;SecurityAmount=500;MonthlyRate=250;Remarks=Demo type/category change request'),
    0,
    CASE c.`DEV_TYPE` WHEN 1 THEN 'JAL1' WHEN 2 THEN 'JAL2' WHEN 3 THEN 'JAL3' ELSE 'JAL1' END,
    'CTC'
FROM `consumer_details_master` c
WHERE CAST(c.`CONS_NO` AS CHAR) COLLATE utf8mb4_unicode_ci = COALESCE(@cons2, @cons1)
  AND NOT EXISTS (
      SELECT 1 FROM `master_application_detail`
      WHERE CAST(`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @ctcApp
  );

INSERT INTO `master_application_detail_history`
(`Application_id`, `serial_number`, `division`, `current_holding_per`, `forward_date`, `remark`, `flag`, `curent_status`, `status`)
SELECT
    @ctcApp, 1, m.`div_name`, m.`current_holding_per`, CURDATE(), 'Demo connection type/category change request created.', 'N', '1', '1'
FROM `master_application_detail` m
WHERE CAST(m.`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @ctcApp
  AND NOT EXISTS (
      SELECT 1 FROM `master_application_detail_history`
      WHERE CAST(`Application_id` AS CHAR) COLLATE utf8mb4_unicode_ci = @ctcApp
        AND `serial_number` = 1
  );
