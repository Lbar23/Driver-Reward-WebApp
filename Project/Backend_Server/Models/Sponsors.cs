using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum SponsorType
    {
        Logistics,
        Trucking,
        Fleeting,
        Insurance,
        Independent
    }
    public class Sponsors
    {
        public int SponsorID { get; set; }
        public required SponsorType SponsorType { get; set; } = SponsorType.Independent;
        public required string CompanyName { get; set; }
        public required decimal PointDollarValue { get; set; } = 0.01m;
        public required int MilestoneThreshold { get; set; } = 0; // default is 0 and this indicates milestones arent enabled
        public ICollection<SponsorDrivers> SponsorDrivers { get; set; } = []; 
        public ICollection<SponsorUsers> SponsorUsers { get; set; } = [];
    }
}