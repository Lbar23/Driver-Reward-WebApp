using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{

    public class SponsorUsers 
    {
        public required int UserID { get; set; }
        public required int SponsorID { get; set; }
        public required bool IsPrimary { get; set; } = false; //easier identifier for main sponsor under company name
        public required DateTime JoinDate { get; set; }
        public required Users User { get; set; } 
        public required Sponsors Sponsor { get; set; }
    }
}