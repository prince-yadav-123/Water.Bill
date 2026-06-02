/*
Compatibility hardening for old-table behavior.

Purpose:
- Keep CHALLAN.PAID_AMT reserved for actual paid amount.
- Pending/demand challans should carry payable amount in old amount-head fields.
- This protects old screens/procs that interpret PAID_AMT > 0 as paid.

Run after taking a DB backup.
*/

UPDATE `challan`
SET
    `BILL_AMT` = CASE
        WHEN COALESCE(`BILL_AMT`, 0) = 0
             AND COALESCE(`NOC`, 0) = 0
             AND COALESCE(`conn_charge`, 0) = 0
             AND COALESCE(`panality_charges`, 0) = 0
             AND COALESCE(`PAID_AMT`, 0) > 0
             AND COALESCE(`REV_BIL_FR`, '') = 'BILL'
            THEN `PAID_AMT`
        ELSE `BILL_AMT`
    END,
    `NOC` = CASE
        WHEN COALESCE(`BILL_AMT`, 0) = 0
             AND COALESCE(`NOC`, 0) = 0
             AND COALESCE(`conn_charge`, 0) = 0
             AND COALESCE(`panality_charges`, 0) = 0
             AND COALESCE(`PAID_AMT`, 0) > 0
             AND COALESCE(`REV_BIL_FR`, '') = 'NDC'
            THEN `PAID_AMT`
        ELSE `NOC`
    END,
    `conn_charge` = CASE
        WHEN COALESCE(`BILL_AMT`, 0) = 0
             AND COALESCE(`NOC`, 0) = 0
             AND COALESCE(`conn_charge`, 0) = 0
             AND COALESCE(`panality_charges`, 0) = 0
             AND COALESCE(`PAID_AMT`, 0) > 0
             AND COALESCE(`REV_BIL_FR`, '') = 'NEWCONN'
            THEN `PAID_AMT`
        ELSE `conn_charge`
    END,
    `panality_charges` = CASE
        WHEN COALESCE(`BILL_AMT`, 0) = 0
             AND COALESCE(`NOC`, 0) = 0
             AND COALESCE(`conn_charge`, 0) = 0
             AND COALESCE(`panality_charges`, 0) = 0
             AND COALESCE(`PAID_AMT`, 0) > 0
             AND COALESCE(`REV_BIL_FR`, '') NOT IN ('BILL', 'NDC', 'NEWCONN')
            THEN `PAID_AMT`
        ELSE `panality_charges`
    END,
    `PAID_AMT` = 0
WHERE `PAY_DATE` IS NULL
  AND COALESCE(`PAID_AMT`, 0) > 0
  AND COALESCE(`CHALLAN_VIA`, '') IN ('ADMIN', 'CONSUMER', 'ONLINE')
  AND COALESCE(`UPD`, '') <> 'PAID'
  AND COALESCE(`STATUS`, '1') = '1'
  AND COALESCE(`CHALLAN_STATUS`, 1) = 1;

