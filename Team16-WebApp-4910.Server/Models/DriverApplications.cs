using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team16_WebApp_4910.Server.Models
{
    public class DriverApplications
    {
        public int ApplicationID { get; set; }
        public int DriverID { get; set; }
        public int SponsorID { get; set; }
        public string Status { get; set; }
        public DateTime ApplyDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Reason { get; set; }
        public Drivers Driver { get; set; }
        public Sponsors Sponsor { get; set; }
    }
}