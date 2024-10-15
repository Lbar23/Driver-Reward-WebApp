-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema Team16_GIDP_DB
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema Team16_GIDP_DB
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `Team16_GIDP_DB` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci ;
USE `Team16_GIDP_DB` ;

-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`About`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`About` (
  `Team` INT NOT NULL,
  `Version` INT NOT NULL,
  `Release` DATE NOT NULL,
  `Product` VARCHAR(45) NOT NULL,
  `Description` VARCHAR(500) NOT NULL,
  PRIMARY KEY (`Team`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`Users`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`Users` (
  `UserID` INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(50) NOT NULL,
  `PasswordHash` VARCHAR(255) NOT NULL,
  `Email` VARCHAR(100) NOT NULL,
  `UserType` ENUM('Driver', 'Sponsor', 'Admin') NOT NULL,
  `CreatedAt` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  `LastLogin` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE INDEX `Username` (`Username` ASC) VISIBLE,
  UNIQUE INDEX `Email` (`Email` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 3
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`AuditLog`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`AuditLog` (
  `LogID` INT NOT NULL AUTO_INCREMENT,
  `UserID` INT NULL DEFAULT NULL,
  `Action` ENUM('Login', 'PasswordChange', 'PointChange', 'Purchase', 'ApplicationStatus') NULL DEFAULT NULL,
  `Description` TEXT NULL DEFAULT NULL,
  `Timestamp` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LogID`),
  INDEX `UserID` (`UserID` ASC) VISIBLE,
  CONSTRAINT `AuditLog_ibfk_1`
    FOREIGN KEY (`UserID`)
    REFERENCES `Team16_GIDP_DB`.`Users` (`UserID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`Sponsors`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`Sponsors` (
  `SponsorID` INT NOT NULL AUTO_INCREMENT,
  `UserID` INT NULL DEFAULT NULL,
  `CompanyName` VARCHAR(100) NOT NULL,
  `PointDollarValue` DECIMAL(10,2) NULL DEFAULT '0.01',
  PRIMARY KEY (`SponsorID`),
  UNIQUE INDEX `UserID` (`UserID` ASC) VISIBLE,
  CONSTRAINT `Sponsors_ibfk_1`
    FOREIGN KEY (`UserID`)
    REFERENCES `Team16_GIDP_DB`.`Users` (`UserID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`Drivers`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`Drivers` (
  `DriverID` INT NOT NULL AUTO_INCREMENT,
  `UserID` INT NULL DEFAULT NULL,
  `SponsorID` INT NULL DEFAULT NULL,
  `TotalPoints` INT NULL DEFAULT '0',
  PRIMARY KEY (`DriverID`),
  UNIQUE INDEX `UserID` (`UserID` ASC) VISIBLE,
  INDEX `SponsorID` (`SponsorID` ASC) VISIBLE,
  CONSTRAINT `Drivers_ibfk_1`
    FOREIGN KEY (`UserID`)
    REFERENCES `Team16_GIDP_DB`.`Users` (`UserID`),
  CONSTRAINT `Drivers_ibfk_2`
    FOREIGN KEY (`SponsorID`)
    REFERENCES `Team16_GIDP_DB`.`Sponsors` (`SponsorID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`DriverApplications`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`DriverApplications` (
  `ApplicationID` INT NOT NULL AUTO_INCREMENT,
  `DriverID` INT NULL DEFAULT NULL,
  `SponsorID` INT NULL DEFAULT NULL,
  `Status` ENUM('Pending', 'Approved', 'Rejected') NULL DEFAULT 'Pending',
  `ApplyDate` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  `ProcessedDate` TIMESTAMP NULL DEFAULT NULL,
  `Reason` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`ApplicationID`),
  INDEX `DriverID` (`DriverID` ASC) VISIBLE,
  INDEX `SponsorID` (`SponsorID` ASC) VISIBLE,
  CONSTRAINT `DriverApplications_ibfk_1`
    FOREIGN KEY (`DriverID`)
    REFERENCES `Team16_GIDP_DB`.`Drivers` (`DriverID`),
  CONSTRAINT `DriverApplications_ibfk_2`
    FOREIGN KEY (`SponsorID`)
    REFERENCES `Team16_GIDP_DB`.`Sponsors` (`SponsorID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`PointTransactions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`PointTransactions` (
  `TransactionID` INT NOT NULL AUTO_INCREMENT,
  `DriverID` INT NULL DEFAULT NULL,
  `SponsorID` INT NULL DEFAULT NULL,
  `PointsChanged` INT NOT NULL,
  `Reason` TEXT NULL DEFAULT NULL,
  `TransactionDate` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`TransactionID`),
  INDEX `DriverID` (`DriverID` ASC) VISIBLE,
  INDEX `SponsorID` (`SponsorID` ASC) VISIBLE,
  CONSTRAINT `PointTransactions_ibfk_1`
    FOREIGN KEY (`DriverID`)
    REFERENCES `Team16_GIDP_DB`.`Drivers` (`DriverID`),
  CONSTRAINT `PointTransactions_ibfk_2`
    FOREIGN KEY (`SponsorID`)
    REFERENCES `Team16_GIDP_DB`.`Sponsors` (`SponsorID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`Products`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`Products` (
  `ProductID` INT NOT NULL AUTO_INCREMENT,
  `SponsorID` INT NULL DEFAULT NULL,
  `Name` VARCHAR(100) NOT NULL,
  `Description` TEXT NULL DEFAULT NULL,
  `PriceInPoints` INT NOT NULL,
  `ExternalID` VARCHAR(100) NULL DEFAULT NULL,
  `Availability` TINYINT(1) NULL DEFAULT '1',
  PRIMARY KEY (`ProductID`),
  INDEX `SponsorID` (`SponsorID` ASC) VISIBLE,
  CONSTRAINT `Products_ibfk_1`
    FOREIGN KEY (`SponsorID`)
    REFERENCES `Team16_GIDP_DB`.`Sponsors` (`SponsorID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `Team16_GIDP_DB`.`Purchases`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Team16_GIDP_DB`.`Purchases` (
  `PurchaseID` INT NOT NULL AUTO_INCREMENT,
  `DriverID` INT NULL DEFAULT NULL,
  `ProductID` INT NULL DEFAULT NULL,
  `PointsSpent` INT NOT NULL,
  `PurchaseDate` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
  `Status` ENUM('Pending', 'Completed', 'Cancelled') NULL DEFAULT 'Pending',
  PRIMARY KEY (`PurchaseID`),
  INDEX `DriverID` (`DriverID` ASC) VISIBLE,
  INDEX `ProductID` (`ProductID` ASC) VISIBLE,
  CONSTRAINT `Purchases_ibfk_1`
    FOREIGN KEY (`DriverID`)
    REFERENCES `Team16_GIDP_DB`.`Drivers` (`DriverID`),
  CONSTRAINT `Purchases_ibfk_2`
    FOREIGN KEY (`ProductID`)
    REFERENCES `Team16_GIDP_DB`.`Products` (`ProductID`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
