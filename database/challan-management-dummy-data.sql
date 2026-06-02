-- Dummy challan data for testing Challan Management.
-- Inserts up to 5 generated challans for existing active consumers.
-- Safe to rerun: demo challan numbers are skipped if already present.

INSERT INTO `challan`
(
    `RECEIPT_ID1`,
    `CONS_NO`,
    `FLAT_NO`,
    `BLK`,
    `SEC`,
    `BL_PER_FR`,
    `BL_PER_TO`,
    `DUE_DT`,
    `BILL_AMT`,
    `SURCHARGE`,
    `PAID_AMT`,
    `PAY_DATE`,
    `ARREAR`,
    `CREDIT`,
    `RECP_NO`,
    `NOC`,
    `RMC`,
    `SECU`,
    `T_FEE`,
    `CSS`,
    `BNK_CD`,
    `BR_NM`,
    `REV_BIL_FR`,
    `DEV_TYPE`,
    `gst`,
    `conn_charge`,
    `panality_charges`,
    `bank_id`,
    `deposeter_name`,
    `receipt_id`,
    `BILL_ID`,
    `STATUS`,
    `ENTRY_DATE`,
    `CHALLAN_VIA`,
    `CHALLAN_STATUS`,
    `USERID`,
    `ADDRESS`,
    `disconnection`,
    `reconnection`
)
SELECT
    CONCAT('DCH', LPAD(x.rn, 6, '0')) AS `RECEIPT_ID1`,
    x.`CONS_NO`,
    x.`FLAT_NO`,
    x.`BLK_NO`,
    x.`SECTOR`,
    DATE_SUB(CURDATE(), INTERVAL 30 DAY),
    CURDATE(),
    DATE_ADD(CURDATE(), INTERVAL 15 DAY),
    CASE x.rn
        WHEN 1 THEN 750.00
        WHEN 2 THEN 1200.00
        WHEN 3 THEN 500.00
        WHEN 4 THEN 2500.00
        ELSE 950.00
    END,
    0,
    CASE x.rn
        WHEN 1 THEN 750.00
        WHEN 2 THEN 1200.00
        WHEN 3 THEN 500.00
        WHEN 4 THEN 2500.00
        ELSE 950.00
    END,
    CASE WHEN x.rn = 2 THEN DATE_SUB(NOW(), INTERVAL 1 DAY) ELSE NULL END,
    0,
    0,
    CONCAT('DCH', LPAD(x.rn, 6, '0')) AS `RECP_NO`,
    CASE WHEN x.rn = 3 THEN 500.00 ELSE 0 END,
    0,
    0,
    0,
    CASE x.rn
        WHEN 1 THEN 'BILL'
        WHEN 2 THEN 'BILL'
        WHEN 3 THEN 'NDC'
        WHEN 4 THEN 'NEWCONN'
        ELSE 'OTHER'
    END,
    COALESCE(x.`Bank_BranchCode`, 'DEMO'),
    COALESCE(x.`bankName`, 'Demo Bank Counter'),
    '0',
    x.`DEV_TYPE`,
    0,
    CASE WHEN x.rn = 4 THEN 2500.00 ELSE 0 END,
    CASE WHEN x.rn = 5 THEN 950.00 ELSE 0 END,
    COALESCE(x.`Bank_BranchCode`, 'DEMO'),
    x.`CONS_NM1`,
    CONCAT('DCH', LPAD(x.rn, 6, '0')) AS `receipt_id`,
    CASE x.rn
        WHEN 1 THEN 'BILL'
        WHEN 2 THEN 'BILL'
        WHEN 3 THEN 'NDC'
        WHEN 4 THEN 'NEWCONN'
        ELSE 'OTHER'
    END,
    CASE WHEN x.rn = 5 THEN '0' ELSE '1' END,
    DATE_SUB(NOW(), INTERVAL x.rn DAY),
    'DEMO',
    CASE WHEN x.rn = 5 THEN 0 ELSE 1 END,
    'Demo Admin',
    COALESCE(x.`CONS_ADDRESS`, CONCAT_WS('/', x.`SECTOR`, CONCAT_WS('-', x.`BLK_NO`, x.`FLAT_NO`))),
    0,
    0
