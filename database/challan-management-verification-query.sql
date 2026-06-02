-- Quick verification query for latest generated challans.
-- Use this after generating a challan from the Admin Portal.

SELECT
    ch.`ID`,
    ch.`RECP_NO` AS `ChallanNo`,
    ch.`receipt_id` AS `ReceiptRef`,
    ch.`CONS_NO` AS `ConsumerNo`,
    c.`CONS_NM1` AS `ConsumerName`,
    CONCAT_WS('/', ch.`SEC`, CONCAT_WS('-', ch.`BLK`, ch.`FLAT_NO`)) AS `Property`,
    ch.`REV_BIL_FR` AS `PurposeCode`,
    ch.`BILL_ID` AS `SourceBillOrPurpose`,
    ch.`BILL_AMT` AS `BillAmount`,
    ch.`ARREAR` AS `ArrearAmount`,
    ch.`NOC` AS `NdcAmount`,
    ch.`conn_charge` AS `ConnectionCharge`,
    ch.`panality_charges` AS `OtherCharge`,
    ch.`PAID_AMT` AS `TotalPayable`,
    CASE
        WHEN ch.`STATUS` = '0' OR ch.`CHALLAN_STATUS` = 0 THEN 'Cancelled'
        WHEN ch.`PAY_DATE` IS NOT NULL THEN 'Paid'
        ELSE 'PendingPayment'
    END AS `DisplayStatus`,
    ch.`ENTRY_DATE` AS `GeneratedOn`,
    ch.`USERID` AS `GeneratedBy`
FROM `challan` ch
LEFT JOIN `consumer_details_master` c ON c.`CONS_NO` = ch.`CONS_NO`
ORDER BY ch.`ID` DESC
LIMIT 20;

SELECT
    p.`Id`,
    p.`ChallanNo`,
    p.`ConsumerNo`,
    c.`CONS_NM1` AS `ConsumerName`,
    p.`SourceBillNo`,
    p.`Amount`,
    p.`PaymentDate`,
    p.`PaymentMode`,
    p.`BankName`,
    p.`TransactionReferenceNo`,
    p.`PostedByName`,
    p.`PostedOn`
FROM `ChallanPaymentHistories` p
LEFT JOIN `consumer_details_master` c ON c.`CONS_NO` = p.`ConsumerNo`
WHERE p.`IsDeleted` = 0
ORDER BY p.`Id` DESC
LIMIT 20;
