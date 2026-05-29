-- Dummy test data for Payment History + NDC Approval/Certificate flows.
-- Run manually in the Water.Bill MySQL database after payment-ndc-admin-modules.sql.
-- This script is additive and avoids inserting duplicates by using fixed demo keys.

SET @adminRoleId := (SELECT `Id` FROM `approles` WHERE `Name` = 'Admin' AND `IsDeleted` = 0 LIMIT 1);
SET @adminUserId := (SELECT `Id` FROM `appusers` WHERE `RoleId` = @adminRoleId AND `IsDeleted` = 0 AND IFNULL(`IsActive`, 1) = 1 ORDER BY `Id` LIMIT 1);
SET @demoDeptId := (SELECT `Id` FROM `master_dept_details` WHERE IFNULL(`STATUS`, '1') = '1' ORDER BY `Id` LIMIT 1);
SET @adminRoleId := IFNULL(@adminRoleId, 1);
SET @adminUserId := IFNULL(@adminUserId, 1);
SET @demoDeptId := IFNULL(@demoDeptId, 1);

INSERT INTO `AuthorityUserDepartments` (`UserId`, `DepartmentId`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @adminUserId, @demoDeptId, 1, 0, NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM `AuthorityUserDepartments`
    WHERE `UserId` = @adminUserId
      AND `DepartmentId` = @demoDeptId
      AND `IsDeleted` = 0
);

-- ---------------------------------------------------------------------------
-- 1. Online payment history demo data: jalnoida_bankpay_master, jalnoida_bankpay_tran
-- ---------------------------------------------------------------------------

INSERT INTO `jalnoida_bankpay_master`
(`JALREFID`, `CONSID`, `CONS_NAME`, `CONS_PROPERTY`, `DATE_FROM`, `DATE_TO`, `PAYAMOUNT`, `EMAIL_ID`, `MOBILE_NO`, `DEPOSIT_BANK`, `STATUS`, `ENTRY_DATE`, `disclaimer`, `PAYMENTSTATUS`, `Challan_No`, `DueDate`, `Bill_Ndc`, `Bill_No`)
SELECT 'DUMYPAY0001', 'C0010001', 'Amit Sharma', '12/A-101', DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 1250.00, 'amit@test.com', '9876500001', 'AXIS', '1', DATE_SUB(NOW(), INTERVAL 4 DAY), 'Y', 'Y', 'CHD001', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 10 DAY), '%d-%m-%Y'), 'BILL', 'WB260501'
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_master` WHERE `JALREFID` = 'DUMYPAY0001');

INSERT INTO `jalnoida_bankpay_master`
(`JALREFID`, `CONSID`, `CONS_NAME`, `CONS_PROPERTY`, `DATE_FROM`, `DATE_TO`, `PAYAMOUNT`, `EMAIL_ID`, `MOBILE_NO`, `DEPOSIT_BANK`, `STATUS`, `ENTRY_DATE`, `disclaimer`, `PAYMENTSTATUS`, `Challan_No`, `DueDate`, `Bill_Ndc`, `Bill_No`)
SELECT 'DUMYPAY0002', 'C0010002', 'Priya Verma', '18/B-204', DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 980.00, 'priya@test.com', '9876500002', 'HDFC', '1', DATE_SUB(NOW(), INTERVAL 3 DAY), 'Y', 'P', 'CHD002', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 8 DAY), '%d-%m-%Y'), 'BILL', 'WB260502'
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_master` WHERE `JALREFID` = 'DUMYPAY0002');

INSERT INTO `jalnoida_bankpay_master`
(`JALREFID`, `CONSID`, `CONS_NAME`, `CONS_PROPERTY`, `DATE_FROM`, `DATE_TO`, `PAYAMOUNT`, `EMAIL_ID`, `MOBILE_NO`, `DEPOSIT_BANK`, `STATUS`, `ENTRY_DATE`, `disclaimer`, `PAYMENTSTATUS`, `Challan_No`, `DueDate`, `Bill_Ndc`, `Bill_No`)
SELECT 'DUMYPAY0003', 'C0010003', 'Rahul Gupta', '21/C-310', DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 1525.00, 'rahul@test.com', '9876500003', 'ICICI', '1', DATE_SUB(NOW(), INTERVAL 2 DAY), 'Y', 'N', 'CHD003', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 7 DAY), '%d-%m-%Y'), 'BILL', 'WB260503'
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_master` WHERE `JALREFID` = 'DUMYPAY0003');

INSERT INTO `jalnoida_bankpay_master`
(`JALREFID`, `CONSID`, `CONS_NAME`, `CONS_PROPERTY`, `DATE_FROM`, `DATE_TO`, `PAYAMOUNT`, `EMAIL_ID`, `MOBILE_NO`, `DEPOSIT_BANK`, `STATUS`, `ENTRY_DATE`, `disclaimer`, `PAYMENTSTATUS`, `Challan_No`, `DueDate`, `Bill_Ndc`, `Bill_No`)
SELECT 'DUMYPAY0004', 'C0010004', 'Neha Singh', '31/D-017', DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 700.00, 'neha@test.com', '9876500004', 'SBI', '1', DATE_SUB(NOW(), INTERVAL 1 DAY), 'Y', 'F', 'CHD004', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 6 DAY), '%d-%m-%Y'), 'BILL', 'WB260504'
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_master` WHERE `JALREFID` = 'DUMYPAY0004');

