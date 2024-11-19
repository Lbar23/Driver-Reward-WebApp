using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum NotificationPref
    {
        None,   // In case 2FA is not enabled - 0 in db
        Phone,  // 1 in db
        Email  // 2 in db
    }
    public enum UserType
    {
        Driver,   // 0 in db
        Sponsor,  // 1 in db
        Admin,  // 2 in db
        Guest // 3 in db, for unapproved drivers
    }
    public class Users : IdentityUser<int>
    {
        public required string UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public NotificationPref NotifyPref { get; set; } = NotificationPref.None; 
        public ICollection<SponsorUsers> SponsorUsers { get; set; } = [];
    }
    // Package inherits AspNetUser Properties:
        // `Id` int NOT NULL AUTO_INCREMENT,
        // `UserName` varchar(256) NOT NULL,
        // `NormalizedUserName` varchar(256) NOT NULL,
        // `Email` varchar(256) NOT NULL,
        // `NormalizedEmail` varchar(256) DEFAULT NULL,
        // `EmailConfirmed` bit(1) NOT NULL,
        // `PasswordHash` varchar(255) DEFAULT NULL,
        // `SecurityStamp` varchar(255) DEFAULT NULL,
        // `ConcurrencyStamp` varchar(255) DEFAULT NULL,
        // `PhoneNumber` varchar(50) DEFAULT NULL,
        // `PhoneNumberConfirmed` bit(1) NOT NULL,
        // `TwoFactorEnabled` bit(1) NOT NULL,
        // `LockoutEnd` datetime DEFAULT NULL,
        // `LockoutEnabled` bit(1) NOT NULL,
        // `AccessFailedCount` int NOT NULL,
        // PRIMARY KEY (`Id`),
        // UNIQUE KEY `UserName` (`UserName`),
        // UNIQUE KEY `NormalizedUserName` (`NormalizedUserName`),
        // UNIQUE KEY `Email` (`Email`)
}