using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum OrderStatus
    {
        Ordered,
        Updated,
        Cancelled,
        Refunded,
        Complete
    }

    public class Purchases
    {
        public int PurchaseID { get; set; } // Primary Key
        public required int SponsorID { get; set; } // Part of SponsorDrivers FK
        public required int UserID { get; set; }  // Part of SponsorDrivers FK
        public required int TotalPointsSpent { get; set; }
        public required DateTime PurchaseDate { get; set; }
        public required OrderStatus Status { get; set; } = OrderStatus.Ordered;

        // Navigation properties
        public required SponsorDrivers SponsorDriver { get; set; } // Maps the sponsor-driver relationship
        public ICollection<PurchaseProducts> PurchaseProducts { get; set; } = []; // Made optional initially
    }
}