INSERT INTO `jalnoida_bankpay_master`
(`JALREFID`, `CONSID`, `CONS_NAME`, `CONS_PROPERTY`, `DATE_FROM`, `DATE_TO`, `PAYAMOUNT`, `EMAIL_ID`, `MOBILE_NO`, `DEPOSIT_BANK`, `STATUS`, `ENTRY_DATE`, `disclaimer`, `PAYMENTSTATUS`, `Challan_No`, `DueDate`, `Bill_Ndc`, `Bill_No`)
SELECT 'DUMYPAY0005', 'C0010005', 'Sanjay Kumar', '44/E-022', DATE_SUB(CURDATE(), INTERVAL 60 DAY), DATE_SUB(CURDATE(), INTERVAL 30 DAY), 2100.00, 'sanjay@test.com', '9876500005', 'PAYTM', '1', NOW(), 'Y', 'Y', 'CHD005', DATE_FORMAT(DATE_ADD(CURDATE(), INTERVAL 5 DAY), '%d-%m-%Y'), 'NDC', 'WB260505'
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_master` WHERE `JALREFID` = 'DUMYPAY0005');

INSERT INTO `jalnoida_bankpay_tran`
(`JALREFID`, `MerchantID`, `TrxReferenceNo`, `BankReferenceNo`, `TxnAmount`, `BankID`, `BankMerchantID`, `TCNType`, `CurrencyName`, `ItemCode`, `SecurityType`, `SecurityID`, `SecurityPassword`, `TxnDate`, `AuthStatus`, `SettlementType`, `AdditionalInfo1`, `AdditionalInfo2`, `AdditionalInfo3`, `AdditionalInfo4`, `AdditionalInfo5`, `AdditionalInfo6`, `AdditionalInfo7`, `ErrorStatus`, `ErrorDescription`, `CheckSum1`, `STATUS`, `ENTRY_DATE`)
SELECT 'DUMYPAY0001', 'NOIDAJAL', 'TXN-DEMO-0001', 'BRN0001', '1250.00', 'AXIS', 'AXISMER', 'SALE', 'INR', 'WATER', 'SHA', 'SEC01', 'NA', DATE_FORMAT(NOW(), '%d-%m-%Y'), 'SUCCESS', 'AUTO', 'C0010001', 'WB260501', NULL, NULL, NULL, NULL, NULL, '0', 'Payment successful', 'CHK001', '1', DATE_SUB(NOW(), INTERVAL 4 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_tran` WHERE `JALREFID` = 'DUMYPAY0001' AND `TrxReferenceNo` = 'TXN-DEMO-0001');

INSERT INTO `jalnoida_bankpay_tran`
(`JALREFID`, `MerchantID`, `TrxReferenceNo`, `BankReferenceNo`, `TxnAmount`, `BankID`, `BankMerchantID`, `TCNType`, `CurrencyName`, `ItemCode`, `SecurityType`, `SecurityID`, `SecurityPassword`, `TxnDate`, `AuthStatus`, `SettlementType`, `AdditionalInfo1`, `AdditionalInfo2`, `AdditionalInfo3`, `AdditionalInfo4`, `AdditionalInfo5`, `AdditionalInfo6`, `AdditionalInfo7`, `ErrorStatus`, `ErrorDescription`, `CheckSum1`, `STATUS`, `ENTRY_DATE`)
SELECT 'DUMYPAY0002', 'NOIDAJAL', 'TXN-DEMO-0002', 'BRN0002', '980.00', 'HDFC', 'HDFCMER', 'SALE', 'INR', 'WATER', 'SHA', 'SEC02', 'NA', DATE_FORMAT(NOW(), '%d-%m-%Y'), 'PENDING', 'AUTO', 'C0010002', 'WB260502', NULL, NULL, NULL, NULL, NULL, '100', 'Awaiting bank response', 'CHK002', '1', DATE_SUB(NOW(), INTERVAL 3 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_tran` WHERE `JALREFID` = 'DUMYPAY0002' AND `TrxReferenceNo` = 'TXN-DEMO-0002');

INSERT INTO `jalnoida_bankpay_tran`
(`JALREFID`, `MerchantID`, `TrxReferenceNo`, `BankReferenceNo`, `TxnAmount`, `BankID`, `BankMerchantID`, `TCNType`, `CurrencyName`, `ItemCode`, `SecurityType`, `SecurityID`, `SecurityPassword`, `TxnDate`, `AuthStatus`, `SettlementType`, `AdditionalInfo1`, `AdditionalInfo2`, `AdditionalInfo3`, `AdditionalInfo4`, `AdditionalInfo5`, `AdditionalInfo6`, `AdditionalInfo7`, `ErrorStatus`, `ErrorDescription`, `CheckSum1`, `STATUS`, `ENTRY_DATE`)
SELECT 'DUMYPAY0003', 'NOIDAJAL', 'TXN-DEMO-0003', 'BRN0003', '1525.00', 'ICICI', 'ICICIMER', 'SALE', 'INR', 'WATER', 'SHA', 'SEC03', 'NA', DATE_FORMAT(NOW(), '%d-%m-%Y'), 'FAILED', 'AUTO', 'C0010003', 'WB260503', NULL, NULL, NULL, NULL, NULL, '300', 'Payment failed by bank', 'CHK003', '1', DATE_SUB(NOW(), INTERVAL 2 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_tran` WHERE `JALREFID` = 'DUMYPAY0003' AND `TrxReferenceNo` = 'TXN-DEMO-0003');

