CREATE SCHEMA IF NOT EXISTS `Team16_GIDP_DB` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci ;
USE `Team16_GIDP_DB` ;

-- IdentityTables
CREATE TABLE AspNetUsers (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserName VARCHAR(256) UNIQUE NOT NULL,
    NormalizedUserName VARCHAR(256) UNIQUE NOT NULL,
    Email VARCHAR(256) UNIQUE NOT NULL,
    NormalizedEmail VARCHAR(256),
    EmailConfirmed BIT NOT NULL,
    PasswordHash VARCHAR(255),
    SecurityStamp VARCHAR(255),
    ConcurrencyStamp VARCHAR(255),
    PhoneNumber VARCHAR(50),
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIME,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    UserType ENUM('Driver', 'Sponsor', 'Admin') NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLogin TIMESTAMP
);

CREATE TABLE AspNetRoles (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(256) UNIQUE NOT NULL,
    NormalizedName VARCHAR(256) UNIQUE NOT NULL,
    ConcurrencyStamp VARCHAR(255)
);

CREATE TABLE AspNetUserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);

-- AspNetUserClaims table
CREATE TABLE AspNetUserClaims (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    ClaimType VARCHAR(255),
    ClaimValue VARCHAR(255),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- AspNetUserLogins table
CREATE TABLE AspNetUserLogins (
    LoginProvider VARCHAR(128) NOT NULL,
    ProviderKey VARCHAR(128) NOT NULL,
    ProviderDisplayName VARCHAR(128),
    UserId INT NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- AspNetUserTokens table
CREATE TABLE AspNetUserTokens (
    UserId INT NOT NULL,
    LoginProvider VARCHAR(128) NOT NULL,
    Name VARCHAR(128) NOT NULL,
    Value VARCHAR(255),
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- AspNetRoleClaims table
CREATE TABLE AspNetRoleClaims (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    RoleId INT NOT NULL,
    ClaimType VARCHAR(255),
    ClaimValue VARCHAR(255),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

-- Updated Scripts below

-- Sponsors table
CREATE TABLE Sponsors (
    SponsorID INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT UNIQUE,
    CompanyName VARCHAR(100) NOT NULL,
    PointDollarValue DECIMAL(10, 2) DEFAULT 0.01,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Drivers table
CREATE TABLE Drivers (
    DriverID INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT UNIQUE,
    SponsorID INT,
    TotalPoints INT DEFAULT 0,
    Address VARCHAR(255),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (SponsorID) REFERENCES Sponsors(SponsorID)
);

-- Products table
CREATE TABLE Products (
    ProductID INT PRIMARY KEY AUTO_INCREMENT,
    SponsorID INT,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    PriceInPoints INT NOT NULL,
    ExternalID VARCHAR(100),
    Availability BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (SponsorID) REFERENCES Sponsors(SponsorID)
);

-- Purchases table
CREATE TABLE Purchases (
    PurchaseID INT PRIMARY KEY AUTO_INCREMENT,
    DriverID INT,
    ProductID INT,
    PointsSpent INT NOT NULL,
    PurchaseDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status ENUM('Pending', 'Completed', 'Cancelled') DEFAULT 'Pending',
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- PointTransactions table
CREATE TABLE PointTransactions (
    TransactionID INT PRIMARY KEY AUTO_INCREMENT,
    DriverID INT,
    SponsorID INT,
    PointsChanged INT NOT NULL,
    Reason TEXT,
    TransactionDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (SponsorID) REFERENCES Sponsors(SponsorID)
);

-- DriverApplications table
CREATE TABLE DriverApplications (
    ApplicationID INT PRIMARY KEY AUTO_INCREMENT,
    DriverID INT,
    SponsorID INT,
    Status ENUM('Pending', 'Approved', 'Rejected') DEFAULT 'Pending',
    ApplyDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ProcessedDate TIMESTAMP,
    Reason TEXT,
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (SponsorID) REFERENCES Sponsors(SponsorID)
);

-- AuditLog table
CREATE TABLE AuditLog (
    LogID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT,
    Action VARCHAR(50) NOT NULL,
    Category ENUM('User', 'Point', 'Purchase', 'Application', 'Product', 'System') NOT NULL,
    Description TEXT,
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserID) REFERENCES AspNetUsers(Id)
);

-- About table
CREATE TABLE About (
    Team INT PRIMARY KEY AUTO_INCREMENT,
    Version INT NOT NULL,
    `Release` DATE NOT NULL,
    `Product` VARCHAR(45) NOT NULL,
    `Description` VARCHAR(500) NOT NULL
);