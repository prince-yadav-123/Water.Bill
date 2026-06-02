-- Optional dummy adjustments for testing.
-- Safe to rerun: demo adjustment numbers are skipped.

INSERT INTO `ConsumerAccountAdjustments`
(`AdjustmentNo`, `ConsumerNo`, `AdjustmentType`, `Amount`, `EffectiveDate`, `Remarks`, `Status`, `CreatedByName`, `CreatedAt`, `IsActive`, `IsDeleted`)
SELECT
    CONCAT('ADJDEMO', LPAD(x.rn, 4, '0')),
    x.`CONS_NO`,
    CASE x.rn
        WHEN 1 THEN 'Arrear'
        WHEN 2 THEN 'Credit'
        WHEN 3 THEN 'Rebate'
        WHEN 4 THEN 'Penalty'
        ELSE 'Advance'
    END,
    CASE x.rn
        WHEN 1 THEN 250.00
        WHEN 2 THEN 100.00
        WHEN 3 THEN 75.00
        WHEN 4 THEN 150.00
        ELSE 200.00
    END,
    CURDATE(),
    CONCAT('Demo adjustment for consumer ', x.`CONS_NO`),
    'Pending',
    'Demo Admin',
    NOW(),
    1,
    0
FROM (
    SELECT (@adjrow := @adjrow + 1) AS rn, c.`CONS_NO`
    FROM `consumer_details_master` c
    CROSS JOIN (SELECT @adjrow := 0) r
    WHERE c.`status` = 1
    ORDER BY c.`CONS_NO`
    LIMIT 5
) x
WHERE NOT EXISTS (
    SELECT 1 FROM `ConsumerAccountAdjustments` a
    WHERE a.`AdjustmentNo` = CONCAT('ADJDEMO', LPAD(x.rn, 4, '0'))
);

INSERT INTO `ConsumerAccountAdjustmentHistories`
(`AdjustmentId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionAt`, `IsDeleted`)
SELECT a.`Id`, NULL, 'Pending', 'Created', a.`Remarks`, 'Demo Admin', a.`CreatedAt`, 0
FROM `ConsumerAccountAdjustments` a
WHERE a.`AdjustmentNo` LIKE 'ADJDEMO%'
  AND NOT EXISTS (
      SELECT 1 FROM `ConsumerAccountAdjustmentHistories` h
      WHERE h.`AdjustmentId` = a.`Id` AND h.`Action` = 'Created'
  );

SELECT `AdjustmentNo`, `ConsumerNo`, `AdjustmentType`, `Amount`, `EffectiveDate`, `Status`
FROM `ConsumerAccountAdjustments`
WHERE `AdjustmentNo` LIKE 'ADJDEMO%'
ORDER BY `AdjustmentNo`;