INSERT INTO `jalnoida_bankpay_tran`
(`JALREFID`, `MerchantID`, `TrxReferenceNo`, `BankReferenceNo`, `TxnAmount`, `BankID`, `BankMerchantID`, `TCNType`, `CurrencyName`, `ItemCode`, `SecurityType`, `SecurityID`, `SecurityPassword`, `TxnDate`, `AuthStatus`, `SettlementType`, `AdditionalInfo1`, `AdditionalInfo2`, `AdditionalInfo3`, `AdditionalInfo4`, `AdditionalInfo5`, `AdditionalInfo6`, `AdditionalInfo7`, `ErrorStatus`, `ErrorDescription`, `CheckSum1`, `STATUS`, `ENTRY_DATE`)
SELECT 'DUMYPAY0004', 'NOIDAJAL', 'TXN-DEMO-0004', 'BRN0004', '700.00', 'SBI', 'SBIMER', 'SALE', 'INR', 'WATER', 'SHA', 'SEC04', 'NA', DATE_FORMAT(NOW(), '%d-%m-%Y'), 'CANCELLED', 'AUTO', 'C0010004', 'WB260504', NULL, NULL, NULL, NULL, NULL, '400', 'User cancelled transaction', 'CHK004', '1', DATE_SUB(NOW(), INTERVAL 1 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_tran` WHERE `JALREFID` = 'DUMYPAY0004' AND `TrxReferenceNo` = 'TXN-DEMO-0004');

INSERT INTO `jalnoida_bankpay_tran`
(`JALREFID`, `MerchantID`, `TrxReferenceNo`, `BankReferenceNo`, `TxnAmount`, `BankID`, `BankMerchantID`, `TCNType`, `CurrencyName`, `ItemCode`, `SecurityType`, `SecurityID`, `SecurityPassword`, `TxnDate`, `AuthStatus`, `SettlementType`, `AdditionalInfo1`, `AdditionalInfo2`, `AdditionalInfo3`, `AdditionalInfo4`, `AdditionalInfo5`, `AdditionalInfo6`, `AdditionalInfo7`, `ErrorStatus`, `ErrorDescription`, `CheckSum1`, `STATUS`, `ENTRY_DATE`)
SELECT 'DUMYPAY0005', 'NOIDAJAL', 'TXN-DEMO-0005', 'BRN0005', '2100.00', 'PAYTM', 'PAYTMMER', 'SALE', 'INR', 'NDC', 'SHA', 'SEC05', 'NA', DATE_FORMAT(NOW(), '%d-%m-%Y'), 'SUCCESS', 'AUTO', 'C0010005', 'WB260505', NULL, NULL, NULL, NULL, NULL, '0', 'NDC fee paid', 'CHK005', '1', NOW()
WHERE NOT EXISTS (SELECT 1 FROM `jalnoida_bankpay_tran` WHERE `JALREFID` = 'DUMYPAY0005' AND `TrxReferenceNo` = 'TXN-DEMO-0005');

-- ---------------------------------------------------------------------------
-- 2. NDC applications demo data: consumer_apply_ndc, ndc_document
-- ---------------------------------------------------------------------------

INSERT INTO `consumer_apply_ndc`
(`CONSUMER_NO`, `APPLICATION_NO`, `RID`, `TRACKING_ID`, `ORDER_ID`, `CHALLAN_NO`, `AMOUNT`, `CONS_NAME`, `MOBILE_NO`, `EMAIL`, `DIVISION_TYPE_NAME`, `DIVISION_TYPE`, `SECTOR`, `BLOCK`, `PLOT_NO`, `PLOT_AREA`, `PIPE_SIZE`, `TYPE`, `STATUS`, `CURRENT_STATUS`, `FINAL_STATUS`, `CREATED_BY`, `CREATED_ON`, `LAST_UPDATED_ON`, `CONS_TP`, `CHALLAN_FILE`, `SUCCESS_STATUS`, `BILL_FROM_DATE`, `BILL_TO_DATE`, `ATTACHMENT1`, `ATTACHMENT2`, `ATTACHMENT3`, `BILL_NO`, `CERTIFICATE_URL`, `PAYMENT_SUCCESS_DATE`)
SELECT 'C0010001', 'NDCDEMO0001', 'RID-NDC-001', 'TRK-NDC-001', 'ORD-NDC-001', 'CHD001', 500.00, 'Amit Sharma', '9876500001', 'amit@test.com', 'JAL1', 1, '12', 'A', '101', '120', '15', 'WEB', 'Submitted', 'Submitted', NULL, @adminUserId, DATE_SUB(NOW(), INTERVAL 4 DAY), DATE_SUB(NOW(), INTERVAL 4 DAY), 'R', '/uploads/ndc/chd001.pdf', 'S', DATE_SUB(CURDATE(), INTERVAL 60 DAY), CURDATE(), '/uploads/ndc/ndc-demo-001-aadhaar.pdf', '/uploads/ndc/ndc-demo-001-sale.pdf', NULL, 'BILLNDC001', NULL, DATE_SUB(NOW(), INTERVAL 4 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0001');

INSERT INTO `consumer_apply_ndc`
(`CONSUMER_NO`, `APPLICATION_NO`, `RID`, `TRACKING_ID`, `ORDER_ID`, `CHALLAN_NO`, `AMOUNT`, `CONS_NAME`, `MOBILE_NO`, `EMAIL`, `DIVISION_TYPE_NAME`, `DIVISION_TYPE`, `SECTOR`, `BLOCK`, `PLOT_NO`, `PLOT_AREA`, `PIPE_SIZE`, `TYPE`, `STATUS`, `CURRENT_STATUS`, `FINAL_STATUS`, `CREATED_BY`, `CREATED_ON`, `LAST_UPDATED_ON`, `CONS_TP`, `CHALLAN_FILE`, `SUCCESS_STATUS`, `BILL_FROM_DATE`, `BILL_TO_DATE`, `ATTACHMENT1`, `ATTACHMENT2`, `ATTACHMENT3`, `BILL_NO`, `CERTIFICATE_URL`, `PAYMENT_SUCCESS_DATE`)
SELECT 'C0010002', 'NDCDEMO0002', 'RID-NDC-002', 'TRK-NDC-002', 'ORD-NDC-002', 'CHD002', 500.00, 'Priya Verma', '9876500002', 'priya@test.com', 'JAL2', 2, '18', 'B', '204', '180', '20', 'WEB', 'Submitted', 'Review', NULL, @adminUserId, DATE_SUB(NOW(), INTERVAL 3 DAY), DATE_SUB(NOW(), INTERVAL 2 DAY), 'R', '/uploads/ndc/chd002.pdf', 'S', DATE_SUB(CURDATE(), INTERVAL 60 DAY), CURDATE(), '/uploads/ndc/ndc-demo-002-aadhaar.pdf', '/uploads/ndc/ndc-demo-002-sale.pdf', NULL, 'BILLNDC002', NULL, DATE_SUB(NOW(), INTERVAL 3 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0002');

INSERT INTO `consumer_apply_ndc`
(`CONSUMER_NO`, `APPLICATION_NO`, `RID`, `TRACKING_ID`, `ORDER_ID`, `CHALLAN_NO`, `AMOUNT`, `CONS_NAME`, `MOBILE_NO`, `EMAIL`, `DIVISION_TYPE_NAME`, `DIVISION_TYPE`, `SECTOR`, `BLOCK`, `PLOT_NO`, `PLOT_AREA`, `PIPE_SIZE`, `TYPE`, `STATUS`, `CURRENT_STATUS`, `FINAL_STATUS`, `CREATED_BY`, `CREATED_ON`, `LAST_UPDATED_ON`, `COMPLETED_DATE`, `CONS_TP`, `CHALLAN_FILE`, `SUCCESS_STATUS`, `BILL_FROM_DATE`, `BILL_TO_DATE`, `ATTACHMENT1`, `ATTACHMENT2`, `ATTACHMENT3`, `BILL_NO`, `CERTIFICATE_URL`, `PAYMENT_SUCCESS_DATE`)
SELECT 'C0010003', 'NDCDEMO0003', 'RID-NDC-003', 'TRK-NDC-003', 'ORD-NDC-003', 'CHD003', 500.00, 'Rahul Gupta', '9876500003', 'rahul@test.com', 'JAL3', 3, '21', 'C', '310', '240', '20', 'WEB', 'A', 'Approved', 'A', @adminUserId, DATE_SUB(NOW(), INTERVAL 8 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 'C', '/uploads/ndc/chd003.pdf', 'S', DATE_SUB(CURDATE(), INTERVAL 60 DAY), CURDATE(), '/uploads/ndc/ndc-demo-003-aadhaar.pdf', '/uploads/ndc/ndc-demo-003-sale.pdf', '/uploads/ndc/ndc-demo-003-photo.pdf', 'BILLNDC003', '/NdcCertificates/Print/3', DATE_SUB(NOW(), INTERVAL 8 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0003');

INSERT INTO `consumer_apply_ndc`
(`CONSUMER_NO`, `APPLICATION_NO`, `RID`, `TRACKING_ID`, `ORDER_ID`, `CHALLAN_NO`, `AMOUNT`, `CONS_NAME`, `MOBILE_NO`, `EMAIL`, `DIVISION_TYPE_NAME`, `DIVISION_TYPE`, `SECTOR`, `BLOCK`, `PLOT_NO`, `PLOT_AREA`, `PIPE_SIZE`, `TYPE`, `STATUS`, `CURRENT_STATUS`, `FINAL_STATUS`, `CREATED_BY`, `CREATED_ON`, `LAST_UPDATED_ON`, `COMPLETED_DATE`, `CONS_TP`, `CHALLAN_FILE`, `SUCCESS_STATUS`, `BILL_FROM_DATE`, `BILL_TO_DATE`, `ATTACHMENT1`, `ATTACHMENT2`, `ATTACHMENT3`, `BILL_NO`, `CERTIFICATE_URL`, `PAYMENT_SUCCESS_DATE`)
SELECT 'C0010004', 'NDCDEMO0004', 'RID-NDC-004', 'TRK-NDC-004', 'ORD-NDC-004', 'CHD004', 500.00, 'Neha Singh', '9876500004', 'neha@test.com', 'JAL1', 1, '31', 'D', '017', '90', '15', 'WEB', 'R', 'Rejected', 'R', @adminUserId, DATE_SUB(NOW(), INTERVAL 6 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 'R', '/uploads/ndc/chd004.pdf', 'S', DATE_SUB(CURDATE(), INTERVAL 60 DAY), CURDATE(), '/uploads/ndc/ndc-demo-004-aadhaar.pdf', '/uploads/ndc/ndc-demo-004-sale.pdf', NULL, 'BILLNDC004', NULL, DATE_SUB(NOW(), INTERVAL 6 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0004');

INSERT INTO `consumer_apply_ndc`
(`CONSUMER_NO`, `APPLICATION_NO`, `RID`, `TRACKING_ID`, `ORDER_ID`, `CHALLAN_NO`, `AMOUNT`, `CONS_NAME`, `MOBILE_NO`, `EMAIL`, `DIVISION_TYPE_NAME`, `DIVISION_TYPE`, `SECTOR`, `BLOCK`, `PLOT_NO`, `PLOT_AREA`, `PIPE_SIZE`, `TYPE`, `STATUS`, `CURRENT_STATUS`, `FINAL_STATUS`, `CREATED_BY`, `CREATED_ON`, `LAST_UPDATED_ON`, `CONS_TP`, `CHALLAN_FILE`, `SUCCESS_STATUS`, `BILL_FROM_DATE`, `BILL_TO_DATE`, `ATTACHMENT1`, `ATTACHMENT2`, `ATTACHMENT3`, `BILL_NO`, `CERTIFICATE_URL`, `PAYMENT_SUCCESS_DATE`)
SELECT 'C0010005', 'NDCDEMO0005', 'RID-NDC-005', 'TRK-NDC-005', 'ORD-NDC-005', 'CHD005', 500.00, 'Sanjay Kumar', '9876500005', 'sanjay@test.com', 'JAL2', 2, '44', 'E', '022', '300', '25', 'WEB', 'Submitted', 'Review', NULL, @adminUserId, DATE_SUB(NOW(), INTERVAL 2 DAY), NOW(), 'I', '/uploads/ndc/chd005.pdf', 'S', DATE_SUB(CURDATE(), INTERVAL 60 DAY), CURDATE(), '/uploads/ndc/ndc-demo-005-aadhaar.pdf', '/uploads/ndc/ndc-demo-005-sale.pdf', '/uploads/ndc/ndc-demo-005-photo.pdf', 'BILLNDC005', NULL, DATE_SUB(NOW(), INTERVAL 2 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0005');

SET @ndc1 := (SELECT `AUTO_ID` FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0001' LIMIT 1);
SET @ndc2 := (SELECT `AUTO_ID` FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0002' LIMIT 1);
SET @ndc3 := (SELECT `AUTO_ID` FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0003' LIMIT 1);
SET @ndc4 := (SELECT `AUTO_ID` FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0004' LIMIT 1);
SET @ndc5 := (SELECT `AUTO_ID` FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` = 'NDCDEMO0005' LIMIT 1);

