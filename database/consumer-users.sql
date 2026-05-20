CREATE TABLE IF NOT EXISTS `ConsumerUsers` (
  `Id` CHAR(36) NOT NULL,
  `ConsumerNo` VARCHAR(10) NOT NULL,
  `Username` VARCHAR(100) NOT NULL,
  `Email` VARCHAR(150) NULL,
  `PasswordHash` VARCHAR(512) NOT NULL,
  `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
  `FailedLoginCount` INT NOT NULL DEFAULT 0,
  `LockoutUntil` DATETIME NULL,
  `LastLoginAt` DATETIME NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` DATETIME NULL,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_ConsumerUsers_Username` (`Username`),
  UNIQUE KEY `UX_ConsumerUsers_Email` (`Email`),
  KEY `IX_ConsumerUsers_ConsumerNo` (`ConsumerNo`),
  CONSTRAINT `FK_ConsumerUsers_ConsumerDetailsMaster_ConsumerNo`
    FOREIGN KEY (`ConsumerNo`) REFERENCES `consumer_details_master` (`CONS_NO`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE
);

-- Optional sample insert.
-- Replace ConsumerNo with an existing value from consumer_details_master.
-- Password hash below must be generated with AuthService.HashPassword("your-password")
-- because the application uses the same salted SHA-256 hash format.
--
-- INSERT INTO `ConsumerUsers`
-- (`Id`, `ConsumerNo`, `Username`, `Email`, `PasswordHash`, `IsActive`, `CreatedAt`, `IsDeleted`)
-- VALUES
-- (UUID(), 'EXISTINGNO', 'consumer.user', 'consumer@example.com', 'GENERATED_PASSWORD_HASH', 1, NOW(), 0);
