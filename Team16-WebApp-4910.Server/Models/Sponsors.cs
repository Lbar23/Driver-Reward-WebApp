using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team16_WebApp_4910.Server.Models
{
    public class Sponsors
    {
        public int SponsorID { get; set; }
        public int UserID { get; set; }
        public string CompanyName { get; set; }
        public decimal PointDollarValue { get; set; }
        public Users User { get; set; }
    }
}