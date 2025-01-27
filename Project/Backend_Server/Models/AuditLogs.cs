using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum AuditLogCategory
    {
        User,
        Point,
        Purchase,
        Application,
        Product,
        System
    }

    public class AuditLogs
    {
        public int UserID { get; set; }
        public int LogID { get; set; }
        public AuditLogCategory Category { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}