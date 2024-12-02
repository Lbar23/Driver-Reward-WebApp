using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum AuditLogCategory
    {
        User,  // for user creation, removal, changes
        Password, // password changes or resets
        Authentication,  // login failure & success, includes 2fa failures
    }

     public enum AuditLogAction
    {
        Add, 
        Remove,  
        Update
    }

    public class AuditLogs
    {
        public int LogID { get; set; } // Primary Key
        public required int UserID { get; set; } // FK to Users
        public required AuditLogCategory Category { get; set; } 
        public required AuditLogAction Action { get; set; } 
        public required bool ActionSuccess { get; set;}
        public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? AdditionalDetails { get; set; } // Stores JSON or other details for context

        // Navigation Properties
        public required Users User { get; set; }
    }

}