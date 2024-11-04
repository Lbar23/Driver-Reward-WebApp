using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class SponsorDrivers //bridge entity
    {
        public int DriverID { get; set; }
        public int SponsorID { get; set; }
        public required Drivers Driver { get; set; }
        public required Sponsors Sponsor { get; set; }
    }
}