FROM (
    SELECT
        (@rownum := @rownum + 1) AS rn,
        c.`CONS_NO`,
        c.`CONS_NM1`,
        c.`FLAT_NO`,
        c.`BLK_NO`,
        c.`SECTOR`,
        c.`DEV_TYPE`,
        c.`CONS_ADDRESS`,
        b.`Bank_BranchCode`,
        b.`bankName`
    FROM `consumer_details_master` c
    CROSS JOIN (SELECT @rownum := 0) r
    LEFT JOIN (
        SELECT `Bank_BranchCode`, `bankName`
        FROM `bank_master`
        WHERE (`status` IS NULL OR `status` = 1)
        ORDER BY `Id`
        LIMIT 1
    ) b ON 1 = 1
    WHERE c.`status` = 1
    ORDER BY c.`CONS_NO`
    LIMIT 5
) x
WHERE NOT EXISTS (
    SELECT 1
    FROM `challan` ch
    WHERE ch.`RECP_NO` = CONCAT('DCH', LPAD(x.rn, 6, '0'))
       OR ch.`receipt_id` = CONCAT('DCH', LPAD(x.rn, 6, '0'))
);

INSERT INTO `ChallanHistories`
(`ChallanId`, `ChallanNo`, `ConsumerNo`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionByName`, `ActionOn`, `IsDeleted`)
SELECT
    ch.`ID`,
    ch.`RECP_NO`,
    ch.`CONS_NO`,
    NULL,
    CASE
        WHEN ch.`STATUS` = '0' OR ch.`CHALLAN_STATUS` = 0 THEN 'Cancelled'
        WHEN ch.`PAY_DATE` IS NOT NULL THEN 'Paid'
        ELSE 'PendingPayment'
    END,
    CASE
        WHEN ch.`STATUS` = '0' OR ch.`CHALLAN_STATUS` = 0 THEN 'Cancelled'
        WHEN ch.`PAY_DATE` IS NOT NULL THEN 'Paid'
        ELSE 'Generated'
    END,
    CASE
        WHEN ch.`STATUS` = '0' OR ch.`CHALLAN_STATUS` = 0 THEN 'Demo cancelled challan.'
        WHEN ch.`PAY_DATE` IS NOT NULL THEN 'Demo paid challan.'
        ELSE 'Demo challan generated.'
    END,
    'Demo Admin',
    COALESCE(ch.`ENTRY_DATE`, NOW()),
    0
FROM `challan` ch
WHERE ch.`RECP_NO` IN ('DCH000001', 'DCH000002', 'DCH000003', 'DCH000004', 'DCH000005')
  AND NOT EXISTS (
      SELECT 1 FROM `ChallanHistories` h
      WHERE h.`ChallanId` = ch.`ID`
        AND h.`Action` IN ('Generated', 'Paid', 'Cancelled')
        AND h.`IsDeleted` = 0
  );

INSERT INTO `ChallanPaymentHistories`
(`ChallanId`, `ChallanNo`, `ConsumerNo`, `SourceBillNo`, `Amount`, `PaymentDate`, `PaymentMode`, `BankCode`, `BankName`, `TransactionReferenceNo`, `Remarks`, `PostedByName`, `PostedOn`, `IsDeleted`)
SELECT
    ch.`ID`,
    ch.`RECP_NO`,
    ch.`CONS_NO`,
    CASE WHEN ch.`REV_BIL_FR` = 'BILL' THEN ch.`BILL_ID` ELSE NULL END,
    ch.`PAID_AMT`,
    ch.`PAY_DATE`,
    'Cash',
    ch.`BNK_CD`,
    ch.`BR_NM`,
    CONCAT('DEMO-PAY-', ch.`RECP_NO`),
    'Demo payment posting.',
    'Demo Admin',
    COALESCE(ch.`PAY_DATE`, NOW()),
    0
FROM `challan` ch
WHERE ch.`RECP_NO` = 'DCH000002'
  AND ch.`PAY_DATE` IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM `ChallanPaymentHistories` p
      WHERE p.`ChallanId` = ch.`ID`
        AND p.`IsDeleted` = 0
  );
