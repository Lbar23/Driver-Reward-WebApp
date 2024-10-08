using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team16_WebApp_4910.Server.Models
{
    public class Products
    {
        public int ProductID { get; set; }
        public int SponsorID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriceInPoints { get; set; }
        public string ExternalID { get; set; }
        public bool Availability { get; set; }
        public Sponsors Sponsor { get; set; }
    }
}