INSERT INTO `ndc_document` (`CONSUMER_NO`, `STATUS`, `ATTACHMENT_PATH`, `ATTACHMENT_NAME`, `CREATED_ON`, `NDC_AUTO_ID`)
SELECT 'C0010001', 1, '/uploads/ndc/ndc-demo-001-aadhaar.pdf', 'Aadhaar / Identity Proof', DATE_SUB(NOW(), INTERVAL 4 DAY), @ndc1
WHERE NOT EXISTS (SELECT 1 FROM `ndc_document` WHERE `NDC_AUTO_ID` = @ndc1 AND `ATTACHMENT_NAME` = 'Aadhaar / Identity Proof');

INSERT INTO `ndc_document` (`CONSUMER_NO`, `STATUS`, `ATTACHMENT_PATH`, `ATTACHMENT_NAME`, `CREATED_ON`, `NDC_AUTO_ID`)
SELECT 'C0010002', 1, '/uploads/ndc/ndc-demo-002-sale.pdf', 'Ownership / Sale Deed', DATE_SUB(NOW(), INTERVAL 3 DAY), @ndc2
WHERE NOT EXISTS (SELECT 1 FROM `ndc_document` WHERE `NDC_AUTO_ID` = @ndc2 AND `ATTACHMENT_NAME` = 'Ownership / Sale Deed');

