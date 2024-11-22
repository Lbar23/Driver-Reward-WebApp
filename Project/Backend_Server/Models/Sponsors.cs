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
        Independent
    }
    public class Sponsors
    {
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public required SponsorType SponsorType { get; set; } = SponsorType.Independent;
        public required string CompanyName { get; set; }
        public decimal PointDollarValue { get; set; } = 0.01m;
        public ICollection<SponsorDrivers> SponsorDrivers { get; set; } = []; //Added after adding bridge entity for connection
        public ICollection<SponsorUsers> SponsorUsers { get; set; } = [];
    }
}