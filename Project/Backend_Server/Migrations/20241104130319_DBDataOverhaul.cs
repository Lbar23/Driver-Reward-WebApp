using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class DBDataOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, disable foreign key checks and clear existing data
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

            // Clear all existing data except AspNetRoles
            migrationBuilder.Sql(@"
                TRUNCATE TABLE AspNetRoleClaims;
                TRUNCATE TABLE AspNetUserRoles;
                TRUNCATE TABLE AspNetUserClaims;
                TRUNCATE TABLE AspNetUserLogins;
                TRUNCATE TABLE AspNetUserTokens;
                TRUNCATE TABLE PointTransactions;
                TRUNCATE TABLE Purchases;
                TRUNCATE TABLE Products;
                TRUNCATE TABLE SponsorDrivers;
                TRUNCATE TABLE DriverApplications;
                TRUNCATE TABLE Drivers;
                TRUNCATE TABLE Sponsors;
                TRUNCATE TABLE Admins;
                TRUNCATE TABLE AuditLog;
                TRUNCATE TABLE FeedbackForms;
                DELETE FROM AspNetUsers;
                
                -- Reset auto-increment counters
                ALTER TABLE AspNetUsers AUTO_INCREMENT = 1;
                ALTER TABLE Sponsors AUTO_INCREMENT = 1;
                ALTER TABLE PointTransactions AUTO_INCREMENT = 1;
                ALTER TABLE Products AUTO_INCREMENT = 1;
                ALTER TABLE Purchases AUTO_INCREMENT = 1;
                ALTER TABLE DriverApplications AUTO_INCREMENT = 1;
                ALTER TABLE AuditLog AUTO_INCREMENT = 1;
            ");

            // Drop Permissions table
            migrationBuilder.Sql("DROP TABLE IF EXISTS Permissions;");

            // // Add name columns to Drivers
            // migrationBuilder.Sql(@"
            //     SELECT COUNT(*) INTO @firstName_exists 
            //     FROM INFORMATION_SCHEMA.COLUMNS 
            //     WHERE TABLE_NAME = 'Drivers' 
            //     AND COLUMN_NAME = 'FirstName';

            //     SELECT COUNT(*) INTO @lastName_exists 
            //     FROM INFORMATION_SCHEMA.COLUMNS 
            //     WHERE TABLE_NAME = 'Drivers' 
            //     AND COLUMN_NAME = 'LastName';

            //     SET @sql = CONCAT(
            //         CASE WHEN @firstName_exists = 0 THEN
            //             'ALTER TABLE Drivers ADD COLUMN FirstName varchar(100) NOT NULL DEFAULT "";'
            //         ELSE '' END,
            //         CASE WHEN @lastName_exists = 0 THEN
            //             'ALTER TABLE Drivers ADD COLUMN LastName varchar(100) NOT NULL DEFAULT "";'
            //         ELSE '' END
            //     );

            //     IF LENGTH(@sql) > 0 THEN
            //         PREPARE stmt FROM @sql;
            //         EXECUTE stmt;
            //         DEALLOCATE PREPARE stmt;
            //     END IF;
            // ");

            // Insert Admin Users
            migrationBuilder.Sql(@"
                INSERT INTO AspNetUsers (
                    UserName, NormalizedUserName, Email, NormalizedEmail,
                    UserType, CreatedAt, NotifyPref, EmailConfirmed,
                    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
                    AccessFailedCount, SecurityStamp, ConcurrencyStamp,
                    PasswordHash
                ) VALUES 
                ('MasterAdmin', 'MASTERADMIN', 'admin1@gitgud.com', 'ADMIN1@GITGUD.COM',
                    'Admin', UTC_TIMESTAMP(), 2, 1, 0, 0, 1, 0,
                    UUID(), UUID(),
                    'AQAAAAIAAYagAAAAEPahxWWyjGPmF3MK+6Tg9Sz3PPyBTgQK8VhvXn+hXF9+WGNTYvWm+wcaF80GX1VEkA=='),
                ('MasterAdmin2', 'MASTERADMIN2', 'admin2@gitgud.com', 'ADMIN2@GITGUD.COM',
                    'Admin', UTC_TIMESTAMP(), 2, 1, 0, 0, 1, 0,
                    UUID(), UUID(),
                    'AQAAAAIAAYagAAAAEPahxWWyjGPmF3MK+6Tg9Sz3PPyBTgQK8VhvXn+hXF9+WGNTYvWm+wcaF80GX1VEkA==');

                INSERT INTO Admins (UserID) 
                SELECT Id FROM AspNetUsers WHERE UserType = 'Admin';

                -- Insert Sponsor Users
                INSERT INTO AspNetUsers (
                    UserName, NormalizedUserName, Email, NormalizedEmail,
                    UserType, CreatedAt, NotifyPref, EmailConfirmed,
                    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
                    AccessFailedCount, SecurityStamp, ConcurrencyStamp,
                    PasswordHash
                )
                SELECT 
                    name, UPPER(name), 
                    CONCAT('sponsor@', LOWER(name), '.com'), 
                    CONCAT('SPONSOR@', UPPER(name), '.COM'),
                    'Sponsor', UTC_TIMESTAMP(), 2, 1, 0, 0, 1, 0,
                    UUID(), UUID(),
                    'AQAAAAIAAYagAAAAEJdMkDIj+E/+TkfZFNwxpiJFtsD0ASUGj/cJAyXfTC2JvEQfh5OP74EKKP30eNfRnw=='
                FROM (
                    SELECT 'TruckCo' as name UNION
                    SELECT 'LogiCorp' UNION
                    SELECT 'FastFleet' UNION
                    SELECT 'SpeedStats'
                ) sponsors;

                -- Insert Sponsor records
                INSERT INTO Sponsors (UserID, SponsorType, CompanyName, PointDollarValue)
                SELECT Id, 'Corporate', UserName,
                    CASE UserName
                        WHEN 'TruckCo' THEN 0.01
                        WHEN 'LogiCorp' THEN 0.015
                        WHEN 'FastFleet' THEN 0.0125
                        WHEN 'SpeedStats' THEN 0.02
                    END
                FROM AspNetUsers
                WHERE UserType = 'Sponsor';

                -- Insert Driver Users with realistic names per company
                INSERT INTO AspNetUsers (
                    UserName, NormalizedUserName, Email, NormalizedEmail,
                    UserType, CreatedAt, NotifyPref, EmailConfirmed,
                    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
                    AccessFailedCount, SecurityStamp, ConcurrencyStamp,
                    PasswordHash
                )
                SELECT 
                    username, UPPER(username),
                    CONCAT(LOWER(username), '@gitgud.com'),
                    CONCAT(UPPER(username), '@GITGUD.COM'),
                    'Driver', UTC_TIMESTAMP(), 2, 1, 0, 0, 1, 0,
                    UUID(), UUID(),
                    'AQAAAAIAAYagAAAAEJdMkDIj+E/+TkfZFNwxpiJFtsD0ASUGj/cJAyXfTC2JvEQfh5OP74EKKP30eNfRnw=='
                FROM (
                    -- TruckCo Drivers
                    SELECT 'DriverTr1' as username, 'Michael' as firstname, 'Thompson' as lastname, 'TruckCo' as company UNION
                    SELECT 'DriverTr2', 'Robert', 'Anderson', 'TruckCo' UNION
                    -- LogiCorp Drivers
                    SELECT 'DriverLc1', 'William', 'Rodriguez', 'LogiCorp' UNION
                    SELECT 'DriverLc2', 'James', 'Martinez', 'LogiCorp' UNION
                    -- FastFleet Drivers
                    SELECT 'DriverFf1', 'David', 'Wilson', 'FastFleet' UNION
                    SELECT 'DriverFf2', 'Richard', 'Taylor', 'FastFleet' UNION
                    -- SpeedStats Drivers
                    SELECT 'DriverSs1', 'Christopher', 'Brown', 'SpeedStats' UNION
                    SELECT 'DriverSs2', 'Joseph', 'Davis', 'SpeedStats'
                ) drivers;

                -- Insert Driver records with proper names
                INSERT INTO Drivers (UserID, SponsorID, FirstName, LastName, TotalPoints)
                SELECT 
                    u.Id,
                    s.SponsorID,
                    CASE 
                        WHEN u.UserName = 'DriverTr1' THEN 'Michael'
                        WHEN u.UserName = 'DriverTr2' THEN 'Robert'
                        WHEN u.UserName = 'DriverLc1' THEN 'William'
                        WHEN u.UserName = 'DriverLc2' THEN 'James'
                        WHEN u.UserName = 'DriverFf1' THEN 'David'
                        WHEN u.UserName = 'DriverFf2' THEN 'Richard'
                        WHEN u.UserName = 'DriverSs1' THEN 'Christopher'
                        WHEN u.UserName = 'DriverSs2' THEN 'Joseph'
                    END,
                    CASE 
                        WHEN u.UserName = 'DriverTr1' THEN 'Thompson'
                        WHEN u.UserName = 'DriverTr2' THEN 'Anderson'
                        WHEN u.UserName = 'DriverLc1' THEN 'Rodriguez'
                        WHEN u.UserName = 'DriverLc2' THEN 'Martinez'
                        WHEN u.UserName = 'DriverFf1' THEN 'Wilson'
                        WHEN u.UserName = 'DriverFf2' THEN 'Taylor'
                        WHEN u.UserName = 'DriverSs1' THEN 'Brown'
                        WHEN u.UserName = 'DriverSs2' THEN 'Davis'
                    END,
                    CASE 
                        WHEN u.UserName LIKE '%1' THEN FLOOR(RAND() * 50)
                        ELSE FLOOR(50 + RAND() * 51)
                    END
                FROM AspNetUsers u
                JOIN Sponsors s ON u.UserName LIKE 
                    CASE 
                        WHEN s.CompanyName = 'TruckCo' THEN 'DriverTr%'
                        WHEN s.CompanyName = 'LogiCorp' THEN 'DriverLc%'
                        WHEN s.CompanyName = 'FastFleet' THEN 'DriverFf%'
                        WHEN s.CompanyName = 'SpeedStats' THEN 'DriverSs%'
                    END
                WHERE u.UserType = 'Driver';

                -- Insert SponsorDrivers records
                INSERT INTO SponsorDrivers (DriverID, SponsorID)
                SELECT UserID, SponsorID FROM Drivers;

                -- Assign Roles
                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, r.Id
                FROM AspNetUsers u
                JOIN AspNetRoles r ON u.UserType = r.Name;

                -- Insert Role Claims
                INSERT INTO AspNetRoleClaims (RoleId, ClaimType, ClaimValue)
                SELECT r.Id, 'Permission', p.Permission
                FROM AspNetRoles r
                CROSS JOIN (
                    -- Admin Permissions
                    SELECT 'Admin' as Role, 'ManageUsers' as Permission UNION
                    SELECT 'Admin', 'ViewReports' UNION
                    SELECT 'Admin', 'ManageSystem' UNION
                    SELECT 'Admin', 'FullAccess' UNION
                    -- Sponsor Permissions
                    SELECT 'Sponsor', 'ManageDrivers' UNION
                    SELECT 'Sponsor', 'ViewReports' UNION
                    SELECT 'Sponsor', 'ManagePoints' UNION
                    SELECT 'Sponsor', 'ManageCatalog' UNION
                    -- Driver Permissions
                    SELECT 'Driver', 'ViewProfile' UNION
                    SELECT 'Driver', 'ViewCatalog' UNION
                    SELECT 'Driver', 'MakePurchases'
                ) p
                WHERE r.Name = p.Role;
            ");

            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Clear all data except AspNetRoles
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");
            migrationBuilder.Sql(@"
                TRUNCATE TABLE AspNetRoleClaims;
                TRUNCATE TABLE AspNetUserRoles;
                TRUNCATE TABLE AspNetUserClaims;
                TRUNCATE TABLE AspNetUserLogins;
                TRUNCATE TABLE AspNetUserTokens;
                TRUNCATE TABLE PointTransactions;
                TRUNCATE TABLE Purchases;
                TRUNCATE TABLE Products;
                TRUNCATE TABLE SponsorDrivers;
                TRUNCATE TABLE DriverApplications;
                TRUNCATE TABLE Drivers;
                TRUNCATE TABLE Sponsors;
                TRUNCATE TABLE Admins;
                TRUNCATE TABLE AuditLog;
                TRUNCATE TABLE FeedbackForms;
                DELETE FROM AspNetUsers;
            ");
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");

            migrationBuilder.Sql(@"
                CREATE TABLE Permissions (
                    PermissionID int NOT NULL AUTO_INCREMENT,
                    Role longtext NOT NULL,
                    Permission longtext NOT NULL,
                    PRIMARY KEY (PermissionID)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            ");
        }
    }
}
