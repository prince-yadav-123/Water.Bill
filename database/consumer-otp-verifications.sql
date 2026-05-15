CREATE TABLE IF NOT EXISTS `ConsumerOtpVerifications` (
    `Id` CHAR(36) NOT NULL,
    `ConsumerNo` VARCHAR(10) NOT NULL,
    `MobileNo` VARCHAR(12) NOT NULL,
    `OtpHash` VARCHAR(128) NOT NULL,
    `OtpSalt` VARCHAR(64) NOT NULL,
    `Purpose` VARCHAR(50) NOT NULL DEFAULT 'ConsumerLogin',
    `ExpiresAt` DATETIME(6) NOT NULL,
    `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `VerifiedAt` DATETIME(6) NULL,
    `AttemptCount` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT `PK_ConsumerOtpVerifications` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ConsumerOtpVerifications_ConsumerDetailsMaster_ConsumerNo`
        FOREIGN KEY (`ConsumerNo`) REFERENCES `consumer_details_master` (`CONS_NO`)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX `IX_ConsumerOtpVerifications_Consumer_Purpose_CreatedAt`
    ON `ConsumerOtpVerifications` (`ConsumerNo`, `Purpose`, `CreatedAt`);

CREATE INDEX `IX_ConsumerOtpVerifications_ActiveLookup`
    ON `ConsumerOtpVerifications` (`ConsumerNo`, `Purpose`, `IsActive`, `IsVerified`);
