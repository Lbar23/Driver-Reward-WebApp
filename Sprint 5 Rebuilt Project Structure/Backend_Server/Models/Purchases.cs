using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Purchases
    {
        public int PurchaseID { get; set; }
        public int DriverID { get; set; }
        public int ProductID { get; set; }
        public int PointsSpent { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Status { get; set; }
        public Drivers Driver { get; set; }
        public Products Product { get; set; }
    }
}