using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Sponsors
    {
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public required string SponsorType { get; set; }
        //public required string AccessCode { get; set; } <-Replace?
        public required string CompanyName { get; set; }
        public decimal PointDollarValue { get; set; }
    }
}