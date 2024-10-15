using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Sponsors
    {
        public int SponsorID { get; set; }
        public int UserID { get; set; }
        public required string CompanyName { get; set; }
        public decimal PointDollarValue { get; set; }
        public required Users User { get; set; }
    }
}