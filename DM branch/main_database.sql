-- Sprint 2 Implementation (delete later)


-- Users table (for all user types)
CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    UserType ENUM('Driver', 'Sponsor', 'Admin') NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLogin TIMESTAMP
);

-- Sponsors table
CREATE TABLE Sponsors (
    SponsorID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT UNIQUE,
    CompanyName VARCHAR(100) NOT NULL,
    PointDollarValue DECIMAL(10, 2) DEFAULT 0.01,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Drivers table
CREATE TABLE Drivers (
    DriverID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT UNIQUE,
    SponsorID INT,
    TotalPoints INT DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
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

CREATE TABLE AuditLog (
    LogID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT,
    Action VARCHAR(50) NOT NULL,
    Category ENUM('User', 'Point', 'Purchase', 'Application', 'Product', 'System') NOT NULL,
    Description TEXT,
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);