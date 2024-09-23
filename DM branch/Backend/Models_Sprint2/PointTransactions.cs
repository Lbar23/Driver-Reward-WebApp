using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class PointTransactions
    {
        public int TransactionID { get; set; }
        public int DriverID { get; set; }
        public int SponsorID { get; set; }
        public int PointsChanged { get; set; }
        public string Reason { get; set; }
        public DateTime TransactionDate { get; set; }
        public Drivers Driver { get; set; }
        public Sponsors Sponsor { get; set; }
    }
}