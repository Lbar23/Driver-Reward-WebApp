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
    public class AuditLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; }
        public AuditLogCategory Category { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public Users User { get; set; }
    }
}