-- Dummy notices for testing.
-- Run after notice-management-module.sql.

INSERT INTO `ConsumerNotices`
(`NoticeNo`, `ConsumerNo`, `TemplateId`, `NoticeType`, `Subject`, `Body`, `NoticeDate`, `DueDate`, `Status`,
 `RelatedBillNo`, `RelatedChallanNo`, `AmountDue`, `Remarks`, `CreatedByName`, `CreatedAt`, `IsActive`, `IsDeleted`)
SELECT CONCAT('NTDEMO', LPAD(x.seq, 4, '0')),
       c.`CONS_NO`,
       t.`Id`,
       x.notice_type,
       COALESCE(t.`Subject`, x.notice_type),
       REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(COALESCE(t.`Body`, 'Notice for consumer no {ConsumerNo}, property {PropertyNo}.'),
           '{ConsumerNo}', c.`CONS_NO`),
           '{ConsumerName}', IFNULL(c.`CONS_NM1`, '')),
           '{PropertyNo}', CONCAT(IFNULL(c.`SECTOR`, 'NA'), '/', IFNULL(c.`BLK_NO`, 'NA'), '-', IFNULL(c.`FLAT_NO`, 'NA'))),
           '{AmountDue}', FORMAT(x.amount_due, 2)),
           '{DueDate}', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 15 DAY), '%d-%b-%Y')),
       DATE_SUB(CURDATE(), INTERVAL x.days_back DAY),
       DATE_ADD(CURDATE(), INTERVAL 15 DAY),
       x.status_value,
       NULL,
       NULL,
       x.amount_due,
       x.remarks,
       'Demo Admin',
       NOW(),
       CASE WHEN x.status_value = 'Cancelled' THEN 0 ELSE 1 END,
       0
FROM (
    SELECT 1 AS seq, 'DueNotice' AS notice_type, 'Draft' AS status_value, 1500.00 AS amount_due, 2 AS days_back, 'Demo draft due notice.' AS remarks
    UNION ALL SELECT 2, 'DisconnectionNotice', 'Issued', 3200.00, 8, 'Demo issued disconnection notice.'
    UNION ALL SELECT 3, 'DemandNotice', 'Issued', 2100.00, 10, 'Demo issued demand notice.'
    UNION ALL SELECT 4, 'ReconnectionOrder', 'Draft', 150.00, 1, 'Demo draft reconnection order.'
    UNION ALL SELECT 5, 'GeneralNotice', 'Cancelled', 0.00, 20, 'Demo cancelled general notice.'
) x
JOIN (
    SELECT `CONS_NO`, `CONS_NM1`, `SECTOR`, `BLK_NO`, `FLAT_NO`, ROW_NUMBER() OVER (ORDER BY `CONS_NO`) AS rn
    FROM `consumer_details_master`
    WHERE `STATUS` = 1
    LIMIT 5
) c ON c.rn = x.seq
LEFT JOIN `NoticeTemplates` t ON t.`NoticeType` = x.notice_type AND t.`IsActive` = 1 AND t.`IsDeleted` = 0
WHERE NOT EXISTS (
    SELECT 1 FROM `ConsumerNotices` n
    WHERE n.`NoticeNo` = CONCAT('NTDEMO', LPAD(x.seq, 4, '0'))
);

INSERT INTO `ConsumerNoticeHistories`
(`NoticeId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionAt`, `IsDeleted`)
SELECT n.`Id`, NULL, n.`Status`,
       CASE n.`Status` WHEN 'Issued' THEN 'Issued' WHEN 'Cancelled' THEN 'Cancelled' ELSE 'Created' END,
       n.`Remarks`, 'Demo Admin', n.`CreatedAt`, 0
FROM `ConsumerNotices` n
WHERE n.`NoticeNo` LIKE 'NTDEMO%'
  AND NOT EXISTS (
      SELECT 1 FROM `ConsumerNoticeHistories` h
      WHERE h.`NoticeId` = n.`Id`
  );
