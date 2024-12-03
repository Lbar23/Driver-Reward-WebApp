using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Server.Models
{
    public enum AuthenticationType
    {
        None,
        Login,
        Logout,
        TwoFactorAuth,
        PasswordReset
    }
    public class Authentications
    {
        public int AuthID { get; set; } // Primary Key
        public int UserID { get; set; } // Foreign Key to Users
        public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required AuthenticationType AuthType { get; set; }
        public required bool Success { get; set; }
        public string? UserAgent { get; set; } // The User's browser string (Mozilla/5.0 (Windows NT 10.0; Win64; x64), Chrome/91.0.4472.124, Safari/537.36, etc.)
        public string? Details { get; set; } // Detailed information about the authentication event

        // Navigation property
        [ForeignKey("UserID")]
        public required Users User { get; set; }
    }
}