INSERT INTO `ndc_document` (`CONSUMER_NO`, `STATUS`, `ATTACHMENT_PATH`, `ATTACHMENT_NAME`, `CREATED_ON`, `NDC_AUTO_ID`)
SELECT 'C0010003', 1, '/uploads/ndc/ndc-demo-003-photo.pdf', 'Applicant Photo', DATE_SUB(NOW(), INTERVAL 8 DAY), @ndc3
WHERE NOT EXISTS (SELECT 1 FROM `ndc_document` WHERE `NDC_AUTO_ID` = @ndc3 AND `ATTACHMENT_NAME` = 'Applicant Photo');

INSERT INTO `ndc_document` (`CONSUMER_NO`, `STATUS`, `ATTACHMENT_PATH`, `ATTACHMENT_NAME`, `CREATED_ON`, `NDC_AUTO_ID`)
SELECT 'C0010004', 1, '/uploads/ndc/ndc-demo-004-sale.pdf', 'Ownership / Sale Deed', DATE_SUB(NOW(), INTERVAL 6 DAY), @ndc4
WHERE NOT EXISTS (SELECT 1 FROM `ndc_document` WHERE `NDC_AUTO_ID` = @ndc4 AND `ATTACHMENT_NAME` = 'Ownership / Sale Deed');

