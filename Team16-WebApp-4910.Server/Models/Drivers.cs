using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team16_WebApp_4910.Server.Models
{
    public class Drivers
    {
        public int DriverID { get; set; }
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public int TotalPoints { get; set; }
        public Users User { get; set; }
        public Sponsors Sponsor { get; set; }
    }
}