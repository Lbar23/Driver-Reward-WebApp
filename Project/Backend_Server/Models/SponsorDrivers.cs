using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class SponsorDrivers
    {
        public required int UserID { get; set; }  // composite key
        public required int SponsorID { get; set; } // composite key
        public required int Points { get; set; } // Points specific to this sponsor
        public required int MilestoneLevel { get; set; } = 0; // Optional milestone level, specific to sponsor
        public required decimal DriverPointValue { get; set; } // Sponsor-specific point value

        // Navigation properties
        public required Users User { get; set; }
        public required Sponsors Sponsor { get; set; }
        public ICollection<Purchases> Purchases { get; set; } = [];
    }
}