INSERT INTO `ndc_document` (`CONSUMER_NO`, `STATUS`, `ATTACHMENT_PATH`, `ATTACHMENT_NAME`, `CREATED_ON`, `NDC_AUTO_ID`)
SELECT 'C0010005', 1, '/uploads/ndc/ndc-demo-005-aadhaar.pdf', 'Aadhaar / Identity Proof', DATE_SUB(NOW(), INTERVAL 2 DAY), @ndc5
WHERE NOT EXISTS (SELECT 1 FROM `ndc_document` WHERE `NDC_AUTO_ID` = @ndc5 AND `ATTACHMENT_NAME` = 'Aadhaar / Identity Proof');

-- ---------------------------------------------------------------------------
-- 3. NDC workflow demo configuration and workflow records.
--    Uses role-based assignment so any Admin user can see assigned demo pending tasks.
-- ---------------------------------------------------------------------------

INSERT INTO `WorkflowMasters` (`WorkflowName`, `ApplicationType`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT 'Demo NDC Approval Workflow', 'NDC', 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowMasters` WHERE `WorkflowName` = 'Demo NDC Approval Workflow' AND `ApplicationType` = 'NDC' AND `IsDeleted` = 0);

SET @workflowId := (SELECT `Id` FROM `WorkflowMasters` WHERE `WorkflowName` = 'Demo NDC Approval Workflow' AND `ApplicationType` = 'NDC' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @workflowId, 'Initial Scrutiny', 1, @demoDeptId, @adminRoleId, NULL, 'AnyOne', 1, 1, 0, 1, 0, 1, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 1 AND `IsDeleted` = 0);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @workflowId, 'Revenue Verification', 2, @demoDeptId, @adminRoleId, NULL, 'AnyOne', 1, 1, 0, 1, 0, 2, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 2 AND `IsDeleted` = 0);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @workflowId, 'Document Verification', 3, @demoDeptId, @adminRoleId, NULL, 'AnyOne', 1, 1, 0, 1, 0, 2, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 3 AND `IsDeleted` = 0);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @workflowId, 'Senior Review', 4, @demoDeptId, @adminRoleId, NULL, 'AnyOne', 1, 1, 0, 1, 0, 2, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 4 AND `IsDeleted` = 0);

INSERT INTO `WorkflowStages`
(`WorkflowId`, `StageName`, `StageOrder`, `DepartmentId`, `ApproverRoleId`, `ApproverUserId`, `ApprovalType`, `CanApprove`, `CanReject`, `CanSendCorrection`, `CanForward`, `IsFinalStage`, `SlaDays`, `IsActive`, `IsDeleted`, `CreatedOn`)
SELECT @workflowId, 'Final Approval', 5, @demoDeptId, @adminRoleId, NULL, 'AnyOne', 1, 1, 0, 0, 1, 1, 1, 0, NOW()
WHERE NOT EXISTS (SELECT 1 FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 5 AND `IsDeleted` = 0);

SET @stage1 := (SELECT `Id` FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 1 AND `IsDeleted` = 0 LIMIT 1);
SET @stage2 := (SELECT `Id` FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 2 AND `IsDeleted` = 0 LIMIT 1);
SET @stage3 := (SELECT `Id` FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 3 AND `IsDeleted` = 0 LIMIT 1);
SET @stage4 := (SELECT `Id` FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 4 AND `IsDeleted` = 0 LIMIT 1);
SET @stage5 := (SELECT `Id` FROM `WorkflowStages` WHERE `WorkflowId` = @workflowId AND `StageOrder` = 5 AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `ApplicationWorkflowInstances` (`ApplicationId`, `ApplicationNo`, `ApplicationType`, `WorkflowId`, `CurrentStageId`, `CurrentStatus`, `StartedOn`, `CompletedOn`, `IsActive`, `IsDeleted`)
SELECT @ndc1, 'NDCDEMO0001', 'NDC', @workflowId, @stage1, 'Submitted', DATE_SUB(NOW(), INTERVAL 4 DAY), NULL, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0001' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowInstances` (`ApplicationId`, `ApplicationNo`, `ApplicationType`, `WorkflowId`, `CurrentStageId`, `CurrentStatus`, `StartedOn`, `CompletedOn`, `IsActive`, `IsDeleted`)
SELECT @ndc2, 'NDCDEMO0002', 'NDC', @workflowId, @stage2, 'Review', DATE_SUB(NOW(), INTERVAL 3 DAY), NULL, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0002' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowInstances` (`ApplicationId`, `ApplicationNo`, `ApplicationType`, `WorkflowId`, `CurrentStageId`, `CurrentStatus`, `StartedOn`, `CompletedOn`, `IsActive`, `IsDeleted`)
SELECT @ndc3, 'NDCDEMO0003', 'NDC', @workflowId, @stage5, 'Approved', DATE_SUB(NOW(), INTERVAL 8 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0003' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowInstances` (`ApplicationId`, `ApplicationNo`, `ApplicationType`, `WorkflowId`, `CurrentStageId`, `CurrentStatus`, `StartedOn`, `CompletedOn`, `IsActive`, `IsDeleted`)
SELECT @ndc4, 'NDCDEMO0004', 'NDC', @workflowId, @stage2, 'Rejected', DATE_SUB(NOW(), INTERVAL 6 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0004' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowInstances` (`ApplicationId`, `ApplicationNo`, `ApplicationType`, `WorkflowId`, `CurrentStageId`, `CurrentStatus`, `StartedOn`, `CompletedOn`, `IsActive`, `IsDeleted`)
SELECT @ndc5, 'NDCDEMO0005', 'NDC', @workflowId, @stage5, 'Review', DATE_SUB(NOW(), INTERVAL 2 DAY), NULL, 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0005' AND `IsDeleted` = 0);

SET @inst1 := (SELECT `Id` FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0001' AND `IsDeleted` = 0 LIMIT 1);
SET @inst2 := (SELECT `Id` FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0002' AND `IsDeleted` = 0 LIMIT 1);
SET @inst3 := (SELECT `Id` FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0003' AND `IsDeleted` = 0 LIMIT 1);
SET @inst4 := (SELECT `Id` FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0004' AND `IsDeleted` = 0 LIMIT 1);
SET @inst5 := (SELECT `Id` FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` = 'NDCDEMO0005' AND `IsDeleted` = 0 LIMIT 1);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst1, @ndc1, 'NDCDEMO0001', @stage1, @demoDeptId, @adminRoleId, NULL, 'Pending', DATE_SUB(NOW(), INTERVAL 4 DAY), NULL, 'Pending initial scrutiny.', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst1 AND `StageId` = @stage1 AND `Status` = 'Pending' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst2, @ndc2, 'NDCDEMO0002', @stage1, @demoDeptId, @adminRoleId, NULL, 'Approved', DATE_SUB(NOW(), INTERVAL 3 DAY), DATE_SUB(NOW(), INTERVAL 2 DAY), 'Initial scrutiny completed.', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst2 AND `StageId` = @stage1 AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst2, @ndc2, 'NDCDEMO0002', @stage2, @demoDeptId, @adminRoleId, NULL, 'Pending', DATE_SUB(NOW(), INTERVAL 2 DAY), NULL, 'Pending revenue verification.', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst2 AND `StageId` = @stage2 AND `Status` = 'Pending' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst3, @ndc3, 'NDCDEMO0003', @stage5, @demoDeptId, @adminRoleId, NULL, 'Approved', DATE_SUB(NOW(), INTERVAL 2 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 'Final NDC approval completed.', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst3 AND `StageId` = @stage5 AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst4, @ndc4, 'NDCDEMO0004', @stage2, @demoDeptId, @adminRoleId, NULL, 'Rejected', DATE_SUB(NOW(), INTERVAL 5 DAY), DATE_SUB(NOW(), INTERVAL 1 DAY), 'Dues mismatch found.', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst4 AND `StageId` = @stage2 AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowTasks` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `AssignedDepartmentId`, `AssignedRoleId`, `AssignedUserId`, `Status`, `AssignedOn`, `ActionOn`, `Remarks`, `IsActive`, `IsDeleted`)
SELECT @inst5, @ndc5, 'NDCDEMO0005', @stage5, @demoDeptId, @adminRoleId, NULL, 'Pending', DATE_SUB(NOW(), INTERVAL 1 DAY), NULL, 'Pending final approval.', 1, 0
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowTasks` WHERE `WorkflowInstanceId` = @inst5 AND `StageId` = @stage5 AND `Status` = 'Pending' AND `IsDeleted` = 0);

INSERT INTO `ApplicationWorkflowHistory` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionBy`, `ActionByName`, `ActionByRole`, `ActionOn`)
SELECT @inst1, @ndc1, 'NDCDEMO0001', @stage1, NULL, 'Submitted', 'Submitted', 'Demo NDC application submitted.', @adminUserId, 'Demo Admin', 'Admin', DATE_SUB(NOW(), INTERVAL 4 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowHistory` WHERE `WorkflowInstanceId` = @inst1 AND `Action` = 'Submitted');

INSERT INTO `ApplicationWorkflowHistory` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionBy`, `ActionByName`, `ActionByRole`, `ActionOn`)
SELECT @inst2, @ndc2, 'NDCDEMO0002', @stage1, 'Submitted', 'Approved', 'Approved', 'Initial scrutiny approved.', @adminUserId, 'Demo Admin', 'Admin', DATE_SUB(NOW(), INTERVAL 2 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowHistory` WHERE `WorkflowInstanceId` = @inst2 AND `StageId` = @stage1 AND `Action` = 'Approved');

INSERT INTO `ApplicationWorkflowHistory` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionBy`, `ActionByName`, `ActionByRole`, `ActionOn`)
SELECT @inst3, @ndc3, 'NDCDEMO0003', @stage5, 'Review', 'Approved', 'Approved', 'Final NDC approval completed.', @adminUserId, 'Demo Admin', 'Admin', DATE_SUB(NOW(), INTERVAL 1 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowHistory` WHERE `WorkflowInstanceId` = @inst3 AND `StageId` = @stage5 AND `Action` = 'Approved');

INSERT INTO `ApplicationWorkflowHistory` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionBy`, `ActionByName`, `ActionByRole`, `ActionOn`)
SELECT @inst4, @ndc4, 'NDCDEMO0004', @stage2, 'Review', 'Rejected', 'Rejected', 'Dues mismatch found during verification.', @adminUserId, 'Demo Admin', 'Admin', DATE_SUB(NOW(), INTERVAL 1 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowHistory` WHERE `WorkflowInstanceId` = @inst4 AND `StageId` = @stage2 AND `Action` = 'Rejected');

INSERT INTO `ApplicationWorkflowHistory` (`WorkflowInstanceId`, `ApplicationId`, `ApplicationNo`, `StageId`, `FromStatus`, `ToStatus`, `Action`, `Remarks`, `ActionBy`, `ActionByName`, `ActionByRole`, `ActionOn`)
SELECT @inst5, @ndc5, 'NDCDEMO0005', @stage5, 'Review', 'Pending', 'Assigned', 'Assigned to final approval stage.', @adminUserId, 'Demo Admin', 'Admin', DATE_SUB(NOW(), INTERVAL 1 DAY)
WHERE NOT EXISTS (SELECT 1 FROM `ApplicationWorkflowHistory` WHERE `WorkflowInstanceId` = @inst5 AND `StageId` = @stage5 AND `Action` = 'Assigned');

-- Quick verification counts.
SELECT 'jalnoida_bankpay_master' AS TableName, COUNT(*) AS DemoRows FROM `jalnoida_bankpay_master` WHERE `JALREFID` LIKE 'DUMYPAY%';
SELECT 'jalnoida_bankpay_tran' AS TableName, COUNT(*) AS DemoRows FROM `jalnoida_bankpay_tran` WHERE `JALREFID` LIKE 'DUMYPAY%';
SELECT 'consumer_apply_ndc' AS TableName, COUNT(*) AS DemoRows FROM `consumer_apply_ndc` WHERE `APPLICATION_NO` LIKE 'NDCDEMO%';
SELECT 'ndc_document' AS TableName, COUNT(*) AS DemoRows FROM `ndc_document` WHERE `NDC_AUTO_ID` IN (@ndc1, @ndc2, @ndc3, @ndc4, @ndc5);
SELECT 'ApplicationWorkflowInstances' AS TableName, COUNT(*) AS DemoRows FROM `ApplicationWorkflowInstances` WHERE `ApplicationType` = 'NDC' AND `ApplicationNo` LIKE 'NDCDEMO%';
SELECT 'ApplicationWorkflowTasks' AS TableName, COUNT(*) AS DemoRows FROM `ApplicationWorkflowTasks` WHERE `ApplicationNo` LIKE 'NDCDEMO%';
SELECT 'ApplicationWorkflowHistory' AS TableName, COUNT(*) AS DemoRows FROM `ApplicationWorkflowHistory` WHERE `ApplicationNo` LIKE 'NDCDEMO%';
