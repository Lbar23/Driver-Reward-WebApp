using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Server.Models
{
    public enum ActivityType
    {
        PasswordChange,
        // EmailChange,
        // PhoneChange,
        UpdateContact,
        UsernameChange,
        ResetPassword,
        AccountRemoved,
        AccountCreated,
        RoleChange,
        UpdateProfile
    }

    public class AccountActivity
    {
        public int ActivityId { get; set; } // Primary Key
        public int UserId { get; set; } // Foreign Key to Users
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public required ActivityType ActivityType { get; set; } // The type of activity
        public string? Details { get; set; } // Additional details about the activity

        // Navigation Properties
        [ForeignKey("UserId")]
        public required Users User { get; set; }
    }
}