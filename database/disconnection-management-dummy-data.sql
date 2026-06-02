-- Dummy disconnection/reconnection cases for testing.
-- Run after disconnection-management-module.sql.

INSERT INTO `ConsumerDisconnectionCases`
(`CaseNo`, `ConsumerNo`, `CaseType`, `Reason`, `Status`, `NoticeDate`, `DueDate`, `OutstandingAmount`,
 `DisconnectionFee`, `ReconnectionFee`, `FieldOfficerName`, `Remarks`, `PreviousConsumerCategory`,
 `PreviousStatus`, `PreviousNewStatus`, `CreatedByName`, `CreatedAt`, `IsActive`, `IsDeleted`)
SELECT CONCAT('DRDEMO', LPAD(x.seq, 4, '0')),
       c.`CONS_NO`,
       'Disconnection',
       x.reason_value,
       x.status_value,
       DATE_SUB(CURDATE(), INTERVAL x.days_back DAY),
       DATE_ADD(DATE_SUB(CURDATE(), INTERVAL x.days_back DAY), INTERVAL 15 DAY),
       x.outstanding,
       x.dis_fee,
       x.rec_fee,
       CONCAT('Field Officer ', x.seq),
       x.remarks,
       c.`CONS_CTG`,
       c.`STATUS`,
       c.`NEW_STATUS`,
       'Demo Admin',
       NOW(),
       CASE WHEN x.status_value IN ('Reconnected', 'Cancelled') THEN 0 ELSE 1 END,
       0
FROM (
    SELECT 1 AS seq, 20 AS days_back, 'NonPayment' AS reason_value, 'NoticeGenerated' AS status_value, 2400.00 AS outstanding, 100.00 AS dis_fee, 150.00 AS rec_fee, 'Demo notice generated for pending dues.' AS remarks
    UNION ALL SELECT 2, 18, 'NonPayment', 'Disconnected', 3200.00, 100.00, 150.00, 'Demo disconnection completed.'
    UNION ALL SELECT 3, 12, 'ConsumerRequest', 'ReconnectionRequested', 0.00, 0.00, 150.00, 'Consumer requested reconnection.'
    UNION ALL SELECT 4, 35, 'TemporaryClosure', 'Reconnected', 0.00, 0.00, 150.00, 'Demo case reconnected.'
    UNION ALL SELECT 5, 10, 'Other', 'Cancelled', 0.00, 0.00, 0.00, 'Demo case cancelled.'
) x
JOIN (
    SELECT `CONS_NO`, `CONS_CTG`, `STATUS`, `NEW_STATUS`, ROW_NUMBER() OVER (ORDER BY `CONS_NO`) AS rn
    FROM `consumer_details_master`
    WHERE `STATUS` = 1
    LIMIT 5
) c ON c.rn = x.seq
WHERE NOT EXISTS (
    SELECT 1 FROM `ConsumerDisconnectionCases` d
    WHERE d.`CaseNo` = CONCAT('DRDEMO', LPAD(x.seq, 4, '0'))
);

INSERT INTO `ConsumerDisconnectionCaseHistories`
(`CaseId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionAt`, `IsDeleted`)
SELECT d.`Id`, NULL, d.`Status`,
       CASE d.`Status`
           WHEN 'NoticeGenerated' THEN 'NoticeGenerated'
           WHEN 'Disconnected' THEN 'Disconnected'
           WHEN 'ReconnectionRequested' THEN 'ReconnectionRequested'
           WHEN 'Reconnected' THEN 'Reconnected'
           WHEN 'Cancelled' THEN 'Cancelled'
           ELSE d.`Status`
       END,
       d.`Remarks`, 'Demo Admin', d.`CreatedAt`, 0
FROM `ConsumerDisconnectionCases` d
WHERE d.`CaseNo` LIKE 'DRDEMO%'
  AND NOT EXISTS (
      SELECT 1 FROM `ConsumerDisconnectionCaseHistories` h
      WHERE h.`CaseId` = d.`Id`
  );
