-- New Connection master seed data for Water.Bill
-- Safe one-time seed for existing master tables used by the dynamic New Connection form.
-- This script does not truncate or delete existing data.

-- If the New Connection staging table was created with older short lengths,
-- these ALTER statements keep it compatible with dynamic master values.
ALTER TABLE new_connection_applications
    MODIFY COLUMN ConnectionCategory varchar(4) NOT NULL,
    MODIFY COLUMN FlatType varchar(50) NOT NULL;

-- Sector Master: sector_detail
INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '1', '1', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '1' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '12', '12', 1, 2, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '12' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '18', '18', 1, 3, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '18' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '50', '50', 1, 4, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '50' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '62', '62', 1, 5, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '62' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '76', '76', 1, 6, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '76' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '100', '100', 1, 7, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '100' AND DEV_TYPE = 1);

INSERT INTO sector_detail (Sector_id, sector_no, status, ORDER_BY, DEV_TYPE)
SELECT '135', '135', 1, 8, 1
WHERE NOT EXISTS (SELECT 1 FROM sector_detail WHERE Sector_id = '135' AND DEV_TYPE = 1);

-- Block Master: block_detail
INSERT INTO block_detail (sector_id, block, status, DEV_TYPE) VALUES
('1', 'A', 1, 1), ('1', 'B', 1, 1), ('1', 'C', 1, 1),
('12', 'A', 1, 1), ('12', 'B', 1, 1), ('12', 'C', 1, 1),
('18', 'A', 1, 1), ('18', 'B', 1, 1), ('18', 'Market', 1, 1),
('50', 'A', 1, 1), ('50', 'B', 1, 1), ('50', 'C', 1, 1),
('62', 'A', 1, 1), ('62', 'B', 1, 1),
('76', 'A', 1, 1), ('76', 'B', 1, 1),
('100', 'A', 1, 1), ('100', 'B', 1, 1),
('135', 'A', 1, 1), ('135', 'B', 1, 1)
ON DUPLICATE KEY UPDATE status = VALUES(status), DEV_TYPE = VALUES(DEV_TYPE);

-- Pipe Size Master: pipe_size_master
INSERT INTO pipe_size_master (PIPE_SIZE_ID, PIPE_SIZE, STATUS, DEV_TYPE)
SELECT 1, 15, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM pipe_size_master WHERE PIPE_SIZE = 15 AND DEV_TYPE = 1);

INSERT INTO pipe_size_master (PIPE_SIZE_ID, PIPE_SIZE, STATUS, DEV_TYPE)
SELECT 2, 20, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM pipe_size_master WHERE PIPE_SIZE = 20 AND DEV_TYPE = 1);

INSERT INTO pipe_size_master (PIPE_SIZE_ID, PIPE_SIZE, STATUS, DEV_TYPE)
SELECT 3, 25, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM pipe_size_master WHERE PIPE_SIZE = 25 AND DEV_TYPE = 1);

INSERT INTO pipe_size_master (PIPE_SIZE_ID, PIPE_SIZE, STATUS, DEV_TYPE)
SELECT 4, 40, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM pipe_size_master WHERE PIPE_SIZE = 40 AND DEV_TYPE = 1);

-- Connection Category Master: master_connection_type_details
INSERT INTO master_connection_type_details (CON_ID, CON_NAME, CON_MAIN_ID, STATUS, DEV_TYPE)
SELECT 'R', 'Residential', 'R', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details WHERE CON_ID = 'R' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details (CON_ID, CON_NAME, CON_MAIN_ID, STATUS, DEV_TYPE)
SELECT 'C', 'Commercial', 'C', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details WHERE CON_ID = 'C' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details (CON_ID, CON_NAME, CON_MAIN_ID, STATUS, DEV_TYPE)
SELECT 'I', 'Industrial', 'I', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details WHERE CON_ID = 'I' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details (CON_ID, CON_NAME, CON_MAIN_ID, STATUS, DEV_TYPE)
SELECT 'N', 'Institutional', 'N', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details WHERE CON_ID = 'N' AND DEV_TYPE = 1);

-- Connection Type Master: connection_type_mst
INSERT INTO connection_type_mst (AUTO_ID, CONNECTION_NAME, CONNECTION_MAIN_ID, STATUS, CREATED_ON)
VALUES
(1, 'Regular', 'R', 1, NOW()),
(2, 'Temporary', 'T', 1, NOW()),
(3, 'RMC', 'M', 1, NOW()),
(4, 'Staff', 'S', 1, NOW())
ON DUPLICATE KEY UPDATE CONNECTION_NAME = VALUES(CONNECTION_NAME), CONNECTION_MAIN_ID = VALUES(CONNECTION_MAIN_ID), STATUS = VALUES(STATUS);

-- Connection Sub-Type / Flat Type Master: master_connection_type_details_trans
INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 1, 'R', 'Flat', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'R' AND SUB_CON_NAME = 'Flat' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 2, 'R', 'House', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'R' AND SUB_CON_NAME = 'House' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 3, 'R', 'Plot', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'R' AND SUB_CON_NAME = 'Plot' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 4, 'C', 'Shop', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'C' AND SUB_CON_NAME = 'Shop' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 5, 'C', 'Office', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'C' AND SUB_CON_NAME = 'Office' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 6, 'I', 'Factory', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'I' AND SUB_CON_NAME = 'Factory' AND DEV_TYPE = 1);

INSERT INTO master_connection_type_details_trans (SUB_CON_ID, CON_ID, SUB_CON_NAME, STATUS, DEV_TYPE)
SELECT 7, 'N', 'Institution', '1', 1
WHERE NOT EXISTS (SELECT 1 FROM master_connection_type_details_trans WHERE CON_ID = 'N' AND SUB_CON_NAME = 'Institution' AND DEV_TYPE = 1);

-- Document Type Master: master_document_upload
INSERT INTO master_document_upload (Document_id, Document_Name, status, Doc_for) VALUES
(1, 'Allotment Letter', 1, 'NCH'),
(2, 'Possession Letter', 1, 'NCH'),
(3, 'Compliance Letter', 1, 'NCH'),
(4, 'SSI Letter', 1, 'NCH'),
(5, 'Affidavit', 1, 'NCH'),
(6, 'ID Proof', 1, 'NCH'),
(7, 'Address Proof', 1, 'NCH'),
(8, 'Property Document', 1, 'NCH'),
(9, 'Other', 1, 'NCH')
ON DUPLICATE KEY UPDATE Document_Name = VALUES(Document_Name), status = VALUES(status), Doc_for = VALUES(Doc_for);

-- Village Master: village_detail
INSERT INTO village_detail (Village_no, Village_id, Village_Name, Village_str, status, DEV_TYPE)
SELECT 1, 1, 'Pachrukhi', 'PAC', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM village_detail WHERE Village_id = 1 AND DEV_TYPE = 1);

INSERT INTO village_detail (Village_no, Village_id, Village_Name, Village_str, status, DEV_TYPE)
SELECT 2, 2, 'Baraula', 'BAR', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM village_detail WHERE Village_id = 2 AND DEV_TYPE = 1);

INSERT INTO village_detail (Village_no, Village_id, Village_Name, Village_str, status, DEV_TYPE)
SELECT 3, 3, 'Sarfabad', 'SAR', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM village_detail WHERE Village_id = 3 AND DEV_TYPE = 1);

INSERT INTO village_detail (Village_no, Village_id, Village_Name, Village_str, status, DEV_TYPE)
SELECT 4, 4, 'Hoshiyarpur', 'HOS', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM village_detail WHERE Village_id = 4 AND DEV_TYPE = 1);
