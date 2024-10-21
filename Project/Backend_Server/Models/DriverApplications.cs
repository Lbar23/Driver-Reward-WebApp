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
        public int DriverID { get; set; }
        public int SponsorID { get; set; }
        public AppStatus Status { get; set; } = AppStatus.Submitted;
        public DateTime ApplyDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public required Drivers Driver { get; set; }
        public required Sponsors Sponsor { get; set; }
    }
}