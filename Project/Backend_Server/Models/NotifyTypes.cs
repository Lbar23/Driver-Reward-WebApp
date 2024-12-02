using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum NotifyCategory
    {
        Auth,
        Purchase,
        PointsChange,
        SystemChange,
        AppStatus,
        OrderIssue,
        PointsReport
    }

    public class NotifyTypes
    {
        public int TypeID { get; set; }
        public required NotifyCategory Category { get; set; }
        public string? Description { get; set; } 
        public string? EmailTemplateID { get; set; } // Template ID for SendGrid
        public string? TemplateFieldsJson { get; set; } = "[]"; // JSON array of expected dynamic fields
        public required bool IsActive { get; set; } = true; // Indicates if the type is currently in use

    }
}