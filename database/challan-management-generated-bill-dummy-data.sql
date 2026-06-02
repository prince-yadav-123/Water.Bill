-- Dummy generated bill data for Challan Management bill-due dropdown testing.
-- Inserts up to 5 active rows into existing imported table `jal_print_bill_master`.
-- Safe to rerun: demo bill numbers are skipped if already present.

INSERT INTO `jal_print_bill_master`
(
    `BILL_NO`,
    `CONS_NO`,
    `BILL_DATE`,
    `BILL_DUE_DATE`,
    `BILL_DATE_FROM`,
    `BILL_DATE_TO`,
    `MIN_RATE`,
    `MIN_TOTAL_AMT`,
    `BILL_REBATE_PER`,
    `BILL_REBATE_AMT`,
    `CESS_AMT`,
    `AREAR`,
    `AREAR_TEXT`,
    `AREAR_INT`,
    `AREAR_INT_TEXT`,
    `LAST_BILL_EXTRA`,
    `TOTAL_BILL_AMT`,
    `BEFORE_DATE`,
    `AFTER_DATE`,
    `AFTER_DATE_AMT`,
    `Div_type`,
    `STATUS`,
    `ENTRY_DATE`,
    `Due_date`,
    `due_amt`,
    `paid_date`,
    `paid_amt`,
    `diff`,
    `PAID_STATUS`,
    `new_record`,
    `update_record`,
    `bill_after_sep_amt`,
    `adv_amt`,
    `PRINT_STATUS`,
    `OLD_RATE`,
    `BILL_TYPE`,
    `LAST_PAID_AMT`,
    `BILL_COUNT`,
    `SCHEME_ID`,
    `BILL_PERCENTAGE`,
    `USERID`,
    `DEV_TYPE`,
    `PAYMENT_TYPE`,
    `CHALLAN_NO`,
    `BANK_CODE`,
    `Challan_Content`,
    `Rid`,
    `Part_Amt`
)
SELECT
    CONCAT('WBDEMO', LPAD(x.rn, 4, '0')) AS `BILL_NO`,
    x.`CONS_NO`,
    CURDATE() AS `BILL_DATE`,
    DATE_ADD(CURDATE(), INTERVAL 15 DAY) AS `BILL_DUE_DATE`,
    DATE_SUB(CURDATE(), INTERVAL 60 DAY) AS `BILL_DATE_FROM`,
    CURDATE() AS `BILL_DATE_TO`,
    CASE x.rn
        WHEN 1 THEN 500.00
        WHEN 2 THEN 750.00
        WHEN 3 THEN 900.00
        WHEN 4 THEN 1200.00
        ELSE 1500.00
    END AS `MIN_RATE`,
    CASE x.rn
        WHEN 1 THEN 500.00
        WHEN 2 THEN 750.00
        WHEN 3 THEN 900.00
        WHEN 4 THEN 1200.00
        ELSE 1500.00
    END AS `MIN_TOTAL_AMT`,
    0 AS `BILL_REBATE_PER`,
    0 AS `BILL_REBATE_AMT`,
    0 AS `CESS_AMT`,
    CASE WHEN x.rn IN (2, 4) THEN 150.00 ELSE 0 END AS `AREAR`,
    '0' AS `AREAR_TEXT`,
    0 AS `AREAR_INT`,
    '0' AS `AREAR_INT_TEXT`,
    0 AS `LAST_BILL_EXTRA`,
    CASE x.rn
        WHEN 1 THEN 500.00
        WHEN 2 THEN 900.00
        WHEN 3 THEN 900.00
        WHEN 4 THEN 1350.00
        ELSE 1500.00
    END AS `TOTAL_BILL_AMT`,
    NULL AS `BEFORE_DATE`,
    NULL AS `AFTER_DATE`,
    0 AS `AFTER_DATE_AMT`,
    CAST(x.`DEV_TYPE` AS CHAR) AS `Div_type`,
    '1' AS `STATUS`,
    NOW() AS `ENTRY_DATE`,
    DATE_ADD(CURDATE(), INTERVAL 15 DAY) AS `Due_date`,
    CASE x.rn
        WHEN 1 THEN 500.00
        WHEN 2 THEN 900.00
        WHEN 3 THEN 900.00
        WHEN 4 THEN 1350.00
        ELSE 1500.00
    END AS `due_amt`,
    NULL AS `paid_date`,
    0 AS `paid_amt`,
    0 AS `diff`,
    'N' AS `PAID_STATUS`,
    'Y' AS `new_record`,
    NULL AS `update_record`,
    0 AS `bill_after_sep_amt`,
    0 AS `adv_amt`,
    1 AS `PRINT_STATUS`,
    0 AS `OLD_RATE`,
    'REGULAR' AS `BILL_TYPE`,
    0 AS `LAST_PAID_AMT`,
    1 AS `BILL_COUNT`,
    NULL AS `SCHEME_ID`,
    100 AS `BILL_PERCENTAGE`,
    'Demo' AS `USERID`,
    x.`DEV_TYPE`,
    NULL AS `PAYMENT_TYPE`,
    NULL AS `CHALLAN_NO`,
    NULL AS `BANK_CODE`,
    CONCAT('Demo bill content for consumer ', x.`CONS_NO`) AS `Challan_Content`,
    CAST(x.`Rid` AS CHAR) AS `Rid`,
    0 AS `Part_Amt`
FROM (
    SELECT
        (@billrow := @billrow + 1) AS rn,
        c.`CONS_NO`,
        c.`DEV_TYPE`,
        c.`Rid`
    FROM `consumer_details_master` c
    CROSS JOIN (SELECT @billrow := 0) r
    WHERE c.`status` = 1
    ORDER BY c.`CONS_NO`
    LIMIT 5
) x
WHERE NOT EXISTS (
    SELECT 1
    FROM `jal_print_bill_master` b
    WHERE b.`BILL_NO` = CONCAT('WBDEMO', LPAD(x.rn, 4, '0'))
);
