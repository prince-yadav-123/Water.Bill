-- Dummy meter readings for testing.
-- Run after meter-reading-management-module.sql.

INSERT INTO `ConsumerMeterReadings`
(`ReadingNo`, `ConsumerNo`, `ReadingDate`, `PeriodFrom`, `PeriodTo`, `PreviousReading`, `CurrentReading`, `Consumption`, `MeterStatus`, `MeterNo`, `Remarks`, `Source`, `RecordedByName`, `RecordedAt`, `IsActive`, `IsDeleted`)
SELECT CONCAT('MRDEMO', LPAD(x.seq, 4, '0')),
       c.`CONS_NO`,
       DATE_SUB(CURDATE(), INTERVAL x.days_back DAY),
       DATE_SUB(CURDATE(), INTERVAL (x.days_back + 30) DAY),
       DATE_SUB(CURDATE(), INTERVAL x.days_back DAY),
       x.prev_reading,
       x.curr_reading,
       GREATEST(x.curr_reading - x.prev_reading, 0),
       x.status_value,
       CONCAT('MTR-', c.`CONS_NO`),
       x.remarks,
       'Admin',
       'Demo Admin',
       NOW(),
       1,
       0
FROM (
    SELECT 1 AS seq, 120 AS days_back, 100.00 AS prev_reading, 135.00 AS curr_reading, 'Normal' AS status_value, 'Opening demo reading.' AS remarks
    UNION ALL SELECT 2, 90, 135.00, 171.00, 'Normal', 'Regular monthly reading.'
    UNION ALL SELECT 3, 60, 171.00, 171.00, 'Locked', 'Premises locked during visit.'
    UNION ALL SELECT 4, 30, 171.00, 205.00, 'Average', 'Average reading entered after locked visit.'
    UNION ALL SELECT 5, 5, 205.00, 244.00, 'Normal', 'Latest demo reading.'
) x
JOIN (
    SELECT `CONS_NO`, ROW_NUMBER() OVER (ORDER BY `CONS_NO`) AS rn
    FROM `consumer_details_master`
    WHERE `STATUS` = 1
    LIMIT 5
) c ON c.rn = x.seq
WHERE NOT EXISTS (
    SELECT 1 FROM `ConsumerMeterReadings` m
    WHERE m.`ReadingNo` = CONCAT('MRDEMO', LPAD(x.seq, 4, '0'))
);
