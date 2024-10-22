using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Drivers
    {
        public int DriverID { get; set; }
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public int TotalPoints { get; set; }
        public required Users User { get; set; }
        public required Sponsors Sponsor { get; set; }
    }
}