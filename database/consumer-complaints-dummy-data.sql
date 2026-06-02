-- Dummy complaint data for testing.
-- Run after consumer-complaints-module.sql.

INSERT INTO `ConsumerComplaints`
(`ComplaintNo`, `ConsumerUserId`, `ConsumerNo`, `ConsumerName`, `MobileNo`, `Email`, `CategoryId`, `CategoryName`,
 `Subject`, `Description`, `Priority`, `Status`, `LocationDetails`, `RelatedBillNo`, `RelatedApplicationNo`,
 `AdminRemarks`, `ResolvedAt`, `ClosedAt`, `CreatedAt`, `UpdatedAt`, `IsActive`, `IsDeleted`)
SELECT CONCAT('CMPDEMO', LPAD(x.seq, 4, '0')),
       NULL,
       c.`CONS_NO`,
       IFNULL(c.`CONS_NM1`, 'Demo Consumer'),
       c.`MOB_NO`,
       c.`EMAIL_ID`,
       cat.`Id`,
       cat.`CategoryName`,
       x.subject_value,
       x.description_value,
       x.priority_value,
       x.status_value,
       CONCAT(IFNULL(c.`SECTOR`, 'NA'), '/', IFNULL(c.`BLK_NO`, 'NA'), '-', IFNULL(c.`FLAT_NO`, 'NA')),
       NULL,
       NULL,
       x.admin_remarks,
       CASE WHEN x.status_value IN ('Resolved', 'Closed') THEN DATE_SUB(NOW(), INTERVAL 1 DAY) ELSE NULL END,
       CASE WHEN x.status_value = 'Closed' THEN NOW() ELSE NULL END,
       DATE_SUB(NOW(), INTERVAL x.days_back DAY),
       NOW(),
       CASE WHEN x.status_value = 'Closed' THEN 0 ELSE 1 END,
       0
FROM (
    SELECT 1 AS seq, 'Water Leakage' AS category_name, 'Leakage near meter chamber' AS subject_value, 'Water leakage observed near the meter chamber.' AS description_value, 'High' AS priority_value, 'Open' AS status_value, 1 AS days_back, NULL AS admin_remarks
    UNION ALL SELECT 2, 'No Water Supply', 'No supply since morning', 'Water supply not available since morning.', 'Normal', 'InProgress', 2, 'Field team informed.'
    UNION ALL SELECT 3, 'Low Pressure', 'Very low pressure', 'Pressure is too low during morning supply.', 'Normal', 'Resolved', 5, 'Valve pressure adjusted.'
    UNION ALL SELECT 4, 'Meter Issue', 'Meter display not visible', 'Meter reading display is not visible.', 'High', 'Rejected', 7, 'Meter issue not found during inspection.'
    UNION ALL SELECT 5, 'Sewer / Drainage Issue', 'Sewer overflow', 'Sewer line overflow near property.', 'Urgent', 'Closed', 10, 'Resolved and closed after field confirmation.'
) x
JOIN (
    SELECT `CONS_NO`, `CONS_NM1`, `MOB_NO`, `EMAIL_ID`, `SECTOR`, `BLK_NO`, `FLAT_NO`, ROW_NUMBER() OVER (ORDER BY `CONS_NO`) AS rn
    FROM `consumer_details_master`
    WHERE `STATUS` = 1
    LIMIT 5
) c ON c.rn = x.seq
JOIN `ComplaintCategories` cat ON cat.`CategoryName` = x.category_name AND cat.`IsDeleted` = 0
WHERE NOT EXISTS (
    SELECT 1 FROM `ConsumerComplaints` existing
    WHERE existing.`ComplaintNo` = CONCAT('CMPDEMO', LPAD(x.seq, 4, '0'))
);

INSERT INTO `ConsumerComplaintHistories`
(`ComplaintId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionByRole`, `ActionAt`, `IsDeleted`)
SELECT c.`Id`, NULL, 'Open', 'Created', 'Demo complaint raised by consumer.', c.`ConsumerName`, 'Consumer', c.`CreatedAt`, 0
FROM `ConsumerComplaints` c
WHERE c.`ComplaintNo` LIKE 'CMPDEMO%'
  AND NOT EXISTS (SELECT 1 FROM `ConsumerComplaintHistories` h WHERE h.`ComplaintId` = c.`Id`);

INSERT INTO `ConsumerComplaintHistories`
(`ComplaintId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionByRole`, `ActionAt`, `IsDeleted`)
SELECT c.`Id`, 'Open', c.`Status`, c.`Status`, c.`AdminRemarks`, 'Demo Admin', 'Admin', c.`UpdatedAt`, 0
FROM `ConsumerComplaints` c
WHERE c.`ComplaintNo` LIKE 'CMPDEMO%'
  AND c.`Status` <> 'Open'
  AND NOT EXISTS (
      SELECT 1 FROM `ConsumerComplaintHistories` h
      WHERE h.`ComplaintId` = c.`Id` AND h.`ToStatus` = c.`Status`
  );
