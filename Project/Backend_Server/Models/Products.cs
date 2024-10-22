using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Products
    {
        public int ProductID { get; set; }
        public int SponsorID { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public int PriceInPoints { get; set; }
        public required string ExternalID { get; set; }
        public bool Availability { get; set; }
        public required Sponsors Sponsor { get; set; }
    }
}