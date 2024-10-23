using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum AppStatus
    {
        Submitted, 
        Approved,
        Rejected,
    }

    public class DriverApplications
    {
        public int ApplicationID { get; set; }
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public AppStatus Status { get; set; } = AppStatus.Submitted;
        public DateOnly ApplyDate { get; set; }
        public DateOnly? ProcessedDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}